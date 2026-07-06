namespace StreamHubs;

[TestClass]
public class StdDevHubTests : StreamHubTestBase, ITestChainObserver, ITestChainProvider
{
    private const int lookbackPeriods = 10;
    private readonly IReadOnlyList<StdDevResult> expectedOriginal = Bars.ToStdDev(lookbackPeriods);

    [TestMethod]
    public void BarObserver_WithWarmupLateArrivalAndRemoval_MatchesSeriesExactly()
    {
        int length = Bars.Count;

        // setup bar provider hub
        BarHub barHub = new();

        // prefill bars at provider
        barHub.Add(Bars.Take(20));

        // initialize observer
        StdDevHub observer = barHub.ToStdDevHub(lookbackPeriods);

        // fetch initial results (early)
        IReadOnlyList<StdDevResult> actuals = observer.Results;

        // emulate adding bars to provider hub
        for (int i = 20; i < length; i++)
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
        actuals.IsExactly(expectedOriginal);

        // delete, should equal series (revised)
        barHub.RemoveAt(removeAtIndex);

        IReadOnlyList<StdDevResult> expectedRevised = RevisedBars.ToStdDev(lookbackPeriods);

        actuals.Should().HaveCount(501);
        actuals.IsExactly(expectedRevised);

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
        IReadOnlyList<StdDevResult> expected = bars
            .ToStdDev(lookbackPeriods)
            .TakeLast(maxCacheSize)
            .ToList();

        // Setup with cache limit
        BarHub barHub = new(maxCacheSize);
        StdDevHub observer = barHub.ToStdDevHub(lookbackPeriods);

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
        const int stdDevPeriods = 10;
        const int smaPeriods = 8;
        int length = Bars.Count;

        // setup bar provider hub
        BarHub barHub = new();

        // initialize observer
        StdDevHub observer = barHub
            .ToSmaHub(smaPeriods)
            .ToStdDevHub(stdDevPeriods);

        // emulate bar stream
        for (int i = 0; i < length; i++) { barHub.Add(Bars[i]); }

        // final results
        IReadOnlyList<StdDevResult> actuals = observer.Results;

        // time-series, for comparison
        IReadOnlyList<StdDevResult> expected = Bars
            .ToSma(smaPeriods)
            .ToStdDev(stdDevPeriods);

        // assert, should equal series
        actuals.Should().HaveCount(length);
        actuals.IsExactly(expected);

        // cleanup
        observer.Unsubscribe();
        barHub.EndTransmission();
    }

    [TestMethod]
    public void ChainProvider_MatchesSeriesExactly()
    {
        const int stdDevPeriods = 10;
        const int smaPeriods = 8;
        int length = Bars.Count;

        // setup bar provider hub
        BarHub barHub = new();

        // initialize observer
        SmaHub observer = barHub
            .ToStdDevHub(stdDevPeriods)
            .ToSmaHub(smaPeriods);

        // emulate adding bars to provider hub
        for (int i = 0; i < length; i++)
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
        IReadOnlyList<SmaResult> actuals
            = observer.Results;

        // time-series, for comparison (revised)
        IReadOnlyList<SmaResult> seriesList = RevisedBars
            .ToStdDev(stdDevPeriods)
            .ToSma(smaPeriods);

        // assert, should equal series
        actuals.Should().HaveCount(length - 1);
        actuals.IsExactly(seriesList);

        // cleanup
        observer.Unsubscribe();
        barHub.EndTransmission();
    }

    [TestMethod]
    public override void ToStringOverride_ReturnsExpectedName()
    {
        StdDevHub hub = new(new BarHub(), 14);
        hub.ToString().Should().Be("STDDEV(14)");
    }
}
