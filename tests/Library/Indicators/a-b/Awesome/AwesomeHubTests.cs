namespace StreamHubs;

[TestClass]
public class AwesomeHubTests : StreamHubTestBase, ITestChainObserver, ITestChainProvider
{
    [TestMethod]
    public void BarObserver_WithWarmupLateArrivalAndRemoval_MatchesSeriesExactly()
    {
        // setup bar provider
        BarHub barHub = new();

        // prefill bars to provider
        for (int i = 0; i < 40; i++)
        {
            barHub.Add(Bars[i]);
        }

        // initialize observer
        AwesomeHub awesomeHub = barHub
            .ToAwesomeHub(5, 34);

        // fetch initial results (early)
        IReadOnlyList<AwesomeResult> actuals
            = awesomeHub.Results;

        // emulate adding bars to provider
        for (int i = 40; i < barsCount; i++)
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
        IReadOnlyList<AwesomeResult> expected = RevisedBars.ToAwesome(5, 34);

        // assert, should equal series
        actuals.Should().HaveCount(barsCount - 1);
        actuals.IsExactly(expected);

        awesomeHub.Unsubscribe();
        barHub.EndTransmission();
    }

    [TestMethod]
    public void WithCachePruning_MatchesSeriesExactly()
    {
        const int maxCacheSize = 50;
        const int totalBars = 100;

        IReadOnlyList<Bar> bars = Bars.Take(totalBars).ToList();
        IReadOnlyList<AwesomeResult> expected = bars
            .ToAwesome(5, 34)
            .TakeLast(maxCacheSize)
            .ToList();

        // Setup with cache limit
        BarHub barHub = new(maxCacheSize);
        AwesomeHub observer = barHub.ToAwesomeHub(5, 34);

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
        const int emaPeriods = 12;
        const int fastPeriods = 5;
        const int slowPeriods = 34;

        List<Bar> barsList = Bars.ToList();

        int length = barsList.Count;

        // setup bar provider
        BarHub barHub = new();

        // initialize observer
        AwesomeHub awesomeHub = barHub
            .ToEmaHub(emaPeriods)
            .ToAwesomeHub(fastPeriods, slowPeriods);

        // emulate bar stream
        for (int i = 0; i < length; i++)
        {
            barHub.Add(barsList[i]);
        }

        // final results
        IReadOnlyList<AwesomeResult> streamList
            = awesomeHub.Results;

        // time-series, for comparison
        IReadOnlyList<AwesomeResult> seriesList
           = barsList
            .ToEma(emaPeriods)
            .ToAwesome(fastPeriods, slowPeriods);

        // assert, should equal series
        streamList.Should().HaveCount(length);
        streamList.IsExactly(seriesList);

        awesomeHub.Unsubscribe();
        barHub.EndTransmission();
    }

    [TestMethod]
    public void ChainProvider_MatchesSeriesExactly()
    {
        const int fastPeriods = 5;
        const int slowPeriods = 34;
        const int emaPeriods = 12;

        // setup bar provider
        BarHub barHub = new();

        // initialize observer
        EmaHub emaHub = barHub
            .ToAwesomeHub(fastPeriods, slowPeriods)
            .ToEmaHub(emaPeriods);

        // emulate bar stream
        for (int i = 0; i < barsCount; i++)
        {
            if (i == 80) { continue; }  // Skip for late arrival

            Bar q = Bars[i];
            barHub.Add(q);

            if (i is > 100 and < 105) { barHub.Add(q); }  // Duplicate bars
        }

        barHub.Add(Bars[80]);  // Late arrival
        barHub.RemoveAt(removeAtIndex);  // Remove

        // final results
        IReadOnlyList<EmaResult> sut = emaHub.Results;

        // time-series, for comparison (revised)
        IReadOnlyList<EmaResult> expected = RevisedBars
            .ToAwesome(fastPeriods, slowPeriods)
            .ToEma(emaPeriods);

        // assert, should equal series
        sut.Should().HaveCount(barsCount - 1);
        sut.IsExactly(expected);

        emaHub.Unsubscribe();
        barHub.EndTransmission();
    }

    [TestMethod]
    public override void ToStringOverride_ReturnsExpectedName()
    {
        AwesomeHub hub = new(new BarHub(), 5, 34);
        hub.ToString().Should().Be("AWESOME(5,34)");
    }
}
