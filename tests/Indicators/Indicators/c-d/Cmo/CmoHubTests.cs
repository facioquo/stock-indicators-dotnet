namespace StreamHubs;

[TestClass]
public class CmoHubTests : StreamHubTestBase, ITestChainObserver, ITestChainProvider
{
    [TestMethod]
    public void Results_AreAlwaysBounded()
    {
        IReadOnlyList<CmoResult> sut = Bars.ToCmoHub(14).Results;
        sut.IsBetween(static x => x.Cmo, -100, 100);
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
        CmoHub observer = barHub
            .ToCmoHub(14);

        // fetch initial results (early)
        IReadOnlyList<CmoResult> actuals
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
        IReadOnlyList<CmoResult> expected = RevisedBars.ToCmo(14);

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
        IReadOnlyList<CmoResult> expected = bars
            .ToCmo(14)
            .TakeLast(maxCacheSize)
            .ToList();

        // Setup with cache limit
        BarHub barHub = new(maxCacheSize);
        CmoHub observer = barHub.ToCmoHub(14);

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
        const int cmoPeriods = 14;

        List<Bar> barsList = Bars.ToList();

        int length = barsList.Count;

        // setup bar provider hub
        BarHub barHub = new();

        // initialize observer
        CmoHub observer = barHub
            .ToEmaHub(emaPeriods)
            .ToCmoHub(cmoPeriods);

        // emulate bar stream
        for (int i = 0; i < length; i++)
        {
            barHub.Add(barsList[i]);
        }

        // final results
        IReadOnlyList<CmoResult> streamList
            = observer.Results;

        // time-series, for comparison
        IReadOnlyList<CmoResult> seriesList
           = barsList
            .ToEma(emaPeriods)
            .ToCmo(cmoPeriods);

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
        const int emaPeriods = 12;
        const int cmoPeriods = 14;

        // setup bar provider hub
        BarHub barHub = new();

        // initialize observer
        EmaHub observer = barHub
            .ToCmoHub(cmoPeriods)
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
        IReadOnlyList<EmaResult> sut = observer.Results;

        // time-series, for comparison (revised)
        IReadOnlyList<EmaResult> expected = RevisedBars
            .ToCmo(cmoPeriods)
            .ToEma(emaPeriods);

        // assert, should equal series
        sut.Should().HaveCount(barsCount - 1);
        sut.IsExactly(expected);

        // cleanup
        observer.Unsubscribe();
        barHub.EndTransmission();
    }

    [TestMethod]
    public override void ToStringOverride_ReturnsExpectedName()
    {
        BarHub barHub = new();
        CmoHub observer = barHub.ToCmoHub(14);

        observer.ToString().Should().Be("CMO(14)");

        // cleanup
        observer.Unsubscribe();
        barHub.EndTransmission();
    }
}
