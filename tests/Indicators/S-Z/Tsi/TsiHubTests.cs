namespace StreamHubs;

[TestClass]
public class TsiHubTests : StreamHubTestBase, ITestChainObserver, ITestChainProvider
{
    private const int lookbackPeriods = 25;
    private const int smoothPeriods = 13;
    private const int signalPeriods = 7;

    [TestMethod]
    public override void ToStringOverride_ReturnsExpectedName()
    {
        // setup bar provider hub
        BarHub barHub = new();

        // initialize observer
        TsiHub observer = barHub.ToTsiHub(lookbackPeriods, smoothPeriods, signalPeriods);

        // test string output
        observer.ToString().Should().Be("TSI(25,13,7)");

        observer.Unsubscribe();
        barHub.EndTransmission();
    }

    [TestMethod]
    public void Results_AreAlwaysBounded()
    {
        IReadOnlyList<TsiResult> sut = Bars.ToTsiHub(25, 13, 7).Results;
        sut.IsBetween(static x => x.Tsi, -100, 100);
        sut.IsBetween(static x => x.Signal, -100, 100);
    }

    [TestMethod]
    public void BarObserver_WithWarmupLateArrivalAndRemoval_MatchesSeriesExactly()
    {
        // setup bar provider hub
        BarHub barHub = new();

        // prefill bars at provider
        barHub.Add(Bars.Take(20));

        // initialize observer
        TsiHub observer = barHub.ToTsiHub(lookbackPeriods, smoothPeriods, signalPeriods);

        // fetch initial results (early)
        IReadOnlyList<TsiResult> sut = observer.Results;

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

        IReadOnlyList<TsiResult> expectedOriginal = Bars.ToTsi(lookbackPeriods, smoothPeriods, signalPeriods);
        sut.IsExactly(expectedOriginal);

        // delete, should equal series (revised)
        barHub.RemoveAt(removeAtIndex);

        IReadOnlyList<TsiResult> expectedRevised = RevisedBars.ToTsi(lookbackPeriods, smoothPeriods, signalPeriods);
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
        IReadOnlyList<TsiResult> expected = bars
            .ToTsi(lookbackPeriods, smoothPeriods, signalPeriods)
            .TakeLast(maxCacheSize)
            .ToList();

        // Setup with cache limit
        BarHub barHub = new(maxCacheSize);
        TsiHub observer = barHub.ToTsiHub(lookbackPeriods, smoothPeriods, signalPeriods);

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
        TsiHub observer = barHub
            .ToEmaHub(emaPeriods)
            .ToTsiHub(lookbackPeriods, smoothPeriods, signalPeriods);

        // emulate bar stream
        for (int i = 0; i < length; i++)
        {
            barHub.Add(barsList[i]);
        }

        // final results
        IReadOnlyList<TsiResult> streamList = observer.Results;

        // time-series, for comparison
        IReadOnlyList<TsiResult> seriesList = barsList
            .ToEma(emaPeriods)
            .ToTsi(lookbackPeriods, smoothPeriods, signalPeriods);

        // assert, should equal series
        streamList.Should().HaveCount(length);
        streamList.IsExactly(seriesList);

        observer.Unsubscribe();
        barHub.EndTransmission();
    }

    [TestMethod]
    public void ChainProvider_MatchesSeriesExactly()
    {
        const int smaPeriods = 10;

        List<Bar> bars = Bars.ToList();
        int length = bars.Count;

        // setup bar provider hub
        BarHub barHub = new();

        // initialize observer
        SmaHub observer = barHub
            .ToTsiHub(lookbackPeriods, smoothPeriods, signalPeriods)
            .ToSmaHub(smaPeriods);

        // emulate adding bars to provider hub
        for (int i = 0; i < length; i++)
        {
            // skip one (add later)
            if (i == 80)
            {
                continue;
            }

            Bar q = bars[i];
            barHub.Add(q);

            // resend duplicate bars
            if (i is > 100 and < 105)
            {
                barHub.Add(q);
            }
        }

        // late arrival
        barHub.Add(bars[80]);

        // delete
        barHub.RemoveAt(removeAtIndex);
        bars.RemoveAt(removeAtIndex);

        // final results
        IReadOnlyList<SmaResult> streamList = observer.Results;

        // time-series, for comparison
        IReadOnlyList<SmaResult> seriesList = bars
            .ToTsi(lookbackPeriods, smoothPeriods, signalPeriods)
            .ToSma(smaPeriods);

        // assert, should equal series
        streamList.Should().HaveCount(length - 1);
        streamList.IsExactly(seriesList);

        observer.Unsubscribe();
        barHub.EndTransmission();
    }
}
