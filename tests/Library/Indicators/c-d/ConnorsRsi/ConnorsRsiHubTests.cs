namespace StreamHubs;

[TestClass]
public class ConnorsRsiHubTests : StreamHubTestBase, ITestChainObserver, ITestChainProvider
{
    private const int rsiPeriods = 3;
    private const int streakPeriods = 2;
    private const int rankPeriods = 100;

    [TestMethod]
    public override void ToStringOverride_ReturnsExpectedName()
    {
        ConnorsRsiHub hub = Bars.ToConnorsRsiHub(rsiPeriods, streakPeriods, rankPeriods);
        string actual = hub.ToString();
        string expected = $"CRSI({rsiPeriods},{streakPeriods},{rankPeriods})";

        actual.Should().Be(expected);
    }

    [TestMethod]
    public void Results_AreAlwaysBounded()
    {
        IReadOnlyList<ConnorsRsiResult> sut = Bars.ToConnorsRsiHub(3, 2, 100).Results;
        sut.IsBetween(static x => x.ConnorsRsi, 0, 100);
    }

    [TestMethod]
    public void Boundary_WithRandomBars_StaysWithinBounds()
    {
        IReadOnlyList<ConnorsRsiResult> sut = Data
            .GetRandom(2500)
            .ToConnorsRsiHub(3, 2, 100)
            .Results;

        sut.IsBetween(static x => x.ConnorsRsi, 0d, 100d);
    }

    [TestMethod]
    public void BarObserver_WithWarmupLateArrivalAndRemoval_MatchesSeriesExactly()
    {
        // setup bar provider hub
        BarHub barHub = new();

        // prefill bars at provider
        for (int i = 0; i < 20; i++)
        {
            barHub.Add(Bars[i]);
        }

        // initialize observer
        ConnorsRsiHub observer = barHub
            .ToConnorsRsiHub(rsiPeriods, streakPeriods, rankPeriods);

        // fetch initial results (early)
        IReadOnlyList<ConnorsRsiResult> actuals
            = observer.Results;

        // emulate adding bars to provider hub
        for (int i = 20; i < barsCount; i++)
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
        IReadOnlyList<ConnorsRsiResult> expected = RevisedBars.ToConnorsRsi(rsiPeriods, streakPeriods, rankPeriods);

        // assert, should equal series
        actuals.Should().HaveCount(barsCount - 1);
        actuals.IsExactly(expected);

        observer.Unsubscribe();
        barHub.EndTransmission();
    }

    [TestMethod]
    public void WithCachePruning_MatchesSeriesExactly()
    {
        const int maxCacheSize = 110;  // 3*2 (rsi periods) + 100 (rank periods) + extra
        const int totalBars = 220;  // ~2x cache size

        IReadOnlyList<Bar> bars = Bars.Take(totalBars).ToList();
        IReadOnlyList<ConnorsRsiResult> expected = bars
            .ToConnorsRsi(rsiPeriods, streakPeriods, rankPeriods)
            .TakeLast(maxCacheSize)
            .ToList();

        // Setup with cache limit
        BarHub barHub = new(maxCacheSize);
        ConnorsRsiHub observer = barHub.ToConnorsRsiHub(rsiPeriods, streakPeriods, rankPeriods);

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

        List<Bar> barsList = Bars.ToList();

        int length = barsList.Count;

        // setup bar provider hub
        BarHub barHub = new();

        // initialize observer
        ConnorsRsiHub observer = barHub
            .ToEmaHub(emaPeriods)
            .ToConnorsRsiHub(rsiPeriods, streakPeriods, rankPeriods);

        // emulate bar stream
        for (int i = 0; i < length; i++)
        {
            barHub.Add(barsList[i]);
        }

        // final results
        IReadOnlyList<ConnorsRsiResult> streamList
            = observer.Results;

        // time-series, for comparison
        IReadOnlyList<ConnorsRsiResult> seriesList
           = barsList
            .ToEma(emaPeriods)
            .ToConnorsRsi(rsiPeriods, streakPeriods, rankPeriods);

        // assert, should equal series
        streamList.Should().HaveCount(length);
        streamList.IsExactly(seriesList);

        observer.Unsubscribe();
        barHub.EndTransmission();
    }

    [TestMethod]
    public void ChainProvider_MatchesSeriesExactly()
    {
        const int emaPeriods = 12;

        // setup bar provider hub
        BarHub barHub = new();

        // initialize observer
        EmaHub observer = barHub
            .ToConnorsRsiHub(rsiPeriods, streakPeriods, rankPeriods)
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
            .ToConnorsRsi(rsiPeriods, streakPeriods, rankPeriods)
            .ToEma(emaPeriods);

        // assert, should equal series
        sut.Should().HaveCount(barsCount - 1);
        sut.IsExactly(expected);

        observer.Unsubscribe();
        barHub.EndTransmission();
    }
}
