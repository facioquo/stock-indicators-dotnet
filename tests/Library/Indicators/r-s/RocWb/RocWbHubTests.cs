namespace StreamHubs;

[TestClass]
public class RocWbHubTests : StreamHubTestBase, ITestChainObserver, ITestChainProvider
{
    private const int lookbackPeriods = 20;
    private const int emaPeriods = 5;
    private const int stdDevPeriods = 5;

    [TestMethod]
    public void BarObserver_WithWarmupLateArrivalAndRemoval_MatchesSeriesExactly()
    {
        // setup bar provider hub
        BarHub barHub = new();

        // prefill bars at provider
        barHub.Add(Bars.Take(25));

        // initialize observer
        RocWbHub observer = barHub
            .ToRocWbHub(lookbackPeriods, emaPeriods, stdDevPeriods);

        // fetch initial results (early)
        IReadOnlyList<RocWbResult> sut = observer.Results;

        // emulate adding bars to provider hub
        for (int i = 25; i < barsCount; i++)
        {
            if (i == 80) { continue; }  // Skip for late arrival

            Bar q = Bars[i];
            barHub.Add(q);

            if (i is > 100 and < 105) { barHub.Add(q); }  // Duplicate bars
        }

        // late arrival, should equal series
        barHub.Add(Bars[80]);

        IReadOnlyList<RocWbResult> expectedOriginal = Bars.ToRocWb(lookbackPeriods, emaPeriods, stdDevPeriods);
        sut.IsExactly(expectedOriginal);
        sut.Should().HaveCount(barsCount);

        // delete, should equal series (revised)
        barHub.RemoveAt(removeAtIndex);

        IReadOnlyList<RocWbResult> expectedRevised = RevisedBars.ToRocWb(lookbackPeriods, emaPeriods, stdDevPeriods);
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
        IReadOnlyList<RocWbResult> expected = bars
            .ToRocWb(lookbackPeriods, emaPeriods, stdDevPeriods)
            .TakeLast(maxCacheSize)
            .ToList();

        // Setup with cache limit
        BarHub barHub = new(maxCacheSize);
        RocWbHub observer = barHub.ToRocWbHub(lookbackPeriods, emaPeriods, stdDevPeriods);

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
        const int emaInnerPeriods = 12;

        List<Bar> barsList = Bars.ToList();

        int length = barsList.Count;

        // setup bar provider hub
        BarHub barHub = new();

        // initialize observer
        RocWbHub observer = barHub
            .ToEmaHub(emaInnerPeriods)
            .ToRocWbHub(lookbackPeriods, emaPeriods, stdDevPeriods);

        // emulate bar stream
        for (int i = 0; i < length; i++)
        {
            barHub.Add(barsList[i]);
        }

        // final results
        IReadOnlyList<RocWbResult> streamList
            = observer.Results;

        // time-series, for comparison
        IReadOnlyList<RocWbResult> seriesList
           = barsList
            .ToEma(emaInnerPeriods)
            .ToRocWb(lookbackPeriods, emaPeriods, stdDevPeriods);

        // assert, should equal series
        streamList.Should().HaveCount(length);
        streamList.IsExactly(seriesList);

        observer.Unsubscribe();
        barHub.EndTransmission();
    }

    [TestMethod]
    public void ChainProvider_MatchesSeriesExactly()
    {
        const int emaOuterPeriods = 12;

        // setup bar provider hub
        BarHub barHub = new();

        // initialize observer
        EmaHub observer = barHub
            .ToRocWbHub(lookbackPeriods, emaPeriods, stdDevPeriods)
            .ToEmaHub(emaOuterPeriods);

        // emulate bar stream
        for (int i = 0; i < barsCount; i++)
        {
            if (i == 80) { continue; }  // Skip for late arrival

            Bar q = Bars[i];
            barHub.Add(q);

            if (i is > 100 and < 105) { barHub.Add(q); }  // Duplicate bars
        }

        barHub.Add(Bars[80]);  // Late arrival
        barHub.RemoveAt(removeAtIndex);  // Remove

        // final results
        IReadOnlyList<EmaResult> sut = observer.Results;

        // time-series, for comparison (revised)
        IReadOnlyList<EmaResult> expected = RevisedBars
            .ToRocWb(lookbackPeriods, emaPeriods, stdDevPeriods)
            .ToEma(emaOuterPeriods);

        // assert, should equal series
        sut.Should().HaveCount(barsCount - 1);
        sut.IsExactly(expected);

        observer.Unsubscribe();
        barHub.EndTransmission();
    }

    [TestMethod]
    public override void ToStringOverride_ReturnsExpectedName()
    {
        RocWbHub hub = new(new BarHub(), lookbackPeriods, emaPeriods, stdDevPeriods);
        hub.ToString().Should().Be($"ROCWB({lookbackPeriods},{emaPeriods},{stdDevPeriods})");
    }
}
