namespace StreamHubs;

[TestClass]
public class AroonHubTests : StreamHubTestBase, ITestBarObserver, ITestChainProvider
{
    [TestMethod]
    public void Results_AreAlwaysBounded()
    {
        IReadOnlyList<AroonResult> sut = Bars.ToAroonHub(25).Results;
        sut.IsBetween(static x => x.AroonUp, 0, 100);
        sut.IsBetween(static x => x.AroonDown, 0, 100);
        sut.IsBetween(static x => x.Oscillator, -100, 100);
    }

    [TestMethod]
    public void Boundary_WithRandomBars_StaysWithinBounds()
    {
        IReadOnlyList<AroonResult> sut = Data
            .GetRandom(2500)
            .ToAroonHub(25)
            .Results;

        sut.IsBetween(static x => x.AroonUp, 0d, 100d);
        sut.IsBetween(static x => x.AroonDown, 0d, 100d);
        sut.IsBetween(static x => x.Oscillator, -100d, 100d);
    }

    [TestMethod]
    public void BarObserver_WithWarmupLateArrivalAndRemoval_MatchesSeriesExactly()
    {
        // setup bar provider
        BarHub barHub = new();

        // prefill bars to provider
        for (int i = 0; i < 30; i++)
        {
            barHub.Add(Bars[i]);
        }

        // initialize observer
        AroonHub aroonHub = barHub
            .ToAroonHub(25);

        // fetch initial results (early)
        IReadOnlyList<AroonResult> actuals
            = aroonHub.Results;

        // emulate adding bars to provider
        for (int i = 30; i < barsCount; i++)
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
        IReadOnlyList<AroonResult> expected = RevisedBars.ToAroon(25);

        // assert, should equal series
        actuals.Should().HaveCount(barsCount - 1);
        actuals.IsExactly(expected);

        aroonHub.Unsubscribe();
        barHub.EndTransmission();
    }

    [TestMethod]
    public void WithCachePruning_MatchesSeriesExactly()
    {
        const int maxCacheSize = 50;
        const int totalBars = 100;

        IReadOnlyList<Bar> bars = Bars.Take(totalBars).ToList();
        IReadOnlyList<AroonResult> expected = bars
            .ToAroon(25)
            .TakeLast(maxCacheSize)
            .ToList();

        // Setup with cache limit
        BarHub barHub = new(maxCacheSize);
        AroonHub observer = barHub.ToAroonHub(25);

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
        AroonHub hub = new(new BarHub(), 25);
        hub.ToString().Should().Be("AROON(25)");
    }

    [TestMethod]
    public void ChainProvider_MatchesSeriesExactly()
    {
        // Setup bar provider
        BarHub barHub = new();

        // Initialize observer - Aroon as provider feeding into EMA
        EmaHub emaHub = barHub
            .ToAroonHub(25)
            .ToEmaHub(12);

        // Emulate bar stream
        for (int i = 0; i < barsCount; i++)
        {
            if (i == 80) { continue; }  // Skip for late arrival

            Bar q = Bars[i];
            barHub.Add(q);

            if (i is > 100 and < 105) { barHub.Add(q); }  // Duplicate bars
        }

        barHub.Add(Bars[80]);  // Late arrival
        barHub.RemoveAt(removeAtIndex);  // Remove

        // Final results
        IReadOnlyList<EmaResult> sut = emaHub.Results;

        // Time-series, for comparison (revised)
        IReadOnlyList<EmaResult> expected = RevisedBars
            .ToAroon(25)
            .ToEma(12);

        // Assert, should equal series
        sut.Should().HaveCount(barsCount - 1);
        sut.IsExactly(expected);

        emaHub.Unsubscribe();
        barHub.EndTransmission();
    }
}
