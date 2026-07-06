namespace StreamHubs;

[TestClass]
public class SmiHubTests : StreamHubTestBase, ITestBarObserver, ITestChainProvider
{
    private const int lookbackPeriods = 13;
    private const int firstSmoothPeriods = 25;
    private const int secondSmoothPeriods = 2;
    private const int signalPeriods = 3;

    private static readonly IReadOnlyList<SmiResult> expectedOriginal
        = Bars.ToSmi(lookbackPeriods, firstSmoothPeriods, secondSmoothPeriods, signalPeriods);

    [TestMethod]
    public void BarObserver_WithWarmupLateArrivalAndRemoval_MatchesSeriesExactly()
    {
        List<Bar> bars = Bars.ToList();
        int length = Bars.Count;

        // setup bar provider hub
        BarHub barHub = new();

        // prefill bars at provider
        barHub.Add(bars.Take(20));

        // initialize observer
        SmiHub observer = barHub.ToSmiHub(
            lookbackPeriods, firstSmoothPeriods, secondSmoothPeriods, signalPeriods);

        // fetch initial results (early)
        IReadOnlyList<SmiResult> actuals = observer.Results;

        // emulate adding bars to provider hub
        for (int i = 20; i < length; i++)
        {
            // skip one (add later)
            if (i == 80) { continue; }

            Bar q = bars[i];
            barHub.Add(q);

            // resend duplicate bars
            if (i is > 100 and < 105) { barHub.Add(q); }
        }

        // late arrival, should equal series
        barHub.Add(bars[80]);
        actuals.IsExactly(expectedOriginal);

        // delete, should equal series (revised)
        barHub.RemoveAt(removeAtIndex);

        IReadOnlyList<SmiResult> expectedRevised = RevisedBars.ToSmi(
            lookbackPeriods, firstSmoothPeriods, secondSmoothPeriods, signalPeriods);

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
        IReadOnlyList<SmiResult> expected = bars
            .ToSmi(lookbackPeriods, firstSmoothPeriods, secondSmoothPeriods, signalPeriods)
            .TakeLast(maxCacheSize)
            .ToList();

        // Setup with cache limit
        BarHub barHub = new(maxCacheSize);
        SmiHub observer = barHub.ToSmiHub(
            lookbackPeriods, firstSmoothPeriods, secondSmoothPeriods, signalPeriods);

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
        // SMI emits IReusable results (SmiResult implements IReusable with Value = Smi),
        // so it can act as a chain provider for downstream indicators.

        const int emaPeriods = 10;

        // setup bar provider hub
        BarHub barHub = new();

        // initialize chain: SMI then EMA over its Value
        EmaHub observer = barHub
            .ToSmiHub(lookbackPeriods, firstSmoothPeriods, secondSmoothPeriods, signalPeriods)
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

        // results from stream
        IReadOnlyList<EmaResult> sut = observer.Results;

        // time-series parity (revised)
        IReadOnlyList<EmaResult> expected = RevisedBars
            .ToSmi(lookbackPeriods, firstSmoothPeriods, secondSmoothPeriods, signalPeriods)
            .ToEma(emaPeriods);

        sut.Should().HaveCount(barsCount - 1);
        sut.IsExactly(expected);

        observer.Unsubscribe();
        barHub.EndTransmission();
    }

    [TestMethod]
    public override void ToStringOverride_ReturnsExpectedName()
    {
        BarHub barHub = new();

        SmiHub hub = new(
            barHub, lookbackPeriods, firstSmoothPeriods, secondSmoothPeriods, signalPeriods);
        hub.ToString().Should().Be($"SMI({lookbackPeriods},{firstSmoothPeriods},{secondSmoothPeriods},{signalPeriods})");
    }

    [TestMethod]
    public void IncrementalUpdates_WithStreamedBars_MatchesSeriesExactly()
    {
        List<Bar> barsList = Bars.ToList();

        // setup bar provider hub with incremental updates
        BarHub barHub = new();
        SmiHub observer = barHub.ToSmiHub(
            lookbackPeriods,
            firstSmoothPeriods,
            secondSmoothPeriods,
            signalPeriods);

        // add bars one by one
        foreach (Bar bar in barsList)
        {
            barHub.Add(bar);
        }

        // close observations
        barHub.EndTransmission();

        // verify consistency
        IReadOnlyList<SmiResult> expected = Bars.ToSmi(
            lookbackPeriods,
            firstSmoothPeriods,
            secondSmoothPeriods,
            signalPeriods);

        observer.Cache.IsExactly(expected);
    }

    [TestMethod]
    public void Properties_OnHubInstantiation_ReflectInputParameters()
    {
        const int testLookbackPeriods = 21;
        const int testFirstSmoothPeriods = 30;
        const int testSecondSmoothPeriods = 5;
        const int testSignalPeriods = 7;

        BarHub barHub = new();
        SmiHub observer = barHub.ToSmiHub(
            testLookbackPeriods,
            testFirstSmoothPeriods,
            testSecondSmoothPeriods,
            testSignalPeriods);

        // verify properties
        observer.LookbackPeriods.Should().Be(testLookbackPeriods);
        observer.FirstSmoothPeriods.Should().Be(testFirstSmoothPeriods);
        observer.SecondSmoothPeriods.Should().Be(testSecondSmoothPeriods);
        observer.SignalPeriods.Should().Be(testSignalPeriods);
        observer.K1.Should().Be(2d / (testFirstSmoothPeriods + 1));
        observer.K2.Should().Be(2d / (testSecondSmoothPeriods + 1));
        observer.KS.Should().Be(2d / (testSignalPeriods + 1));
        observer.ToString().Should().Be($"SMI({testLookbackPeriods},{testFirstSmoothPeriods},{testSecondSmoothPeriods},{testSignalPeriods})");
    }

    [TestMethod]
    public void DefaultParameters_OnHubInstantiation_UseExpectedDefaults()
    {
        BarHub barHub = new();
        SmiHub observer = barHub.ToSmiHub();

        // verify default properties
        observer.LookbackPeriods.Should().Be(13);
        observer.FirstSmoothPeriods.Should().Be(25);
        observer.SecondSmoothPeriods.Should().Be(2);
        observer.SignalPeriods.Should().Be(3);
        observer.ToString().Should().Be("SMI(13,25,2,3)");
    }

    [TestMethod]
    public void StreamingAccuracy_VersusBatch_MatchesExactly()
    {
        // Test that streaming produces accurate results compared to batch processing
        List<Bar> barsList = Bars.ToList();

        // streaming calculation
        BarHub barHub = new();
        SmiHub streamObserver = barHub.ToSmiHub(
            lookbackPeriods,
            firstSmoothPeriods,
            secondSmoothPeriods,
            signalPeriods);

        foreach (Bar bar in barsList)
        {
            barHub.Add(bar);
        }

        barHub.EndTransmission();

        // batch calculation
        IReadOnlyList<SmiResult> batchResults = Bars.ToSmi(
            lookbackPeriods,
            firstSmoothPeriods,
            secondSmoothPeriods,
            signalPeriods);

        // compare results with strict ordering
        streamObserver.Cache.Should().HaveCount(batchResults.Count);
        streamObserver.Cache.IsExactly(batchResults);
    }

    [TestMethod]
    public void BatchProcessing_WithAllBarsAtOnce_MatchesSeriesExactly()
    {
        // Test batch processing with all bars added at once
        BarHub barHub = new();
        SmiHub observer = barHub.ToSmiHub(
            lookbackPeriods,
            firstSmoothPeriods,
            secondSmoothPeriods,
            signalPeriods);

        // add all bars at once
        barHub.Add(Bars);
        barHub.EndTransmission();

        // verify against static series calculation
        IReadOnlyList<SmiResult> expected = Bars.ToSmi(
            lookbackPeriods,
            firstSmoothPeriods,
            secondSmoothPeriods,
            signalPeriods);

        observer.Cache.Should().HaveCount(Bars.Count);
        observer.Cache.IsExactly(expected);
    }

    [TestMethod]
    public void Results_AreAlwaysBounded()
    {
        IReadOnlyList<SmiResult> sut = Bars.ToSmiHub(14, 20, 5, 3).Results;
        sut.IsBetween(static x => x.Smi, -100, 100);
        sut.IsBetween(static x => x.Signal, -100, 100);
    }
}
