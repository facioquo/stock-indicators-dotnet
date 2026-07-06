namespace StreamHubs;

[TestClass]
public class MacdHubTests : StreamHubTestBase, ITestChainObserver, ITestChainProvider
{
    public override void ToStringOverride_ReturnsExpectedName()
    {
        List<Bar> barsList = Bars.ToList();

        // setup bar provider hub
        BarHub barHub = new();

        // initialize observer
        MacdHub observer = barHub
            .ToMacdHub(12, 26, 9);

        // emulate bar stream
        for (int i = 0; i < 20; i++)
        {
            barHub.Add(barsList[i]);
        }

        // test string output
        observer.ToString().Should().Be("MACD(12,26,9)");

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
        MacdHub observer = barHub
            .ToMacdHub(12, 26, 9);

        // fetch initial results (early)
        IReadOnlyList<MacdResult> actuals
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
        IReadOnlyList<MacdResult> expected = RevisedBars.ToMacd(12, 26, 9);

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
        IReadOnlyList<MacdResult> expected = bars
            .ToMacd(12, 26, 9)
            .TakeLast(maxCacheSize)
            .ToList();

        // Setup with cache limit
        BarHub barHub = new(maxCacheSize);
        MacdHub observer = barHub.ToMacdHub(12, 26, 9);

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
        const int emaPeriods = 14;
        const int macdFast = 12;
        const int macdSlow = 26;
        const int macdSignal = 9;

        List<Bar> barsList = Bars.ToList();

        int length = barsList.Count;

        // setup bar provider hub
        BarHub barHub = new();

        // initialize observer
        MacdHub observer = barHub
            .ToEmaHub(emaPeriods)
            .ToMacdHub(macdFast, macdSlow, macdSignal);

        // emulate bar stream
        for (int i = 0; i < length; i++)
        {
            barHub.Add(barsList[i]);
        }

        // final results
        IReadOnlyList<MacdResult> streamList
            = observer.Results;

        // time-series, for comparison
        IReadOnlyList<MacdResult> seriesList
           = barsList
            .ToEma(emaPeriods)
            .ToMacd(macdFast, macdSlow, macdSignal);

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
        const int smaPeriods = 10;
        const int macdFast = 5;
        const int macdSlow = 10;
        const int macdSignal = 3;

        // setup chain barHub
        BarHub barHub = new();
        SmaHub smaHub = barHub.ToSmaHub(smaPeriods);

        // initialize observer
        MacdHub observer = smaHub
            .ToMacdHub(macdFast, macdSlow, macdSignal);

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
        IReadOnlyList<MacdResult> sut = observer.Results;

        // time-series, for comparison (revised)
        IReadOnlyList<MacdResult> expected = RevisedBars
            .ToSma(smaPeriods)
            .ToMacd(macdFast, macdSlow, macdSignal);

        // assert
        sut.Should().HaveCount(barsCount - 1);
        sut.IsExactly(expected);

        // cleanup
        observer.Unsubscribe();
        smaHub.EndTransmission();
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
        MacdHub observer = barHub
            .ToMacdHub(fastPeriods, slowPeriods, signalPeriods);

        // stream first 100 bars
        for (int i = 0; i < 100; i++)
        {
            barHub.Add(barsList[i]);
        }

        // get streaming results
        IReadOnlyList<MacdResult> streamResults = observer.Results;

        // time-series for comparison
        IReadOnlyList<MacdResult> seriesResults = barsList.Take(100).ToList().ToMacd(fastPeriods, slowPeriods, signalPeriods);

        // validate specific data points
        MacdResult streamResult = streamResults[50];
        MacdResult seriesResult = seriesResults[50];

        streamResult.Macd.Should().Be(seriesResult.Macd);
        streamResult.Signal.Should().Be(seriesResult.Signal);
        streamResult.Histogram.Should().Be(seriesResult.Histogram);
        streamResult.FastEma.Should().Be(seriesResult.FastEma);
        streamResult.SlowEma.Should().Be(seriesResult.SlowEma);

        // cleanup
        observer.Unsubscribe();
        barHub.EndTransmission();
    }

    [TestMethod]
    public void LateArrival_MidStream_MatchesFreshStream()
    {
        // Skip one bar during streaming, then add it after the cache
        // head has advanced; result cache must equal a fresh hub that
        // received the same bars in correct timestamp order.
        const int totalBars = 300;
        const int lateIndex = 150;

        IReadOnlyList<Bar> bars = Bars.Take(totalBars).ToList();

        BarHub lateSource = new();
        MacdHub lateHub = lateSource.ToMacdHub(12, 26, 9);
        for (int i = 0; i < totalBars; i++)
        {
            if (i == lateIndex) { continue; }

            lateSource.Add(bars[i]);
        }

        lateSource.Add(bars[lateIndex]);

        BarHub freshSource = new();
        MacdHub freshHub = freshSource.ToMacdHub(12, 26, 9);
        freshSource.Add(bars);

        lateHub.Results.Should().HaveCount(totalBars);
        lateHub.Results.IsExactly(freshHub.Results);

        lateHub.Unsubscribe();
        freshHub.Unsubscribe();
        lateSource.EndTransmission();
        freshSource.EndTransmission();
    }

    [TestMethod]
    public void LateArrival_AtSignalWarmupBoundary_MatchesFreshStream()
    {
        // MACD signal line starts emitting at slow + signal - 1
        // (= 26 + 9 - 1 = 34), so a late arrival at index 40 forces the
        // rollback path to replay across the most-fragile state transition
        // in the three-stage EMA cascade.
        const int totalBars = 300;
        const int lateIndex = 40;

        IReadOnlyList<Bar> bars = Bars.Take(totalBars).ToList();

        BarHub lateSource = new();
        MacdHub lateHub = lateSource.ToMacdHub(12, 26, 9);
        for (int i = 0; i < totalBars; i++)
        {
            if (i == lateIndex) { continue; }

            lateSource.Add(bars[i]);
        }

        lateSource.Add(bars[lateIndex]);

        BarHub freshSource = new();
        MacdHub freshHub = freshSource.ToMacdHub(12, 26, 9);
        freshSource.Add(bars);

        lateHub.Results.Should().HaveCount(totalBars);
        lateHub.Results.IsExactly(freshHub.Results);

        lateHub.Unsubscribe();
        freshHub.Unsubscribe();
        lateSource.EndTransmission();
        freshSource.EndTransmission();
    }

    [TestMethod]
    public void Parameters_WithCustomValues_AreSetCorrectly()
    {
        List<Bar> barsList = Bars.ToList();

        // setup bar provider hub
        BarHub barHub = new();

        // initialize observer with custom parameters
        MacdHub observer = barHub
            .ToMacdHub(8, 21, 5);

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
        IReadOnlyList<MacdResult> streamResults = observer.Results;
        IReadOnlyList<MacdResult> seriesResults = barsList.Take(50).ToList().ToMacd(8, 21, 5);

        streamResults.IsExactly(seriesResults);

        // cleanup
        observer.Unsubscribe();
        barHub.EndTransmission();
    }
}
