namespace StreamHubs;

[TestClass]
public class FractalHubTests : StreamHubTestBase, ITestBarObserver
{
    [TestMethod]
    public void BarObserver_WithWarmupLateArrivalAndRemoval_MatchesSeriesExactly()
    {
        // setup bar provider hub
        BarHub barHub = new();

        // prefill bars at provider (batch)
        barHub.Add(Bars.Take(20));

        // initialize observer
        FractalHub observer = barHub
            .ToFractalHub();

        observer.Results.Should().HaveCount(20);

        // fetch initial results (early)
        IReadOnlyList<FractalResult> actuals
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

        // Ensure all fractals are calculated with full context
        observer.Rebuild(0);

        // time-series, for comparison
        IReadOnlyList<FractalResult> expected = RevisedBars.ToFractal();

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
        IReadOnlyList<FractalResult> expected = bars
            .ToFractal()
            .TakeLast(maxCacheSize)
            .ToList();

        // Setup with cache limit
        BarHub barHub = new(maxCacheSize);
        FractalHub observer = barHub.ToFractalHub();

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
        FractalHub observer = barHub
            .ToFractalHub(endType: EndType.Close);

        // add bars to barHub
        barHub.Add(Bars);

        // Trigger rebuild to calculate all fractals with complete future context
        observer.Rebuild(0);

        // stream results
        IReadOnlyList<FractalResult> streamList
            = observer.Results;

        // time-series, for comparison
        IReadOnlyList<FractalResult> seriesList
           = Bars
            .ToFractal(endType: EndType.Close);

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
        // test with different left and right spans

        // setup bar provider hub
        BarHub barHub = new();

        // initialize observer with different spans
        FractalHub observer = barHub
            .ToFractalHub(3, 4);

        // add bars to barHub
        barHub.Add(Bars);

        // Trigger rebuild to calculate all fractals with complete future context
        observer.Rebuild(0);

        // stream results
        IReadOnlyList<FractalResult> streamList
            = observer.Results;

        // time-series, for comparison
        IReadOnlyList<FractalResult> seriesList
           = Bars
            .ToFractal(3, 4);

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
        FractalHub hub1 = new(new BarHub(), 2, EndType.HighLow);
        hub1.ToString().Should().Be("FRACTAL(2,2,HIGHLOW)");

        FractalHub hub2 = new(new BarHub(), 3, 4, EndType.Close);
        hub2.ToString().Should().Be("FRACTAL(3,4,CLOSE)");
    }
}
