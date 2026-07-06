namespace StreamHubs;

[TestClass]
public class ParabolicSarHubTests : StreamHubTestBase, ITestBarObserver, ITestChainProvider
{
    private const double accelerationStep = 0.02;
    private const double maxAccelerationFactor = 0.2;

    [TestMethod]
    public void BarObserver_WithWarmupLateArrivalAndRemoval_MatchesSeriesExactly()
    {
        List<Bar> bars = Bars.ToList();
        int length = bars.Count;

        // setup bar provider hub
        BarHub barHub = new();

        // prefill bars at provider
        for (int i = 0; i < 50; i++)
        {
            barHub.Add(bars[i]);
        }

        // initialize observer
        ParabolicSarHub observer = barHub
            .ToParabolicSarHub(accelerationStep, maxAccelerationFactor);

        // fetch initial results (early)
        IReadOnlyList<ParabolicSarResult> streamList = observer.Results;

        // emulate adding bars to provider hub
        for (int i = 50; i < length; i++)
        {
            // skip one (add later)
            if (i == 80)
            {
                continue;
            }

            Bar q = bars[i];
            barHub.Add(q);

            // resend duplicate bars
            if (i is > 100 and < 105)
            {
                barHub.Add(q);
            }
        }

        // late arrival
        barHub.Add(bars[80]);

        // Should match series after all bars added
        IReadOnlyList<ParabolicSarResult> expectedOriginal = bars
            .ToParabolicSar(accelerationStep, maxAccelerationFactor);

        streamList.IsExactly(expectedOriginal);

        // delete
        barHub.RemoveAt(removeAtIndex);
        bars.RemoveAt(removeAtIndex);

        // time-series, for comparison (revised)
        IReadOnlyList<ParabolicSarResult> seriesList = bars
            .ToParabolicSar(accelerationStep, maxAccelerationFactor);

        // assert, should equal series (revised)
        streamList.Should().HaveCount(501);
        streamList.IsExactly(seriesList);

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
        IReadOnlyList<ParabolicSarResult> expected = bars
            .ToParabolicSar(accelerationStep, maxAccelerationFactor)
            .TakeLast(maxCacheSize)
            .ToList();

        // Setup with cache limit
        BarHub barHub = new(maxCacheSize);
        ParabolicSarHub observer = barHub.ToParabolicSarHub(accelerationStep, maxAccelerationFactor);

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
    public void ChainProvider_MatchesSeriesExactly()
    {
        const int smaPeriods = 10;
        List<Bar> bars = Bars.ToList();
        int length = bars.Count;

        // setup bar provider hub
        BarHub barHub = new();

        // initialize observer
        SmaHub observer = barHub
            .ToParabolicSarHub(accelerationStep, maxAccelerationFactor)
            .ToSmaHub(smaPeriods);

        // emulate adding bars to provider hub
        for (int i = 0; i < length; i++)
        {
            // skip one (add later)
            if (i == 80)
            {
                continue;
            }

            Bar q = bars[i];
            barHub.Add(q);

            // resend duplicate bars
            if (i is > 100 and < 105)
            {
                barHub.Add(q);
            }
        }

        // late arrival
        barHub.Add(bars[80]);

        // delete
        barHub.RemoveAt(removeAtIndex);
        bars.RemoveAt(removeAtIndex);

        // final results
        IReadOnlyList<SmaResult> streamList = observer.Results;

        // time-series, for comparison (revised)
        IReadOnlyList<SmaResult> seriesList = bars
            .ToParabolicSar(accelerationStep, maxAccelerationFactor)
            .ToSma(smaPeriods);

        // assert, should equal series
        streamList.Should().HaveCount(501);
        streamList.IsExactly(seriesList);

        // cleanup
        observer.Unsubscribe();
        barHub.EndTransmission();
    }

    [TestMethod]
    public override void ToStringOverride_ReturnsExpectedName()
    {
        ParabolicSarHub hub = new(new BarHub(), 0.02, 0.2);
        hub.ToString().Should().Be("PSAR(0.02,0.2,0.02)");
    }

    [TestMethod]
    public void CustomInitialFactor_WithCustomValue_MatchesSeriesExactly()
    {
        const double customInitialFactor = 0.05;
        List<Bar> barsList = Bars.ToList();

        // setup bar provider hub
        BarHub barHub = new();
        barHub.Add(barsList);

        // initialize observer with custom initial factor
        ParabolicSarHub observer = barHub
            .ToParabolicSarHub(accelerationStep, maxAccelerationFactor, customInitialFactor);

        // fetch results
        IReadOnlyList<ParabolicSarResult> streamList = observer.Results;

        // time-series, for comparison
        IReadOnlyList<ParabolicSarResult> seriesList = barsList
            .ToParabolicSar(accelerationStep, maxAccelerationFactor, customInitialFactor);

        // assert, should equal series
        streamList.IsExactly(seriesList);

        // cleanup
        observer.Unsubscribe();
        barHub.EndTransmission();
    }
}
