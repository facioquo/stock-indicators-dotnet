namespace StreamHubs;

[TestClass]
public class ChaikinOscHubTests : StreamHubTestBase, ITestBarObserver, ITestChainProvider
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
        ChaikinOscHub chaikinOscHub = barHub
            .ToChaikinOscHub(3, 10);

        // fetch initial results (early)
        IReadOnlyList<ChaikinOscResult> actuals
            = chaikinOscHub.Results;

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
        IReadOnlyList<ChaikinOscResult> expected = RevisedBars.ToChaikinOsc(3, 10);

        // assert
        actuals.Should().HaveCount(barsCount - 1);
        actuals.IsExactly(expected);

        chaikinOscHub.Unsubscribe();
        barHub.EndTransmission();
    }

    [TestMethod]
    public void WithCachePruning_MatchesSeriesExactly()
    {
        const int maxCacheSize = 50;
        const int totalBars = 100;

        IReadOnlyList<Bar> bars = Bars.Take(totalBars).ToList();
        IReadOnlyList<ChaikinOscResult> expected = bars
            .ToChaikinOsc(3, 10)
            .TakeLast(maxCacheSize)
            .ToList();

        // Setup with cache limit
        BarHub barHub = new(maxCacheSize);
        ChaikinOscHub observer = barHub.ToChaikinOscHub(3, 10);

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
    public void ChainObserver_FromBarHub_MatchesSeriesExactly()
    {
        // ChaikinOsc requires IBar input, so similar to BOP pattern
        const int fastPeriods = 3;
        const int slowPeriods = 10;

        List<Bar> barsList = Bars.ToList();

        int length = barsList.Count;

        // setup bar provider
        BarHub barHub = new();

        // initialize observer
        ChaikinOscHub chaikinOscHub = barHub
            .ToChaikinOscHub(fastPeriods, slowPeriods);

        // emulate bar stream
        for (int i = 0; i < length; i++)
        {
            barHub.Add(barsList[i]);
        }

        // final results
        IReadOnlyList<ChaikinOscResult> streamList
            = chaikinOscHub.Results;

        // time-series, for comparison
        IReadOnlyList<ChaikinOscResult> seriesList
           = barsList
            .ToChaikinOsc(fastPeriods, slowPeriods);

        // assert
        streamList.Should().HaveCount(length);
        streamList.IsExactly(seriesList);

        chaikinOscHub.Unsubscribe();
        barHub.EndTransmission();
    }

    [TestMethod]
    public void ChainProvider_MatchesSeriesExactly()
    {
        const int fastPeriods = 3;
        const int slowPeriods = 10;
        const int emaPeriods = 12;

        // setup bar provider
        BarHub barHub = new();

        // initialize observer
        EmaHub emaHub = barHub
            .ToChaikinOscHub(fastPeriods, slowPeriods)
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
            .ToChaikinOsc(fastPeriods, slowPeriods)
            .ToEma(emaPeriods);

        // assert
        sut.Should().HaveCount(barsCount - 1);
        sut.IsExactly(expected);

        emaHub.Unsubscribe();
        barHub.EndTransmission();
    }

    [TestMethod]
    public override void ToStringOverride_ReturnsExpectedName()
    {
        ChaikinOscHub hub = new(new BarHub(), 3, 10);
        hub.ToString().Should().Be("CHAIKIN_OSC(3,10)");
    }
}
