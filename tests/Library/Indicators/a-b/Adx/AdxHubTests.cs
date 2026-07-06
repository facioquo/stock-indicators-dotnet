namespace StreamHubs;

[TestClass]
public class AdxHubTests : StreamHubTestBase, ITestBarObserver, ITestChainProvider
{
    private const int lookbackPeriods = 14;
    private static readonly IReadOnlyList<AdxResult> expectedOriginal = Bars.ToAdx(lookbackPeriods);

    [TestMethod]
    public void BarObserver_WithWarmupLateArrivalAndRemoval_MatchesSeriesExactly()
    {
        int length = Bars.Count;

        // setup bar provider hub
        BarHub barHub = new();

        // prefill bars at provider (warmup coverage)
        barHub.Add(Bars.Take(20));

        // initialize observer
        AdxHub observer = barHub.ToAdxHub(lookbackPeriods);

        // fetch initial results (early)
        IReadOnlyList<AdxResult> actuals = observer.Results;

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

        actuals.Should().HaveCount(length);
        actuals.IsExactly(expectedOriginal);

        // delete, should equal series (revised)
        barHub.RemoveAt(removeAtIndex);

        IReadOnlyList<AdxResult> expectedRevised = RevisedBars.ToAdx(lookbackPeriods);

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
        IReadOnlyList<AdxResult> expected = bars
            .ToAdx(lookbackPeriods)
            .TakeLast(maxCacheSize)
            .ToList();

        // Setup with cache limit
        BarHub barHub = new(maxCacheSize);
        AdxHub observer = barHub.ToAdxHub(lookbackPeriods);

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
        // ADX emits IReusable results (AdxResult implements IReusable with Value = Adx),
        // so it can act as a chain provider for downstream indicators.

        const int adxPeriods = 14;
        const int emaPeriods = 10;

        // setup bar provider hub
        BarHub barHub = new();

        // initialize chain: ADX then EMA over its Value
        EmaHub observer = barHub
            .ToAdxHub(adxPeriods)
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

        // results from stream
        IReadOnlyList<EmaResult> sut = observer.Results;

        // time-series parity (revised)
        IReadOnlyList<EmaResult> expected = RevisedBars
            .ToAdx(adxPeriods)
            .ToEma(emaPeriods);

        sut.Should().HaveCount(barsCount - 1);
        sut.IsExactly(expected);

        observer.Unsubscribe();
        barHub.EndTransmission();
    }

    [TestMethod]
    public override void ToStringOverride_ReturnsExpectedName()
    {
        BarHub barHub = new();

        AdxHub hub = new(barHub, 14);
        hub.ToString().Should().Be("ADX(14)");

        barHub.Add(Bars[0]);
        barHub.Add(Bars[1]);

        string s = $"ADX(14)({Bars[0].Timestamp:d})";
        hub.ToString().Should().Be(s);
    }

    [TestMethod]
    public void LateArrival_MidStream_MatchesFreshStream()
    {
        const int totalBars = 300;
        const int lateIndex = 150;

        IReadOnlyList<Bar> bars = Bars.Take(totalBars).ToList();

        BarHub lateSource = new();
        AdxHub lateHub = lateSource.ToAdxHub(lookbackPeriods);
        for (int i = 0; i < totalBars; i++)
        {
            if (i == lateIndex) { continue; }

            lateSource.Add(bars[i]);
        }

        lateSource.Add(bars[lateIndex]);

        BarHub freshSource = new();
        AdxHub freshHub = freshSource.ToAdxHub(lookbackPeriods);
        freshSource.Add(bars);

        lateHub.Results.Should().HaveCount(totalBars);
        lateHub.Results.IsExactly(freshHub.Results);

        lateHub.Unsubscribe();
        freshHub.Unsubscribe();
        lateSource.EndTransmission();
        freshSource.EndTransmission();
    }

    [TestMethod]
    public void LateArrival_AtDmiWarmupBoundary_MatchesFreshStream()
    {
        // ADX requires 2 * lookback periods (= 28) before the first
        // non-null ADX value; the late arrival at index 33 forces the
        // rollback path to replay across the SMMA / Wilder smoothing
        // transition that gates DI+, DI-, and ADX emission.
        const int totalBars = 300;
        const int lateIndex = 33;

        IReadOnlyList<Bar> bars = Bars.Take(totalBars).ToList();

        BarHub lateSource = new();
        AdxHub lateHub = lateSource.ToAdxHub(lookbackPeriods);
        for (int i = 0; i < totalBars; i++)
        {
            if (i == lateIndex) { continue; }

            lateSource.Add(bars[i]);
        }

        lateSource.Add(bars[lateIndex]);

        BarHub freshSource = new();
        AdxHub freshHub = freshSource.ToAdxHub(lookbackPeriods);
        freshSource.Add(bars);

        lateHub.Results.Should().HaveCount(totalBars);
        lateHub.Results.IsExactly(freshHub.Results);

        lateHub.Unsubscribe();
        freshHub.Unsubscribe();
        lateSource.EndTransmission();
        freshSource.EndTransmission();
    }

    [TestMethod]
    public void RollbackValidation_OnRollback_RestoresState()
    {
        BarHub barHub = new();

        // Precondition: Normal bar stream with 502 expected entries
        AdxHub observer = barHub.ToAdxHub(lookbackPeriods);
        barHub.Add(Bars);

        observer.Results.Should().HaveCount(502);
        observer.Results.IsExactly(expectedOriginal);

        // Act: Remove a single historical value
        barHub.RemoveAt(removeAtIndex);

        // Assert: Observer should have 501 results and match revised series
        IReadOnlyList<AdxResult> expectedRevised = RevisedBars.ToAdx(lookbackPeriods);

        observer.Results.Should().HaveCount(501);
        observer.Results.IsExactly(expectedRevised);

        // cleanup
        observer.Unsubscribe();
        barHub.EndTransmission();
    }
}
