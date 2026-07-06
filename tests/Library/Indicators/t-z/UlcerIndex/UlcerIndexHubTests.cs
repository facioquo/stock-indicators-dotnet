namespace StreamHubs;

[TestClass]
public class UlcerIndexHubTests : StreamHubTestBase, ITestChainObserver, ITestChainProvider
{
    private const int lookbackPeriods = 14;
    private readonly IReadOnlyList<UlcerIndexResult> expectedOriginal = Bars.ToUlcerIndex(lookbackPeriods);

    [TestMethod]
    public void BarObserver_WithWarmupLateArrivalAndRemoval_MatchesSeriesExactly()
    {
        int length = Bars.Count;

        // setup bar provider hub
        BarHub barHub = new();

        // prefill bars at provider
        barHub.Add(Bars.Take(20));

        // initialize observer
        UlcerIndexHub observer = barHub.ToUlcerIndexHub(lookbackPeriods);

        // fetch initial results (early)
        IReadOnlyList<UlcerIndexResult> actuals = observer.Results;

        // emulate adding bars to provider hub
        for (int i = 20; i < length; i++)
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
        actuals.IsExactly(expectedOriginal);

        // delete, should equal series (revised)
        barHub.RemoveAt(removeAtIndex);

        IReadOnlyList<UlcerIndexResult> expectedRevised = RevisedBars.ToUlcerIndex(lookbackPeriods);

        actuals.Should().HaveCount(501);
        actuals.IsExactly(expectedRevised);

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
        IReadOnlyList<UlcerIndexResult> expected = bars
            .ToUlcerIndex(lookbackPeriods)
            .TakeLast(maxCacheSize)
            .ToList();

        // Setup with cache limit
        BarHub barHub = new(maxCacheSize);
        UlcerIndexHub observer = barHub.ToUlcerIndexHub(lookbackPeriods);

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
        const int ulcerPeriods = 14;
        const int smaPeriods = 8;
        int length = Bars.Count;

        // setup bar provider hub
        BarHub barHub = new();

        // initialize observer
        UlcerIndexHub observer = barHub
            .ToSmaHub(smaPeriods)
            .ToUlcerIndexHub(ulcerPeriods);

        // emulate bar stream
        for (int i = 0; i < length; i++) { barHub.Add(Bars[i]); }

        // final results
        IReadOnlyList<UlcerIndexResult> actuals = observer.Results;

        // time-series, for comparison
        IReadOnlyList<UlcerIndexResult> expected = Bars
            .ToSma(smaPeriods)
            .ToUlcerIndex(ulcerPeriods);

        // assert, should equal series
        actuals.Should().HaveCount(length);
        actuals.IsExactly(expected);

        // cleanup
        observer.Unsubscribe();
        barHub.EndTransmission();
    }

    [TestMethod]
    public void ChainProvider_MatchesSeriesExactly()
    {
        const int ulcerPeriods = 14;
        const int smaPeriods = 10;
        int length = Bars.Count;

        // setup bar provider hub
        BarHub barHub = new();

        // initialize observer
        SmaHub observer = barHub
            .ToUlcerIndexHub(ulcerPeriods)
            .ToSmaHub(smaPeriods);

        // emulate adding bars to provider hub
        for (int i = 0; i < length; i++)
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
        IReadOnlyList<SmaResult> actuals
            = observer.Results;

        // time-series, for comparison (revised)
        IReadOnlyList<SmaResult> seriesList = RevisedBars
            .ToUlcerIndex(ulcerPeriods)
            .ToSma(smaPeriods);

        // assert, should equal series
        actuals.Should().HaveCount(length - 1);
        actuals.IsExactly(seriesList);

        // cleanup
        observer.Unsubscribe();
        barHub.EndTransmission();
    }

    [TestMethod]
    public override void ToStringOverride_ReturnsExpectedName()
    {
        UlcerIndexHub hub = new(new BarHub(), 14);
        hub.ToString().Should().Be("ULCER(14)");
    }

    [TestMethod]
    public void Results_AreAlwaysBounded()
    {
        IReadOnlyList<UlcerIndexResult> sut = Bars.ToUlcerIndexHub(14).Results;
        sut.IsBetween(static x => x.UlcerIndex, 0, 100);
    }
}
