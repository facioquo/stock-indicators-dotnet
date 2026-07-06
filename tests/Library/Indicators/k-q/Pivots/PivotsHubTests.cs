namespace StreamHubs;

[TestClass]
public class PivotsHubTests : StreamHubTestBase, ITestBarObserver
{
    [TestMethod]
    public void BarObserver_WithWarmupLateArrivalAndRemoval_MatchesSeriesExactly()
    {
        // setup bar provider hub
        BarHub barHub = new();

        // prefill bars at provider (batch)
        barHub.Add(Bars.Take(20));

        // initialize observer
        PivotsHub observer = barHub
            .ToPivotsHub();

        observer.Results.Should().HaveCount(20);

        // fetch initial results (early)
        IReadOnlyList<PivotsResult> actuals
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

        // Ensure all pivots are calculated with full context
        observer.Rebuild(0);

        // time-series, for comparison
        IReadOnlyList<PivotsResult> expected = RevisedBars.ToPivots();

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
        IReadOnlyList<PivotsResult> expected = bars
            .ToPivots()
            .TakeLast(maxCacheSize)
            .ToList();

        // Setup with cache limit
        BarHub barHub = new(maxCacheSize);
        PivotsHub observer = barHub.ToPivotsHub();

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
    public void BarObserver_WithWarmupLateArrivalAndRemoval_MatchesSeriesExactlyClose()
    {
        // simple test, just to check Close variant

        // setup bar provider hub
        BarHub barHub = new();

        // initialize observer
        PivotsHub observer = barHub
            .ToPivotsHub(endType: EndType.Close);

        // add bars to barHub
        barHub.Add(Bars);

        // Trigger rebuild to calculate all pivots with complete future context
        observer.Rebuild(0);

        // stream results
        IReadOnlyList<PivotsResult> streamList
            = observer.Results;

        // time-series, for comparison
        IReadOnlyList<PivotsResult> seriesList
           = Bars
            .ToPivots(endType: EndType.Close);

        // assert, should equal series
        streamList.Should().HaveCount(Bars.Count);
        streamList.IsExactly(seriesList);

        // cleanup
        observer.Unsubscribe();
        barHub.EndTransmission();
    }

    [TestMethod]
    public void BarObserver_WithWarmupLateArrivalAndRemoval_MatchesSeriesExactlyDifferentSpans()
    {
        // test with different left and right spans and maxTrendPeriods

        // setup bar provider hub
        BarHub barHub = new();

        // initialize observer with different parameters
        PivotsHub observer = barHub
            .ToPivotsHub(leftSpan: 3, rightSpan: 4, maxTrendPeriods: 15);

        // add bars to barHub
        barHub.Add(Bars);

        // Trigger rebuild to calculate all pivots with complete future context
        observer.Rebuild(0);

        // stream results
        IReadOnlyList<PivotsResult> streamList
            = observer.Results;

        // time-series, for comparison
        IReadOnlyList<PivotsResult> seriesList
           = Bars
            .ToPivots(leftSpan: 3, rightSpan: 4, maxTrendPeriods: 15);

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
        PivotsHub hub1 = new(new BarHub(), 2, 2, 20, EndType.HighLow);
        hub1.ToString().Should().Be("PIVOTS(2,2,20,HIGHLOW)");

        PivotsHub hub2 = new(new BarHub(), 3, 4, 15, EndType.Close);
        hub2.ToString().Should().Be("PIVOTS(3,4,15,CLOSE)");
    }
}
