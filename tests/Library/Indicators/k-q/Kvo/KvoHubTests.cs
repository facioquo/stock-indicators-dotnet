namespace StreamHubs;

[TestClass]
public class KvoHubTests : StreamHubTestBase, ITestBarObserver, ITestChainProvider
{
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
        KvoHub observer = barHub.ToKvoHub(34, 55, 13);

        // fetch initial results (early)
        IReadOnlyList<KvoResult> actuals = observer.Results;

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

        // removal
        barHub.RemoveAt(removeAtIndex);

        // final results
        IReadOnlyList<KvoResult> expected = RevisedBars.ToKvo(34, 55, 13);

        actuals.Should().HaveCount(barsCount - 1);
        actuals.IsExactly(expected);

        // cleanup
        observer.Unsubscribe();
        barHub.EndTransmission();
    }

    [TestMethod]
    public void WithCachePruning_MatchesSeriesExactly()
    {
        const int maxCacheSize = 70;  // 55 (longest period) + 15 extra
        const int totalBars = 140;  // ~2x cache size

        IReadOnlyList<Bar> bars = Bars.Take(totalBars).ToList();
        IReadOnlyList<KvoResult> expected = bars
            .ToKvo(34, 55, 13)
            .TakeLast(maxCacheSize)
            .ToList();

        // Setup with cache limit
        BarHub barHub = new(maxCacheSize);
        KvoHub observer = barHub.ToKvoHub(34, 55, 13);

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
    public override void ToStringOverride_ReturnsExpectedName()
    {
        KvoHub hub = new(new BarHub(), 34, 55, 13);
        hub.ToString().Should().Be("KVO(34,55,13)");
    }

    [TestMethod]
    public void Standard_WithStandardBars_ReturnsExpectedResult()
    {
        int length = Bars.Count;

        // setup bar provider hub
        BarHub barHub = new();

        // initialize observer
        KvoHub observer = barHub.ToKvoHub(34, 55, 13);

        // emulate bar stream
        foreach (Bar q in Bars)
        {
            barHub.Add(q);
        }

        // final results
        IReadOnlyList<KvoResult> streamList = observer.Results;

        // time-series, for comparison
        IReadOnlyList<KvoResult> seriesList = Bars.ToKvo(34, 55, 13);

        streamList.Should().HaveCount(length);
        streamList.IsExactly(seriesList);

        // cleanup
        observer.Unsubscribe();
        barHub.EndTransmission();
    }

    [TestMethod]
    public void Variants_WithAllConfigurations_ReturnsExpectedResult()
    {
        int length = Bars.Count;

        // setup bar provider hub
        BarHub barHub = new();

        // initialize observer with different parameters
        KvoHub observer = barHub.ToKvoHub(20, 40, 10);

        // emulate bar stream
        foreach (Bar q in Bars)
        {
            barHub.Add(q);
        }

        // final results
        IReadOnlyList<KvoResult> streamList = observer.Results;

        // time-series, for comparison
        IReadOnlyList<KvoResult> seriesList = Bars.ToKvo(20, 40, 10);

        streamList.Should().HaveCount(length);
        streamList.IsExactly(seriesList);

        // cleanup
        observer.Unsubscribe();
        barHub.EndTransmission();
    }

    [TestMethod]
    public void ChainProvider_MatchesSeriesExactly()
    {
        const int kvoFast = 34;
        const int kvoSlow = 55;
        const int kvoSignal = 13;
        const int smaPeriods = 10;

        // setup bar provider hub
        BarHub barHub = new();

        // initialize observer - chain KVO to SMA
        SmaHub observer = barHub
            .ToKvoHub(kvoFast, kvoSlow, kvoSignal)
            .ToSmaHub(smaPeriods);

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
        IReadOnlyList<SmaResult> sut = observer.Results;

        // time-series, for comparison (revised)
        IReadOnlyList<SmaResult> expected = RevisedBars
            .ToKvo(kvoFast, kvoSlow, kvoSignal)
            .ToSma(smaPeriods);

        // assert, should equal series
        sut.Should().HaveCount(barsCount - 1);
        sut.IsExactly(expected);

        // cleanup
        observer.Unsubscribe();
        barHub.EndTransmission();
    }

    [TestMethod]
    public void BadData_WithInvalidValues_DoesNotFail()
    {
        // setup bar provider hub
        BarHub barHub = new();

        // bad fast periods
        Action act = () => barHub.ToKvoHub(2, 55, 13);
        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("fastPeriods");

        // bad slow periods (less than fast)
        act = () => barHub.ToKvoHub(34, 30, 13);
        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("slowPeriods");

        // bad signal periods
        act = () => barHub.ToKvoHub(34, 55, 0);
        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("signalPeriods");

        barHub.EndTransmission();
    }
}
