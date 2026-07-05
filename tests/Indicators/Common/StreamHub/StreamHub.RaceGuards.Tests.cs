using System.Collections.Concurrent;

namespace Observables;

/// <summary>
/// Pins the streaming correctness-race fixes: the observer set is safe to
/// mutate (Subscribe/Unsubscribe) concurrently with the notify fan-out, and
/// <c>Reinitialize</c> does not drop an item the provider appends in the
/// rebuild→subscribe gap.
/// </summary>
[TestClass]
public class RaceGuards : TestBase
{
    [TestMethod]
    public void Reinitialize_WhenProviderGrowsDuringSubscribe_FoldsInGapItem()
    {
        // 30 initial items are present when the hub rebuilds; a 31st is injected
        // during Subscribe to simulate an item arriving in the rebuild→subscribe
        // gap. The catch-up rebuild must fold it in.
        List<IReusable> initial = Bars.Take(30).Cast<IReusable>().ToList();
        IReusable gapItem = Bars[30];

        GapInjectingProvider provider = new(initial, gapItem);

        // constructing the hub runs Reinitialize internally
        EmaHub ema = provider.ToEmaHub(20);

        provider.SubscribeCount.Should().Be(1, "the hub subscribes exactly once");
        ema.Results.Should().HaveCount(31, "the gap item must not be dropped");
        ema.Results[^1].Timestamp.Should().Be(gapItem.Timestamp);
    }

    [TestMethod]
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Design", "CA1031:Do not catch general exception types",
        Justification = "The test deliberately records any exception thrown by the concurrent threads to assert the race guard holds.")]
    public void Subscribe_ConcurrentWithNotify_DoesNotCorruptObserverSet()
    {
        // One single writer feeds the hub while another thread churns
        // subscribe/unsubscribe. Without synchronization, the notify loop's
        // snapshot of the observer set races the churn's mutation and throws
        // (or corrupts). With the guard, neither thread faults.
        BarHub barHub = new();

        // a stable population so each notify fan-out enumerates a non-trivial
        // set (widens the race window the guard must close)
        List<IDisposable> baseSubs = [];
        for (int i = 0; i < 64; i++)
        {
            baseSubs.Add(barHub.Subscribe(new NoopObserver()));
        }

        ConcurrentBag<Exception> failures = [];
        using CancellationTokenSource done = new();

        Thread churn = new(() => {
            try
            {
                NoopObserver[] pool = [.. Enumerable.Range(0, 16).Select(_ => new NoopObserver())];
                while (!done.Token.IsCancellationRequested)
                {
                    foreach (IDisposable s in pool.Select(barHub.Subscribe).ToList())
                    {
                        s.Dispose();
                    }
                }
            }
            catch (Exception ex)
            {
                failures.Add(ex);
            }
        });
        churn.Start();

        // single writer: many synthetic, strictly-increasing bars
        try
        {
            DateTime t = new(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            for (int i = 0; i < 30_000; i++)
            {
                barHub.Add(new Bar(t, 100m, 101m, 99m, 100m, 1000));
                t = t.AddMinutes(1);
            }
        }
        catch (Exception ex)
        {
            failures.Add(ex);
        }
        finally
        {
            done.Cancel();
            churn.Join();
        }

        failures.Should().BeEmpty(
            "concurrent Subscribe/Unsubscribe and notify must not corrupt the observer set");

        foreach (IDisposable s in baseSubs)
        {
            s.Dispose();
        }

        barHub.EndTransmission();
    }

    [TestMethod]
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Design", "CA1031:Do not catch general exception types",
        Justification = "The test deliberately records any exception thrown by the concurrent threads to assert the race guard holds.")]
    public void EndTransmission_ConcurrentWithNotify_DoesNotThrow()
    {
        // EndTransmission clears the observer set under the lock while the
        // writer notifies; neither side may throw.
        ConcurrentBag<Exception> failures = [];
        using CancellationTokenSource done = new();

        BarHub barHub = new();

        Thread teardown = new(() => {
            try
            {
                NoopObserver[] pool = [.. Enumerable.Range(0, 32).Select(_ => new NoopObserver())];
                while (!done.Token.IsCancellationRequested)
                {
                    foreach (NoopObserver o in pool)
                    {
                        barHub.Subscribe(o);
                    }

                    barHub.EndTransmission();
                }
            }
            catch (Exception ex)
            {
                failures.Add(ex);
            }
        });
        teardown.Start();

        try
        {
            DateTime t = new(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            for (int i = 0; i < 20_000; i++)
            {
                barHub.Add(new Bar(t, 100m, 101m, 99m, 100m, 1000));
                t = t.AddMinutes(1);
            }
        }
        catch (Exception ex)
        {
            failures.Add(ex);
        }
        finally
        {
            done.Cancel();
            teardown.Join();
        }

        failures.Should().BeEmpty();

        barHub.EndTransmission();
    }

    /// <summary>
    /// A chain provider whose <c>Subscribe</c> appends one item to its results,
    /// simulating an item that arrives in the rebuild→subscribe gap. Returns the
    /// same backing list from <c>Results</c> so the appended item is visible to
    /// the hub's <c>ProviderCache</c>.
    /// </summary>
    private sealed class GapInjectingProvider(IEnumerable<IReusable> initial, IReusable gapItem)
        : IChainProvider<IReusable>
    {
        private readonly List<IReusable> _items = [.. initial];
        private bool _injected;

        public int SubscribeCount { get; private set; }

        public BinarySettings Properties { get; } = new(0);
        public IReadOnlyList<IReusable> Results => _items;
        public int MaxCacheSize => 100_000;
        public int ObserverCount => 0;
        public bool HasObservers => false;
        public bool HasSubscriber(IStreamObserver<IReusable> observer) => false;

        public IDisposable Subscribe(IStreamObserver<IReusable> observer)
        {
            SubscribeCount++;

            if (!_injected)
            {
                _items.Add(gapItem);
                _injected = true;
            }

            return new NoopDisposable();
        }

        public bool Unsubscribe(IStreamObserver<IReusable> observer) => false;
        public void EndTransmission() { }

        private sealed class NoopDisposable : IDisposable
        {
            public void Dispose() { }
        }
    }

    private sealed class NoopObserver : IStreamObserver<IBar>
    {
        public bool IsSubscribed => false;
        public void Unsubscribe() { }
        public void OnAdd(IBar item, bool notify, int? indexHint) { }
        public void OnRebuild(DateTime fromTimestamp) { }
        public void OnPrune(DateTime toTimestamp) { }
        public void OnError(Exception exception) { }
        public void OnCompleted() { }
    }
}
