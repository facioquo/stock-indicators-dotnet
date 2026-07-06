namespace StreamHubs;

[TestClass]
public class TrixHubTests : StreamHubTestBase, ITestChainObserver, ITestChainProvider
{
    [TestMethod]
    public void BarObserver_WithWarmupLateArrivalAndRemoval_MatchesSeriesExactly()
    {
        // setup bar provider hub
        BarHub barHub = new();

        // prefill bars at provider
        barHub.Add(Bars.Take(20));

        // initialize observer
        TrixHub observer = barHub.ToTrixHub(14);

        // fetch initial results (early)
        IReadOnlyList<TrixResult> sut = observer.Results;

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

        IReadOnlyList<TrixResult> expectedOriginal = Bars.ToTrix(14);
        sut.IsExactly(expectedOriginal);

        // delete, should equal series (revised)
        barHub.RemoveAt(removeAtIndex);

        IReadOnlyList<TrixResult> expectedRevised = RevisedBars.ToTrix(14);
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
        IReadOnlyList<TrixResult> expected = bars
            .ToTrix(14)
            .TakeLast(maxCacheSize)
            .ToList();

        // Setup with cache limit
        BarHub barHub = new(maxCacheSize);
        TrixHub observer = barHub.ToTrixHub(14);

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
        const int trixPeriods = 14;
        const int smaPeriods = 8;

        List<Bar> barsList = Bars.ToList();

        int length = barsList.Count;

        // setup bar provider hub
        BarHub barHub = new();

        // initialize observer
        TrixHub observer = barHub
            .ToSmaHub(smaPeriods)
            .ToTrixHub(trixPeriods);

        // emulate bar stream
        for (int i = 0; i < length; i++)
        {
            barHub.Add(barsList[i]);
        }

        // final results
        IReadOnlyList<TrixResult> streamList
            = observer.Results;

        // time-series, for comparison
        IReadOnlyList<TrixResult> seriesList
           = barsList
            .ToSma(smaPeriods)
            .ToTrix(trixPeriods);

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
        const int trixPeriods = 14;
        const int emaPeriods = 10;

        // setup bar provider hub
        BarHub barHub = new();

        // initialize chain with TRIX as input to EMA
        EmaHub emaOfTrix = barHub
            .ToTrixHub(trixPeriods)
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
        IReadOnlyList<EmaResult> sut = emaOfTrix.Results;

        // time-series, for comparison (revised)
        IReadOnlyList<EmaResult> expected = RevisedBars
            .ToTrix(trixPeriods)
            .ToEma(emaPeriods);

        // assert
        sut.Should().HaveCount(barsCount - 1);
        sut.IsExactly(expected);

        emaOfTrix.Unsubscribe();
        barHub.EndTransmission();
    }

    public override void ToStringOverride_ReturnsExpectedName()
    {
        TrixHub hub = new(new BarHub(), 14);
        hub.ToString().Should().Be("TRIX(14)");
    }
}
