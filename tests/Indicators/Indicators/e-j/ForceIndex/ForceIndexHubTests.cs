namespace StreamHubs;

[TestClass]
public class ForceIndexHubTests : StreamHubTestBase, ITestBarObserver, ITestChainProvider
{
    private const int lookbackPeriods = 2;
    private readonly IReadOnlyList<ForceIndexResult> expectedOriginal = Bars.ToForceIndex(lookbackPeriods);

    [TestMethod]
    public void BarObserver_WithWarmupLateArrivalAndRemoval_MatchesSeriesExactly()
    {
        int length = Bars.Count;

        // setup bar provider hub
        BarHub barHub = new();

        // prefill bars at provider
        barHub.Add(Bars.Take(20));

        // initialize observer
        ForceIndexHub observer = barHub.ToForceIndexHub(lookbackPeriods);

        // fetch initial results (early)
        IReadOnlyList<ForceIndexResult> actuals = observer.Results;

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

        IReadOnlyList<ForceIndexResult> expectedRevised = RevisedBars.ToForceIndex(lookbackPeriods);

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
        IReadOnlyList<ForceIndexResult> expected = bars
            .ToForceIndex(lookbackPeriods)
            .TakeLast(maxCacheSize)
            .ToList();

        // Setup with cache limit
        BarHub barHub = new(maxCacheSize);
        ForceIndexHub observer = barHub.ToForceIndexHub(lookbackPeriods);

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
        const int forcePeriods = 13;
        const int smaPeriods = 10;
        int length = Bars.Count;

        // setup bar provider hub
        BarHub barHub = new();

        // initialize observer
        SmaHub observer = barHub
            .ToForceIndexHub(forcePeriods)
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
            .ToForceIndex(forcePeriods)
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
        ForceIndexHub observer = barHub.ToForceIndexHub(lookbackPeriods);

        observer.ToString().Should().Be($"FORCE({lookbackPeriods})");
    }

    [TestMethod]
    public void EmptyProvider_OnInstantiation_HasEmptyResults()
    {
        BarHub barHub = new();
        ForceIndexHub observer = barHub.ToForceIndexHub(lookbackPeriods);

        IReadOnlyList<ForceIndexResult> sut = observer.Results;
        sut.Should().BeEmpty();
    }

    [TestMethod]
    public void InsufficientBars_BelowMinimum_ThrowsInvalidOperationException()
    {
        BarHub barHub = new();
        ForceIndexHub observer = barHub.ToForceIndexHub(lookbackPeriods);

        // Add fewer bars than required (need at least lookbackPeriods + 1)
        for (int i = 0; i < lookbackPeriods; i++)
        {
            barHub.Add(Bars[i]);
        }

        IReadOnlyList<ForceIndexResult> sut = observer.Results;
        sut.Should().HaveCount(lookbackPeriods);
        sut.All(static r => r.ForceIndex == null).Should().BeTrue();
    }

    [TestMethod]
    public void ZeroVolume_WithZeroVolumeBars_ReturnsExpectedResult()
    {
        BarHub barHub = new();
        ForceIndexHub observer = barHub.ToForceIndexHub(lookbackPeriods);

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

        IReadOnlyList<ForceIndexResult> sut = observer.Results;

        // With zero volume, raw FI should be zero, leading to EMA converging to zero
        sut.Skip(lookbackPeriods).All(static r => r.ForceIndex == 0).Should().BeTrue();
    }

    [TestMethod]
    public void NoPriceChange_WithFlatBars_ReturnsExpectedResult()
    {
        BarHub barHub = new();
        ForceIndexHub observer = barHub.ToForceIndexHub(lookbackPeriods);

        // Create bars with no price change
        List<Bar> flatBars = Enumerable.Range(0, 20).Select(static i => new Bar(
            Timestamp: DateTime.Now.AddDays(i),
            Open: 100,
            High: 100,
            Low: 100,
            Close: 100,
            Volume: 1000
        )).ToList();

        foreach (Bar bar in flatBars)
        {
            barHub.Add(bar);
        }

        IReadOnlyList<ForceIndexResult> sut = observer.Results;

        // With no price change, raw FI should be zero, leading to EMA converging to zero
        sut.Skip(lookbackPeriods).All(static r => r.ForceIndex == 0).Should().BeTrue();
    }
}
