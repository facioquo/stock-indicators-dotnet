namespace StreamHubs;

[TestClass]
public class AlmaHubTests : StreamHubTestBase, ITestChainObserver, ITestChainProvider
{
    [TestMethod]
    public void BarObserver_WithWarmupLateArrivalAndRemoval_MatchesSeriesExactly()
    {
        // setup bar provider hub
        BarHub barHub = new();

        // prefill bars at provider
        barHub.Add(Bars.Take(20));

        // initialize observer
        AlmaHub observer = barHub.ToAlmaHub(10, 0.85, 6);

        // fetch initial results (early)
        IReadOnlyList<AlmaResult> sut = observer.Results;

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

        IReadOnlyList<AlmaResult> expectedOriginal = Bars.ToAlma(10, 0.85, 6);
        sut.IsExactly(expectedOriginal);

        // delete, should equal series (revised)
        barHub.RemoveAt(removeAtIndex);

        IReadOnlyList<AlmaResult> expectedRevised = RevisedBars.ToAlma(10, 0.85, 6);
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
        IReadOnlyList<AlmaResult> expected = bars
            .ToAlma(10, 0.85, 6)
            .TakeLast(maxCacheSize)
            .ToList();

        // Setup with cache limit
        BarHub barHub = new(maxCacheSize);
        AlmaHub observer = barHub.ToAlmaHub(10, 0.85, 6);

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
        const int almaPeriods = 12;
        const int smaPeriods = 8;

        List<Bar> barsList = Bars.ToList();

        int length = barsList.Count;

        // setup bar provider hub
        BarHub barHub = new();

        // initialize observer
        AlmaHub observer = barHub
            .ToSmaHub(smaPeriods)
            .ToAlmaHub(almaPeriods, 0.85, 6);

        // emulate bar stream
        for (int i = 0; i < length; i++)
        {
            barHub.Add(barsList[i]);
        }

        // final results
        IReadOnlyList<AlmaResult> streamList
            = observer.Results;

        // time-series, for comparison
        IReadOnlyList<AlmaResult> seriesList
           = barsList
            .ToSma(smaPeriods)
            .ToAlma(almaPeriods, 0.85, 6);

        // assert, should equal series
        streamList.Should().HaveCount(length);
        streamList.IsExactly(seriesList);

        // cleanup
        observer.Unsubscribe();
        barHub.EndTransmission();
    }

    [TestMethod]
    public void ChainProvider_MatchesSeriesExactly()
    {
        const int almaPeriods = 20;
        const int smaPeriods = 10;

        // setup bar provider hub
        BarHub barHub = new();

        // initialize ALMA observer as barHub
        AlmaHub almaObserver = barHub
            .ToAlmaHub(almaPeriods, 0.85, 6);

        // initialize SMA observer
        SmaHub smaObserver = almaObserver
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
        IReadOnlyList<SmaResult> sut = smaObserver.Results;

        // time-series, for comparison (revised)
        IReadOnlyList<SmaResult> expected = RevisedBars
            .ToAlma(almaPeriods, 0.85, 6)
            .ToSma(smaPeriods);

        // assert, should equal series
        sut.Should().HaveCount(barsCount - 1);
        sut.IsExactly(expected);

        almaObserver.Unsubscribe();
        smaObserver.Unsubscribe();
        barHub.EndTransmission();
    }

    [TestMethod]
    public override void ToStringOverride_ReturnsExpectedName()
    {
        BarHub barHub = new();
        AlmaHub observer = barHub.ToAlmaHub(14, 0.85, 6);

        observer.ToString().Should().Be("ALMA(14,0.85,6)");

        barHub.EndTransmission();
    }

    [TestMethod]
    public void AlmaHub_ParameterCombinations_MatchesSeriesExactly()
    {
        // Test various parameter combinations
        (int lookback, double offset, double sigma)[] parameters =
        [
            (lookback: 5, offset: 0.85, sigma: 6.0),
            (lookback: 10, offset: 0.5, sigma: 4.0),
            (lookback: 14, offset: 0.9, sigma: 8.0),
            (lookback: 20, offset: 0.25, sigma: 3.0)
        ];

        foreach ((int lookback, double offset, double sigma) in parameters)
        {
            // setup bar provider hub
            BarHub barHub = new();

            // initialize observer
            AlmaHub observer = barHub
                .ToAlmaHub(lookback, offset, sigma);

            // emulate bar stream
            for (int i = 0; i < Bars.Count; i++)
            {
                barHub.Add(Bars[i]);
            }

            // final results
            IReadOnlyList<AlmaResult> streamList = observer.Results;

            // time-series, for comparison
            IReadOnlyList<AlmaResult> seriesList = Bars.ToAlma(lookback, offset, sigma);

            // assert, should equal series
            streamList.Should().HaveCount(Bars.Count,
                $"Count mismatch for parameters: lookback={lookback}, offset={offset}, sigma={sigma}");
            streamList.IsExactly(seriesList,
                $"Results mismatch for parameters: lookback={lookback}, offset={offset}, sigma={sigma}");

            // cleanup
            observer.Unsubscribe();
            barHub.EndTransmission();
        }
    }

    [TestMethod]
    public void Reinitialize_OnSubscribedHub_Throws()
    {
        // setup bar provider hub
        BarHub barHub = new();

        // initialize observer with sample parameters
        AlmaHub observer = barHub.ToAlmaHub(14, 0.85, 6);

        // Add ~50 bars to populate state
        for (int i = 0; i < 50; i++)
        {
            barHub.Add(Bars[i]);
        }

        // assert observer.Results has 50 entries and the last result has a non-null Alma value
        observer.Results.Should().HaveCount(50);
        observer.Results[^1].Alma.Should().NotBeNull();

        // reinitializing a subscribed hub is forbidden — it is driven by its provider
        Assert.ThrowsExactly<InvalidOperationException>(observer.Reinitialize);

        // the observer is unchanged and stays in sync via its provider
        observer.Results.Should().HaveCount(50);

        // Now test with a completely fresh setup after unsubscribing
        // cleanup
        observer.Unsubscribe();
        barHub.EndTransmission();

        // Create a new barHub with just one bar
        BarHub freshProvider = new();
        AlmaHub freshObserver = freshProvider.ToAlmaHub(14, 0.85, 6);

        // Add one bar and assert observer.Results has count 1 and that the single result's Alma is null (since lookback period is 14)
        freshProvider.Add(Bars[0]);

        freshObserver.Results.Should().HaveCount(1);
        freshObserver.Results[^1].Alma.Should().BeNull();

        // cleanup
        freshObserver.Unsubscribe();
        freshProvider.EndTransmission();
    }

    [TestMethod]
    public void AlmaHub_InvalidParameters_ThrowsArgumentOutOfRangeException()
    {
        BarHub barHub = new();

        // test constructor validation
        Action act1 = () => barHub.ToAlmaHub(1, 0.85, 6);
        act1.Should().Throw<ArgumentOutOfRangeException>("Lookback periods must be greater than 1");

        Action act2 = () => barHub.ToAlmaHub(0, 0.85, 6);
        act2.Should().Throw<ArgumentOutOfRangeException>("Lookback periods must be greater than 1");

        Action act3 = () => barHub.ToAlmaHub(-1, 0.85, 6);
        act3.Should().Throw<ArgumentOutOfRangeException>("Lookback periods must be greater than 1");

        Action act4 = () => barHub.ToAlmaHub(10, 1.1, 6);
        act4.Should().Throw<ArgumentOutOfRangeException>("Offset must be between 0 and 1");

        Action act5 = () => barHub.ToAlmaHub(10, -0.1, 6);
        act5.Should().Throw<ArgumentOutOfRangeException>("Offset must be between 0 and 1");

        Action act6 = () => barHub.ToAlmaHub(10, 0.85, 0);
        act6.Should().Throw<ArgumentOutOfRangeException>("Sigma must be greater than 0");

        Action act7 = () => barHub.ToAlmaHub(10, 0.85, -1);
        act7.Should().Throw<ArgumentOutOfRangeException>("Sigma must be greater than 0");

        barHub.EndTransmission();
    }
}
