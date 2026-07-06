namespace StreamHubs;

[TestClass]
public class DpoHubTests : StreamHubTestBase, ITestChainObserver, ITestChainProvider
{
    private const int lookbackPeriods = 14;

    [TestMethod]
    public void BarObserver_WithWarmupLateArrivalAndRemoval_MatchesSeriesExactly()
    {
        // setup bar provider hub
        BarHub barHub = new();

        // prefill bars at provider
        barHub.Add(Bars.Take(20));

        // initialize observer
        DpoHub observer = barHub.ToDpoHub(lookbackPeriods);

        // fetch initial results (early)
        IReadOnlyList<DpoResult> sut = observer.Results;

        // emulate adding bars to provider hub
        for (int i = 20; i < barsCount; i++)
        {
            // skip one (add later)
            if (i == 80) { continue; }

            Bar q = Bars[i];
            barHub.Add(q);

            // resend duplicate bars
            if (i is > 100 and < 105) { barHub.Add(q); }
        }

        // late arrival, should equal series
        barHub.Add(Bars[80]);

        IReadOnlyList<DpoResult> expectedOriginal = Bars.ToDpo(lookbackPeriods);
        sut.IsExactly(expectedOriginal);

        // delete, should equal series (revised)
        barHub.RemoveAt(removeAtIndex);

        IReadOnlyList<DpoResult> expectedRevised = RevisedBars.ToDpo(lookbackPeriods);
        sut.IsExactly(expectedRevised);
        sut.Should().HaveCount(barsCount - 1);

        // cleanup
        observer.Unsubscribe();
        barHub.EndTransmission();
    }

    [TestMethod]
    public void WithCachePruning_MatchesSeriesExactly()
    {
        const int maxCacheSize = 50;
        const int totalBars = 100;

        IReadOnlyList<Bar> bars = Bars.Take(totalBars).ToList();
        IReadOnlyList<DpoResult> expected = bars
            .ToDpo(lookbackPeriods)
            .TakeLast(maxCacheSize)
            .ToList();

        // Setup with cache limit
        BarHub barHub = new(maxCacheSize);
        DpoHub observer = barHub.ToDpoHub(lookbackPeriods);

        // Stream more bars than cache can hold
        barHub.Add(bars);

        // Verify cache was pruned
        barHub.Bars.Should().HaveCount(maxCacheSize);
        observer.Results.Should().HaveCount(maxCacheSize);

        // Streaming results should match last N from full series (original series with front chopped off)
        // NOT recomputation on just the cached bars (which would have different warmup)
        observer.Results.IsExactly(expected);

        observer.Unsubscribe();
        barHub.EndTransmission();
    }

    [TestMethod]
    public void ChainObserver_ChainedProvider_MatchesSeriesExactly()
    {
        const int dpoPeriods = 14;
        const int smaPeriods = 8;

        // setup bar provider hub
        BarHub barHub = new();

        // initialize observer
        DpoHub observer = barHub
            .ToSmaHub(smaPeriods)
            .ToDpoHub(dpoPeriods);

        // emulate bar stream
        for (int i = 0; i < barsCount; i++) { barHub.Add(Bars[i]); }

        // final results
        IReadOnlyList<DpoResult> actuals = observer.Results;

        // time-series, for comparison
        IReadOnlyList<DpoResult> expected = Bars
            .ToSma(smaPeriods)
            .ToDpo(dpoPeriods);

        // assert, should equal series
        actuals.Should().HaveCount(barsCount);
        actuals.IsExactly(expected);

        // cleanup
        observer.Unsubscribe();
        barHub.EndTransmission();
    }

    [TestMethod]
    public void ChainProvider_MatchesSeriesExactly()
    {
        // FRAMEWORK FIX APPLIED: StreamHub.Rebuild() and OnRebuild() are now virtual methods.
        // DpoHub overrides Rebuild() to adjust the timestamp backward by offset, ensuring
        // downstream observers (e.g., SmaHub) are notified of the actual affected position
        // during provider history mutations (Add/Remove).
        //
        // How it works: When Add(Bars[80]) occurs with offset=11:
        //   1. DpoHub.RollbackState() removes cache from position 69 ✓
        //   2. DpoHub.OnAdd() recalculates positions [69, 80] ✓
        //   3. DpoHub.Rebuild() adjusts timestamp to position 69 and notifies downstream ✓
        //   4. Downstream SmaHub recalculates from position 69, maintaining correctness ✓
        //
        // Note: DPO BarObserver test passes without this override because it has no
        // downstream observers requiring adjusted rebuild positions.

        const int dpoPeriods = 20;
        const int smaPeriods = 10;

        // setup bar provider hub
        BarHub barHub = new();

        // initialize observer
        SmaHub observer = barHub
            .ToDpoHub(dpoPeriods)
            .ToSmaHub(smaPeriods);

        // emulate adding bars to provider hub
        for (int i = 0; i < barsCount; i++)
        {
            // skip one (add later)
            if (i == 80) { continue; }

            Bar q = Bars[i];
            barHub.Add(q);

            // resend duplicate bars
            if (i is > 100 and < 105) { barHub.Add(q); }
        }

        // late arrival
        barHub.Add(Bars[80]);

        // delete
        barHub.RemoveAt(removeAtIndex);

        // final results
        IReadOnlyList<SmaResult> sut = observer.Results;

        // time-series, for comparison (revised)
        IReadOnlyList<SmaResult> expected = RevisedBars
            .ToDpo(dpoPeriods)
            .ToSma(smaPeriods);

        // assert, should equal series
        sut.IsExactly(expected);
        sut.Should().HaveCount(barsCount - 1);

        // cleanup
        observer.Unsubscribe();
        barHub.EndTransmission();
    }

    [TestMethod]
    public override void ToStringOverride_ReturnsExpectedName()
    {
        DpoHub hub = new(new BarHub(), 14);
        hub.ToString().Should().Be("DPO(14)");
    }
}
