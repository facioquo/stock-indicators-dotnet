namespace StreamHubs;

[TestClass]
public class PvoHubTests : StreamHubTestBase, ITestChainObserver, ITestChainProvider
{
    public override void ToStringOverride_ReturnsExpectedName()
    {
        // setup bar provider hub
        BarHub barHub = new();

        // initialize observer
        PvoHub observer = barHub.ToPvoHub(12, 26, 9);

        // test string output
        observer.ToString().Should().Be("PVO(12,26,9)");

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
        PvoHub observer = barHub
            .ToPvoHub(12, 26, 9);

        // fetch initial results (early)
        IReadOnlyList<PvoResult> actuals
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
        IReadOnlyList<PvoResult> expected = RevisedBars.ToPvo(12, 26, 9);

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
        IReadOnlyList<PvoResult> expected = bars
            .ToPvo(12, 26, 9)
            .TakeLast(maxCacheSize)
            .ToList();

        // Setup with cache limit
        BarHub barHub = new(maxCacheSize);
        PvoHub observer = barHub.ToPvoHub(12, 26, 9);

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
        const int pvoFast = 12;
        const int pvoSlow = 26;
        const int pvoSignal = 9;

        List<Bar> barsList = Bars.ToList();

        int length = barsList.Count;

        // setup bar provider hub
        BarHub barHub = new();

        // initialize observer from BarPartHub(Volume)
        PvoHub observer = barHub
            .ToBarPartHub(CandlePart.Volume)
            .ToPvoHub(pvoFast, pvoSlow, pvoSignal);

        // emulate bar stream
        for (int i = 0; i < length; i++)
        {
            barHub.Add(barsList[i]);
        }

        // final results
        IReadOnlyList<PvoResult> streamList
            = observer.Results;

        // time-series, for comparison
        IReadOnlyList<PvoResult> seriesList
           = barsList
            .ToPvo(pvoFast, pvoSlow, pvoSignal);

        // assert
        streamList.Should().HaveCount(length);
        streamList.IsExactly(seriesList);

        // cleanup
        observer.Unsubscribe();
        barHub.EndTransmission();
    }

    [TestMethod]
    public void ChainProvider_MatchesSeriesExactly()
    {
        const int pvoFast = 12;
        const int pvoSlow = 26;
        const int pvoSignal = 9;
        const int emaPeriods = 10;

        // setup chain barHub
        BarHub barHub = new();
        PvoHub pvoHub = barHub.ToPvoHub(pvoFast, pvoSlow, pvoSignal);

        // initialize observer (EMA of PVO)
        EmaHub observer = pvoHub
            .ToEmaHub(emaPeriods);

        // emulate live bars
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
        IReadOnlyList<EmaResult> sut = observer.Results;

        // time-series, for comparison (revised)
        IReadOnlyList<EmaResult> expected = RevisedBars
            .ToPvo(pvoFast, pvoSlow, pvoSignal)
            .ToEma(emaPeriods);

        // assert
        sut.Should().HaveCount(barsCount - 1);
        sut.IsExactly(expected);

        // cleanup
        observer.Unsubscribe();
        pvoHub.EndTransmission();
        barHub.EndTransmission();
    }

    [TestMethod]
    public void StreamingAccuracy_PartialBars_MatchesSeriesExactly()
    {
        const int fastPeriods = 12;
        const int slowPeriods = 26;
        const int signalPeriods = 9;

        List<Bar> barsList = Bars.ToList();

        // setup bar provider hub
        BarHub barHub = new();

        // initialize observer
        PvoHub observer = barHub
            .ToPvoHub(fastPeriods, slowPeriods, signalPeriods);

        // stream first 100 bars
        for (int i = 0; i < 100; i++)
        {
            barHub.Add(barsList[i]);
        }

        // get streaming results
        IReadOnlyList<PvoResult> streamResults = observer.Results;

        // time-series for comparison
        IReadOnlyList<PvoResult> seriesResults = barsList.Take(100).ToList().ToPvo(fastPeriods, slowPeriods, signalPeriods);

        // validate specific data points
        PvoResult streamResult = streamResults[50];
        PvoResult seriesResult = seriesResults[50];

        streamResult.Pvo.Should().Be(seriesResult.Pvo);
        streamResult.Signal.Should().Be(seriesResult.Signal);
        streamResult.Histogram.Should().Be(seriesResult.Histogram);

        // cleanup
        observer.Unsubscribe();
        barHub.EndTransmission();
    }

    [TestMethod]
    public void Parameters_WithCustomValues_AreSetCorrectly()
    {
        List<Bar> barsList = Bars.ToList();

        // setup bar provider hub
        BarHub barHub = new();

        // initialize observer with custom parameters
        PvoHub observer = barHub
            .ToPvoHub(8, 21, 5);

        // verify parameters
        observer.FastPeriods.Should().Be(8);
        observer.SlowPeriods.Should().Be(21);
        observer.SignalPeriods.Should().Be(5);

        // process some bars
        for (int i = 0; i < 50; i++)
        {
            barHub.Add(barsList[i]);
        }

        // verify results consistency
        IReadOnlyList<PvoResult> streamResults = observer.Results;
        IReadOnlyList<PvoResult> seriesResults = barsList.Take(50).ToList().ToPvo(8, 21, 5);

        streamResults.IsExactly(seriesResults);

        // cleanup
        observer.Unsubscribe();
        barHub.EndTransmission();
    }
}
