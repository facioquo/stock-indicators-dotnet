namespace StreamHubs;

[TestClass]
public class VwapHubTests : StreamHubTestBase, ITestBarObserver, ITestChainProvider
{
    [TestMethod]
    public void BarObserver_WithWarmupLateArrivalAndRemoval_MatchesSeriesExactly()
    {
        // setup bar provider hub
        BarHub barHub = new();

        // prefill bars at provider
        barHub.Add(Bars.Take(20));

        // initialize observer
        VwapHub observer = barHub.ToVwapHub();

        // fetch initial results (early)
        IReadOnlyList<VwapResult> sut = observer.Results;

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

        IReadOnlyList<VwapResult> expectedOriginal = Bars.ToVwap();
        sut.IsExactly(expectedOriginal);

        // delete, should equal series (revised)
        barHub.RemoveAt(removeAtIndex);

        IReadOnlyList<VwapResult> expectedRevised = RevisedBars.ToVwap();
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
        IReadOnlyList<VwapResult> expected = bars
            .ToVwap()
            .TakeLast(maxCacheSize)
            .ToList();

        // Setup with cache limit
        BarHub barHub = new(maxCacheSize);
        VwapHub observer = barHub.ToVwapHub();

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
    public void BarObserver_WithWarmupLateArrivalAndRemoval_MatchesSeriesExactlyWithStartDate()
    {
        List<Bar> bars = Bars.ToList();
        int length = bars.Count;

        // Use a start date somewhere in the middle
        DateTime startDate = bars[100].Timestamp;

        // setup bar provider hub
        BarHub barHub = new();

        // prefill bars at provider
        for (int i = 0; i < 20; i++)
        {
            barHub.Add(bars[i]);
        }

        // initialize observer with start date
        VwapHub observer = barHub.ToVwapHub(startDate);

        // emulate adding bars to provider hub
        for (int i = 20; i < length; i++)
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

        // removal
        barHub.RemoveAt(removeAtIndex);
        bars.RemoveAt(removeAtIndex);

        // final results
        IReadOnlyList<VwapResult> seriesList = bars.ToVwap(startDate);

        observer.Results.Should().HaveCount(length - 1);
        observer.Results.IsExactly(seriesList);

        // cleanup
        observer.Unsubscribe();
        barHub.EndTransmission();
    }

    [TestMethod]
    public void ChainProvider_MatchesSeriesExactly()
    {
        const int smaPeriods = 8;

        // setup bar provider hub
        BarHub barHub = new();

        // initialize observer
        VwapHub vwapHub = barHub.ToVwapHub();

        SmaHub observer = vwapHub
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
            .ToVwap()
            .ToSma(smaPeriods);

        // assert, should equal series
        sut.Should().HaveCount(barsCount - 1);
        sut.IsExactly(expected);

        // cleanup
        observer.Unsubscribe();
        barHub.EndTransmission();
    }

    [TestMethod]
    public override void ToStringOverride_ReturnsExpectedName()
    {
        BarHub barHub = new();

        VwapHub hub = new(barHub);
        hub.ToString().Should().Be("VWAP");

        barHub.Add(Bars[0]);
        barHub.Add(Bars[1]);

        string s = $"VWAP({Bars[0].Timestamp:d})";
        hub.ToString().Should().Be(s);
    }

    [TestMethod]
    public void CustomToStringWithStartDate_OnInstantiation_FormatsCorrectly()
    {
        BarHub barHub = new();
        DateTime startDate = new(2018, 1, 15);

        VwapHub hub = new(barHub, startDate);
        string expected = $"VWAP({startDate:d})";
        hub.ToString().Should().Be(expected);
    }
}
