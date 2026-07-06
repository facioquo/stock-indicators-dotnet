namespace StreamHubs;

[TestClass]
public class T3HubTests : StreamHubTestBase, ITestChainObserver, ITestChainProvider
{
    [TestMethod]
    public void BarObserver_WithWarmupLateArrivalAndRemoval_MatchesSeriesExactly()
    {
        const int lookbackPeriods = 5;
        const double volumeFactor = 0.7;

        // setup bar provider hub
        BarHub barHub = new();

        // prefill bars at provider
        barHub.Add(Bars.Take(20));

        // initialize observer
        T3Hub observer = barHub.ToT3Hub(lookbackPeriods, volumeFactor);

        // fetch initial results (early)
        IReadOnlyList<T3Result> sut = observer.Results;

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

        IReadOnlyList<T3Result> expectedOriginal = Bars.ToT3(lookbackPeriods, volumeFactor);
        sut.IsExactly(expectedOriginal);

        // delete, should equal series (revised)
        barHub.RemoveAt(removeAtIndex);
        IReadOnlyList<T3Result> expectedRevised = RevisedBars.ToT3(lookbackPeriods, volumeFactor);
        sut.IsExactly(expectedRevised);
        sut.Should().HaveCount(barsCount - 1);

        // cleanup
        observer.Unsubscribe();
        barHub.EndTransmission();
    }

    [TestMethod]
    public void WithCachePruning_MatchesSeriesExactly()
    {
        const int maxCacheSize = 200;
        const int totalBars = 502;
        const int lookbackPeriods = 5;
        const double volumeFactor = 0.7;

        IReadOnlyList<Bar> bars = Bars.Take(totalBars).ToList();
        IReadOnlyList<T3Result> expected = bars
            .ToT3(lookbackPeriods, volumeFactor)
            .TakeLast(maxCacheSize)
            .ToList();

        // Setup with cache limit
        BarHub barHub = new(maxCacheSize);
        T3Hub observer = barHub.ToT3Hub(lookbackPeriods, volumeFactor);

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
        const int t3Periods = 5;
        const double volumeFactor = 0.7;

        // setup bar provider hub
        BarHub barHub = new();

        // initialize observer
        T3Hub observer = barHub
            .ToSmaHub(smaPeriods)
            .ToT3Hub(t3Periods, volumeFactor);

        // emulate bar stream
        for (int i = 0; i < barsCount; i++) { barHub.Add(Bars[i]); }

        // final results
        IReadOnlyList<T3Result> sut = observer.Results;

        // time-series, for comparison
        IReadOnlyList<T3Result> expected = Bars
            .ToSma(smaPeriods)
            .ToT3(t3Periods, volumeFactor);

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
        const int t3Periods = 5;
        const double volumeFactor = 0.7;
        const int smaPeriods = 10;

        // setup bar provider hub
        BarHub barHub = new();

        // initialize observer
        SmaHub observer = barHub
            .ToT3Hub(t3Periods, volumeFactor)
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
            .ToT3(t3Periods, volumeFactor)
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
        T3Hub hub = new(new BarHub(), 5, 0.7);
        hub.ToString().Should().Be("T3(5,0.7)");
    }

    [TestMethod]
    public void SettingsInheritance_FromParentHubs_PropagatesCorrectly()
    {
        // setup bar hub (1st level)
        BarHub barHub = new();

        // setup t3 hub (2nd level)
        T3Hub t3Hub = barHub
            .ToT3Hub(lookbackPeriods: 5, volumeFactor: 0.7);

        // setup child hub (3rd level)
        SmaHub childHub = t3Hub
            .ToSmaHub(lookbackPeriods: 5);

        // note: despite `barHub` being parentless,
        // it has default properties; it should not
        // inherit its own empty barHub settings

        // assert
        barHub.Properties.Settings.Should().Be(0b00000000, "it has default settings, not inherited");
        t3Hub.Properties.Settings.Should().Be(0b00000000, "it has default properties");
        childHub.Properties.Settings.Should().Be(0b00000000, "it inherits default properties");
    }
}
