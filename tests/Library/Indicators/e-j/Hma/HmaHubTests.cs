namespace StreamHubs;

[TestClass]
public class HmaHubTests : StreamHubTestBase, ITestChainObserver, ITestChainProvider
{
    private const int LookbackPeriods = 20;

    [TestMethod]
    public void BarObserver_WithWarmupLateArrivalAndRemoval_MatchesSeriesExactly()
    {
        // setup bar provider hub
        BarHub barHub = new();

        // prefill bars at provider
        barHub.Add(Bars.Take(20));

        // initialize observer
        HmaHub observer = barHub.ToHmaHub(LookbackPeriods);

        // fetch initial results (early)
        IReadOnlyList<HmaResult> sut = observer.Results;

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

        IReadOnlyList<HmaResult> expectedOriginal = Bars.ToHma(LookbackPeriods);
        sut.IsExactly(expectedOriginal);

        // delete, should equal series (revised)
        barHub.RemoveAt(removeAtIndex);
        IReadOnlyList<HmaResult> expectedRevised = RevisedBars.ToHma(LookbackPeriods);
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
        IReadOnlyList<HmaResult> expected = bars
            .ToHma(LookbackPeriods)
            .TakeLast(maxCacheSize)
            .ToList();

        // Setup with cache limit
        BarHub barHub = new(maxCacheSize);
        HmaHub observer = barHub.ToHmaHub(LookbackPeriods);

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
        const int smaPeriods = 10;

        // setup bar provider hub
        BarHub barHub = new();

        // initialize observer
        HmaHub observer = barHub
            .ToSmaHub(smaPeriods)
            .ToHmaHub(LookbackPeriods);

        // emulate bar stream
        for (int i = 0; i < barsCount; i++) { barHub.Add(Bars[i]); }

        // final results
        IReadOnlyList<HmaResult> sut = observer.Results;

        // time-series, for comparison
        IReadOnlyList<HmaResult> expected = Bars
            .ToSma(smaPeriods)
            .ToHma(LookbackPeriods);

        // assert, should equal series
        sut.IsExactly(expected);
        sut.Should().HaveCount(barsCount);

        // cleanup
        observer.Unsubscribe();
        barHub.EndTransmission();
    }

    [TestMethod]
    public void ChainProvider_MatchesSeriesExactly()
    {
        const int hmaPeriods = LookbackPeriods;
        const int smaPeriods = 10;

        // setup bar provider hub
        BarHub barHub = new();

        // initialize observer
        SmaHub observer = barHub
            .ToHmaHub(hmaPeriods)
            .ToSmaHub(smaPeriods);

        // emulate adding bars to provider hub
        for (int i = 0; i < barsCount; i++)
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
        IReadOnlyList<SmaResult> sut = observer.Results;

        // time-series, for comparison (revised)
        IReadOnlyList<SmaResult> expected = RevisedBars
            .ToHma(hmaPeriods)
            .ToSma(smaPeriods);

        // assert, should equal series
        sut.Should().HaveCount(barsCount - 1);
        sut.IsExactly(expected);

        // cleanup
        observer.Unsubscribe();
        barHub.EndTransmission();
    }

    [TestMethod]
    public void MidBufferMutation_WhenBarModified_RehydratesCorrectly()
    {
        List<Bar> barsList = Bars
            .Take(200)
            .ToList();

        BarHub barHub = new();
        HmaHub observer = barHub.ToHmaHub(LookbackPeriods);

        for (int i = 0; i < barsList.Count; i++)
        {
            barHub.Add(barsList[i]);
        }

        const int targetIndex = 120;
        observer.Results.Should().HaveCount(barsList.Count);
        double? before = observer.Results[targetIndex].Hma;

        Bar original = barsList[targetIndex];
        const decimal delta = 1m;
        Bar mutated = original with {
            Open = original.Open + delta,
            High = original.High + delta,
            Low = original.Low + delta,
            Close = original.Close + delta
        };

        barHub.RemoveAt(targetIndex);
        barHub.Add(mutated);
        barsList[targetIndex] = mutated;

        observer.Results.Should().HaveCount(barsList.Count);
        double? after = observer.Results[targetIndex].Hma;
        after.Should().NotBe(before);

        IReadOnlyList<HmaResult> seriesList = barsList.ToHma(LookbackPeriods);
        observer.Results.IsExactly(seriesList);

        observer.Unsubscribe();
        barHub.EndTransmission();
    }

    [TestMethod]
    public void WarmupPeriods_WithInsufficientData_RemainNull()
    {
        int sqrtPeriods = (int)Math.Sqrt(LookbackPeriods);
        int minSamples = LookbackPeriods - 1 + sqrtPeriods - 1;

        BarHub barHub = new();
        HmaHub observer = barHub.ToHmaHub(LookbackPeriods);

        for (int i = 0; i < minSamples; i++)
        {
            barHub.Add(Bars[i]);
        }

        observer.Results.Should().HaveCount(minSamples);
        observer.Results.Should().OnlyContain(static r => !r.Hma.HasValue);

        observer.Unsubscribe();
        barHub.EndTransmission();
    }

    [TestMethod]
    public void NaNInput_WhenProvided_ProducesNullResult()
    {
        const int injectionIndex = LookbackPeriods + 5;
        const int totalCount = injectionIndex + (LookbackPeriods * 2);

        BarHub barHub = new();
        HmaHub observer = barHub.ToHmaHub(LookbackPeriods);

        for (int i = 0; i < totalCount; i++)
        {
            DateTime timestamp = Bars[i].Timestamp;
            double value = i == injectionIndex ? double.NaN : Bars[i].Value;
            barHub.Add(new SyntheticBar(timestamp, value));
        }

        observer.Results[injectionIndex].Hma.Should().BeNull();

        int? recoveryIndex = null;

        for (int i = injectionIndex + 1; i < observer.Results.Count; i++)
        {
            if (observer.Results[i].Hma.HasValue)
            {
                recoveryIndex = i;
                break;
            }
        }

        recoveryIndex.Should().NotBeNull();

        observer.Unsubscribe();
        barHub.EndTransmission();
    }

    [TestMethod]
    public override void ToStringOverride_ReturnsExpectedName()
    {
        HmaHub hub = new(new BarHub(), LookbackPeriods);
        hub.ToString().Should().Be($"HMA({LookbackPeriods})");
    }

    private sealed record SyntheticBar(DateTime Timestamp, double RawValue) : IBar
    {
        public decimal Open => 0m;

        public decimal High => 0m;

        public decimal Low => 0m;

        public decimal Close => 0m;

        public decimal Volume => 0m;

        public double Value => RawValue;
    }
}
