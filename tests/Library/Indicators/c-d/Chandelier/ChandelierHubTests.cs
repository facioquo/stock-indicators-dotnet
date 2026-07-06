namespace StreamHubs;

[TestClass]
public class ChandelierHubTests : StreamHubTestBase, ITestBarObserver
{
    [TestMethod]
    public void BarObserver_WithWarmupLateArrivalAndRemoval_MatchesSeriesExactly()
    {
        // setup bar provider hub
        BarHub barHub = new();

        // prefill bars at provider (batch)
        barHub.Add(Bars.Take(25));

        // initialize observer
        ChandelierHub observer = barHub
            .ToChandelierHub(22, 3);

        observer.Results.Should().HaveCount(25);

        // fetch initial results (early)
        IReadOnlyList<ChandelierResult> actuals
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
        IReadOnlyList<ChandelierResult> expected = RevisedBars.ToChandelier(22, 3);

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
        IReadOnlyList<ChandelierResult> expected = bars
            .ToChandelier(22, 3)
            .TakeLast(maxCacheSize)
            .ToList();

        // Setup with cache limit
        BarHub barHub = new(maxCacheSize);
        ChandelierHub observer = barHub.ToChandelierHub(22, 3);

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
    public void BarObserver_WithWarmupLateArrivalAndRemoval_MatchesSeriesExactlyShort()
    {
        // simple test, just to check Short variant

        // setup bar provider hub
        BarHub barHub = new();

        // initialize observer
        ChandelierHub observer = barHub
            .ToChandelierHub(22, 3, Direction.Short);

        // add bars to barHub
        barHub.Add(Bars);

        // stream results
        IReadOnlyList<ChandelierResult> streamList
            = observer.Results;

        // time-series, for comparison
        IReadOnlyList<ChandelierResult> seriesList
           = Bars
            .ToChandelier(22, 3, Direction.Short);

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
        const int totalBars = 300;
        const int lateIndex = 150;

        IReadOnlyList<Bar> bars = Bars.Take(totalBars).ToList();

        BarHub lateSource = new();
        ChandelierHub lateHub = lateSource.ToChandelierHub(22, 3);
        for (int i = 0; i < totalBars; i++)
        {
            if (i == lateIndex) { continue; }

            lateSource.Add(bars[i]);
        }

        lateSource.Add(bars[lateIndex]);

        BarHub freshSource = new();
        ChandelierHub freshHub = freshSource.ToChandelierHub(22, 3);
        freshSource.Add(bars);

        lateHub.Results.Should().HaveCount(totalBars);
        lateHub.Results.IsExactly(freshHub.Results);

        lateHub.Unsubscribe();
        freshHub.Unsubscribe();
        lateSource.EndTransmission();
        freshSource.EndTransmission();
    }

    [TestMethod]
    public void LateArrival_AtAtrWarmupBoundary_MatchesFreshStream()
    {
        // Chandelier emits first non-null result at lookback (= 22); index
        // 28 forces replay across the rolling-extremum + ATR seeding
        // transition that sets the initial stop.
        const int totalBars = 300;
        const int lateIndex = 28;

        IReadOnlyList<Bar> bars = Bars.Take(totalBars).ToList();

        BarHub lateSource = new();
        ChandelierHub lateHub = lateSource.ToChandelierHub(22, 3);
        for (int i = 0; i < totalBars; i++)
        {
            if (i == lateIndex) { continue; }

            lateSource.Add(bars[i]);
        }

        lateSource.Add(bars[lateIndex]);

        BarHub freshSource = new();
        ChandelierHub freshHub = freshSource.ToChandelierHub(22, 3);
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
        ChandelierHub hub = new(new BarHub(), 22, 3, Direction.Long);
        hub.ToString().Should().Be("CHEXIT(22,3,LONG)");
    }
}
