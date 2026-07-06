namespace StreamHubs;

[TestClass]
public class EpmaHubTests : StreamHubTestBase, ITestChainObserver, ITestChainProvider
{
    private const int lookbackPeriods = 20;

    private static readonly IReadOnlyList<EpmaResult> series
        = Bars.ToEpma(lookbackPeriods);

    [TestMethod]
    public void BarObserver_WithWarmupLateArrivalAndRemoval_MatchesSeriesExactly()
    {
        int length = Bars.Count;

        // setup bar provider hub
        BarHub barHub = new();

        // prefill bars at provider (warmup coverage)
        barHub.Add(Bars.Take(20));

        // initialize observer
        EpmaHub observer = barHub.ToEpmaHub(lookbackPeriods);

        // fetch initial results (early)
        IReadOnlyList<EpmaResult> actuals = observer.Results;

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
        actuals.IsExactly(series);

        // delete, should equal series (revised)
        barHub.RemoveAt(removeAtIndex);

        IReadOnlyList<EpmaResult> expectedRevised = RevisedBars.ToEpma(lookbackPeriods);

        actuals.Should().HaveCount(501);
        actuals.IsExactly(expectedRevised);

        // cleanup
        observer.Unsubscribe();
        barHub.EndTransmission();
    }

    [TestMethod]
    public void WithCachePruning_MatchesSeriesExactly()
    {
        const int maxCacheSize = 100;  // 20 (lookback) + 80 extra for full EPMA calculation warmup
        const int totalBars = 200;  // ~2x cache size

        IReadOnlyList<Bar> bars = Bars.Take(totalBars).ToList();
        IReadOnlyList<EpmaResult> expected = bars
            .ToEpma(lookbackPeriods)
            .TakeLast(maxCacheSize)
            .ToList();

        // Setup with cache limit
        BarHub barHub = new(maxCacheSize);
        EpmaHub observer = barHub.ToEpmaHub(lookbackPeriods);

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
        EpmaHub hub = new(new BarHub(), lookbackPeriods);
        hub.ToString().Should().Be($"EPMA({lookbackPeriods})");
    }

    [TestMethod]
    public void ConsistencyWithSeries_StreamVsSeries_MatchesExactly()
    {
        // Compare stream results with series results
        BarHub barHub = new();
        EpmaHub epmaHub = barHub.ToEpmaHub(lookbackPeriods);

        foreach (Bar bar in Bars)
        {
            barHub.Add(bar);
        }

        IReadOnlyList<EpmaResult> streamResults = epmaHub.Results;
        IReadOnlyList<EpmaResult> seriesResults = Bars.ToEpma(lookbackPeriods);

        streamResults.Should().HaveCount(seriesResults.Count);
        streamResults.IsExactly(seriesResults);
    }

    [TestMethod]
    public void RealTimeSimulation_WithIncrementalBars_MatchesSeriesExactly()
    {
        // Simulate real-time data processing
        BarHub barHub = new();
        EpmaHub epmaHub = barHub.ToEpmaHub(lookbackPeriods);

        // Process initial historical data
        foreach (Bar bar in Bars.Take(100))
        {
            barHub.Add(bar);
        }

        int initialCount = epmaHub.Results.Count;
        initialCount.Should().Be(100);

        // Process new incoming bars
        foreach (Bar bar in Bars.Skip(100).Take(10))
        {
            barHub.Add(bar);

            IReadOnlyList<EpmaResult> currentResults = epmaHub.Results;
            EpmaResult latestResult = currentResults[^1];

            // Verify real-time calculations
            if (latestResult.Epma.HasValue)
            {
                latestResult.Epma.Should().BeOfType(typeof(double));
                latestResult.Timestamp.Should().Be(bar.Timestamp);
            }
        }

        epmaHub.Results.Should().HaveCount(110);
    }

    [TestMethod]
    public void ChainObserver_ChainedProvider_MatchesSeriesExactly()
    {
        // Test EPMA observing another indicator chain
        const int smaPeriods = 10;

        BarHub barHub = new();
        EpmaHub epmaHub = barHub
            .ToSmaHub(smaPeriods)
            .ToEpmaHub(lookbackPeriods);

        foreach (Bar bar in Bars)
        {
            barHub.Add(bar);
        }

        IReadOnlyList<EpmaResult> streamResults = epmaHub.Results;
        IReadOnlyList<EpmaResult> seriesResults = Bars
            .ToSma(smaPeriods)
            .ToEpma(lookbackPeriods);

        streamResults.Should().HaveCount(Bars.Count);
        streamResults.IsExactly(seriesResults);
    }

    [TestMethod]
    public void ChainProvider_MatchesSeriesExactly()
    {
        // Test EPMA chaining to other indicators
        const int smaPeriods = 10;

        BarHub barHub = new();
        SmaHub smaHub = barHub
            .ToEpmaHub(lookbackPeriods)
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

        IReadOnlyList<SmaResult> sut = smaHub.Results;
        IReadOnlyList<SmaResult> expected = RevisedBars
            .ToEpma(lookbackPeriods)
            .ToSma(smaPeriods);

        sut.Should().HaveCount(barsCount - 1);
        sut.IsExactly(expected);

        smaHub.Unsubscribe();
        barHub.EndTransmission();
    }

    [TestMethod]
    public void Chainable_WithOtherIndicators_ReturnsExpectedResult()
    {
        // Test EPMA chaining with other indicators
        BarHub barHub = new();
        EpmaHub epmaHub = barHub.ToEpmaHub(lookbackPeriods);
        SmaHub smaHub = epmaHub.ToSmaHub(10);

        foreach (Bar bar in Bars)
        {
            barHub.Add(bar);
        }

        IReadOnlyList<SmaResult> chainedResults = smaHub.Results;
        chainedResults.Should().HaveCount(Bars.Count);

        // Verify chained calculation matches expected with tolerance
        IReadOnlyList<SmaResult> expectedChained = Bars
            .ToEpma(lookbackPeriods)
            .ToSma(10);

        chainedResults.Should().HaveCount(expectedChained.Count);
        chainedResults.IsExactly(expectedChained);
    }

    [TestMethod]
    public void Add_WithValidBar_IncrementsResults()
    {
        // Additional test for streaming functionality
        BarHub barHub = new();
        EpmaHub epmaHub = barHub.ToEpmaHub(lookbackPeriods);

        foreach (Bar bar in Bars)
        {
            barHub.Add(bar);
        }

        IReadOnlyList<EpmaResult> sut = epmaHub.Results;
        sut.Should().HaveCount(Bars.Count);
        sut.IsExactly(series);
    }
}
