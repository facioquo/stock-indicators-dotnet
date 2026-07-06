namespace StreamHubs;

[TestClass]
public class SmaHubTests : StreamHubTestBase, ITestChainObserver, ITestChainProvider
{
    [TestMethod]
    public void BarObserver_WithWarmupLateArrivalAndRemoval_MatchesSeriesExactly()
    {
        // setup bar provider hub
        BarHub barHub = new();

        // prefill bars at provider
        barHub.Add(Bars.Take(20));

        // initialize observer
        SmaHub observer = barHub.ToSmaHub(5);

        // fetch initial results (early)
        IReadOnlyList<SmaResult> sut = observer.Results;

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

        // late arrival triggers observer rebuild, should equal series
        barHub.Add(Bars[80]);

        IReadOnlyList<SmaResult> expectedOriginal = Bars.ToSma(5);
        sut.IsExactly(expectedOriginal);

        // delete, should equal series (revised)
        barHub.RemoveAt(removeAtIndex);
        IReadOnlyList<SmaResult> expectedRevised = RevisedBars.ToSma(5);
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
        IReadOnlyList<SmaResult> expected = bars
            .ToSma(5)
            .TakeLast(maxCacheSize)
            .ToList();

        // Setup with cache limit
        BarHub barHub = new(maxCacheSize);
        SmaHub observer = barHub.ToSmaHub(5);

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
        const int smaPeriods = 11;

        // setup bar provider hub
        BarHub barHub = new();

        // initialize observer
        SmaHub observer = barHub
            .ToBarPartHub(CandlePart.OC2)
            .ToSmaHub(smaPeriods);

        // emulate bar stream
        for (int i = 0; i < barsCount; i++) { barHub.Add(Bars[i]); }

        // final results
        IReadOnlyList<SmaResult> sut = observer.Results;

        // time-series, for comparison
        IReadOnlyList<SmaResult> expected = Bars
            .Use(CandlePart.OC2)
            .ToSma(smaPeriods);

        // assert, should equal series
        sut.IsExactly(expected);
        sut.Should().HaveCount(barsCount);

        // cleanup
        observer.Unsubscribe();
        barHub.EndTransmission();
    }

    [TestMethod]
    public void ChainProvider_MatchesSeriesExactly()
    {
        const int smaPeriods = 8;
        const int emaPeriods = 12;

        // setup bar provider hub
        BarHub barHub = new();

        // initialize observer
        EmaHub observer = barHub
            .ToSmaHub(smaPeriods)
            .ToEmaHub(emaPeriods);

        // emulate adding bars to provider hub
        for (int i = 0; i < barsCount; i++)
        {
            // skip one (add later)
            if (i == 80) { continue; }

            Bar q = Bars[i];
            barHub.Add(q);

            // resend duplicate bars
            if (i is > 100 and < 105) { barHub.Add(q); }
        }

        // late arrival triggers observer rebuild
        barHub.Add(Bars[80]);

        // delete
        barHub.RemoveAt(removeAtIndex);

        // final results
        IReadOnlyList<EmaResult> sut = observer.Results;

        // time-series, for comparison (revised)
        IReadOnlyList<EmaResult> expected = RevisedBars
            .ToSma(smaPeriods)
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
        SmaHub hub = new(new BarHub(), 5);
        hub.ToString().Should().Be("SMA(5)");
    }
}
