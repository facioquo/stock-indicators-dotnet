using System.Collections.Concurrent;

namespace Observables;

/// <summary>
/// End-to-end safety net for the documented streaming threading contract: a
/// single writer (one thread, or many funneled through a gate) drives results
/// that are bit-for-bit identical to the batch Series, and concurrent readers
/// or observer churn never corrupt the hub. These exercise real indicator
/// chains and assert Series convergence, complementing the low-level
/// mechanism pins in <c>StreamHub.RaceGuards.Tests.cs</c> (which assert only
/// that the observer set survives concurrent mutation).
/// </summary>
[TestClass]
public class Concurrency : TestBase
{
    public TestContext TestContext { get; set; }

    // Generous failsafe for rendezvous waits and thread joins. It costs nothing
    // on the normal path (each Wait/Join returns the instant the threads signal
    // or finish) and turns a regression deadlock into a fast, clear test failure
    // instead of a hung CI job — while staying far above any real scheduling
    // delay, so it cannot itself flake.
    private static readonly TimeSpan ThreadBudget = TimeSpan.FromSeconds(30);

    [TestMethod]
    public void SingleWriterGate_OutOfOrderParallelProducers_ConvergeToSeries()
    {
        // N producers each feed a strided subset (ascending), funneled through a
        // single-writer gate. Threads interleave, so bars arrive globally out
        // of order and drive rollback/replay. Regardless of arrival order, every
        // bar is added exactly once under serialization, so a deep chain must
        // converge bit-for-bit to the batch Series.
        const int count = 500;
        const int producerCount = 8;
        const int emaPeriods = 20;
        const int smaPeriods = 10;

        List<Bar> bars = Bars.Take(count).ToList();
        IReadOnlyList<EmaResult> expectedEma = bars.ToEma(emaPeriods);
        IReadOnlyList<SmaResult> expectedSma = expectedEma.ToSma(smaPeriods);

        BarHub barHub = new();
        EmaHub emaHub = barHub.ToEmaHub(emaPeriods);
        SmaHub smaHub = emaHub.ToSmaHub(smaPeriods);

        // Seed the global-minimum bar first so no later out-of-order arrival is
        // "before head". A standalone BarHub drops before-head bars by
        // design; once index 0 is the head, every other bar is >= it and lands
        // mid-cache (rollback) or at the tail.
        barHub.Add(bars[0]);

        object gate = new();
        Thread[] producers = new Thread[producerCount];
        for (int t = 0; t < producerCount; t++)
        {
            int start = t;
            producers[t] = new Thread(() => {
                for (int i = start; i < count; i += producerCount)
                {
                    if (i == 0) { continue; }  // already seeded

                    lock (gate)
                    {
                        barHub.Add(bars[i]);
                    }
                }
            });
            producers[t].Start();
        }

        foreach (Thread p in producers)
        {
            p.Join(ThreadBudget).Should().BeTrue("producer threads must finish promptly");
        }

        barHub.Bars.Should().HaveCount(count);
        emaHub.Results.IsExactly(expectedEma);
        smaHub.Results.IsExactly(expectedSma);

        smaHub.Unsubscribe();
        emaHub.Unsubscribe();
        barHub.EndTransmission();
    }

    [TestMethod]
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Design", "CA1031:Do not catch general exception types",
        Justification = "The test deliberately records any exception thrown by the concurrent reader threads to assert thread-safety.")]
    public void ConcurrentSnapshotReaders_DuringFeed_StayConsistentAndConverge()
    {
        // A single writer feeds the chain while reader threads continuously
        // Snapshot a downstream hub. Each snapshot must be internally consistent
        // (sorted) and no read may throw; the final results must equal the
        // Series. This pins Snapshot() against a chained hub under live writes —
        // the standalone single-reader pin lives in StreamHub.Snapshot.Tests.cs.
        const int emaPeriods = 20;
        const int readerCount = 4;

        // the default fixture caps at ~500 bars; feed all of them rather than a
        // larger constant that would silently truncate
        List<Bar> bars = Bars.ToList();
        IReadOnlyList<EmaResult> expected = bars.ToEma(emaPeriods);

        BarHub barHub = new();
        EmaHub emaHub = barHub.ToEmaHub(emaPeriods);

        ConcurrentBag<Exception> failures = [];
        using CancellationTokenSource done = new();
        using CountdownEvent readersStarted = new(readerCount);

        Thread[] readers = new Thread[readerCount];
        for (int r = 0; r < readerCount; r++)
        {
            readers[r] = new Thread(() => {
                try
                {
                    readersStarted.Signal();  // rendezvous: guarantee overlap with the feed
                    while (!done.Token.IsCancellationRequested)
                    {
                        IReadOnlyList<EmaResult> snap = emaHub.Snapshot();
                        for (int i = 1; i < snap.Count; i++)
                        {
                            snap[i].Timestamp.Should().BeOnOrAfter(snap[i - 1].Timestamp);
                        }
                    }
                }
                catch (Exception ex)
                {
                    failures.Add(ex);
                }
            });
            readers[r].Start();
        }

        // ensure every reader is live before feeding (assert, so a thread that
        // dies before signaling fails loudly instead of silently under-testing)
        readersStarted.Wait(ThreadBudget, TestContext.CancellationToken)
            .Should().BeTrue("all reader threads must be live before feeding");

        foreach (Bar q in bars)
        {
            barHub.Add(q);
        }

        done.Cancel();
        foreach (Thread reader in readers)
        {
            reader.Join(ThreadBudget).Should().BeTrue("reader threads must finish promptly");
        }

        failures.Should().BeEmpty("concurrent Snapshot() reads must never throw");
        emaHub.Results.IsExactly(expected);

        emaHub.Unsubscribe();
        barHub.EndTransmission();
    }

    [TestMethod]
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Design", "CA1031:Do not catch general exception types",
        Justification = "The test deliberately records any exception thrown by the concurrent churn thread to assert thread-safety.")]
    public void SubscribeDisposeChurn_DuringFeed_PreservesStableObserver()
    {
        // A stable downstream hub stays subscribed while another thread churns
        // subscribe/dispose of throwaway observers. RaceGuards pins that the
        // observer set survives this; here the added assertion is that the
        // stable observer still converges bit-for-bit to the Series despite the
        // churn.
        const int emaPeriods = 20;

        // feed the whole default fixture (a larger constant would silently truncate)
        List<Bar> bars = Bars.ToList();
        IReadOnlyList<EmaResult> expected = bars.ToEma(emaPeriods);

        BarHub barHub = new();
        EmaHub stable = barHub.ToEmaHub(emaPeriods);

        ConcurrentBag<Exception> failures = [];
        using CancellationTokenSource done = new();
        using ManualResetEventSlim churnStarted = new(false);

        Thread churn = new(() => {
            try
            {
                NoopObserver[] pool = [.. Enumerable.Range(0, 8).Select(_ => new NoopObserver())];
                while (!done.Token.IsCancellationRequested)
                {
                    List<IDisposable> subs = [.. pool.Select(barHub.Subscribe)];
                    churnStarted.Set();  // rendezvous: guarantee overlap with the feed
                    foreach (IDisposable s in subs)
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

        // ensure the churn is live before feeding (assert, per the reader analog)
        churnStarted.Wait(ThreadBudget, TestContext.CancellationToken)
            .Should().BeTrue("the churn thread must overlap the live feed");

        try
        {
            foreach (Bar q in bars)
            {
                barHub.Add(q);
            }
        }
        catch (Exception ex)
        {
            failures.Add(ex);
        }
        finally
        {
            done.Cancel();
            churn.Join(ThreadBudget).Should().BeTrue("the churn thread must finish promptly");
        }

        failures.Should().BeEmpty();
        stable.Results.IsExactly(expected);

        stable.Unsubscribe();
        barHub.EndTransmission();
    }

    [TestMethod]
    public void SingleWriterGate_WithLateArrivalAndRemoval_ConvergeToRevisedSeries()
    {
        // Gated parallel producers plus a late arrival and a removal, all
        // serialized through the single-writer gate — the chain converges to the
        // revised Series.
        const int count = 400;
        const int skipIndex = 200;
        const int removeIndex = 120;
        const int emaPeriods = 20;

        List<Bar> bars = Bars.Take(count).ToList();

        BarHub barHub = new();
        EmaHub emaHub = barHub.ToEmaHub(emaPeriods);

        // seed the global-minimum bar first (see the convergence test for why)
        barHub.Add(bars[0]);

        object gate = new();

        // feed everything except one index, from 4 strided producers
        Thread[] producers = new Thread[4];
        for (int t = 0; t < 4; t++)
        {
            int start = t;
            producers[t] = new Thread(() => {
                for (int i = start; i < count; i += 4)
                {
                    if (i is 0 or skipIndex) { continue; }

                    lock (gate)
                    {
                        barHub.Add(bars[i]);
                    }
                }
            });
            producers[t].Start();
        }

        foreach (Thread p in producers)
        {
            p.Join(ThreadBudget).Should().BeTrue("producer threads must finish promptly");
        }

        // late arrival + removal (single writer)
        lock (gate)
        {
            barHub.Add(bars[skipIndex]);
            barHub.RemoveAt(removeIndex);
        }

        List<Bar> revised = [.. bars];
        revised.RemoveAt(removeIndex);

        emaHub.Results.IsExactly(revised.ToEma(emaPeriods));

        emaHub.Unsubscribe();
        barHub.EndTransmission();
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
