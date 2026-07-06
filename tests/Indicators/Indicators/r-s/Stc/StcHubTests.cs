namespace StreamHubs;

[TestClass]
public class StcHubTests : StreamHubTestBase, ITestChainObserver, ITestChainProvider
{
    private const int cyclePeriods = 9;
    private const int fastPeriods = 12;
    private const int slowPeriods = 26;
    private readonly IReadOnlyList<StcResult> expectedOriginal = Bars.ToStc(cyclePeriods, fastPeriods, slowPeriods);

    [TestMethod]
    public void BarObserver_WithWarmupLateArrivalAndRemoval_MatchesSeriesExactly()
    {
        int length = Bars.Count;

        // setup bar provider hub
        BarHub barHub = new();

        // prefill bars at provider
        barHub.Add(Bars.Take(20));

        // initialize observer
        StcHub observer = barHub.ToStcHub(cyclePeriods, fastPeriods, slowPeriods);

        // fetch initial results (early)
        IReadOnlyList<StcResult> actuals = observer.Results;

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

        IReadOnlyList<StcResult> expectedRevised = RevisedBars.ToStc(cyclePeriods, fastPeriods, slowPeriods);

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
        IReadOnlyList<StcResult> expected = bars
            .ToStc(cyclePeriods, fastPeriods, slowPeriods)
            .TakeLast(maxCacheSize)
            .ToList();

        // Setup with cache limit
        BarHub barHub = new(maxCacheSize);
        StcHub observer = barHub.ToStcHub(cyclePeriods, fastPeriods, slowPeriods);

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
        const int smaPeriods = 8;
        int length = Bars.Count;

        // setup bar provider hub
        BarHub barHub = new();

        // initialize observer
        StcHub observer = barHub
            .ToSmaHub(smaPeriods)
            .ToStcHub(cyclePeriods, fastPeriods, slowPeriods);

        // emulate bar stream
        for (int i = 0; i < length; i++) { barHub.Add(Bars[i]); }

        // final results
        IReadOnlyList<StcResult> actuals = observer.Results;

        // time-series, for comparison
        IReadOnlyList<StcResult> expected = Bars
            .ToSma(smaPeriods)
            .ToStc(cyclePeriods, fastPeriods, slowPeriods);

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
        const int smaPeriods = 10;
        int length = Bars.Count;

        // setup bar provider hub
        BarHub barHub = new();

        // initialize observer
        SmaHub observer = barHub
            .ToStcHub(cyclePeriods, fastPeriods, slowPeriods)
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
        IReadOnlyList<SmaResult> actuals = observer.Results;

        // time-series, for comparison (revised)
        IReadOnlyList<SmaResult> seriesList = RevisedBars
            .ToStc(cyclePeriods, fastPeriods, slowPeriods)
            .ToSma(smaPeriods);

        // assert, should equal series
        actuals.Should().HaveCount(length - 1);
        actuals.IsExactly(seriesList);

        // cleanup
        observer.Unsubscribe();
        barHub.EndTransmission();
    }

    [TestMethod]
    public void Results_AreAlwaysBounded()
    {
        IReadOnlyList<StcResult> sut = Bars.ToStcHub(9, 12, 26).Results;
        sut.IsBetween(static x => x.Stc, 0, 100);
    }

    [TestMethod]
    public void BarObserver_WithWarmupLateArrivalAndRemoval_MatchesSeriesExactlyDefaults()
    {
        int length = Bars.Count;

        // setup bar provider hub
        BarHub barHub = new();

        // initialize observer with default parameters (10, 23, 50)
        StcHub observer = barHub.ToStcHub();

        // emulate bar stream
        for (int i = 0; i < length; i++) { barHub.Add(Bars[i]); }

        // final results
        IReadOnlyList<StcResult> actuals = observer.Results;

        // time-series, for comparison
        IReadOnlyList<StcResult> expected = Bars.ToStc();

        // assert, should equal series
        actuals.Should().HaveCount(length);
        actuals.IsExactly(expected);

        // cleanup
        observer.Unsubscribe();
        barHub.EndTransmission();
    }

    [TestMethod]
    public override void ToStringOverride_ReturnsExpectedName()
    {
        StcHub hub = new(new BarHub(), cyclePeriods, fastPeriods, slowPeriods);
        hub.ToString().Should().Be($"STC({cyclePeriods},{fastPeriods},{slowPeriods})");
    }
}
