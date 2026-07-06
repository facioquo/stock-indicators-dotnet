namespace StreamHubs;

[TestClass]
public class StochHubTests : StreamHubTestBase, ITestBarObserver
{
    [TestMethod]
    public void BarObserver_WithWarmupLateArrivalAndRemoval_MatchesSeriesExactly()
    {
        const int lookbackPeriods = 14;
        const int signalPeriods = 3;
        const int smoothPeriods = 3;
        int length = Bars.Count;

        // setup bar provider hub
        BarHub barHub = new();

        // prefill bars at provider (warmup coverage)
        barHub.Add(Bars.Take(20));

        // initialize observer
        StochHub observer = barHub.ToStochHub(lookbackPeriods, signalPeriods, smoothPeriods);

        // fetch initial results (early)
        IReadOnlyList<StochResult> actuals = observer.Results;

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

        IReadOnlyList<StochResult> expected = Bars.ToStoch(lookbackPeriods, signalPeriods, smoothPeriods);
        actuals.Should().HaveCount(length);
        actuals.IsExactly(expected);

        // delete, should equal series (revised)
        barHub.RemoveAt(removeAtIndex);

        IReadOnlyList<StochResult> expectedRevised = RevisedBars.ToStoch(lookbackPeriods, signalPeriods, smoothPeriods);

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
        const int lookbackPeriods = 14;
        const int signalPeriods = 3;
        const int smoothPeriods = 3;

        IReadOnlyList<Bar> bars = Bars.Take(totalBars).ToList();
        IReadOnlyList<StochResult> expected = bars
            .ToStoch(lookbackPeriods, signalPeriods, smoothPeriods)
            .TakeLast(maxCacheSize)
            .ToList();

        // Setup with cache limit
        BarHub barHub = new(maxCacheSize);
        StochHub observer = barHub.ToStochHub(lookbackPeriods, signalPeriods, smoothPeriods);

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
    public override void ToStringOverride_ReturnsExpectedName()
    {
        StochHub hub = new(new BarHub(), 14, 3, 3);
        hub.ToString().Should().Be("STOCH(14,3,3)");
    }

    [TestMethod]
    public void LateArrival_MidStream_MatchesFreshStream()
    {
        const int totalBars = 300;
        const int lateIndex = 150;
        const int lookbackPeriods = 14;
        const int signalPeriods = 3;
        const int smoothPeriods = 3;

        IReadOnlyList<Bar> bars = Bars.Take(totalBars).ToList();

        BarHub lateSource = new();
        StochHub lateHub = lateSource.ToStochHub(lookbackPeriods, signalPeriods, smoothPeriods);
        for (int i = 0; i < totalBars; i++)
        {
            if (i == lateIndex) { continue; }

            lateSource.Add(bars[i]);
        }

        lateSource.Add(bars[lateIndex]);

        BarHub freshSource = new();
        StochHub freshHub = freshSource.ToStochHub(lookbackPeriods, signalPeriods, smoothPeriods);
        freshSource.Add(bars);

        lateHub.Results.Should().HaveCount(totalBars);
        lateHub.Results.IsExactly(freshHub.Results);

        lateHub.Unsubscribe();
        freshHub.Unsubscribe();
        lateSource.EndTransmission();
        freshSource.EndTransmission();
    }

    [TestMethod]
    public void LateArrival_AtSmoothingWarmupBoundary_MatchesFreshStream()
    {
        // %K emits at lookback + smooth - 1 (= 14 + 3 - 1 = 16), %D after
        // signal SMA over %K (~18). Index 22 forces replay across both
        // smoothing-stage transitions.
        const int totalBars = 300;
        const int lateIndex = 22;
        const int lookbackPeriods = 14;
        const int signalPeriods = 3;
        const int smoothPeriods = 3;

        IReadOnlyList<Bar> bars = Bars.Take(totalBars).ToList();

        BarHub lateSource = new();
        StochHub lateHub = lateSource.ToStochHub(lookbackPeriods, signalPeriods, smoothPeriods);
        for (int i = 0; i < totalBars; i++)
        {
            if (i == lateIndex) { continue; }

            lateSource.Add(bars[i]);
        }

        lateSource.Add(bars[lateIndex]);

        BarHub freshSource = new();
        StochHub freshHub = freshSource.ToStochHub(lookbackPeriods, signalPeriods, smoothPeriods);
        freshSource.Add(bars);

        lateHub.Results.Should().HaveCount(totalBars);
        lateHub.Results.IsExactly(freshHub.Results);

        lateHub.Unsubscribe();
        freshHub.Unsubscribe();
        lateSource.EndTransmission();
        freshSource.EndTransmission();
    }

    [TestMethod]
    public void Results_AreAlwaysBounded()
    {
        IReadOnlyList<StochResult> sut = Bars.ToStochHub(14, 3, 3).Results;
        sut.IsBetween(static x => x.Oscillator, 0, 100);
        sut.IsBetween(static x => x.Signal, 0, 100);
    }

    [TestMethod]
    public void Boundary_WithRandomBars_StaysWithinBounds()
    {
        // %J is unbounded by design so not asserted.
        IReadOnlyList<StochResult> sut = Data
            .GetRandom(2500)
            .ToStochHub(14, 3, 3)
            .Results;

        sut.IsBetween(static x => x.Oscillator, 0d, 100d);
        sut.IsBetween(static x => x.Signal, 0d, 100d);
    }

    [TestMethod]
    public void ExtendedParameters_WithCustomMaType_MatchesSeriesExactly()
    {
        const int lookbackPeriods = 9;
        const int signalPeriods = 5;
        const int smoothPeriods = 2;
        const double kFactor = 5;
        const double dFactor = 4;
        const MaType movingAverageType = MaType.SMMA;

        List<Bar> barsList = Bars.ToList();

        // setup bar provider hub and observer BEFORE adding data
        BarHub barHub = new();
        StochHub observer = barHub.ToStoch(
            lookbackPeriods, signalPeriods, smoothPeriods,
            kFactor, dFactor, movingAverageType);

        // add bars
        barHub.Add(barsList);

        // close observations
        barHub.EndTransmission();

        // verify against static series calculation
        IReadOnlyList<StochResult> expected = Bars.ToStoch(
            lookbackPeriods, signalPeriods, smoothPeriods,
            kFactor, dFactor, movingAverageType);

        observer.Cache.Should().HaveCount(Bars.Count);
        observer.Cache.IsExactly(expected);
    }

    [TestMethod]
    public void IncrementalUpdates_WithStreamedBars_MatchesSeriesExactly()
    {
        const int lookbackPeriods = 14;
        const int signalPeriods = 3;
        const int smoothPeriods = 3;

        List<Bar> barsList = Bars.ToList();

        // setup bar provider hub with incremental updates
        BarHub barHub = new();
        StochHub observer = barHub.ToStochHub(lookbackPeriods, signalPeriods, smoothPeriods);

        // add bars one by one
        foreach (Bar bar in barsList)
        {
            barHub.Add(bar);
        }

        // close observations
        barHub.EndTransmission();

        // verify consistency
        IReadOnlyList<StochResult> expected = Bars.ToStoch(lookbackPeriods, signalPeriods, smoothPeriods);
        observer.Cache.IsExactly(expected);
    }

    [TestMethod]
    public void Properties_OnHubInstantiation_ReflectInputParameters()
    {
        const int lookbackPeriods = 21;
        const int signalPeriods = 5;
        const int smoothPeriods = 2;
        const double kFactor = 5;
        const double dFactor = 4;
        const MaType movingAverageType = MaType.SMMA;

        BarHub barHub = new();
        StochHub observer = barHub.ToStoch(
            lookbackPeriods, signalPeriods, smoothPeriods,
            kFactor, dFactor, movingAverageType);

        // verify properties
        observer.LookbackPeriods.Should().Be(lookbackPeriods);
        observer.SignalPeriods.Should().Be(signalPeriods);
        observer.SmoothPeriods.Should().Be(smoothPeriods);
        observer.KFactor.Should().Be(kFactor);
        observer.DFactor.Should().Be(dFactor);
        observer.MovingAverageType.Should().Be(movingAverageType);
        observer.ToString().Should().Be($"STOCH({lookbackPeriods},{signalPeriods},{smoothPeriods})");
    }

    [TestMethod]
    public void DefaultParameters_OnHubInstantiation_UseExpectedDefaults()
    {
        BarHub barHub = new();
        StochHub observer = barHub.ToStochHub();

        // verify default properties
        observer.LookbackPeriods.Should().Be(14);
        observer.SignalPeriods.Should().Be(3);
        observer.SmoothPeriods.Should().Be(3);
        observer.KFactor.Should().Be(3);
        observer.DFactor.Should().Be(2);
        observer.MovingAverageType.Should().Be(MaType.SMA);
        observer.ToString().Should().Be("STOCH(14,3,3)");
    }

    [TestMethod]
    public void StreamingAccuracy_VersusBatch_MatchesExactly()
    {
        // Test that streaming produces accurate results compared to batch processing
        const int lookbackPeriods = 14;
        const int signalPeriods = 3;
        const int smoothPeriods = 3;

        List<Bar> barsList = Bars.ToList();

        // streaming calculation
        BarHub barHub = new();
        StochHub streamObserver = barHub.ToStochHub(lookbackPeriods, signalPeriods, smoothPeriods);

        foreach (Bar bar in barsList)
        {
            barHub.Add(bar);
        }

        barHub.EndTransmission();

        // batch calculation
        IReadOnlyList<StochResult> batchResults = Bars.ToStoch(lookbackPeriods, signalPeriods, smoothPeriods);

        // compare results with reasonable precision
        streamObserver.Cache.Should().HaveCount(batchResults.Count);

        for (int i = 0; i < streamObserver.Cache.Count; i++)
        {
            StochResult streamResult = streamObserver.Cache[i];
            StochResult batchResult = batchResults[i];

            streamResult.Timestamp.Should().Be(batchResult.Timestamp);
            streamResult.Oscillator.Should().Be(batchResult.Oscillator);
            streamResult.Signal.Should().Be(batchResult.Signal);
            streamResult.PercentJ.Should().Be(batchResult.PercentJ);
        }
    }
}
