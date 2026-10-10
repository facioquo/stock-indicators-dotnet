namespace StreamHubs;

[TestClass]
public class BarAggregatorHubTests : StreamHubTestBase, ITestBarObserver, ITestChainProvider
{
    [TestMethod]
    public override void ToStringOverride_ReturnsExpectedName()
    {
        BarHub provider = new();
        BarAggregatorHub aggregator = provider.ToBarAggregatorHub(BarInterval.FiveMinutes);

        string result = aggregator.ToString();

        result.Should().Contain("BAR-AGG");
        result.Should().Contain("00:05:00");

        aggregator.Unsubscribe();
        provider.EndTransmission();
    }

    [TestMethod]
    public void BasicAggregation_OneMinuteToFiveMinute()
    {
        // Setup: Create minute-level bars
        List<Bar> minuteBars =
        [
            new(DateTime.Parse("2023-11-09 10:00", invariantCulture), 100, 105, 99, 102, 1000),
            new(DateTime.Parse("2023-11-09 10:01", invariantCulture), 102, 106, 101, 104, 1100),
            new(DateTime.Parse("2023-11-09 10:02", invariantCulture), 104, 107, 103, 105, 1200),
            new(DateTime.Parse("2023-11-09 10:03", invariantCulture), 105, 108, 104, 106, 1300),
            new(DateTime.Parse("2023-11-09 10:04", invariantCulture), 106, 109, 105, 107, 1400),
            new(DateTime.Parse("2023-11-09 10:05", invariantCulture), 107, 110, 106, 108, 1500),
            new(DateTime.Parse("2023-11-09 10:06", invariantCulture), 108, 111, 107, 109, 1600),
        ];

        BarHub provider = new();
        BarAggregatorHub aggregator = provider.ToBarAggregatorHub(BarInterval.FiveMinutes);

        // Add bars
        foreach (Bar q in minuteBars)
        {
            provider.Add(q);
        }

        // Verify results
        IReadOnlyList<IBar> results = aggregator.Results;

        results.Should().HaveCount(2);

        // First 5-minute bar (10:00-10:04)
        IBar bar1 = results[0];
        bar1.Timestamp.Should().Be(DateTime.Parse("2023-11-09 10:00", invariantCulture));
        bar1.Open.Should().Be(100);
        bar1.High.Should().Be(109);  // Max of all highs in period
        bar1.Low.Should().Be(99);    // Min of all lows in period
        bar1.Close.Should().Be(107);  // Last close in period
        bar1.Volume.Should().Be(6000); // Sum of all volumes

        // Second 5-minute bar (10:05-10:06, incomplete)
        IBar bar2 = results[1];
        bar2.Timestamp.Should().Be(DateTime.Parse("2023-11-09 10:05", invariantCulture));
        bar2.Open.Should().Be(107);
        bar2.High.Should().Be(111);
        bar2.Low.Should().Be(106);
        bar2.Close.Should().Be(109);
        bar2.Volume.Should().Be(3100);

        // Cleanup
        aggregator.Unsubscribe();
        provider.EndTransmission();
    }

    [TestMethod]
    public void AggregationWithTimeSpan()
    {
        List<Bar> minuteBars =
        [
            new(DateTime.Parse("2023-11-09 10:00", invariantCulture), 100, 105, 99, 102, 1000),
            new(DateTime.Parse("2023-11-09 10:01", invariantCulture), 102, 106, 101, 104, 1100),
            new(DateTime.Parse("2023-11-09 10:02", invariantCulture), 104, 107, 103, 105, 1200),
        ];

        BarHub provider = new();
        BarAggregatorHub aggregator = provider.ToBarAggregatorHub(TimeSpan.FromMinutes(3));

        foreach (Bar q in minuteBars)
        {
            provider.Add(q);
        }

        IReadOnlyList<IBar> results = aggregator.Results;

        results.Should().HaveCount(1);

        IBar bar = results[0];
        bar.Timestamp.Should().Be(DateTime.Parse("2023-11-09 10:00", invariantCulture));
        bar.Open.Should().Be(100);
        bar.High.Should().Be(107);
        bar.Low.Should().Be(99);
        bar.Close.Should().Be(105);
        bar.Volume.Should().Be(3300);

        aggregator.Unsubscribe();
        provider.EndTransmission();
    }

    [TestMethod]
    public void GapFilling_CarriesForwardPrices()
    {
        List<Bar> minuteBars =
        [
            new(DateTime.Parse("2023-11-09 10:00", invariantCulture), 100, 105, 99, 102, 1000),
            new(DateTime.Parse("2023-11-09 10:01", invariantCulture), 102, 106, 101, 104, 1100),
            // Gap: 10:02 missing
            new(DateTime.Parse("2023-11-09 10:03", invariantCulture), 105, 108, 104, 106, 1300),
        ];

        BarHub provider = new();
        BarAggregatorHub aggregator = provider.ToBarAggregatorHub(
            BarInterval.OneMinute,
            fillGaps: true);

        foreach (Bar q in minuteBars)
        {
            provider.Add(q);
        }

        IReadOnlyList<IBar> results = aggregator.Results;

        // Should have 4 bars: 10:00, 10:01, 10:02 (gap-filled), 10:03
        results.Should().HaveCount(4);

        // Verify gap-filled bar at 10:02
        IBar gapBar = results[2];
        gapBar.Timestamp.Should().Be(DateTime.Parse("2023-11-09 10:02", invariantCulture));
        gapBar.Open.Should().Be(104);  // Carried forward from 10:01 close
        gapBar.High.Should().Be(104);
        gapBar.Low.Should().Be(104);
        gapBar.Close.Should().Be(104);
        gapBar.Volume.Should().Be(0);

        aggregator.Unsubscribe();
        provider.EndTransmission();
    }

    [TestMethod]
    public void GapFilling_MultipleConsecutiveGaps()
    {
        List<Bar> minuteBars =
        [
            new(DateTime.Parse("2023-11-09 10:00", invariantCulture), 100, 105, 99, 102, 1000),
            // Gaps: 10:01, 10:02, 10:03 missing
            new(DateTime.Parse("2023-11-09 10:04", invariantCulture), 105, 108, 104, 106, 1300),
        ];

        BarHub provider = new();
        BarAggregatorHub aggregator = provider.ToBarAggregatorHub(
            BarInterval.OneMinute,
            fillGaps: true);

        foreach (Bar q in minuteBars)
        {
            provider.Add(q);
        }

        IReadOnlyList<IBar> results = aggregator.Results;

        // Should have 5 bars: 10:00, 10:01 (gap), 10:02 (gap), 10:03 (gap), 10:04
        results.Should().HaveCount(5);

        // Verify all gap-filled bars carry forward price of 102
        for (int i = 1; i <= 3; i++)
        {
            IBar gapBar = results[i];
            gapBar.Open.Should().Be(102);
            gapBar.High.Should().Be(102);
            gapBar.Low.Should().Be(102);
            gapBar.Close.Should().Be(102);
            gapBar.Volume.Should().Be(0);
        }

        aggregator.Unsubscribe();
        provider.EndTransmission();
    }

    [TestMethod]
    public void NoGapFilling_SkipsMissingPeriods()
    {
        List<Bar> minuteBars =
        [
            new(DateTime.Parse("2023-11-09 10:00", invariantCulture), 100, 105, 99, 102, 1000),
            // Gap: 10:01, 10:02 missing
            new(DateTime.Parse("2023-11-09 10:03", invariantCulture), 105, 108, 104, 106, 1300),
        ];

        BarHub provider = new();
        BarAggregatorHub aggregator = provider.ToBarAggregatorHub(
            BarInterval.OneMinute,
            fillGaps: false);

        foreach (Bar q in minuteBars)
        {
            provider.Add(q);
        }

        IReadOnlyList<IBar> results = aggregator.Results;

        // Should have only 2 bars (no gap filling)
        results.Should().HaveCount(2);

        results[0].Timestamp.Should().Be(DateTime.Parse("2023-11-09 10:00", invariantCulture));
        results[1].Timestamp.Should().Be(DateTime.Parse("2023-11-09 10:03", invariantCulture));

        aggregator.Unsubscribe();
        provider.EndTransmission();
    }

    [TestMethod]
    public void SameTimestampUpdates_ReplaceWithinSameBar()
    {
        BarHub provider = new();
        BarAggregatorHub aggregator = provider.ToBarAggregatorHub(BarInterval.FiveMinutes);

        // Add initial bar
        provider.Add(new Bar(
            DateTime.Parse("2023-11-09 10:00", invariantCulture), 100, 105, 99, 102, 1000));

        // Add another bar in the same 5-minute period (should update the bar)
        provider.Add(new Bar(
            DateTime.Parse("2023-11-09 10:02", invariantCulture), 102, 110, 98, 108, 1500));

        IReadOnlyList<IBar> results = aggregator.Results;

        results.Should().HaveCount(1);

        IBar bar = results[0];
        bar.Timestamp.Should().Be(DateTime.Parse("2023-11-09 10:00", invariantCulture));
        bar.Open.Should().Be(100);   // Original open
        bar.High.Should().Be(110);   // Updated high
        bar.Low.Should().Be(98);     // Updated low
        bar.Close.Should().Be(108);  // Latest close
        bar.Volume.Should().Be(2500); // Sum of volumes

        aggregator.Unsubscribe();
        provider.EndTransmission();
    }

    [TestMethod]
    public void ChainWithDownstreamIndicator_WorksCorrectly()
    {
        List<Bar> minuteBars =
        [
            new(DateTime.Parse("2023-11-09 10:00", invariantCulture), 100, 105, 99, 102, 1000),
            new(DateTime.Parse("2023-11-09 10:05", invariantCulture), 102, 107, 101, 104, 1100),
            new(DateTime.Parse("2023-11-09 10:10", invariantCulture), 104, 109, 103, 106, 1200),
            new(DateTime.Parse("2023-11-09 10:15", invariantCulture), 106, 111, 105, 108, 1300),
            new(DateTime.Parse("2023-11-09 10:20", invariantCulture), 108, 113, 107, 110, 1400),
        ];

        BarHub provider = new();
        BarAggregatorHub aggregator = provider.ToBarAggregatorHub(BarInterval.FiveMinutes);

        // Chain with SMA
        SmaHub sma = aggregator.ToSmaHub(3);

        foreach (Bar q in minuteBars)
        {
            provider.Add(q);
        }

        // Verify aggregated bars
        IReadOnlyList<IBar> aggResults = aggregator.Results;
        aggResults.Should().HaveCount(5);

        // Verify SMA results
        IReadOnlyList<SmaResult> smaResults = sma.Results;
        smaResults.Should().HaveCount(5);

        // First two should be null (not enough data)
        smaResults[0].Sma.Should().BeNull();
        smaResults[1].Sma.Should().BeNull();

        // Third should be average of first three closes
        smaResults[2].Sma.Should().NotBeNull();
        const double expectedSma = (102 + 104 + 106) / 3.0;
        smaResults[2].Sma.Should().BeApproximately(expectedSma, Money4);

        sma.Unsubscribe();
        aggregator.Unsubscribe();
        provider.EndTransmission();
    }

    [TestMethod]
    public void InvalidBarInterval_Month_ThrowsException()
    {
        BarHub provider = new();

        FluentActions
            .Invoking(() => provider.ToBarAggregatorHub(BarInterval.Month))
            .Should()
            .ThrowExactly<ArgumentException>()
            .WithMessage("*Month aggregation is not supported*");

        provider.EndTransmission();
    }

    [TestMethod]
    public void InvalidTimeSpan_Zero_ThrowsException()
    {
        BarHub provider = new();

        FluentActions
            .Invoking(() => provider.ToBarAggregatorHub(TimeSpan.Zero))
            .Should()
            .ThrowExactly<ArgumentOutOfRangeException>()
            .WithMessage("*must be greater than zero*");

        provider.EndTransmission();
    }

    [TestMethod]
    public void InvalidTimeSpan_Negative_ThrowsException()
    {
        BarHub provider = new();

        FluentActions
            .Invoking(() => provider.ToBarAggregatorHub(TimeSpan.FromMinutes(-5)))
            .Should()
            .ThrowExactly<ArgumentOutOfRangeException>()
            .WithMessage("*must be greater than zero*");

        provider.EndTransmission();
    }

    [TestMethod]
    [DataRow(10080)] // one week: fixed Monday 00:00 buckets
    [DataRow(7)]     // 7 minutes: a bucket spans midnight
    public void PeriodBoundaries_MatchSeries(int periodMinutes)
    {
        TimeSpan period = TimeSpan.FromMinutes(periodMinutes);

        // 61-minute bars from a Sunday evening (crossing Monday 00:00) through Wednesday morning
        List<Bar> bars = [];
        DateTime start = DateTime.Parse("2023-11-05 20:00", invariantCulture);
        for (int i = 0; i < 60; i++)
        {
            bars.Add(new Bar(start.AddMinutes(i * 61), 100m + i, 105m + i, 99m + i, 102m + i, 1000m + i));
        }

        BarHub provider = new();
        BarAggregatorHub aggregator = provider.ToBarAggregatorHub(period);
        provider.Add(bars);

        IReadOnlyList<Bar> expected = bars.Aggregate(period);
        aggregator.Results.Should().BeEquivalentTo(expected, static o => o.WithStrictOrdering());

        aggregator.Unsubscribe();
        provider.EndTransmission();
    }

    [TestMethod]
    public void WithCachePruning_MatchesSeriesExactly()
    {
        const int maxCacheSize = 20;

        // Create enough minute bars to produce many 5-minute bars
        List<Bar> minuteBars = [];
        for (int i = 0; i < 200; i++)
        {
            minuteBars.Add(new Bar(
                DateTime.Parse("2023-11-09 10:00", invariantCulture).AddMinutes(i),
                100m + i, 105m + i, 99m + i, 102m + i, 1000m + i));
        }

        // Setup with cache limit
        BarHub provider = new(maxCacheSize);
        BarAggregatorHub aggregator = provider.ToBarAggregatorHub(BarInterval.FiveMinutes);

        // Stream all bars
        provider.Add(minuteBars);

        // Verify results are structurally valid
        IReadOnlyList<IBar> results = aggregator.Results;
        results.Should().NotBeEmpty();

        results.Should().AllSatisfy(q => {
            q.Timestamp.Should().NotBe(default);
            q.Open.Should().BeGreaterThan(0);
            q.High.Should().BeGreaterThanOrEqualTo(q.Low);
            q.Close.Should().BeGreaterThan(0);
        });

        // Verify ordering
        for (int i = 1; i < results.Count; i++)
        {
            results[i].Timestamp.Should().BeAfter(results[i - 1].Timestamp);
        }

        // Cleanup
        aggregator.Unsubscribe();
        provider.EndTransmission();
    }

    [TestMethod]
    public void BarObserver_WithWarmupLateArrivalAndRemoval_MatchesSeriesExactly()
    {
        BarHub provider = new();

        // Prefill bars at provider (warmup window)
        provider.Add(Bars.Take(20));

        // Initialize aggregator (1-minute aggregation)
        BarAggregatorHub aggregator = provider.ToBarAggregatorHub(BarInterval.OneMinute);

        // Fetch live results reference
        IReadOnlyList<IBar> sut = aggregator.Results;

        // Stream additional bars
        for (int i = 20; i < Bars.Count; i++)
        {
            // Skip one (add later as late arrival)
            if (i == 80) { continue; }

            Bar q = Bars[i];
            provider.Add(q);

            // Resend duplicate bars
            if (i is > 100 and < 105) { provider.Add(q); }
        }

        // Late arrival (add the skipped bar)
        provider.Add(Bars[80]);

        // Build expected Series after all streaming (prefill + duplicates + late arrival)
        IReadOnlyList<Bar> expectedOriginal = Bars.Aggregate(BarInterval.OneMinute);

        // Strict count parity and ordering after late arrival
        sut.Should().HaveCount(expectedOriginal.Count);
        for (int i = 0; i < expectedOriginal.Count; i++)
        {
            sut[i].Timestamp.Should().Be(expectedOriginal[i].Timestamp);
            sut[i].Open.Should().Be(expectedOriginal[i].Open);
            sut[i].High.Should().Be(expectedOriginal[i].High);
            sut[i].Low.Should().Be(expectedOriginal[i].Low);
            sut[i].Close.Should().Be(expectedOriginal[i].Close);
            sut[i].Volume.Should().Be(expectedOriginal[i].Volume);
        }

        // Rollback: remove a historical bar to simulate deletion
        provider.RemoveAt(removeAtIndex);

        // Build revised expected Series after removal
        IReadOnlyList<Bar> expectedRevised = RevisedBars.Aggregate(BarInterval.OneMinute);

        // Strict count parity and ordering after rollback
        sut.Should().HaveCount(expectedRevised.Count);
        for (int i = 0; i < expectedRevised.Count; i++)
        {
            sut[i].Timestamp.Should().Be(expectedRevised[i].Timestamp);
            sut[i].Open.Should().Be(expectedRevised[i].Open);
            sut[i].High.Should().Be(expectedRevised[i].High);
            sut[i].Low.Should().Be(expectedRevised[i].Low);
            sut[i].Close.Should().Be(expectedRevised[i].Close);
            sut[i].Volume.Should().Be(expectedRevised[i].Volume);
        }

        // Cleanup
        aggregator.Unsubscribe();
        provider.EndTransmission();
    }

    [TestMethod]
    public void ChainProvider_MatchesSeriesExactly()
    {
        const int emaPeriods = 14;

        // Setup bar provider hub
        BarHub provider = new();

        // Initialize aggregator and chain with EMA
        EmaHub observer = provider
            .ToBarAggregatorHub(BarInterval.FiveMinutes)
            .ToEmaHub(emaPeriods);

        // Emulate bar stream - for aggregator, use simple sequential adding
        // (late arrivals and removals don't make sense for time-based aggregation)
        foreach (Bar q in Bars)
        {
            provider.Add(q);
        }

        // Final results
        IReadOnlyList<EmaResult> sut = observer.Results;

        // Compare with aggregated series
        IReadOnlyList<Bar> aggregatedBars = Bars.Aggregate(BarInterval.FiveMinutes);

        IReadOnlyList<EmaResult> expected = aggregatedBars
            .ToEma(emaPeriods);

        // Assert: should have same count as batch aggregation
        sut.Should().HaveCount(expected.Count);

        // Verify EMA values match Series exactly (strict parity)
        for (int i = 0; i < sut.Count; i++)
        {
            sut[i].Timestamp.Should().Be(expected[i].Timestamp,
                $"timestamp at index {i} should match Series reference");

            if (expected[i].Ema.HasValue)
            {
                sut[i].Ema.Should().BeApproximately(expected[i].Ema.Value, Money4,
                    $"EMA at index {i} should match Series reference");
            }
            else
            {
                sut[i].Ema.Should().BeNull(
                    $"EMA at index {i} should be null like Series reference");
            }
        }

        // Cleanup
        observer.Unsubscribe();
        provider.EndTransmission();
    }

    [TestMethod]
    public void Properties_AreSetCorrectly()
    {
        BarHub provider = new();
        BarAggregatorHub aggregator = provider.ToBarAggregatorHub(
            BarInterval.FifteenMinutes,
            fillGaps: true);

        aggregator.FillGaps.Should().BeTrue();
        aggregator.GapFillMode.Should().Be(GapFillMode.ForwardFill);
        aggregator.AggregationPeriod.Should().Be(TimeSpan.FromMinutes(15));

        aggregator.Unsubscribe();
        provider.EndTransmission();
    }

    [TestMethod]
    public void SameTimestampBars_ReplacesPriorBar()
    {
        BarHub provider = new();
        BarAggregatorHub aggregator = provider.ToBarAggregatorHub(BarInterval.FiveMinutes);

        // Add initial bar at 10:00
        provider.Add(new Bar(
            DateTime.Parse("2023-11-09 10:00", invariantCulture),
            100m, 110m, 99m, 105m, 1000m));

        // Add another bar also at 10:00 (within same 5-min period)
        provider.Add(new Bar(
            DateTime.Parse("2023-11-09 10:01", invariantCulture),
            105m, 115m, 104m, 108m, 1100m));

        IReadOnlyList<IBar> results = aggregator.Results;

        results.Should().HaveCount(1);

        // Bar should incorporate both bars
        IBar bar = results[0];
        bar.Open.Should().Be(100m);  // First bar's open
        bar.High.Should().Be(115m);  // Max of both highs
        bar.Low.Should().Be(99m);    // Min of both lows
        bar.Close.Should().Be(108m); // Last bar's close
        bar.Volume.Should().Be(2100m); // Sum of volumes

        aggregator.Unsubscribe();
        provider.EndTransmission();
    }

    [TestMethod]
    public void LateArrival_CrossingClosedBucket_RebuildsCorrectly()
    {
        // A late bar belonging to a 5-minute bucket that has already been
        // closed (by bars in subsequent buckets) must trigger a rebuild
        // and produce a bar reflecting the additional bar's OHLCV
        // contributions. The two later buckets must remain unchanged.

        BarHub provider = new();
        BarAggregatorHub aggregator = provider.ToBarAggregatorHub(BarInterval.FiveMinutes);

        // Bucket 10:00 — two bars
        provider.Add(new Bar(
            DateTime.Parse("2023-11-09 10:00", invariantCulture), 100m, 105m, 99m, 102m, 1000m));
        provider.Add(new Bar(
            DateTime.Parse("2023-11-09 10:02", invariantCulture), 102m, 106m, 101m, 104m, 1100m));

        // Bucket 10:05 — closes bucket 10:00
        provider.Add(new Bar(
            DateTime.Parse("2023-11-09 10:05", invariantCulture), 104m, 108m, 103m, 107m, 1200m));
        provider.Add(new Bar(
            DateTime.Parse("2023-11-09 10:07", invariantCulture), 107m, 109m, 106m, 108m, 1300m));

        // Bucket 10:10 — closes bucket 10:05
        provider.Add(new Bar(
            DateTime.Parse("2023-11-09 10:10", invariantCulture), 108m, 112m, 107m, 110m, 1400m));
        provider.Add(new Bar(
            DateTime.Parse("2023-11-09 10:12", invariantCulture), 110m, 114m, 109m, 113m, 1500m));

        // Late arrival to the closed 10:00 bucket with values that move
        // both the high and the low; pinning these in the assertion proves
        // the rebuild path incorporated the late bar rather than skipping it.
        provider.Add(new Bar(
            DateTime.Parse("2023-11-09 10:03", invariantCulture), 103m, 120m, 90m, 105m, 500m));

        IReadOnlyList<IBar> results = aggregator.Results;
        results.Should().HaveCount(3);

        // Bucket 10:00 now reflects the late bar
        IBar bar0 = results[0];
        bar0.Timestamp.Should().Be(DateTime.Parse("2023-11-09 10:00", invariantCulture));
        bar0.Open.Should().Be(100m);
        bar0.High.Should().Be(120m);
        bar0.Low.Should().Be(90m);
        bar0.Close.Should().Be(105m);   // Close from latest-by-timestamp in bucket (10:03)
        bar0.Volume.Should().Be(2600m); // 1000 + 1100 + 500

        // Bucket 10:05 unchanged
        IBar bar1 = results[1];
        bar1.Timestamp.Should().Be(DateTime.Parse("2023-11-09 10:05", invariantCulture));
        bar1.Open.Should().Be(104m);
        bar1.High.Should().Be(109m);
        bar1.Low.Should().Be(103m);
        bar1.Close.Should().Be(108m);
        bar1.Volume.Should().Be(2500m);

        // Bucket 10:10 unchanged
        IBar bar2 = results[2];
        bar2.Timestamp.Should().Be(DateTime.Parse("2023-11-09 10:10", invariantCulture));
        bar2.Open.Should().Be(108m);
        bar2.High.Should().Be(114m);
        bar2.Low.Should().Be(107m);
        bar2.Close.Should().Be(113m);
        bar2.Volume.Should().Be(2900m);

        aggregator.Unsubscribe();
        provider.EndTransmission();
    }

    [TestMethod]
    public void PartialBucket_OnStreamEnd_IsEmittedAsIncomplete()
    {
        // The aggregator emits the current (in-progress) bucket as soon as
        // its first bar arrives, and that bar mutates in place as more
        // bars inside the same bucket arrive. When the stream ends
        // mid-bucket the partial bar remains in Results — it is NOT
        // trimmed, hidden, or frozen at first emission. Pin both the
        // live-mutation contract and the survives-on-stream-end contract
        // so downstream consumers (e.g. live-bar charting) can rely on it.

        BarHub provider = new();
        BarAggregatorHub aggregator = provider.ToBarAggregatorHub(BarInterval.FiveMinutes);

        // First bar opens the 10:00-10:04 bucket as a partial bar
        provider.Add(new Bar(
            DateTime.Parse("2023-11-09 10:00", invariantCulture), 100m, 105m, 99m, 102m, 1000m));

        IReadOnlyList<IBar> results = aggregator.Results;
        results.Should().HaveCount(1);
        results[0].Open.Should().Be(100m);
        results[0].High.Should().Be(105m);
        results[0].Low.Should().Be(99m);
        results[0].Close.Should().Be(102m);
        results[0].Volume.Should().Be(1000m);

        // Second bar in the same bucket — the partial bar must mutate
        provider.Add(new Bar(
            DateTime.Parse("2023-11-09 10:02", invariantCulture), 102m, 110m, 98m, 108m, 1100m));

        results.Should().HaveCount(1);
        IBar partial = results[0];
        partial.Timestamp.Should().Be(DateTime.Parse("2023-11-09 10:00", invariantCulture));
        partial.Open.Should().Be(100m);
        partial.High.Should().Be(110m);
        partial.Low.Should().Be(98m);
        partial.Close.Should().Be(108m);
        partial.Volume.Should().Be(2100m);

        aggregator.Unsubscribe();
        provider.EndTransmission();
    }

    [TestMethod]
    public void LateArrival_MatchesFreshStream()
    {
        // Skip a mid-stream bar, then re-add it after the cache head
        // has advanced several buckets. The late hub's aggregated bars
        // must equal a fresh hub fed the same bars in correct order;
        // this is the rollback-equivalence invariant exercised at the
        // bucket-aware aggregator layer rather than at a per-indicator
        // hub.

        const int totalBars = 60;
        const int lateIndex = 17;

        // Use minute-spaced bars so each 5-minute bucket holds multiple inputs
        List<Bar> bars = [];
        for (int i = 0; i < totalBars; i++)
        {
            bars.Add(new Bar(
                DateTime.Parse("2023-11-09 10:00", invariantCulture).AddMinutes(i),
                100m + i, 105m + i, 99m + i, 102m + i, 1000m + i));
        }

        BarHub lateSource = new();
        BarAggregatorHub lateHub = lateSource.ToBarAggregatorHub(BarInterval.FiveMinutes);
        for (int i = 0; i < totalBars; i++)
        {
            if (i == lateIndex) { continue; }

            lateSource.Add(bars[i]);
        }

        lateSource.Add(bars[lateIndex]);

        BarHub freshSource = new();
        BarAggregatorHub freshHub = freshSource.ToBarAggregatorHub(BarInterval.FiveMinutes);
        freshSource.Add(bars);

        IReadOnlyList<IBar> lateResults = lateHub.Results;
        IReadOnlyList<IBar> freshResults = freshHub.Results;

        lateResults.Should().HaveSameCount(freshResults);
        for (int i = 0; i < freshResults.Count; i++)
        {
            lateResults[i].Timestamp.Should().Be(freshResults[i].Timestamp);
            lateResults[i].Open.Should().Be(freshResults[i].Open);
            lateResults[i].High.Should().Be(freshResults[i].High);
            lateResults[i].Low.Should().Be(freshResults[i].Low);
            lateResults[i].Close.Should().Be(freshResults[i].Close);
            lateResults[i].Volume.Should().Be(freshResults[i].Volume);
        }

        lateHub.Unsubscribe();
        freshHub.Unsubscribe();
        lateSource.EndTransmission();
        freshSource.EndTransmission();
    }

    [TestMethod]
    public void GapFillMode_None_ViaEnumOverload_OmitsSilentBuckets()
    {
        List<Bar> minuteBars =
        [
            new(DateTime.Parse("2023-11-09 10:00", invariantCulture), 100, 105, 99, 102, 1000),
            // Gap: 10:01, 10:02 missing
            new(DateTime.Parse("2023-11-09 10:03", invariantCulture), 105, 108, 104, 106, 1300),
        ];

        BarHub provider = new();
        BarAggregatorHub aggregator = provider.ToBarAggregatorHub(
            BarInterval.OneMinute,
            GapFillMode.None);

        foreach (Bar q in minuteBars)
        {
            provider.Add(q);
        }

        aggregator.GapFillMode.Should().Be(GapFillMode.None);
        aggregator.FillGaps.Should().BeFalse();

        IReadOnlyList<IBar> results = aggregator.Results;
        results.Should().HaveCount(2);
        results[0].Timestamp.Should().Be(DateTime.Parse("2023-11-09 10:00", invariantCulture));
        results[1].Timestamp.Should().Be(DateTime.Parse("2023-11-09 10:03", invariantCulture));

        aggregator.Unsubscribe();
        provider.EndTransmission();
    }

    [TestMethod]
    public void GapFillMode_ForwardFill_ViaEnumOverload_MatchesLegacyBehavior()
    {
        List<Bar> minuteBars =
        [
            new(DateTime.Parse("2023-11-09 10:00", invariantCulture), 100, 105, 99, 102, 1000),
            // Gap: 10:01 missing
            new(DateTime.Parse("2023-11-09 10:02", invariantCulture), 105, 108, 104, 106, 1300),
        ];

        BarHub provider = new();
        BarAggregatorHub aggregator = provider.ToBarAggregatorHub(
            BarInterval.OneMinute,
            GapFillMode.ForwardFill);

        foreach (Bar q in minuteBars)
        {
            provider.Add(q);
        }

        aggregator.GapFillMode.Should().Be(GapFillMode.ForwardFill);
        aggregator.FillGaps.Should().BeTrue();

        IReadOnlyList<IBar> results = aggregator.Results;
        results.Should().HaveCount(3);

        // Gap bar carries the prior bar's close forward, same as fillGaps: true
        IBar gapBar = results[1];
        gapBar.Timestamp.Should().Be(DateTime.Parse("2023-11-09 10:01", invariantCulture));
        gapBar.Open.Should().Be(102m);
        gapBar.High.Should().Be(102m);
        gapBar.Low.Should().Be(102m);
        gapBar.Close.Should().Be(102m);
        gapBar.Volume.Should().Be(0m);

        aggregator.Unsubscribe();
        provider.EndTransmission();
    }

    [TestMethod]
    public void GapFillMode_Interpolate_LinearlyInterpolatesPrices()
    {
        List<Bar> minuteBars =
        [
            new(DateTime.Parse("2023-11-09 10:00", invariantCulture), 100, 105, 99, 102, 1000),
            // Gaps: 10:01, 10:02, 10:03 missing
            new(DateTime.Parse("2023-11-09 10:04", invariantCulture), 105, 108, 104, 106, 1300),
        ];

        BarHub provider = new();
        BarAggregatorHub aggregator = provider.ToBarAggregatorHub(
            BarInterval.OneMinute,
            GapFillMode.Interpolate);

        foreach (Bar q in minuteBars)
        {
            provider.Add(q);
        }

        aggregator.GapFillMode.Should().Be(GapFillMode.Interpolate);
        aggregator.FillGaps.Should().BeTrue();

        IReadOnlyList<IBar> results = aggregator.Results;

        // Should have 5 bars: 10:00, 10:01-10:03 (interpolated), 10:04
        results.Should().HaveCount(5);

        // Linearly interpolated between 10:00's close (102) and 10:04's
        // open (105) across the 3 missing buckets: 102.75, 103.50, 104.25
        decimal[] expected = [102.75m, 103.50m, 104.25m];
        for (int i = 1; i <= 3; i++)
        {
            IBar gapBar = results[i];
            gapBar.Open.Should().Be(expected[i - 1]);
            gapBar.High.Should().Be(expected[i - 1]);
            gapBar.Low.Should().Be(expected[i - 1]);
            gapBar.Close.Should().Be(expected[i - 1]);
            gapBar.Volume.Should().Be(0m);
        }

        aggregator.Unsubscribe();
        provider.EndTransmission();
    }

    [TestMethod]
    public void LateArrival_IntoGapFilledBucket_ReplacesGapBar()
    {
        // A gap-filled bucket (10:01) is later populated by a real,
        // late-arriving bar. The rebuild triggered by the past-bar path
        // must replace the synthetic gap bar with the real one — not
        // leave the gap bar in place or duplicate an entry — and any
        // still-open gap after it must recompute from the new real data.

        BarHub provider = new();
        BarAggregatorHub aggregator = provider.ToBarAggregatorHub(
            BarInterval.OneMinute,
            GapFillMode.ForwardFill);

        // 10:00 real bar
        provider.Add(new Bar(
            DateTime.Parse("2023-11-09 10:00", invariantCulture), 100m, 105m, 99m, 102m, 1000m));

        // 10:03 real bar closes the 10:00 bucket, gap-filling 10:01 and 10:02
        // (both carry 10:00's close of 102)
        provider.Add(new Bar(
            DateTime.Parse("2023-11-09 10:03", invariantCulture), 105m, 108m, 104m, 106m, 1300m));

        IReadOnlyList<IBar> resultsBeforeLateArrival = aggregator.Results;
        resultsBeforeLateArrival.Should().HaveCount(4);
        resultsBeforeLateArrival[1].Close.Should().Be(102m); // gap bar, pre-replacement

        // Late arrival lands squarely in the gap-filled 10:01 bucket
        provider.Add(new Bar(
            DateTime.Parse("2023-11-09 10:01", invariantCulture), 150m, 160m, 140m, 155m, 500m));

        IReadOnlyList<IBar> results = aggregator.Results;

        // Still 4 bars: the gap bar at 10:01 was replaced, not duplicated
        results.Should().HaveCount(4);

        results[0].Timestamp.Should().Be(DateTime.Parse("2023-11-09 10:00", invariantCulture));
        results[0].Open.Should().Be(100m);
        results[0].Close.Should().Be(102m);

        // 10:01 now holds the real late bar's own OHLCV, not the gap fill
        IBar realBar = results[1];
        realBar.Timestamp.Should().Be(DateTime.Parse("2023-11-09 10:01", invariantCulture));
        realBar.Open.Should().Be(150m);
        realBar.High.Should().Be(160m);
        realBar.Low.Should().Be(140m);
        realBar.Close.Should().Be(155m);
        realBar.Volume.Should().Be(500m);

        // 10:02 is still gap-filled, but now carries forward the real
        // 10:01 close (155) instead of the stale value (102)
        IBar recomputedGap = results[2];
        recomputedGap.Timestamp.Should().Be(DateTime.Parse("2023-11-09 10:02", invariantCulture));
        recomputedGap.Open.Should().Be(155m);
        recomputedGap.High.Should().Be(155m);
        recomputedGap.Low.Should().Be(155m);
        recomputedGap.Close.Should().Be(155m);
        recomputedGap.Volume.Should().Be(0m);

        // 10:03 is unaffected
        IBar finalBar = results[3];
        finalBar.Timestamp.Should().Be(DateTime.Parse("2023-11-09 10:03", invariantCulture));
        finalBar.Open.Should().Be(105m);
        finalBar.High.Should().Be(108m);
        finalBar.Low.Should().Be(104m);
        finalBar.Close.Should().Be(106m);
        finalBar.Volume.Should().Be(1300m);

        aggregator.Unsubscribe();
        provider.EndTransmission();
    }

    [TestMethod]
    public void LateArrival_IntoGapFilledBucket_ReplacesGapBar_Interpolate()
    {
        // Same shape as the ForwardFill version above, but under Interpolate
        // a late arrival moves both interpolation anchors: it becomes the
        // start anchor for the gap bar(s) still ahead of it, so a downstream
        // gap must recompute from the new real value, not the stale one.

        BarHub provider = new();
        BarAggregatorHub aggregator = provider.ToBarAggregatorHub(
            BarInterval.OneMinute,
            GapFillMode.Interpolate);

        // 10:00 real bar
        provider.Add(new Bar(
            DateTime.Parse("2023-11-09 10:00", invariantCulture), 100m, 105m, 99m, 102m, 1000m));

        // 10:03 real bar closes the 10:00 bucket, interpolating 10:01 and
        // 10:02 between 10:00's close (102) and 10:03's open (105)
        provider.Add(new Bar(
            DateTime.Parse("2023-11-09 10:03", invariantCulture), 105m, 108m, 104m, 106m, 1300m));

        aggregator.Results[1].Close.Should().Be(103m); // gap bar, pre-replacement
        aggregator.Results[2].Close.Should().Be(104m); // gap bar, pre-replacement

        // Late arrival lands squarely in the interpolated 10:01 bucket
        provider.Add(new Bar(
            DateTime.Parse("2023-11-09 10:01", invariantCulture), 150m, 160m, 140m, 155m, 500m));

        IReadOnlyList<IBar> results = aggregator.Results;
        results.Should().HaveCount(4);

        // 10:01 now holds the real late bar's own OHLCV, not the gap fill
        IBar realBar = results[1];
        realBar.Timestamp.Should().Be(DateTime.Parse("2023-11-09 10:01", invariantCulture));
        realBar.Close.Should().Be(155m);

        // 10:02 recomputes between the new 10:01 close (155) and 10:03's
        // open (105): 155 + (105-155)*1/2 = 130, not the stale 104
        IBar recomputedGap = results[2];
        recomputedGap.Timestamp.Should().Be(DateTime.Parse("2023-11-09 10:02", invariantCulture));
        recomputedGap.Open.Should().Be(130m);
        recomputedGap.High.Should().Be(130m);
        recomputedGap.Low.Should().Be(130m);
        recomputedGap.Close.Should().Be(130m);
        recomputedGap.Volume.Should().Be(0m);

        aggregator.Unsubscribe();
        provider.EndTransmission();
    }

    [TestMethod]
    public void GapFillMode_Interpolate_DoesNotRoundNonTerminatingQuotients()
    {
        // Anchors 100 and 101 across 2 gap steps land on 1/3 and 2/3 —
        // neither terminates in decimal. GapFillMode.Interpolate does not
        // round; this pins the full-precision decimal value so a future
        // change can't silently start rounding (or stop).
        List<Bar> minuteBars =
        [
            new(DateTime.Parse("2023-11-09 10:00", invariantCulture), 100, 100, 100, 100, 1000),
            // Gaps: 10:01, 10:02 missing
            new(DateTime.Parse("2023-11-09 10:03", invariantCulture), 101, 101, 101, 101, 1300),
        ];

        BarHub provider = new();
        BarAggregatorHub aggregator = provider.ToBarAggregatorHub(
            BarInterval.OneMinute,
            GapFillMode.Interpolate);

        foreach (Bar q in minuteBars)
        {
            provider.Add(q);
        }

        IReadOnlyList<IBar> results = aggregator.Results;
        results.Should().HaveCount(4);

        results[1].Close.Should().Be(100.33333333333333333333333333m);
        results[2].Close.Should().Be(100.66666666666666666666666667m);

        aggregator.Unsubscribe();
        provider.EndTransmission();
    }

    [TestMethod]
    public void Interpolate_RevisedBarAfterGap_MatchesFreshStream()
    {
        // The gap bars interpolate toward the open of the bar after them, so
        // revising that bar must regenerate the gap, not keep stale values.
        Bar[] first =
        [
            new(DateTime.Parse("2023-11-09 10:00", invariantCulture), 100m, 105m, 99m, 102m, 1000m),
            new(DateTime.Parse("2023-11-09 10:03", invariantCulture), 105m, 108m, 104m, 106m, 1300m),
        ];

        Bar revised = new(DateTime.Parse("2023-11-09 10:03", invariantCulture), 111m, 112m, 104m, 106m, 1300m);

        BarHub provider = new();
        BarAggregatorHub aggregator = provider.ToBarAggregatorHub(BarInterval.OneMinute, GapFillMode.Interpolate);
        provider.Add(first);
        provider.Add(revised);

        BarHub freshProvider = new();
        BarAggregatorHub fresh = freshProvider.ToBarAggregatorHub(BarInterval.OneMinute, GapFillMode.Interpolate);
        freshProvider.Add(first[0]);
        freshProvider.Add(revised);

        aggregator.Results.Select(r => (r.Timestamp, r.Open, r.High, r.Low, r.Close, r.Volume))
            .Should().Equal(fresh.Results.Select(r => (r.Timestamp, r.Open, r.High, r.Low, r.Close, r.Volume)));
        aggregator.Results[1].Close.Should().Be(105m);
        aggregator.Results[2].Close.Should().Be(108m);

        aggregator.Unsubscribe();
        provider.EndTransmission();
        fresh.Unsubscribe();
        freshProvider.EndTransmission();
    }

    [TestMethod]
    public void Interpolate_LateBarInBucketAfterGap_MatchesFreshStream()
    {
        // 5-minute buckets over 1-minute bars: a late bar earlier in the same
        // bucket as the post-gap bar becomes the new end anchor.
        Bar[] ordered =
        [
            new(DateTime.Parse("2023-11-09 10:00", invariantCulture), 100m, 102m, 100m, 102m, 10m),
            new(DateTime.Parse("2023-11-09 10:15", invariantCulture), 120m, 120m, 120m, 120m, 10m),
            new(DateTime.Parse("2023-11-09 10:16", invariantCulture), 112m, 112m, 112m, 112m, 10m),
        ];

        BarHub provider = new();
        BarAggregatorHub aggregator = provider.ToBarAggregatorHub(BarInterval.FiveMinutes, GapFillMode.Interpolate);
        provider.Add(ordered[0]);
        provider.Add(ordered[2]);
        provider.Add(ordered[1]); // late: earlier input in the 10:15 bucket

        BarHub freshProvider = new();
        BarAggregatorHub fresh = freshProvider.ToBarAggregatorHub(BarInterval.FiveMinutes, GapFillMode.Interpolate);
        freshProvider.Add(ordered);

        aggregator.Results.Select(r => (r.Timestamp, r.Open, r.High, r.Low, r.Close, r.Volume))
            .Should().Equal(fresh.Results.Select(r => (r.Timestamp, r.Open, r.High, r.Low, r.Close, r.Volume)));
        aggregator.Results[1].Close.Should().Be(108m);
        aggregator.Results[2].Close.Should().Be(114m);

        aggregator.Unsubscribe();
        provider.EndTransmission();
        fresh.Unsubscribe();
        freshProvider.EndTransmission();
    }

    [TestMethod]
    public void Interpolate_LateBarInSecondGapBucket_MatchesFreshStream()
    {
        Bar[] ordered =
        [
            new(DateTime.Parse("2023-11-09 10:00", invariantCulture), 100m, 100m, 100m, 100m, 10m),
            new(DateTime.Parse("2023-11-09 10:02", invariantCulture), 150m, 150m, 150m, 150m, 10m),
            new(DateTime.Parse("2023-11-09 10:04", invariantCulture), 110m, 110m, 110m, 110m, 10m),
        ];

        BarHub provider = new();
        BarAggregatorHub aggregator = provider.ToBarAggregatorHub(BarInterval.OneMinute, GapFillMode.Interpolate);
        provider.Add(ordered[0]);
        provider.Add(ordered[2]);
        provider.Add(ordered[1]); // late: lands in the middle of the gap

        BarHub freshProvider = new();
        BarAggregatorHub fresh = freshProvider.ToBarAggregatorHub(BarInterval.OneMinute, GapFillMode.Interpolate);
        freshProvider.Add(ordered);

        aggregator.Results.Select(r => (r.Timestamp, r.Open, r.High, r.Low, r.Close, r.Volume))
            .Should().Equal(fresh.Results.Select(r => (r.Timestamp, r.Open, r.High, r.Low, r.Close, r.Volume)));

        aggregator.Unsubscribe();
        provider.EndTransmission();
        fresh.Unsubscribe();
        freshProvider.EndTransmission();
    }

    [TestMethod]
    public void GapFillMode_TimeSpanOverload_Interpolates()
    {
        BarHub provider = new();
        BarAggregatorHub aggregator = provider.ToBarAggregatorHub(TimeSpan.FromMinutes(1), GapFillMode.Interpolate);

        provider.Add(new Bar(DateTime.Parse("2023-11-09 10:00", invariantCulture), 100m, 100m, 100m, 102m, 10m));
        provider.Add(new Bar(DateTime.Parse("2023-11-09 10:03", invariantCulture), 105m, 105m, 105m, 105m, 10m));

        aggregator.GapFillMode.Should().Be(GapFillMode.Interpolate);
        aggregator.Results.Select(r => r.Close).Should().Equal(102m, 103m, 104m, 105m);

        aggregator.Unsubscribe();
        provider.EndTransmission();
    }

    [TestMethod]
    public void GapFillMode_UndefinedValue_Throws()
    {
        BarHub provider = new();
        const GapFillMode invalid = (GapFillMode)99;

        FluentActions.Invoking(() => new BarAggregatorHub(provider, BarInterval.OneMinute, invalid))
            .Should().Throw<ArgumentOutOfRangeException>().WithParameterName("gapFillMode");

        FluentActions.Invoking(() => new BarAggregatorHub(provider, TimeSpan.FromMinutes(1), invalid))
            .Should().Throw<ArgumentOutOfRangeException>().WithParameterName("gapFillMode");

        FluentActions.Invoking(() => provider.ToBarAggregatorHub(TimeSpan.FromMinutes(1), invalid))
            .Should().Throw<ArgumentOutOfRangeException>().WithParameterName("gapFillMode");
    }

    [TestMethod]
    public void Interpolate_RevisionKeepingAnchor_LeavesGapBarsUntouched()
    {
        // Revising only Close and Volume of the bar after a gap does not move
        // the interpolation anchor, so the gap bars must not be regenerated.
        BarHub provider = new();
        BarAggregatorHub aggregator = provider.ToBarAggregatorHub(BarInterval.OneMinute, GapFillMode.Interpolate);

        provider.Add(new Bar(DateTime.Parse("2023-11-09 10:00", invariantCulture), 100m, 105m, 99m, 102m, 1000m));
        provider.Add(new Bar(DateTime.Parse("2023-11-09 10:03", invariantCulture), 105m, 108m, 104m, 106m, 1300m));

        IBar gapBefore = aggregator.Results[1];

        provider.Add(new Bar(DateTime.Parse("2023-11-09 10:03", invariantCulture), 105m, 110m, 104m, 109m, 1500m));

        aggregator.Results[1].Should().BeSameAs(gapBefore);
        aggregator.Results[3].Close.Should().Be(109m);

        aggregator.Unsubscribe();
        provider.EndTransmission();
    }

    [TestMethod]
    [DataRow(GapFillMode.ForwardFill)]
    [DataRow(GapFillMode.Interpolate)]
    public void GapFill_RemoveBarBeforeGap_MatchesFreshStream(GapFillMode mode)
    {
        // Removing a bar leaves a silent bucket, which the rebuild must fill
        // the way a fresh stream would.
        Bar[] bars =
        [
            new(DateTime.Parse("2023-11-09 10:00", invariantCulture), 100m, 100m, 100m, 100m, 10m),
            new(DateTime.Parse("2023-11-09 10:01", invariantCulture), 101m, 101m, 101m, 101m, 10m),
            new(DateTime.Parse("2023-11-09 10:02", invariantCulture), 102m, 102m, 102m, 102m, 10m),
            new(DateTime.Parse("2023-11-09 10:03", invariantCulture), 103m, 103m, 103m, 103m, 10m),
            new(DateTime.Parse("2023-11-09 10:05", invariantCulture), 110m, 110m, 110m, 110m, 10m),
        ];

        BarHub provider = new();
        BarAggregatorHub aggregator = provider.ToBarAggregatorHub(BarInterval.OneMinute, mode);
        provider.Add(bars);
        provider.Remove(bars[3]);

        BarHub freshProvider = new();
        BarAggregatorHub fresh = freshProvider.ToBarAggregatorHub(BarInterval.OneMinute, mode);
        freshProvider.Add([bars[0], bars[1], bars[2], bars[4]]);

        aggregator.Results.Select(r => (r.Timestamp, r.Open, r.High, r.Low, r.Close, r.Volume))
            .Should().Equal(fresh.Results.Select(r => (r.Timestamp, r.Open, r.High, r.Low, r.Close, r.Volume)));
        aggregator.Results.Should().HaveCount(6);

        aggregator.Unsubscribe();
        provider.EndTransmission();
        fresh.Unsubscribe();
        freshProvider.EndTransmission();
    }

    [TestMethod]
    public void Interpolate_SparseStreamPastCacheLimit_KeepsGapRunsWithinCache()
    {
        // Each new gap run is tracked and the oldest pruned with the cache, so
        // the tracked count stays bounded instead of growing with the stream.
        BarHub provider = new();
        BarAggregatorHub aggregator = provider.ToBarAggregatorHub(BarInterval.OneMinute, GapFillMode.Interpolate);

        DateTime start = DateTime.Parse("2023-11-09 00:00", invariantCulture);

        for (int i = 0; i < 120_000; i++)
        {
            provider.Add(new Bar(start.AddMinutes(i * 4), 100m, 100m, 100m, 100m, 1m));
        }

        aggregator.GapRunCount.Should().BeGreaterThan(0);
        aggregator.GapRunCount.Should().BeLessThanOrEqualTo(aggregator.Results.Count);

        aggregator.Unsubscribe();
        provider.EndTransmission();
    }

    [TestMethod]
    public void Interpolate_RevisionOfEarlierRun_MatchesFreshStream()
    {
        // Two runs around a real bar; revising the first anchor must pass
        // over the later run to rewind.
        Bar[] bars =
        [
            new(DateTime.Parse("2023-11-09 10:00", invariantCulture), 100m, 100m, 100m, 100m, 10m),
            new(DateTime.Parse("2023-11-09 10:03", invariantCulture), 106m, 106m, 106m, 106m, 10m),
            new(DateTime.Parse("2023-11-09 10:06", invariantCulture), 118m, 118m, 118m, 118m, 10m),
        ];

        Bar revised = new(DateTime.Parse("2023-11-09 10:03", invariantCulture), 130m, 130m, 130m, 130m, 10m);

        BarHub provider = new();
        BarAggregatorHub aggregator = provider.ToBarAggregatorHub(BarInterval.OneMinute, GapFillMode.Interpolate);
        provider.Add(bars);
        provider.Add(revised);

        BarHub freshProvider = new();
        BarAggregatorHub fresh = freshProvider.ToBarAggregatorHub(BarInterval.OneMinute, GapFillMode.Interpolate);
        freshProvider.Add([bars[0], revised, bars[2]]);

        aggregator.Results.Select(r => (r.Timestamp, r.Open, r.High, r.Low, r.Close, r.Volume))
            .Should().Equal(fresh.Results.Select(r => (r.Timestamp, r.Open, r.High, r.Low, r.Close, r.Volume)));

        aggregator.Unsubscribe();
        provider.EndTransmission();
        fresh.Unsubscribe();
        freshProvider.EndTransmission();
    }

    [TestMethod]
    public void Interpolate_RemoveAnchorBarAfterGap_MatchesFreshStream()
    {
        // Removing the bar a gap ends on leaves the next bar as the new anchor,
        // so the gap must rewind and refill.
        Bar[] bars =
        [
            new(DateTime.Parse("2023-11-09 10:00", invariantCulture), 100m, 100m, 100m, 100m, 10m),
            new(DateTime.Parse("2023-11-09 10:03", invariantCulture), 106m, 106m, 106m, 106m, 10m),
            new(DateTime.Parse("2023-11-09 10:04", invariantCulture), 120m, 120m, 120m, 120m, 10m),
        ];

        BarHub provider = new();
        BarAggregatorHub aggregator = provider.ToBarAggregatorHub(BarInterval.OneMinute, GapFillMode.Interpolate);
        provider.Add(bars);
        provider.Remove(bars[1]);

        BarHub freshProvider = new();
        BarAggregatorHub fresh = freshProvider.ToBarAggregatorHub(BarInterval.OneMinute, GapFillMode.Interpolate);
        freshProvider.Add([bars[0], bars[2]]);

        aggregator.Results.Select(r => (r.Timestamp, r.Open, r.High, r.Low, r.Close, r.Volume))
            .Should().Equal(fresh.Results.Select(r => (r.Timestamp, r.Open, r.High, r.Low, r.Close, r.Volume)));

        aggregator.Unsubscribe();
        provider.EndTransmission();
        fresh.Unsubscribe();
        freshProvider.EndTransmission();
    }

    [TestMethod]
    [DataRow(GapFillMode.ForwardFill)]
    [DataRow(GapFillMode.Interpolate)]
    public void GapFill_RemoveLastRealBar_DropsTrailingGapAndAcceptsNextBar(GapFillMode mode)
    {
        // Removing the last real bar leaves only synthetic bars after the
        // previous real one; a fresh stream holds none, and a real bar that
        // then lands in a former gap bucket must start its own bar.
        Bar first = new(DateTime.Parse("2023-11-09 10:00", invariantCulture), 100m, 100m, 100m, 100m, 10m);
        Bar last = new(DateTime.Parse("2023-11-09 10:03", invariantCulture), 103m, 103m, 103m, 103m, 10m);
        Bar next = new(DateTime.Parse("2023-11-09 10:02", invariantCulture), 150m, 155m, 149m, 152m, 10m);

        BarHub provider = new();
        BarAggregatorHub aggregator = provider.ToBarAggregatorHub(BarInterval.OneMinute, mode);
        provider.Add([first, last]);
        provider.Remove(last);

        BarHub freshProvider = new();
        BarAggregatorHub fresh = freshProvider.ToBarAggregatorHub(BarInterval.OneMinute, mode);
        freshProvider.Add(first);

        aggregator.Results.Select(r => (r.Timestamp, r.Open, r.High, r.Low, r.Close, r.Volume))
            .Should().Equal(fresh.Results.Select(r => (r.Timestamp, r.Open, r.High, r.Low, r.Close, r.Volume)));

        provider.Add(next);
        freshProvider.Add(next);

        aggregator.Results.Select(r => (r.Timestamp, r.Open, r.High, r.Low, r.Close, r.Volume))
            .Should().Equal(fresh.Results.Select(r => (r.Timestamp, r.Open, r.High, r.Low, r.Close, r.Volume)));

        aggregator.Unsubscribe();
        provider.EndTransmission();
        fresh.Unsubscribe();
        freshProvider.EndTransmission();
    }

    [TestMethod]
    [DataRow(GapFillMode.ForwardFill)]
    [DataRow(GapFillMode.Interpolate)]
    public void GapFill_RemoveLateBarAndItsSuccessor_DropsTrailingGap(GapFillMode mode)
    {
        // A late bar splits the gap run; removing the later bar and then the
        // late one must still drop every trailing synthetic bar.
        Bar first = new(DateTime.Parse("2023-11-09 10:00", invariantCulture), 100m, 100m, 100m, 100m, 10m);
        Bar last = new(DateTime.Parse("2023-11-09 10:05", invariantCulture), 105m, 105m, 105m, 105m, 10m);
        Bar late = new(DateTime.Parse("2023-11-09 10:03", invariantCulture), 120m, 120m, 120m, 120m, 10m);

        BarHub provider = new();
        BarAggregatorHub aggregator = provider.ToBarAggregatorHub(BarInterval.OneMinute, mode);
        provider.Add([first, last]);
        provider.Add(late);
        provider.Remove(last);
        provider.Remove(late);

        BarHub freshProvider = new();
        BarAggregatorHub fresh = freshProvider.ToBarAggregatorHub(BarInterval.OneMinute, mode);
        freshProvider.Add(first);

        aggregator.Results.Select(r => (r.Timestamp, r.Open, r.High, r.Low, r.Close, r.Volume))
            .Should().Equal(fresh.Results.Select(r => (r.Timestamp, r.Open, r.High, r.Low, r.Close, r.Volume)));

        aggregator.Unsubscribe();
        provider.EndTransmission();
        fresh.Unsubscribe();
        freshProvider.EndTransmission();
    }
}
