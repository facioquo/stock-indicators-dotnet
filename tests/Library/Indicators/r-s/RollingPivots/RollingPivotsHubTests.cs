namespace StreamHubs;

[TestClass]
public class RollingPivotsHubTests : StreamHubTestBase, ITestBarObserver
{
    [TestMethod]
    public void BarObserver_WithWarmupLateArrivalAndRemoval_MatchesSeriesExactly()
    {
        // setup bar provider hub
        BarHub barHub = new();

        // prefill bars at provider (batch)
        barHub.Add(Bars.Take(25));

        // initialize observer
        RollingPivotsHub observer = barHub
            .ToRollingPivotsHub(20, 0, PivotPointType.Standard);

        observer.Results.Should().HaveCount(25);

        // fetch initial results (early)
        IReadOnlyList<RollingPivotsResult> actuals
            = observer.Results;

        // emulate adding bars to provider hub
        for (int i = 25; i < barsCount; i++)
        {
            // skip one (add later)
            if (i == 80)
            {
                continue;
            }

            Bar q = Bars[i];
            barHub.Add(q);

            // resend duplicate bars
            if (i is > 100 and < 105)
            {
                barHub.Add(q);
            }
        }

        // late arrival
        barHub.Add(Bars[80]);

        // delete
        barHub.RemoveAt(removeAtIndex);

        // time-series, for comparison
        IReadOnlyList<RollingPivotsResult> expected = RevisedBars.ToRollingPivots(20, 0, PivotPointType.Standard);

        // assert, should equal series
        actuals.Should().HaveCount(barsCount - 1);
        actuals.IsExactly(expected);

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
        IReadOnlyList<RollingPivotsResult> expected = bars
            .ToRollingPivots(20, 0, PivotPointType.Standard)
            .TakeLast(maxCacheSize)
            .ToList();

        // Setup with cache limit
        BarHub barHub = new(maxCacheSize);
        RollingPivotsHub observer = barHub.ToRollingPivotsHub(20, 0, PivotPointType.Standard);

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
        RollingPivotsHub hub = new(new BarHub(), 20, 0, PivotPointType.Standard);
        hub.ToString().Should().Be("ROLLING-PIVOTS(20,0,Standard)");
    }
}
