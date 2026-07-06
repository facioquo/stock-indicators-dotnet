namespace StreamHubs;

[TestClass]
public class MaEnvelopesHubTests : StreamHubTestBase, ITestChainObserver
{
    private const int lookbackPeriods = 20;
    private const double percentOffset = 2.5;
    private readonly IReadOnlyList<MaEnvelopeResult> expectedOriginal = Bars.ToMaEnvelopes(lookbackPeriods, percentOffset);

    [TestMethod]
    public void BarObserver_WithWarmupLateArrivalAndRemoval_MatchesSeriesExactly()
    {
        int length = Bars.Count;

        // setup bar provider hub
        BarHub barHub = new();

        // prefill bars at provider
        barHub.Add(Bars.Take(20));

        // initialize observer
        MaEnvelopesHub observer = barHub.ToMaEnvelopesHub(lookbackPeriods, percentOffset);

        // fetch initial results (early)
        IReadOnlyList<MaEnvelopeResult> actuals = observer.Results;

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

        IReadOnlyList<MaEnvelopeResult> expectedRevised = RevisedBars.ToMaEnvelopes(lookbackPeriods, percentOffset);

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
        IReadOnlyList<MaEnvelopeResult> expected = bars
            .ToMaEnvelopes(lookbackPeriods, percentOffset)
            .TakeLast(maxCacheSize)
            .ToList();

        // Setup with cache limit
        BarHub barHub = new(maxCacheSize);
        MaEnvelopesHub observer = barHub.ToMaEnvelopesHub(lookbackPeriods, percentOffset);

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
        const int smaPeriods = 12;
        const int maPeriods = 20;
        int length = Bars.Count;

        // setup bar provider hub
        BarHub barHub = new();

        // initialize observer
        MaEnvelopesHub observer = barHub
            .ToSmaHub(smaPeriods)
            .ToMaEnvelopesHub(maPeriods, percentOffset);

        // emulate bar stream
        for (int i = 0; i < length; i++) { barHub.Add(Bars[i]); }

        // final results
        IReadOnlyList<MaEnvelopeResult> actuals = observer.Results;

        // time-series, for comparison
        IReadOnlyList<MaEnvelopeResult> expected = Bars
            .ToSma(smaPeriods)
            .ToMaEnvelopes(maPeriods, percentOffset);

        // assert, should equal series
        actuals.Should().HaveCount(length);
        actuals.IsExactly(expected);

        // cleanup
        observer.Unsubscribe();
        barHub.EndTransmission();
    }

    [TestMethod]
    public void WithDemaType_AsMaType_MatchesSeriesExactly()
    {
        int length = Bars.Count;

        // setup bar provider hub
        BarHub barHub = new();

        // initialize observer with DEMA type
        MaEnvelopesHub observer = barHub.ToMaEnvelopesHub(lookbackPeriods, percentOffset, MaType.DEMA);

        // emulate bar stream
        for (int i = 0; i < length; i++) { barHub.Add(Bars[i]); }

        // final results
        IReadOnlyList<MaEnvelopeResult> actuals = observer.Results;

        // time-series, for comparison
        IReadOnlyList<MaEnvelopeResult> expected = Bars.ToMaEnvelopes(lookbackPeriods, percentOffset, MaType.DEMA);

        // assert, should equal series
        actuals.Should().HaveCount(length);
        actuals.IsExactly(expected);

        // cleanup
        observer.Unsubscribe();
        barHub.EndTransmission();
    }

    public override void ToStringOverride_ReturnsExpectedName()
    {
        MaEnvelopesHub hub = new(new BarHub(), lookbackPeriods, percentOffset, MaType.SMA);
        hub.ToString().Should().Be($"MAENV({lookbackPeriods},{percentOffset},{MaType.SMA})");
    }
}
