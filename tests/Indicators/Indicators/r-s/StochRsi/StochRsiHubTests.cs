namespace StreamHubs;

[TestClass]
public class StochRsiHubTests : StreamHubTestBase, ITestChainObserver, ITestChainProvider
{
    [TestMethod]
    public void Results_AreAlwaysBounded()
    {
        IReadOnlyList<StochRsiResult> sut = Bars.ToStochRsiHub(14, 14, 3, 1).Results;
        sut.IsBetween(static x => x.StochRsi, 0, 100);
        sut.IsBetween(static x => x.Signal, 0, 100);
    }

    [TestMethod]
    public void BarObserver_WithWarmupLateArrivalAndRemoval_MatchesSeriesExactly()
    {
        // setup bar provider hub
        BarHub barHub = new();

        // prefill bars at provider
        barHub.Add(Bars.Take(20));

        // initialize observer
        StochRsiHub observer = barHub.ToStochRsiHub(14, 14, 3, 1);

        // fetch initial results (early)
        IReadOnlyList<StochRsiResult> sut = observer.Results;

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

        IReadOnlyList<StochRsiResult> expectedOriginal = Bars.ToStochRsi(14, 14, 3, 1);
        sut.IsExactly(expectedOriginal);

        // delete, should equal series (revised)
        barHub.RemoveAt(removeAtIndex);

        IReadOnlyList<StochRsiResult> expectedRevised = RevisedBars.ToStochRsi(14, 14, 3, 1);
        sut.IsExactly(expectedRevised);
        sut.Should().HaveCount(barsCount - 1);

        observer.Unsubscribe();
        barHub.EndTransmission();
    }

    [TestMethod]
    public void WithCachePruning_MatchesSeriesExactly()
    {
        const int maxCacheSize = 50;
        const int totalBars = 100;

        IReadOnlyList<Bar> bars = Bars.Take(totalBars).ToList();
        IReadOnlyList<StochRsiResult> expected = bars
            .ToStochRsi(14, 14, 3, 1)
            .TakeLast(maxCacheSize)
            .ToList();

        // Setup with cache limit
        BarHub barHub = new(maxCacheSize);
        StochRsiHub observer = barHub.ToStochRsiHub(14, 14, 3, 1);

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
        const int emaPeriods = 12;
        const int rsiPeriods = 14;
        const int stochPeriods = 14;
        const int signalPeriods = 3;
        const int smoothPeriods = 1;

        List<Bar> barsList = Bars.ToList();

        int length = barsList.Count;

        // setup bar provider hub
        BarHub barHub = new();

        // initialize observer
        StochRsiHub observer = barHub
            .ToEmaHub(emaPeriods)
            .ToStochRsiHub(rsiPeriods, stochPeriods, signalPeriods, smoothPeriods);

        // emulate bar stream
        for (int i = 0; i < length; i++)
        {
            barHub.Add(barsList[i]);
        }

        // final results
        IReadOnlyList<StochRsiResult> streamList
            = observer.Results;

        // time-series, for comparison
        IReadOnlyList<StochRsiResult> seriesList
           = barsList
            .ToEma(emaPeriods)
            .ToStochRsi(rsiPeriods, stochPeriods, signalPeriods, smoothPeriods);

        // assert, should equal series
        streamList.Should().HaveCount(length);
        streamList.IsExactly(seriesList);

        observer.Unsubscribe();
        barHub.EndTransmission();
    }

    [TestMethod]
    public void Provider_IsRsiHub()
    {
        BarHub barHub = new();
        StochRsiHub observer = barHub.ToStochRsiHub(14, 14, 3, 1);

        System.Reflection.PropertyInfo property = typeof(StochRsiHub)
            .GetProperty(
                "Provider",
                System.Reflection.BindingFlags.NonPublic
                | System.Reflection.BindingFlags.Instance
                | System.Reflection.BindingFlags.FlattenHierarchy)!;

        object provider = property.GetValue(observer)!;
        provider.Should().BeOfType<RsiHub>();

        observer.Unsubscribe();
        barHub.EndTransmission();
    }

    [TestMethod]
    public void ChainProvider_MatchesSeriesExactly()
    {
        const int rsiPeriods = 14;
        const int stochPeriods = 14;
        const int signalPeriods = 3;
        const int smoothPeriods = 1;
        const int emaPeriods = 12;

        // setup bar provider hub
        BarHub barHub = new();

        // initialize observer
        EmaHub observer = barHub
            .ToStochRsiHub(rsiPeriods, stochPeriods, signalPeriods, smoothPeriods)
            .ToEmaHub(emaPeriods);

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
            .ToStochRsi(rsiPeriods, stochPeriods, signalPeriods, smoothPeriods)
            .ToEma(emaPeriods);

        // assert, should equal series
        sut.Should().HaveCount(barsCount - 1);
        sut.IsExactly(expected);

        observer.Unsubscribe();
        barHub.EndTransmission();
    }

    [TestMethod]
    public override void ToStringOverride_ReturnsExpectedName()
    {
        StochRsiHub hub = new(new BarHub(), 14, 14, 3, 1);
        hub.ToString().Should().Be("STOCH-RSI(14,14,3,1)");
    }
}
