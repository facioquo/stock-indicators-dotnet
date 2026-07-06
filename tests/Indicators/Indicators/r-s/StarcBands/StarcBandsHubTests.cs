namespace StreamHubs;

[TestClass]
public class StarcBandsHubTests : StreamHubTestBase, ITestBarObserver
{
    [TestMethod]
    public void BarObserver_WithWarmupLateArrivalAndRemoval_MatchesSeriesExactly()
    {
        // setup bar provider hub
        BarHub barHub = new();

        // prefill bars at provider
        barHub.Add(Bars.Take(20));

        // initialize observer
        StarcBandsHub observer = barHub.ToStarcBandsHub(5, 2, 10);

        // fetch initial results (early)
        IReadOnlyList<StarcBandsResult> sut = observer.Results;

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

        IReadOnlyList<StarcBandsResult> expectedOriginal = Bars.ToStarcBands(5, 2, 10);
        sut.IsExactly(expectedOriginal);

        // delete, should equal series (revised)
        barHub.RemoveAt(removeAtIndex);

        IReadOnlyList<StarcBandsResult> expectedRevised = RevisedBars.ToStarcBands(5, 2, 10);
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
        IReadOnlyList<StarcBandsResult> expected = bars
            .ToStarcBands(5, 2, 10)
            .TakeLast(maxCacheSize)
            .ToList();

        // Setup with cache limit
        BarHub barHub = new(maxCacheSize);
        StarcBandsHub observer = barHub.ToStarcBandsHub(5, 2, 10);

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
        StarcBandsHub observer = barHub.ToStarcBandsHub(5, 2, 10);

        observer.ToString().Should().Be("STARCBANDS(5,2,10)");
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

        StarcBandsHub observer = barHub.ToStarcBandsHub(5, 2, 3);

        IReadOnlyList<StarcBandsResult> initialResults = observer.Results;
        IReadOnlyList<StarcBandsResult> expectedInitial = bars
            .Take(5)
            .ToList()
            .ToStarcBands(5, 2, 3);

        initialResults.IsExactly(expectedInitial);

        for (int i = 5; i < bars.Count; i++)
        {
            barHub.Add(bars[i]);
        }

        observer.Results.IsExactly(bars.ToStarcBands(5, 2, 3));

        // cleanup
        observer.Unsubscribe();
        barHub.EndTransmission();
    }
}
