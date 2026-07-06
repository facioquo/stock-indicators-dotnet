namespace StreamHubs;

[TestClass]
public class SuperTrendHubTests : StreamHubTestBase, ITestBarObserver
{
    private const int lookbackPeriods = 14;
    private const double multiplier = 3;
    private readonly IReadOnlyList<SuperTrendResult> expectedOriginal
        = Bars.ToSuperTrend(lookbackPeriods, multiplier);

    [TestMethod]
    public void BarObserver_WithWarmupLateArrivalAndRemoval_MatchesSeriesExactly()
    {
        int length = Bars.Count;

        // setup bar provider hub
        BarHub barHub = new();

        // prefill bars at provider
        barHub.Add(Bars.Take(20));

        // initialize observer
        SuperTrendHub observer = barHub.ToSuperTrendHub(lookbackPeriods, multiplier);

        // fetch initial results (early)
        IReadOnlyList<SuperTrendResult> actuals = observer.Results;

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

        IReadOnlyList<SuperTrendResult> expectedRevised = RevisedBars.ToSuperTrend(lookbackPeriods, multiplier);

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
        IReadOnlyList<SuperTrendResult> expected = bars
            .ToSuperTrend(lookbackPeriods, multiplier)
            .TakeLast(maxCacheSize)
            .ToList();

        // Setup with cache limit
        BarHub barHub = new(maxCacheSize);
        SuperTrendHub observer = barHub.ToSuperTrendHub(lookbackPeriods, multiplier);

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
    public void LateArrival_MidStream_MatchesFreshStream()
    {
        const int totalBars = 300;
        const int lateIndex = 150;

        IReadOnlyList<Bar> bars = Bars.Take(totalBars).ToList();

        BarHub lateSource = new();
        SuperTrendHub lateHub = lateSource.ToSuperTrendHub(lookbackPeriods, multiplier);
        for (int i = 0; i < totalBars; i++)
        {
            if (i == lateIndex) { continue; }

            lateSource.Add(bars[i]);
        }

        lateSource.Add(bars[lateIndex]);

        BarHub freshSource = new();
        SuperTrendHub freshHub = freshSource.ToSuperTrendHub(lookbackPeriods, multiplier);
        freshSource.Add(bars);

        lateHub.Results.Should().HaveCount(totalBars);
        lateHub.Results.IsExactly(freshHub.Results);

        lateHub.Unsubscribe();
        freshHub.Unsubscribe();
        lateSource.EndTransmission();
        freshSource.EndTransmission();
    }

    [TestMethod]
    public void LateArrival_AtAtrWarmupBoundary_MatchesFreshStream()
    {
        // SuperTrend emits first non-null result at lookback (= 14); index
        // 20 forces the rollback path to replay across the ATR-seeded
        // direction-state transition that determines stop placement.
        const int totalBars = 300;
        const int lateIndex = 20;

        IReadOnlyList<Bar> bars = Bars.Take(totalBars).ToList();

        BarHub lateSource = new();
        SuperTrendHub lateHub = lateSource.ToSuperTrendHub(lookbackPeriods, multiplier);
        for (int i = 0; i < totalBars; i++)
        {
            if (i == lateIndex) { continue; }

            lateSource.Add(bars[i]);
        }

        lateSource.Add(bars[lateIndex]);

        BarHub freshSource = new();
        SuperTrendHub freshHub = freshSource.ToSuperTrendHub(lookbackPeriods, multiplier);
        freshSource.Add(bars);

        lateHub.Results.Should().HaveCount(totalBars);
        lateHub.Results.IsExactly(freshHub.Results);

        lateHub.Unsubscribe();
        freshHub.Unsubscribe();
        lateSource.EndTransmission();
        freshSource.EndTransmission();
    }

    [TestMethod]
    public override void ToStringOverride_ReturnsExpectedName()
    {
        SuperTrendHub hub = new(new BarHub(), 14, 3.0);
        hub.ToString().Should().Be("SUPERTREND(14,3)");
    }
}
