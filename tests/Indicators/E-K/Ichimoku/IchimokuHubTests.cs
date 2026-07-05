namespace StreamHubs;

[TestClass]
public class IchimokuHubTests : StreamHubTestBase, ITestBarObserver
{
    private const int tenkanPeriods = 9;
    private const int kijunPeriods = 26;
    private const int senkouBPeriods = 52;

    private static readonly IReadOnlyList<IchimokuResult> expectedOriginal
        = Bars.ToIchimoku(tenkanPeriods, kijunPeriods, senkouBPeriods);

    [TestMethod]
    public void BarObserver_WithWarmupLateArrivalAndRemoval_MatchesSeriesExactly()
    {
        int length = Bars.Count;

        // setup bar provider hub
        BarHub barHub = new();

        // prefill bars at provider (warmup coverage)
        barHub.Add(Bars.Take(20));

        // initialize observer
        IchimokuHub sut = barHub
            .ToIchimokuHub(tenkanPeriods, kijunPeriods, senkouBPeriods);

        // fetch initial results (early)
        IReadOnlyList<IchimokuResult> actuals = sut.Results;

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

        actuals.Should().HaveCount(length);
        actuals.IsExactly(expectedOriginal);

        // delete, should equal series (revised)
        barHub.RemoveAt(removeAtIndex);

        IReadOnlyList<IchimokuResult> expectedRevised
            = RevisedBars.ToIchimoku(tenkanPeriods, kijunPeriods, senkouBPeriods);

        actuals.Should().HaveCount(501);
        actuals.IsExactly(expectedRevised);

        // cleanup
        sut.Unsubscribe();
        barHub.EndTransmission();
    }

    [TestMethod]
    public void WithCachePruning_MatchesSeriesExactly()
    {
        const int maxCacheSize = 90;  // 52 (senkouB) + 26 (displacement) + 12 extra
        const int totalBars = 180;  // ~2x cache size

        IReadOnlyList<Bar> bars = Bars.Take(totalBars).ToList();
        IReadOnlyList<IchimokuResult> expected = bars
            .ToIchimoku(tenkanPeriods, kijunPeriods, senkouBPeriods)
            .TakeLast(maxCacheSize)
            .ToList();

        // Setup with cache limit
        BarHub barHub = new(maxCacheSize);
        IchimokuHub observer = barHub.ToIchimokuHub(tenkanPeriods, kijunPeriods, senkouBPeriods);

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
    public void BarObserver_WithWarmupLateArrivalAndRemoval_MatchesSeriesExactlyWithOffsets()
    {
        // Simple test for different offset parameters

        List<Bar> barsList = Bars.ToList();

        // setup bar provider hub
        BarHub barHub = new();

        // initialize observer with custom offsets
        IchimokuHub sut = barHub
            .ToIchimokuHub(3, 13, 40, 0);

        // add bars to barHub
        barHub.Add(barsList);

        // stream results
        IReadOnlyList<IchimokuResult> streamList
            = sut.Results;

        // time-series, for comparison
        IReadOnlyList<IchimokuResult> seriesList
           = barsList
            .ToIchimoku(3, 13, 40, 0);

        // assert, should equal series
        streamList.Should().HaveCount(barsList.Count);
        streamList.IsExactly(seriesList);

        sut.Unsubscribe();
        barHub.EndTransmission();
    }

    [TestMethod]
    public override void ToStringOverride_ReturnsExpectedName()
    {
        IchimokuHub sut = new(new BarHub(), 9, 26, 52, 26, 26);
        sut.ToString().Should().Be("ICHIMOKU(9,26,52)");
    }
}
