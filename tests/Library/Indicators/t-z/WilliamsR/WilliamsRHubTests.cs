namespace StreamHubs;

[TestClass]
public class WilliamsRHubTests : StreamHubTestBase, ITestBarObserver
{
    [TestMethod]
    public void Results_AreAlwaysBounded()
    {
        IReadOnlyList<WilliamsResult> sut = Bars.ToWilliamsRHub(14).Results;
        sut.IsBetween(static x => x.WilliamsR, -100, 0);
    }

    [TestMethod]
    public void Boundary_WithRandomBars_StaysWithinBounds()
    {
        IReadOnlyList<WilliamsResult> sut = Data
            .GetRandom(2500)
            .ToWilliamsRHub(14)
            .Results;

        sut.IsBetween(static x => x.WilliamsR, -100d, 0d);
    }

    [TestMethod]
    public void BarObserver_WithWarmupLateArrivalAndRemoval_MatchesSeriesExactly()
    {
        const int lookbackPeriods = 14;
        int length = Bars.Count;

        // setup bar provider hub
        BarHub barHub = new();

        // prefill bars at provider (warmup coverage)
        barHub.Add(Bars.Take(20));

        // initialize observer
        WilliamsRHub observer = barHub.ToWilliamsRHub(lookbackPeriods);

        // fetch initial results (early)
        IReadOnlyList<WilliamsResult> actuals = observer.Results;

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

        IReadOnlyList<WilliamsResult> expectedOriginal = Bars.ToWilliamsR(lookbackPeriods);

        actuals.Should().HaveCount(length);
        actuals.IsExactly(expectedOriginal);

        // delete, should equal series (revised)
        barHub.RemoveAt(removeAtIndex);

        IReadOnlyList<WilliamsResult> expectedRevised = RevisedBars.ToWilliamsR(lookbackPeriods);

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

        IReadOnlyList<Bar> bars = Bars.Take(totalBars).ToList();
        IReadOnlyList<WilliamsResult> expected = bars
            .ToWilliamsR(lookbackPeriods)
            .TakeLast(maxCacheSize)
            .ToList();

        // Setup with cache limit
        BarHub barHub = new(maxCacheSize);
        WilliamsRHub observer = barHub.ToWilliamsRHub(lookbackPeriods);

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
        WilliamsRHub hub = new(new BarHub(), 14);
        hub.ToString().Should().Be("WILLR(14)");
    }

    [TestMethod]
    public void IncrementalUpdates_WithStreamedBars_MatchesSeriesExactly()
    {
        const int lookbackPeriods = 14;

        List<Bar> barsList = Bars.ToList();

        // setup bar provider hub with incremental updates
        BarHub barHub = new();
        WilliamsRHub observer = barHub.ToWilliamsRHub(lookbackPeriods);

        // add bars one by one
        foreach (Bar bar in barsList)
        {
            barHub.Add(bar);
        }

        // close observations
        barHub.EndTransmission();

        // verify consistency
        IReadOnlyList<WilliamsResult> expected = Bars.ToWilliamsR(lookbackPeriods);
        observer.Cache.Should().HaveCount(expected.Count);
        observer.Cache.IsExactly(expected);
    }

    [TestMethod]
    public void Properties_OnHubInstantiation_ReflectInputParameters()
    {
        const int lookbackPeriods = 21;

        BarHub barHub = new();
        WilliamsRHub observer = barHub.ToWilliamsRHub(lookbackPeriods);

        // verify properties
        observer.LookbackPeriods.Should().Be(lookbackPeriods);
        observer.ToString().Should().Be($"WILLR({lookbackPeriods})");
    }

    [TestMethod]
    public void DefaultParameters_OnHubInstantiation_UseExpectedDefaults()
    {
        BarHub barHub = new();
        WilliamsRHub observer = barHub.ToWilliamsRHub();

        // verify default properties
        observer.LookbackPeriods.Should().Be(14);
        observer.ToString().Should().Be("WILLR(14)");
    }

    [TestMethod]
    public void StreamingAccuracy_VersusBatch_MatchesExactly()
    {
        // Test that streaming produces accurate results compared to batch processing
        const int lookbackPeriods = 14;

        List<Bar> barsList = Bars.ToList();

        // streaming calculation
        BarHub barHub = new();
        WilliamsRHub streamObserver = barHub.ToWilliamsRHub(lookbackPeriods);

        foreach (Bar bar in barsList)
        {
            barHub.Add(bar);
        }

        barHub.EndTransmission();

        // batch calculation
        IReadOnlyList<WilliamsResult> batchResults = Bars.ToWilliamsR(lookbackPeriods);

        // compare results
        streamObserver.Cache.Should().HaveCount(batchResults.Count);
        streamObserver.Cache.IsExactly(batchResults);
    }

    [TestMethod]
    public void ParameterValidation_InvalidLookback_ThrowsArgumentOutOfRangeException()
    {
        BarHub barHub = new();

        // Test parameter validation
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => barHub.ToWilliamsRHub(0));
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => barHub.ToWilliamsRHub(-1));
    }
}
