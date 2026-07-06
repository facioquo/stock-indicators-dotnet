namespace StreamHubs;

[TestClass]
public class CmfHubTests : StreamHubTestBase, ITestBarObserver, ITestChainProvider
{
    private const int lookbackPeriods = 20;
    private static readonly IReadOnlyList<CmfResult> expectedOriginal = Bars.ToCmf(lookbackPeriods);

    [TestMethod]
    public void BarObserver_WithWarmupLateArrivalAndRemoval_MatchesSeriesExactly()
    {
        int length = Bars.Count;

        // setup bar provider hub
        BarHub barHub = new();

        // prefill bars at provider (warmup coverage)
        barHub.Add(Bars.Take(25));

        // initialize observer
        CmfHub observer = barHub.ToCmfHub(lookbackPeriods);

        // fetch initial results (early)
        IReadOnlyList<CmfResult> actuals = observer.Results;

        // emulate adding bars to provider hub
        for (int i = 25; i < length; i++)
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

        IReadOnlyList<CmfResult> expectedRevised = RevisedBars.ToCmf(lookbackPeriods);

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
        IReadOnlyList<CmfResult> expected = bars
            .ToCmf(lookbackPeriods)
            .TakeLast(maxCacheSize)
            .ToList();

        // Setup with cache limit
        BarHub barHub = new(maxCacheSize);
        CmfHub observer = barHub.ToCmfHub(lookbackPeriods);

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
        // CMF emits IReusable results (CmfResult implements IReusable with Value = Cmf),
        // so it can act as a chain provider for downstream indicators.

        const int cmfPeriods = 20;
        const int emaPeriods = 10;

        // setup bar provider hub
        BarHub barHub = new();

        // initialize chain: CMF then EMA over its Value
        EmaHub observer = barHub
            .ToCmfHub(cmfPeriods)
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
            .ToCmf(cmfPeriods)
            .ToEma(emaPeriods);

        sut.Should().HaveCount(barsCount - 1);
        sut.IsExactly(expected);

        observer.Unsubscribe();
        barHub.EndTransmission();
    }

    [TestMethod]
    public void Results_AreAlwaysBounded()
    {
        IReadOnlyList<CmfResult> sut = Bars.ToCmfHub(20).Results;
        sut.IsBetween(static x => x.Cmf, -1, 1);
    }

    [TestMethod]
    public override void ToStringOverride_ReturnsExpectedName()
    {
        BarHub barHub = new();

        CmfHub hub = new(barHub, lookbackPeriods);
        hub.ToString().Should().Be($"CMF({lookbackPeriods})");

        barHub.Add(Bars[0]);
        barHub.Add(Bars[1]);

        string s = $"CMF({lookbackPeriods})({Bars[0].Timestamp:d})";
        hub.ToString().Should().Be(s);
    }

    [TestMethod]
    public void RollbackValidation_OnRollback_RestoresState()
    {
        BarHub barHub = new();

        // Precondition: Normal bar stream with 502 expected entries
        CmfHub observer = barHub.ToCmfHub(lookbackPeriods);
        barHub.Add(Bars);

        observer.Results.Should().HaveCount(502);
        observer.Results.IsExactly(expectedOriginal);

        // Act: Remove a single historical value
        barHub.RemoveAt(removeAtIndex);

        // Assert: Observer should have 501 results and match revised series
        IReadOnlyList<CmfResult> expectedRevised = RevisedBars.ToCmf(lookbackPeriods);

        observer.Results.Should().HaveCount(501);
        observer.Results.IsExactly(expectedRevised);

        // cleanup
        observer.Unsubscribe();
        barHub.EndTransmission();
    }
}
