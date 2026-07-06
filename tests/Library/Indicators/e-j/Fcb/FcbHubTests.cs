namespace StreamHubs;

[TestClass]
public class FcbHubTests : StreamHubTestBase, ITestBarObserver
{
    [TestMethod]
    public void BarObserver_WithWarmupLateArrivalAndRemoval_MatchesSeriesExactly()
    {
        // setup bar provider hub
        BarHub barHub = new();

        // prefill bars at provider (batch)
        barHub.Add(Bars.Take(20));

        // initialize observer
        FcbHub observer = barHub.ToFcbHub();

        observer.Results.Should().HaveCount(20);

        // fetch initial results (early)
        IReadOnlyList<FcbResult> sut
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
        barHub.Add(Bars[30]);
        barHub.Add(Bars[80]);

        // delete
        barHub.RemoveAt(removeAtIndex);

        // time-series, for comparison
        IReadOnlyList<FcbResult> expected
           = RevisedBars.ToFcb();

        // assert, should equal series
        sut.Should().HaveCount(barsCount - 1);
        sut.IsExactly(expected);

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
        IReadOnlyList<FcbResult> expected = bars
            .ToFcb()
            .TakeLast(maxCacheSize)
            .ToList();

        // Setup with cache limit
        BarHub barHub = new(maxCacheSize);
        FcbHub observer = barHub.ToFcbHub();

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
    public void BarObserver_WithWarmupLateArrivalAndRemoval_MatchesSeriesExactlyWithCustomWindowSpan()
    {
        // simple test with custom window span

        // setup bar provider hub
        BarHub barHub = new();

        // initialize observer with windowSpan = 3
        FcbHub observer = barHub.ToFcbHub(windowSpan: 3);

        // add bars to barHub
        barHub.Add(Bars);

        // stream results
        IReadOnlyList<FcbResult> streamList
            = observer.Results;

        // time-series, for comparison
        IReadOnlyList<FcbResult> seriesList
           = Bars.ToFcb(windowSpan: 3);

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
        FcbHub hub = new(new BarHub(), 2);
        hub.ToString().Should().Be("FCB(2)");
    }
}
