namespace StreamHubs;

[TestClass]
public class KeltnerHubTests : StreamHubTestBase, ITestBarObserver
{
    [TestMethod]
    public void BarObserver_WithWarmupLateArrivalAndRemoval_MatchesSeriesExactly()
    {
        // setup bar provider hub
        BarHub barHub = new();

        // prefill bars at provider
        for (int i = 0; i < 20; i++)
        {
            barHub.Add(Bars[i]);
        }

        // initialize observer
        KeltnerHub observer = barHub.ToKeltnerHub(20, 2, 10);

        // fetch initial results (early)
        IReadOnlyList<KeltnerResult> actuals = observer.Results;

        // emulate adding bars to provider hub
        for (int i = 20; i < barsCount; i++)
        {
            // skip one (add later)
            if (i == 80)
            {
                continue;
            }

            Bar q = Bars[i];
            barHub.Add(q);

            // resend duplicate bars
            if (i is > 100 and < 105)
            {
                barHub.Add(q);
            }
        }

        // late arrival
        barHub.Add(Bars[80]);

        // delete
        barHub.RemoveAt(removeAtIndex);

        // time-series, for comparison
        IReadOnlyList<KeltnerResult> expected = RevisedBars.ToKeltner(20, 2, 10);

        // assert, should equal series
        actuals.Should().HaveCount(barsCount - 1);
        actuals.IsExactly(expected);

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
        IReadOnlyList<KeltnerResult> expected = bars
            .ToKeltner(20, 2, 10)
            .TakeLast(maxCacheSize)
            .ToList();

        // Setup with cache limit
        BarHub barHub = new(maxCacheSize);
        KeltnerHub observer = barHub.ToKeltnerHub(20, 2, 10);

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
    public override void ToStringOverride_ReturnsExpectedName()
    {
        BarHub barHub = new();
        barHub.Add(Bars);
        KeltnerHub observer = barHub.ToKeltnerHub(20, 2, 10);

        observer.ToString().Should().Be("KELTNER(20,2,10)");
    }

    [TestMethod]
    public void LateArrival_MidStream_MatchesFreshStream()
    {
        const int totalBars = 300;
        const int lateIndex = 150;

        IReadOnlyList<Bar> bars = Bars.Take(totalBars).ToList();

        BarHub lateSource = new();
        KeltnerHub lateHub = lateSource.ToKeltnerHub(20, 2, 10);
        for (int i = 0; i < totalBars; i++)
        {
            if (i == lateIndex) { continue; }

            lateSource.Add(bars[i]);
        }

        lateSource.Add(bars[lateIndex]);

        BarHub freshSource = new();
        KeltnerHub freshHub = freshSource.ToKeltnerHub(20, 2, 10);
        freshSource.Add(bars);

        lateHub.Results.Should().HaveCount(totalBars);
        lateHub.Results.IsExactly(freshHub.Results);

        lateHub.Unsubscribe();
        freshHub.Unsubscribe();
        lateSource.EndTransmission();
        freshSource.EndTransmission();
    }

    [TestMethod]
    public void LateArrival_AtBandsWarmupBoundary_MatchesFreshStream()
    {
        // Bands emit first non-null result at max(emaPeriods, atrPeriods)
        // (= 20); index 25 forces replay across the EMA seed + ATR seed
        // dual-state transition that determines centerline + bandwidth.
        const int totalBars = 300;
        const int lateIndex = 25;

        IReadOnlyList<Bar> bars = Bars.Take(totalBars).ToList();

        BarHub lateSource = new();
        KeltnerHub lateHub = lateSource.ToKeltnerHub(20, 2, 10);
        for (int i = 0; i < totalBars; i++)
        {
            if (i == lateIndex) { continue; }

            lateSource.Add(bars[i]);
        }

        lateSource.Add(bars[lateIndex]);

        BarHub freshSource = new();
        KeltnerHub freshHub = freshSource.ToKeltnerHub(20, 2, 10);
        freshSource.Add(bars);

        lateHub.Results.Should().HaveCount(totalBars);
        lateHub.Results.IsExactly(freshHub.Results);

        lateHub.Unsubscribe();
        freshHub.Unsubscribe();
        lateSource.EndTransmission();
        freshSource.EndTransmission();
    }

    [TestMethod]
    public void PrefilledProvider_OnRebuild_MatchesSeriesExactly()
    {
        BarHub barHub = new();
        List<Bar> bars = Bars.Take(25).ToList();

        for (int i = 0; i < 5; i++)
        {
            barHub.Add(bars[i]);
        }

        KeltnerHub observer = barHub.ToKeltnerHub(5, 1, 3);

        IReadOnlyList<KeltnerResult> initialResults = observer.Results;
        IReadOnlyList<KeltnerResult> expectedInitial = bars
            .Take(5)
            .ToList()
            .ToKeltner(5, 1, 3);

        initialResults.IsExactly(expectedInitial);

        for (int i = 5; i < bars.Count; i++)
        {
            barHub.Add(bars[i]);
        }

        observer.Results.IsExactly(bars.ToKeltner(5, 1, 3));

        // cleanup
        observer.Unsubscribe();
        barHub.EndTransmission();
    }
}
