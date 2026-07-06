namespace Observables;

/// <summary>
/// Pins the <c>Snapshot()</c> contract: an immutable point-in-time copy of the
/// cache that is decoupled from the live <c>Results</c> view, so it can be
/// safely handed to another thread.
/// </summary>
[TestClass]
public class Snapshot : TestBase
{
    public TestContext TestContext { get; set; }

    [TestMethod]
    public void Snapshot_MatchesResultsContent()
    {
        BarHub barHub = new();
        SmaHub smaHub = barHub.ToSmaHub(20);

        barHub.Add(Bars.Take(50));

        IReadOnlyList<SmaResult> snapshot = smaHub.Snapshot();

        snapshot.Should().HaveCount(smaHub.Results.Count);
        snapshot.Should().Equal(smaHub.Results);

        barHub.EndTransmission();
    }

    [TestMethod]
    public void Snapshot_IsDecoupledFromLiveCache()
    {
        BarHub barHub = new();

        barHub.Add(Bars.Take(30));

        IReadOnlyList<IBar> snapshot = barHub.Snapshot();
        snapshot.Should().HaveCount(30);

        // mutate the hub after taking the snapshot
        barHub.Add(Bars.Skip(30).Take(20));
        barHub.RemoveAt(0);

        // the snapshot is a detached copy — unchanged by later mutation
        snapshot.Should().HaveCount(30);

        // ...while the live view reflects the changes
        barHub.Results.Should().HaveCount(49);

        barHub.EndTransmission();
    }

    [TestMethod]
    public void Snapshot_IsNotTheLiveResultsInstance()
    {
        BarHub barHub = new();
        barHub.Add(Bars.Take(10));

        IReadOnlyList<IBar> snapshot = barHub.Snapshot();

        snapshot.Should().NotBeSameAs(barHub.Results);
        snapshot.Should().Equal(barHub.Results);

        barHub.EndTransmission();
    }

    [TestMethod]
    public async Task Snapshot_ConcurrentWithAdds_NeverThrowsAndStaysConsistent()
    {
        // Snapshot() copies under the cache lock, so a reader taking snapshots
        // while a single writer adds must never throw and each snapshot must be
        // an internally consistent, sorted prefix.
        BarHub barHub = new();
        List<Bar> bars = Bars.Take(400).ToList();

        using CancellationTokenSource cts = new();

        Task reader = Task.Run(() => {
            while (!cts.IsCancellationRequested)
            {
                IReadOnlyList<IBar> snap = barHub.Snapshot();

                // a snapshot is always sorted ascending (internally consistent)
                for (int i = 1; i < snap.Count; i++)
                {
                    snap[i].Timestamp.Should().BeOnOrAfter(snap[i - 1].Timestamp);
                }
            }
        }, TestContext.CancellationToken);

        // single writer; cancel the reader when done (even if the writer faults,
        // so WhenAll surfaces that fault promptly instead of hanging to timeout)
        Task writer = Task.Run(() => {
            try
            {
                foreach (Bar q in bars)
                {
                    barHub.Add(q);
                }
            }
            finally
            {
                cts.Cancel();
            }
        }, TestContext.CancellationToken);

        await Task.WhenAll(reader, writer).ConfigureAwait(false); // surfaces any assertion failure

        barHub.Snapshot().Should().HaveCount(bars.Count);

        barHub.EndTransmission();
    }
}
