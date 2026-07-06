namespace StreamHubs;

[TestClass]
public class BollingerBandsHubTests : StreamHubTestBase, ITestBarObserver, ITestChainProvider
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
        BollingerBandsHub observer = barHub.ToBollingerBandsHub(20, 2);

        // fetch initial results (early)
        IReadOnlyList<BollingerBandsResult> actuals = observer.Results;

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
        IReadOnlyList<BollingerBandsResult> expected = RevisedBars.ToBollingerBands(20, 2);

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
        IReadOnlyList<BollingerBandsResult> expected = bars
            .ToBollingerBands(20, 2)
            .TakeLast(maxCacheSize)
            .ToList();

        // Setup with cache limit
        BarHub barHub = new(maxCacheSize);
        BollingerBandsHub observer = barHub.ToBollingerBandsHub(20, 2);

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
        BollingerBandsHub observer = barHub.ToBollingerBandsHub(20, 2);

        observer.ToString().Should().Be("BB(20,2)");
    }

    [TestMethod]
    public void ChainProvider_MatchesSeriesExactly()
    {
        // arrange
        const int lookbackPeriods = 20;
        const double standardDeviations = 2;
        const int smaPeriods = 10;

        // setup bar provider hub
        BarHub barHub = new();

        // initialize observer - chain SMA to Bollinger Bands
        SmaHub observer = barHub
            .ToBollingerBandsHub(lookbackPeriods, standardDeviations)
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
            .ToBollingerBands(lookbackPeriods, standardDeviations)
            .ToSma(smaPeriods);

        // assert, should equal series
        sut.Should().HaveCount(barsCount - 1);
        sut.IsExactly(expected);

        // cleanup
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
        BollingerBandsHub lateHub = lateSource.ToBollingerBandsHub(20, 2);
        for (int i = 0; i < totalBars; i++)
        {
            if (i == lateIndex) { continue; }

            lateSource.Add(bars[i]);
        }

        lateSource.Add(bars[lateIndex]);

        BarHub freshSource = new();
        BollingerBandsHub freshHub = freshSource.ToBollingerBandsHub(20, 2);
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
        // Bands emit first non-null result at lookbackPeriods (= 20); index
        // 25 forces replay across the rolling SMA + standard-deviation
        // window transition that gates upper/lower band emission.
        const int totalBars = 300;
        const int lateIndex = 25;

        IReadOnlyList<Bar> bars = Bars.Take(totalBars).ToList();

        BarHub lateSource = new();
        BollingerBandsHub lateHub = lateSource.ToBollingerBandsHub(20, 2);
        for (int i = 0; i < totalBars; i++)
        {
            if (i == lateIndex) { continue; }

            lateSource.Add(bars[i]);
        }

        lateSource.Add(bars[lateIndex]);

        BarHub freshSource = new();
        BollingerBandsHub freshHub = freshSource.ToBollingerBandsHub(20, 2);
        freshSource.Add(bars);

        lateHub.Results.Should().HaveCount(totalBars);
        lateHub.Results.IsExactly(freshHub.Results);

        lateHub.Unsubscribe();
        freshHub.Unsubscribe();
        lateSource.EndTransmission();
        freshSource.EndTransmission();
    }

    [TestMethod]
    public void PrefilledProviderRebuilds_WithPrefilledBars_MatchesSeriesExactly()
    {
        BarHub barHub = new();
        List<Bar> bars = Bars.Take(25).ToList();

        for (int i = 0; i < 5; i++)
        {
            barHub.Add(bars[i]);
        }

        BollingerBandsHub observer = barHub.ToBollingerBandsHub(5, 2);

        IReadOnlyList<BollingerBandsResult> initialResults = observer.Results;
        IReadOnlyList<BollingerBandsResult> expectedInitial = bars
            .Take(5)
            .ToList()
            .ToBollingerBands(5, 2);

        initialResults.IsExactly(expectedInitial);

        for (int i = 5; i < bars.Count; i++)
        {
            barHub.Add(bars[i]);
        }

        observer.Results.IsExactly(bars.ToBollingerBands(5, 2));

        // cleanup
        observer.Unsubscribe();
        barHub.EndTransmission();
    }
}
