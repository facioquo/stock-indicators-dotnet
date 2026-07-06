namespace StreamHubs;

[TestClass]
public class BopHubTests : StreamHubTestBase, ITestBarObserver, ITestChainProvider
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
        BopHub bopHub = barHub
            .ToBopHub(14);

        // fetch initial results (early)
        IReadOnlyList<BopResult> actuals
            = bopHub.Results;

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
        IReadOnlyList<BopResult> expected = RevisedBars.ToBop(14);

        // assert
        actuals.Should().HaveCount(barsCount - 1);
        actuals.IsExactly(expected);

        bopHub.Unsubscribe();
        barHub.EndTransmission();
    }

    [TestMethod]
    public void WithCachePruning_MatchesSeriesExactly()
    {
        const int maxCacheSize = 50;
        const int totalBars = 100;

        IReadOnlyList<Bar> bars = Bars.Take(totalBars).ToList();
        IReadOnlyList<BopResult> expected = bars
            .ToBop(14)
            .TakeLast(maxCacheSize)
            .ToList();

        // Setup with cache limit
        BarHub barHub = new(maxCacheSize);
        BopHub observer = barHub.ToBopHub(14);

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
    public void Results_AreAlwaysBounded()
    {
        IReadOnlyList<BopResult> sut = Bars.ToBopHub(14).Results;
        sut.IsBetween(static x => x.Bop, -1, 1);
    }

    [TestMethod]
    public void ChainObserver_FromBarHub_MatchesSeriesExactly()
    {
        // BOP requires IBar input (OHLC data), so we can't chain from EMA
        // Instead, test chaining from a bar converter that produces bars
        const int smoothPeriods = 14;

        List<Bar> barsList = Bars.ToList();

        int length = barsList.Count;

        // setup bar provider
        BarHub barHub = new();

        // initialize observer - BOP directly from bars
        BopHub bopHub = barHub
            .ToBopHub(smoothPeriods);

        // emulate bar stream
        for (int i = 0; i < length; i++)
        {
            barHub.Add(barsList[i]);
        }

        // final results
        IReadOnlyList<BopResult> streamList
            = bopHub.Results;

        // time-series, for comparison
        IReadOnlyList<BopResult> seriesList
           = barsList
            .ToBop(smoothPeriods);

        // assert
        streamList.Should().HaveCount(length);
        streamList.IsExactly(seriesList);

        bopHub.Unsubscribe();
        barHub.EndTransmission();
    }

    [TestMethod]
    public void ChainProvider_MatchesSeriesExactly()
    {
        const int smoothPeriods = 14;
        const int emaPeriods = 12;

        // setup bar provider
        BarHub barHub = new();

        // initialize observer
        EmaHub emaHub = barHub
            .ToBopHub(smoothPeriods)
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
            .ToBop(smoothPeriods)
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
        BopHub hub = new(new BarHub(), 14);
        hub.ToString().Should().Be("BOP(14)");
    }
}
