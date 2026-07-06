namespace StreamHubs;

[TestClass]
public class AtrStopHubTests : StreamHubTestBase, ITestBarObserver
{
    [TestMethod]
    public void BarObserver_WithWarmupLateArrivalAndRemoval_MatchesSeriesExactly()
    {
        // setup bar provider hub
        BarHub barHub = new();

        // prefill bars at provider (batch)
        barHub.Add(Bars.Take(20));

        // initialize observer
        AtrStopHub observer = barHub
            .ToAtrStopHub();

        observer.Results.Should().HaveCount(20);

        // fetch initial results (early)
        IReadOnlyList<AtrStopResult> actuals
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
        barHub.Add(Bars[80]);  // rebuilds from last reversal

        // delete
        barHub.RemoveAt(removeAtIndex);

        // time-series, for comparison
        IReadOnlyList<AtrStopResult> expected = RevisedBars.ToAtrStop();

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
        IReadOnlyList<AtrStopResult> expected = bars
            .ToAtrStop()
            .TakeLast(maxCacheSize)
            .ToList();

        // Setup with cache limit
        BarHub barHub = new(maxCacheSize);
        AtrStopHub observer = barHub.ToAtrStopHub();

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
    public void BarObserver_WithWarmupLateArrivalAndRemoval_MatchesSeriesExactlyHighLow()
    {
        // simple test, just to check High/Low variant

        // setup bar provider hub
        BarHub barHub = new();

        // initialize observer
        AtrStopHub observer = barHub
            .ToAtrStopHub(endType: EndType.HighLow);

        // add bars to barHub
        barHub.Add(Bars);

        // stream results
        IReadOnlyList<AtrStopResult> streamList
            = observer.Results;

        // time-series, for comparison
        IReadOnlyList<AtrStopResult> seriesList
           = Bars
            .ToAtrStop(endType: EndType.HighLow);

        // assert, should equal series
        streamList.Should().HaveCount(Bars.Count);
        streamList.IsExactly(seriesList);

        // cleanup
        observer.Unsubscribe();
        barHub.EndTransmission();
    }

    [TestMethod]
    public void LateArrival_MidStream_MatchesFreshStream()
    {
        // ATR-Stop carries reversal state across bars; a late arrival
        // landing inside an established trend must rebuild the same
        // stop sequence as the fresh stream.
        const int totalBars = 300;
        const int lateIndex = 150;

        IReadOnlyList<Bar> bars = Bars.Take(totalBars).ToList();

        BarHub lateSource = new();
        AtrStopHub lateHub = lateSource.ToAtrStopHub();
        for (int i = 0; i < totalBars; i++)
        {
            if (i == lateIndex) { continue; }

            lateSource.Add(bars[i]);
        }

        lateSource.Add(bars[lateIndex]);

        BarHub freshSource = new();
        AtrStopHub freshHub = freshSource.ToAtrStopHub();
        freshSource.Add(bars);

        lateHub.Results.Should().HaveCount(totalBars);
        lateHub.Results.IsExactly(freshHub.Results);

        lateHub.Unsubscribe();
        freshHub.Unsubscribe();
        lateSource.EndTransmission();
        freshSource.EndTransmission();
    }

    [TestMethod]
    public void LateArrival_AtAtrStopSeedBoundary_MatchesFreshStream()
    {
        // ATR-Stop emits first non-null result at lookback (= 14); index
        // 20 forces replay across the ATR seed + initial direction
        // transition that anchors the trailing-stop level.
        const int totalBars = 300;
        const int lateIndex = 20;

        IReadOnlyList<Bar> bars = Bars.Take(totalBars).ToList();

        BarHub lateSource = new();
        AtrStopHub lateHub = lateSource.ToAtrStopHub();
        for (int i = 0; i < totalBars; i++)
        {
            if (i == lateIndex) { continue; }

            lateSource.Add(bars[i]);
        }

        lateSource.Add(bars[lateIndex]);

        BarHub freshSource = new();
        AtrStopHub freshHub = freshSource.ToAtrStopHub();
        freshSource.Add(bars);

        lateHub.Results.Should().HaveCount(totalBars);
        lateHub.Results.IsExactly(freshHub.Results);

        lateHub.Unsubscribe();
        freshHub.Unsubscribe();
        lateSource.EndTransmission();
        freshSource.EndTransmission();
    }

    [TestMethod]
    public override void ToStringOverride_ReturnsExpectedName()
    {
        AtrStopHub hub = new(new BarHub(), 14, 3, EndType.Close);
        hub.ToString().Should().Be("ATR-STOP(14,3,CLOSE)");
    }
}
