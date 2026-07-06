namespace StreamHubs;

[TestClass]
public class EmaHubTests : StreamHubTestBase, ITestChainObserver, ITestChainProvider
{
    private const int lookbackPeriods = 14;

    [TestMethod]
    public void BarObserver_WithWarmupLateArrivalAndRemoval_MatchesSeriesExactly()
    {
        // setup bar provider hub
        BarHub barHub = new();

        // prefill bars at provider
        barHub.Add(Bars.Take(20));

        // initialize observer
        EmaHub observer = barHub.ToEmaHub(lookbackPeriods);

        // fetch initial results (early)
        IReadOnlyList<EmaResult> sut = observer.Results;

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

        IReadOnlyList<EmaResult> expectedOriginal = Bars.ToEma(lookbackPeriods);
        sut.IsExactly(expectedOriginal);

        // delete, should equal series (revised)
        barHub.RemoveAt(removeAtIndex);

        IReadOnlyList<EmaResult> expectedRevised = RevisedBars.ToEma(lookbackPeriods);
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
        IReadOnlyList<EmaResult> expected = bars
            .ToEma(lookbackPeriods)
            .TakeLast(maxCacheSize)
            .ToList();

        // Setup with cache limit
        BarHub barHub = new(maxCacheSize);
        EmaHub observer = barHub.ToEmaHub(lookbackPeriods);

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
        const int smaPeriods = 8;
        const int emaPeriods = 12;

        // setup bar provider hub
        BarHub barHub = new();

        // initialize observer
        EmaHub observer = barHub
            .ToSmaHub(smaPeriods)
            .ToEmaHub(emaPeriods);

        // emulate bar stream
        for (int i = 0; i < barsCount; i++) { barHub.Add(Bars[i]); }

        // final results
        IReadOnlyList<EmaResult> sut = observer.Results;

        // time-series, for comparison
        IReadOnlyList<EmaResult> expected = Bars
            .ToSma(smaPeriods)
            .ToEma(emaPeriods);

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
        const int emaPeriods = 20;
        const int smaPeriods = 10;

        // setup bar provider hub
        BarHub barHub = new();

        // initialize observer
        SmaHub observer = barHub
            .ToEmaHub(emaPeriods)
            .ToSmaHub(smaPeriods);

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
        IReadOnlyList<SmaResult> sut = observer.Results;

        // time-series, for comparison (revised)
        IReadOnlyList<SmaResult> expected = RevisedBars
            .ToEma(emaPeriods)
            .ToSma(smaPeriods);

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
        EmaHub hub = new(new BarHub(), 14);
        hub.ToString().Should().Be("EMA(14)");
    }
}
