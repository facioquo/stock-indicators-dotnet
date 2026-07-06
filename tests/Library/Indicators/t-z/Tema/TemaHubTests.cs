namespace StreamHubs;

[TestClass]
public class TemaHubTests : StreamHubTestBase, ITestChainObserver, ITestChainProvider
{
    [TestMethod]
    public void BarObserver_WithWarmupLateArrivalAndRemoval_MatchesSeriesExactly()
    {
        const int lookbackPeriods = 20;

        // setup bar provider hub
        BarHub barHub = new();

        // prefill bars at provider
        barHub.Add(Bars.Take(20));

        // initialize observer
        TemaHub observer = barHub.ToTemaHub(lookbackPeriods);

        // fetch initial results (early)
        IReadOnlyList<TemaResult> sut = observer.Results;

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

        IReadOnlyList<TemaResult> expectedOriginal = Bars.ToTema(lookbackPeriods);
        sut.IsExactly(expectedOriginal);

        // delete, should equal series (revised)
        barHub.RemoveAt(removeAtIndex);
        IReadOnlyList<TemaResult> expectedRevised = RevisedBars.ToTema(lookbackPeriods);
        sut.IsExactly(expectedRevised);
        sut.Should().HaveCount(barsCount - 1);

        // cleanup
        observer.Unsubscribe();
        barHub.EndTransmission();
    }

    [TestMethod]
    public void WithCachePruning_MatchesSeriesExactly()
    {
        const int maxCacheSize = 65;  // 20*3 (triple smoothing) + 5 extra
        const int totalBars = 130;  // ~2x cache size
        const int lookbackPeriods = 20;

        IReadOnlyList<Bar> bars = Bars.Take(totalBars).ToList();
        IReadOnlyList<TemaResult> expected = bars
            .ToTema(lookbackPeriods)
            .TakeLast(maxCacheSize)
            .ToList();

        // Setup with cache limit
        BarHub barHub = new(maxCacheSize);
        TemaHub observer = barHub.ToTemaHub(lookbackPeriods);

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
        const int smaPeriods = 10;
        const int temaPeriods = 20;

        // setup bar provider hub
        BarHub barHub = new();

        // initialize observer
        TemaHub observer = barHub
            .ToSmaHub(smaPeriods)
            .ToTemaHub(temaPeriods);

        // emulate bar stream
        for (int i = 0; i < barsCount; i++) { barHub.Add(Bars[i]); }

        // final results
        IReadOnlyList<TemaResult> sut = observer.Results;

        // time-series, for comparison
        IReadOnlyList<TemaResult> expected = Bars
            .ToSma(smaPeriods)
            .ToTema(temaPeriods);

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
        const int temaPeriods = 20;
        const int smaPeriods = 10;

        // setup bar provider hub
        BarHub barHub = new();

        // initialize observer
        SmaHub observer = barHub
            .ToTemaHub(temaPeriods)
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
            .ToTema(temaPeriods)
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
        TemaHub hub = new(new BarHub(), 20);
        hub.ToString().Should().Be("TEMA(20)");
    }

    [TestMethod]
    public void SettingsInheritance_FromParentHubs_PropagatesCorrectly()
    {
        // setup bar hub (1st level)
        BarHub barHub = new();

        // setup tema hub (2nd level)
        TemaHub temaHub = barHub
            .ToTemaHub(lookbackPeriods: 20);

        // setup child hub (3rd level)
        SmaHub childHub = temaHub
            .ToSmaHub(lookbackPeriods: 5);

        // note: despite `barHub` being parentless,
        // it has default properties; it should not
        // inherit its own empty barHub settings

        // assert
        barHub.Properties.Settings.Should().Be(0b00000000, "it has default settings, not inherited");
        temaHub.Properties.Settings.Should().Be(0b00000000, "it has default properties");
        childHub.Properties.Settings.Should().Be(0b00000000, "it inherits default properties");
    }
}
