namespace StreamHubs;

[TestClass]
public class PivotPointsHubTests : StreamHubTestBase, ITestBarObserver
{
    [TestMethod]
    public void BarObserver_WithWarmupLateArrivalAndRemoval_MatchesSeriesExactly()
    {
        // setup bar provider hub
        BarHub barHub = new();

        // prefill bars at provider (batch)
        barHub.Add(Bars.Take(20));

        // initialize observer
        PivotPointsHub observer = barHub
            .ToPivotPointsHub();

        observer.Results.Should().HaveCount(20);

        // fetch initial results (early)
        IReadOnlyList<PivotPointsResult> actuals
            = observer.Results;

        // emulate adding bars to provider hub
        for (int i = 20; i < barsCount; i++)
        {
            // skip one (add later)
            if (i is 30 or 80)
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

        // late arrivals
        barHub.Add(Bars[30]);  // rebuilds complete series
        barHub.Add(Bars[80]);  // rebuilds from insertion point

        // delete
        barHub.RemoveAt(removeAtIndex);

        // time-series, for comparison
        IReadOnlyList<PivotPointsResult> expected = RevisedBars.ToPivotPoints();

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
        IReadOnlyList<PivotPointsResult> expected = bars
            .ToPivotPoints()
            .TakeLast(maxCacheSize)
            .ToList();

        // Setup with cache limit
        BarHub barHub = new(maxCacheSize);
        PivotPointsHub observer = barHub.ToPivotPointsHub();

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
    public void BarObserver_WithWarmupLateArrivalAndRemoval_MatchesSeriesExactlyWithWeekly()
    {
        // Test with weekly window size

        // setup bar provider hub
        BarHub barHub = new();

        // initialize observer
        PivotPointsHub observer = barHub
            .ToPivotPointsHub(BarInterval.Week);

        // add bars to barHub
        barHub.Add(Bars);

        // stream results
        IReadOnlyList<PivotPointsResult> streamList
            = observer.Results;

        // time-series, for comparison
        IReadOnlyList<PivotPointsResult> seriesList
           = Bars
            .ToPivotPoints(BarInterval.Week);

        // assert, should equal series
        streamList.Should().HaveCount(Bars.Count);
        streamList.IsExactly(seriesList);

        // cleanup
        observer.Unsubscribe();
        barHub.EndTransmission();
    }

    [TestMethod]
    public void BarObserver_WithWarmupLateArrivalAndRemoval_MatchesSeriesExactlyWithCamarilla()
    {
        // Test with Camarilla pivot point type

        // setup bar provider hub
        BarHub barHub = new();

        // initialize observer
        PivotPointsHub observer = barHub
            .ToPivotPointsHub(pointType: PivotPointType.Camarilla);

        // add bars to barHub
        barHub.Add(Bars);

        // stream results
        IReadOnlyList<PivotPointsResult> streamList
            = observer.Results;

        // time-series, for comparison
        IReadOnlyList<PivotPointsResult> seriesList
           = Bars
            .ToPivotPoints(pointType: PivotPointType.Camarilla);

        // assert, should equal series
        streamList.Should().HaveCount(Bars.Count);
        streamList.IsExactly(seriesList);

        // cleanup
        observer.Unsubscribe();
        barHub.EndTransmission();
    }

    [TestMethod]
    public void BarObserver_WithWarmupLateArrivalAndRemoval_MatchesSeriesExactlyWithDemark()
    {
        // Test with Demark pivot point type

        // setup bar provider hub
        BarHub barHub = new();

        // initialize observer
        PivotPointsHub observer = barHub
            .ToPivotPointsHub(pointType: PivotPointType.Demark);

        // add bars to barHub
        barHub.Add(Bars);

        // stream results
        IReadOnlyList<PivotPointsResult> streamList
            = observer.Results;

        // time-series, for comparison
        IReadOnlyList<PivotPointsResult> seriesList
           = Bars
            .ToPivotPoints(pointType: PivotPointType.Demark);

        // assert, should equal series
        streamList.Should().HaveCount(Bars.Count);
        streamList.IsExactly(seriesList);

        // cleanup
        observer.Unsubscribe();
        barHub.EndTransmission();
    }

    [TestMethod]
    public void BarObserver_WithWarmupLateArrivalAndRemoval_MatchesSeriesExactlyWithFibonacci()
    {
        // Test with Fibonacci pivot point type

        // setup bar provider hub
        BarHub barHub = new();

        // initialize observer
        PivotPointsHub observer = barHub
            .ToPivotPointsHub(pointType: PivotPointType.Fibonacci);

        // add bars to barHub
        barHub.Add(Bars);

        // stream results
        IReadOnlyList<PivotPointsResult> streamList
            = observer.Results;

        // time-series, for comparison
        IReadOnlyList<PivotPointsResult> seriesList
           = Bars
            .ToPivotPoints(pointType: PivotPointType.Fibonacci);

        // assert, should equal series
        streamList.Should().HaveCount(Bars.Count);
        streamList.IsExactly(seriesList);

        // cleanup
        observer.Unsubscribe();
        barHub.EndTransmission();
    }

    [TestMethod]
    public void BarObserver_WithWarmupLateArrivalAndRemoval_MatchesSeriesExactlyWithWoodie()
    {
        // Test with Woodie pivot point type

        // setup bar provider hub
        BarHub barHub = new();

        // initialize observer
        PivotPointsHub observer = barHub
            .ToPivotPointsHub(pointType: PivotPointType.Woodie);

        // add bars to barHub
        barHub.Add(Bars);

        // stream results
        IReadOnlyList<PivotPointsResult> streamList
            = observer.Results;

        // time-series, for comparison
        IReadOnlyList<PivotPointsResult> seriesList
           = Bars
            .ToPivotPoints(pointType: PivotPointType.Woodie);

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
        PivotPointsHub hub1 = new(new BarHub(), BarInterval.Month, PivotPointType.Standard);
        hub1.ToString().Should().Be("PIVOT-POINTS(Month,Standard)");

        PivotPointsHub hub2 = new(new BarHub(), BarInterval.Week, PivotPointType.Camarilla);
        hub2.ToString().Should().Be("PIVOT-POINTS(Week,Camarilla)");
    }
}
