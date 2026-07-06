namespace StreamHubs;

[TestClass]
public class VolatilityStopHubTests : StreamHubTestBase, ITestBarObserver
{
    [TestMethod]
    public void BarObserver_WithWarmupLateArrivalAndRemoval_MatchesSeriesExactly()
    {
        // setup bar provider hub
        BarHub barHub = new();

        // prefill bars at provider (batch)
        barHub.Add(Bars.Take(20));

        // initialize observer
        VolatilityStopHub observer = barHub.ToVolatilityStopHub();

        observer.Results.Should().HaveCount(20);

        // fetch initial results (early)
        IReadOnlyList<VolatilityStopResult> sut = observer.Results;

        // emulate adding bars to provider hub
        for (int i = 20; i < barsCount; i++)
        {
            // skip one (add later)
            if (i is 30 or 80) { continue; }

            Bar q = Bars[i];
            barHub.Add(q);

            // resend duplicate bars
            if (i is > 100 and < 105) { barHub.Add(q); }
        }

        // late arrivals (test Add late-arrival handling)
        barHub.Add(Bars[30]);  // rebuilds complete series
        barHub.Add(Bars[80]);  // rebuilds from insertion point

        IReadOnlyList<VolatilityStopResult> expectedOriginal = Bars.ToVolatilityStop();
        sut.IsExactly(expectedOriginal);

        // delete (test Remove functionality), should equal series (revised)
        barHub.RemoveAt(removeAtIndex);

        IReadOnlyList<VolatilityStopResult> expectedRevised = RevisedBars.ToVolatilityStop();
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
        IReadOnlyList<VolatilityStopResult> expected = bars
            .ToVolatilityStop()
            .TakeLast(maxCacheSize)
            .ToList();

        // Setup with cache limit
        BarHub barHub = new(maxCacheSize);
        VolatilityStopHub observer = barHub.ToVolatilityStopHub();

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
    public void BarObserver_WithWarmupLateArrivalAndRemoval_MatchesSeriesExactlyCustomParameters()
    {
        // simple test with custom parameters

        // setup bar provider hub
        BarHub barHub = new();

        // initialize observer with custom parameters
        VolatilityStopHub observer = barHub
            .ToVolatilityStopHub(lookbackPeriods: 14, multiplier: 2.5);

        // add bars to barHub
        barHub.Add(Bars);

        // stream results
        IReadOnlyList<VolatilityStopResult> streamList
            = observer.Results;

        // time-series, for comparison
        IReadOnlyList<VolatilityStopResult> seriesList
           = Bars
            .ToVolatilityStop(lookbackPeriods: 14, multiplier: 2.5);

        // assert, should equal series
        streamList.Should().HaveCount(Bars.Count);
        streamList.IsExactly(seriesList);

        // cleanup
        observer.Unsubscribe();
        barHub.EndTransmission();
    }

    [TestMethod]
    public override void ToStringOverride_ReturnsExpectedName()
    {
        VolatilityStopHub hub = new(new BarHub(), 7, 3);
        hub.ToString().Should().Be("VOLATILITY-STOP(7,3)");
    }
}
