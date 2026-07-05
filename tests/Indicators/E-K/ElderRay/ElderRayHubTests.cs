namespace StreamHubs;

[TestClass]
public class ElderRayHubTests : StreamHubTestBase, ITestBarObserver
{
    private const int lookbackPeriods = 13;
    private static readonly IReadOnlyList<ElderRayResult> expectedOriginal = Bars.ToElderRay(lookbackPeriods);

    [TestMethod]
    public void BarObserver_WithWarmupLateArrivalAndRemoval_MatchesSeriesExactly()
    {
        List<Bar> bars = Bars.ToList();
        int length = bars.Count;

        // setup bar provider hub
        BarHub barHub = new();

        // prefill bars at provider
        barHub.Add(bars.Take(20));

        // initialize observer
        ElderRayHub observer = barHub.ToElderRayHub(lookbackPeriods);

        // fetch initial results (early)
        IReadOnlyList<ElderRayResult> actuals = observer.Results;

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
        bars.RemoveAt(removeAtIndex);

        IReadOnlyList<ElderRayResult> expectedRevised = bars.ToElderRay(lookbackPeriods);

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
        IReadOnlyList<ElderRayResult> expected = bars
            .ToElderRay(lookbackPeriods)
            .TakeLast(maxCacheSize)
            .ToList();

        // Setup with cache limit
        BarHub barHub = new(maxCacheSize);
        ElderRayHub observer = barHub.ToElderRayHub(lookbackPeriods);

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
        ElderRayHub hub = new(new BarHub(), 14);
        hub.ToString().Should().Be("ELDER-RAY(14)");
    }
}
