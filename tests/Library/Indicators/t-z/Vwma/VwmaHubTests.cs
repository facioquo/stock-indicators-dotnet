namespace StreamHubs;

[TestClass]
public class VwmaHubTests : StreamHubTestBase, ITestBarObserver, ITestChainProvider
{
    private const int lookbackPeriods = 10;
    private readonly IReadOnlyList<VwmaResult> expectedOriginal = Bars.ToVwma(lookbackPeriods);

    [TestMethod]
    public void BarObserver_WithWarmupLateArrivalAndRemoval_MatchesSeriesExactly()
    {
        int length = Bars.Count;

        // setup bar provider hub
        BarHub barHub = new();

        // prefill bars at provider
        barHub.Add(Bars.Take(20));

        // initialize observer
        VwmaHub observer = barHub.ToVwmaHub(lookbackPeriods);

        // fetch initial results (early)
        IReadOnlyList<VwmaResult> actuals = observer.Results;

        // emulate adding bars to provider hub
        for (int i = 20; i < length; i++)
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
        actuals.IsExactly(expectedOriginal);

        // delete, should equal series (revised)
        barHub.RemoveAt(removeAtIndex);

        IReadOnlyList<VwmaResult> expectedRevised = RevisedBars.ToVwma(lookbackPeriods);

        actuals.Should().HaveCount(501);
        actuals.IsExactly(expectedRevised);

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
        IReadOnlyList<VwmaResult> expected = bars
            .ToVwma(lookbackPeriods)
            .TakeLast(maxCacheSize)
            .ToList();

        // Setup with cache limit
        BarHub barHub = new(maxCacheSize);
        VwmaHub observer = barHub.ToVwmaHub(lookbackPeriods);

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
    public void ChainProvider_MatchesSeriesExactly()
    {
        const int vwmaPeriods = 20;
        const int smaPeriods = 10;
        int length = Bars.Count;

        // setup bar provider hub
        BarHub barHub = new();

        // initialize observer
        SmaHub observer = barHub
            .ToVwmaHub(vwmaPeriods)
            .ToSmaHub(smaPeriods);

        // emulate adding bars to provider hub
        for (int i = 0; i < length; i++)
        {
            // skip one (add later)
            if (i == 80) { continue; }

            Bar q = Bars[i];
            barHub.Add(q);

            // resend duplicate bars
            if (i is > 100 and < 105) { barHub.Add(q); }
        }

        // late arrival
        barHub.Add(Bars[80]);

        // delete
        barHub.RemoveAt(removeAtIndex);

        // final results
        IReadOnlyList<SmaResult> actuals
            = observer.Results;

        // time-series, for comparison (revised)
        IReadOnlyList<SmaResult> seriesList = RevisedBars
            .ToVwma(vwmaPeriods)
            .ToSma(smaPeriods);

        // assert, should equal series
        actuals.Should().HaveCount(length - 1);
        actuals.IsExactly(seriesList);

        // cleanup
        observer.Unsubscribe();
        barHub.EndTransmission();
    }

    [TestMethod]
    public override void ToStringOverride_ReturnsExpectedName()
    {
        BarHub barHub = new();
        VwmaHub observer = barHub.ToVwmaHub(lookbackPeriods);

        observer.ToString().Should().Be($"VWMA({lookbackPeriods})");
    }

    [TestMethod]
    public void EmptyProvider_NoBars_ReturnsEmptyResults()
    {
        BarHub barHub = new();
        VwmaHub observer = barHub.ToVwmaHub(lookbackPeriods);

        IReadOnlyList<VwmaResult> sut = observer.Results;
        sut.Should().BeEmpty();
    }

    [TestMethod]
    public void InsufficientBars_WithFewerThanLookback_ProducesNullResults()
    {
        BarHub barHub = new();
        VwmaHub observer = barHub.ToVwmaHub(lookbackPeriods);

        // Add fewer bars than required
        for (int i = 0; i < lookbackPeriods - 1; i++)
        {
            barHub.Add(Bars[i]);
        }

        IReadOnlyList<VwmaResult> sut = observer.Results;
        sut.Should().HaveCount(lookbackPeriods - 1);
        sut.All(static r => r.Vwma == null).Should().BeTrue();
    }

    [TestMethod]
    public void ZeroVolume_WithZeroVolumeBars_HandlesGracefully()
    {
        BarHub barHub = new();
        VwmaHub observer = barHub.ToVwmaHub(lookbackPeriods);

        // Create bars with zero volume
        List<Bar> zeroVolumeBars = Bars.Take(20).Select(static q => new Bar(
            Timestamp: q.Timestamp,
            Open: q.Open,
            High: q.High,
            Low: q.Low,
            Close: q.Close,
            Volume: 0
        )).ToList();

        foreach (Bar bar in zeroVolumeBars)
        {
            barHub.Add(bar);
        }

        IReadOnlyList<VwmaResult> sut = observer.Results;

        // Results with sufficient data but zero volume should have null VWMA
        sut.Skip(lookbackPeriods - 1).All(static r => r.Vwma == null).Should().BeTrue();
    }
}
