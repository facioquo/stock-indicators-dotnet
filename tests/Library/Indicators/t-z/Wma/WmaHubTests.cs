namespace StreamHubs;

[TestClass]
public class WmaHubTests : StreamHubTestBase, ITestChainObserver, ITestChainProvider
{
    private const int LookbackPeriods = 14;

    [TestMethod]
    public void BarObserver_WithWarmupLateArrivalAndRemoval_MatchesSeriesExactly()
    {
        // setup bar provider hub
        BarHub barHub = new();

        // prefill bars at provider
        barHub.Add(Bars.Take(20));

        // initialize observer
        WmaHub observer = barHub.ToWmaHub(LookbackPeriods);

        // fetch initial results (early)
        IReadOnlyList<WmaResult> sut = observer.Results;

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

        IReadOnlyList<WmaResult> expectedOriginal = Bars.ToWma(LookbackPeriods);
        sut.IsExactly(expectedOriginal);

        // delete, should equal series (revised)
        barHub.RemoveAt(removeAtIndex);
        IReadOnlyList<WmaResult> expectedRevised = RevisedBars.ToWma(LookbackPeriods);
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
        IReadOnlyList<WmaResult> expected = bars
            .ToWma(LookbackPeriods)
            .TakeLast(maxCacheSize)
            .ToList();

        // Setup with cache limit
        BarHub barHub = new(maxCacheSize);
        WmaHub observer = barHub.ToWmaHub(LookbackPeriods);

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
        // setup bar provider hub
        BarHub barHub = new();

        // initialize observer
        WmaHub observer = barHub
            .ToBarPartHub(CandlePart.HL2)
            .ToWmaHub(LookbackPeriods);

        // emulate bar stream
        for (int i = 0; i < barsCount; i++) { barHub.Add(Bars[i]); }

        // final results
        IReadOnlyList<WmaResult> sut = observer.Results;

        // time-series, for comparison
        IReadOnlyList<WmaResult> expected = Bars
            .Use(CandlePart.HL2)
            .ToWma(LookbackPeriods);

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
        const int wmaPeriods = LookbackPeriods;
        const int emaPeriods = 10;

        // setup bar provider hub
        BarHub barHub = new();

        // initialize observer
        EmaHub observer = barHub
            .ToWmaHub(wmaPeriods)
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

        // late arrival
        barHub.Add(Bars[80]);

        // delete
        barHub.RemoveAt(removeAtIndex);

        // final results
        IReadOnlyList<EmaResult> sut = observer.Results;

        // time-series, for comparison (revised)
        IReadOnlyList<EmaResult> expected = RevisedBars
            .ToWma(wmaPeriods)
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
        WmaHub hub = new(new BarHub(), LookbackPeriods);
        hub.ToString().Should().Be($"WMA({LookbackPeriods})");
    }
}
