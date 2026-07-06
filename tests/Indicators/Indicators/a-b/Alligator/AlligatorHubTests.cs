namespace StreamHubs;

[TestClass]
public class AlligatorHubTests : StreamHubTestBase, ITestChainObserver
{
    [TestMethod]
    public void BarObserver_WithWarmupLateArrivalAndRemoval_MatchesSeriesExactly()
    {
        // setup bar provider hub
        BarHub barHub = new();

        // prefill bars at provider
        barHub.Add(Bars.Take(20));

        // initialize observer
        AlligatorHub observer = barHub.ToAlligatorHub();

        // fetch initial results (early)
        IReadOnlyList<AlligatorResult> sut = observer.Results;

        // emulate adding bars to provider hub
        for (int i = 20; i < barsCount; i++)
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

        IReadOnlyList<AlligatorResult> expectedOriginal = Bars.ToAlligator();
        sut.IsExactly(expectedOriginal);

        // delete, should equal series (revised)
        barHub.RemoveAt(removeAtIndex);

        IReadOnlyList<AlligatorResult> expectedRevised = RevisedBars.ToAlligator();
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
        IReadOnlyList<AlligatorResult> expected = bars
            .ToAlligator()
            .TakeLast(maxCacheSize)
            .ToList();

        // Setup with cache limit
        BarHub barHub = new(maxCacheSize);
        AlligatorHub observer = barHub.ToAlligatorHub();

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
    public void ChainObserver_ChainedProvider_MatchesSeriesExactly()
    {
        List<Bar> barsList = Bars.ToList();

        int length = barsList.Count;

        // setup bar provider hub
        BarHub barHub = new();

        // initialize observer
        AlligatorHub observer = barHub
            .ToSmaHub(10)
            .ToAlligatorHub();

        // emulate adding bars out of order
        // note: this works when graceful order
        for (int i = 0; i < length; i++)
        {
            // skip one (add later)
            if (i == 80)
            {
                continue;
            }

            Bar q = barsList[i];
            barHub.Add(q);

            // resend duplicate bars
            if (i is > 100 and < 105)
            {
                barHub.Add(q);
            }
        }

        // late arrival
        barHub.Add(barsList[80]);

        // delete
        barHub.RemoveAt(removeAtIndex);

        // final results
        IReadOnlyList<AlligatorResult> sut
            = observer.Results;

        // time-series, for comparison (revised)
        IReadOnlyList<AlligatorResult> expected
           = RevisedBars
            .ToSma(10)
            .ToAlligator();

        // assert, should equal series
        sut.Should().HaveCount(barsCount - 1);
        sut.IsExactly(expected);

        // cleanup
        observer.Unsubscribe();
        barHub.EndTransmission();
    }

    [TestMethod]
    public override void ToStringOverride_ReturnsExpectedName()
    {
        AlligatorHub hub = new(new BarHub(), 13, 8, 7, 5, 4, 3);
        hub.ToString().Should().Be("ALLIGATOR(13,8,7,5,4,3)");
    }
}
