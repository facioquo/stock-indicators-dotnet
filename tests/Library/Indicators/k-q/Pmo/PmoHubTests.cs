namespace StreamHubs;

[TestClass]
public class PmoHubTests : StreamHubTestBase, ITestChainObserver, ITestChainProvider
{
    [TestMethod]
    public override void ToStringOverride_ReturnsExpectedName()
    {
        List<Bar> barsList = Bars.ToList();

        // setup bar provider hub
        BarHub barHub = new();

        // initialize observer
        PmoHub observer = barHub
            .ToPmoHub(35, 20, 10);

        // test string output
        observer.ToString().Should().Be("PMO(35,20,10)");

        // cleanup
        observer.Unsubscribe();
        barHub.EndTransmission();
    }

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
        PmoHub observer = barHub
            .ToPmoHub(35, 20, 10);

        // fetch initial results (early)
        IReadOnlyList<PmoResult> actuals
            = observer.Results;

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
        IReadOnlyList<PmoResult> expected = RevisedBars.ToPmo(35, 20, 10);

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
        const int maxCacheSize = 100;  // 35 (longest PMO period) + 65 extra for double smoothing warmup
        const int totalBars = 200;  // ~2x cache size

        IReadOnlyList<Bar> bars = Bars.Take(totalBars).ToList();
        IReadOnlyList<PmoResult> expected = bars
            .ToPmo(35, 20, 10)
            .TakeLast(maxCacheSize)
            .ToList();

        // Setup with cache limit
        BarHub barHub = new(maxCacheSize);
        PmoHub observer = barHub.ToPmoHub(35, 20, 10);

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
        const int timePeriods = 35;
        const int smoothPeriods = 20;
        const int signalPeriods = 10;

        List<Bar> barsList = Bars.ToList();

        int length = barsList.Count;

        // setup bar provider hub
        BarHub barHub = new();

        // initialize observer
        PmoHub observer = barHub
            .ToEmaHub(emaPeriods)
            .ToPmoHub(timePeriods, smoothPeriods, signalPeriods);

        // emulate bar stream
        for (int i = 0; i < length; i++)
        {
            barHub.Add(barsList[i]);
        }

        // final results
        IReadOnlyList<PmoResult> streamList
            = observer.Results;

        // time-series, for comparison
        IReadOnlyList<PmoResult> seriesList
           = barsList
            .ToEma(emaPeriods)
            .ToPmo(timePeriods, smoothPeriods, signalPeriods);

        // assert, should equal series
        streamList.Should().HaveCount(length);
        streamList.IsExactly(seriesList);

        // cleanup
        observer.Unsubscribe();
        barHub.EndTransmission();
    }

    [TestMethod]
    public void ChainProvider_MatchesSeriesExactly()
    {
        const int smaPeriods = 10;
        const int timePeriods = 35;
        const int smoothPeriods = 20;
        const int signalPeriods = 10;

        // setup chain barHub
        BarHub barHub = new();
        SmaHub smaHub = barHub.ToSmaHub(smaPeriods);

        // initialize observer
        SmaHub observer = smaHub
            .ToPmoHub(timePeriods, smoothPeriods, signalPeriods)
            .ToSmaHub(smaPeriods);

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
        IReadOnlyList<SmaResult> sut = observer.Results;

        // time-series, for comparison (revised)
        IReadOnlyList<SmaResult> expected = RevisedBars
            .ToSma(smaPeriods)
            .ToPmo(timePeriods, smoothPeriods, signalPeriods)
            .ToSma(smaPeriods);

        // assert, should equal series
        sut.Should().HaveCount(barsCount - 1);
        sut.IsExactly(expected);

        // cleanup
        observer.Unsubscribe();
        smaHub.Unsubscribe();
        barHub.EndTransmission();
    }
}
