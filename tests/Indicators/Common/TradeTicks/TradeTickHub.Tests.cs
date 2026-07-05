namespace StreamHubs;

[TestClass]
public class TradeTickStreamHubTests : StreamHubTestBase, ITestTradeTickObserver
{
    [TestMethod]
    public override void ToStringOverride_ReturnsExpectedName()
    {
        TradeTickHub hub = new();
        string result = hub.ToString();

        result.Should().Contain("TICKS");
        result.Should().Contain("0 items");

        hub.EndTransmission();
    }

    [TestMethod]
    public void StandaloneInitialization_WorksCorrectly()
    {
        TradeTickHub hub = new();

        hub.Should().NotBeNull();
        hub.MaxCacheSize.Should().BeGreaterThan(0);
        hub.Cache.Should().BeEmpty();

        hub.EndTransmission();
    }

    [TestMethod]
    public void ProviderBackedInitialization_WorksCorrectly()
    {
        TradeTickHub provider = new();
        TradeTickHub hub = new(provider);

        hub.Should().NotBeNull();
        hub.Cache.Should().BeEmpty();

        hub.Unsubscribe();
        provider.EndTransmission();
    }

    [TestMethod]
    public void SameTimestamp_WithoutExecutionId_ReplacesAndNotifiesAsAddition()
    {
        TradeTickHub hub = new();
        int addCount = 0;

        TestTradeTickObserver observer = new() {
            OnAddAction = (_, _, _) => addCount++
        };

        hub.Subscribe(observer);

        // Add first tick without execution ID
        TradeTick tick1 = new(
            DateTime.Parse("2023-11-09 10:00:00", invariantCulture),
            100m, 10m, null);
        hub.Add(tick1);

        // Add second tick with same timestamp but different price (no execution ID)
        TradeTick tick2 = new(
            DateTime.Parse("2023-11-09 10:00:00", invariantCulture),
            105m, 15m, null);
        hub.Add(tick2);

        // Cache should have only one tick (the latest)
        hub.Cache.Should().HaveCount(1);
        hub.Cache[0].Price.Should().Be(105m);
        hub.Cache[0].Volume.Should().Be(15m);

        // Both ticks should be notified as additions for aggregators to process
        addCount.Should().Be(2);

        observer.Unsubscribe();
        hub.EndTransmission();
    }

    [TestMethod]
    public void SameTimestamp_WithSameExecutionId_ReplacesAndNotifiesRebuild()
    {
        TradeTickHub hub = new();
        int rebuildCount = 0;

        TestTradeTickObserver observer = new() {
            OnRebuildAction = _ => rebuildCount++
        };

        hub.Subscribe(observer);

        // Add first tick with execution ID
        TradeTick tick1 = new(
            DateTime.Parse("2023-11-09 10:00:00", invariantCulture),
            100m, 10m, "EXEC-001");
        hub.Add(tick1);

        // Add updated tick with same execution ID (correction)
        TradeTick tick2 = new(
            DateTime.Parse("2023-11-09 10:00:00", invariantCulture),
            101m, 11m, "EXEC-001");
        hub.Add(tick2);

        // Cache should have only one tick (the corrected one)
        hub.Cache.Should().HaveCount(1);
        hub.Cache[0].Price.Should().Be(101m);
        hub.Cache[0].ExecutionId.Should().Be("EXEC-001");

        // Should trigger rebuild for correction
        rebuildCount.Should().Be(1);

        observer.Unsubscribe();
        hub.EndTransmission();
    }

    [TestMethod]
    public void SameTimestamp_WithDifferentExecutionId_NotifiesAddition()
    {
        TradeTickHub hub = new();
        int addCount = 0;

        TestTradeTickObserver observer = new() {
            OnAddAction = (_, _, _) => addCount++
        };

        hub.Subscribe(observer);

        // Add first tick with execution ID
        TradeTick tick1 = new(
            DateTime.Parse("2023-11-09 10:00:00", invariantCulture),
            100m, 10m, "EXEC-001");
        hub.Add(tick1);

        // Add second tick with same timestamp but different execution ID
        TradeTick tick2 = new(
            DateTime.Parse("2023-11-09 10:00:00", invariantCulture),
            105m, 15m, "EXEC-002");
        hub.Add(tick2);

        // Both ticks should be notified to observers (not dropped)
        addCount.Should().Be(2);

        observer.Unsubscribe();
        hub.EndTransmission();
    }

    [TestMethod]
    public void ExactDuplicate_DefersToAppendCache()
    {
        TradeTickHub hub = new();
        int addCount = 0;

        TestTradeTickObserver observer = new() {
            OnAddAction = (_, _, _) => addCount++
        };

        hub.Subscribe(observer);

        // Add same tick twice
        TradeTick tick = new(
            DateTime.Parse("2023-11-09 10:00:00", invariantCulture),
            100m, 10m, "EXEC-001");

        hub.Add(tick);
        hub.Add(tick);

        // Should use overflow tracking from base class
        hub.Cache.Should().HaveCount(1);
        addCount.Should().Be(1); // Only first add notified

        observer.Unsubscribe();
        hub.EndTransmission();
    }

    [TestMethod]
    public void StandaloneRebuild_PreservesCacheAndNotifiesObservers()
    {
        TradeTickHub hub = new();
        int rebuildCount = 0;
        DateTime? rebuildTimestamp = null;

        TestTradeTickObserver observer = new() {
            OnRebuildAction = ts => {
                rebuildCount++;
                rebuildTimestamp = ts;
            }
        };

        hub.Subscribe(observer);

        // Add ticks
        hub.Add(new TradeTick(DateTime.Parse("2023-11-09 10:00", invariantCulture), 100m, 10m));
        hub.Add(new TradeTick(DateTime.Parse("2023-11-09 10:01", invariantCulture), 101m, 11m));
        hub.Add(new TradeTick(DateTime.Parse("2023-11-09 10:02", invariantCulture), 102m, 12m));

        int initialCount = hub.Cache.Count;

        // Trigger rebuild
        hub.Rebuild(DateTime.Parse("2023-11-09 10:01", invariantCulture));

        // Cache should still have all ticks (standalone doesn't clear)
        hub.Cache.Should().HaveCount(initialCount);

        // Observers should be notified
        rebuildCount.Should().Be(1);
        rebuildTimestamp.Should().Be(DateTime.Parse("2023-11-09 10:01", invariantCulture));

        observer.Unsubscribe();
        hub.EndTransmission();
    }

    [TestMethod]
    public void ProviderBackedRebuild_UsesStandardBehavior()
    {
        TradeTickHub provider = new();
        TradeTickHub hub = new(provider);

        // Add ticks through provider
        provider.Add(new TradeTick(DateTime.Parse("2023-11-09 10:00", invariantCulture), 100m, 10m));
        provider.Add(new TradeTick(DateTime.Parse("2023-11-09 10:01", invariantCulture), 101m, 11m));
        provider.Add(new TradeTick(DateTime.Parse("2023-11-09 10:02", invariantCulture), 102m, 12m));

        hub.Cache.Should().HaveCount(3);

        // Trigger rebuild on hub (not provider)
        hub.Rebuild(DateTime.Parse("2023-11-09 10:01", invariantCulture));

        // Provider-backed hub should rebuild normally
        hub.Cache.Should().HaveCountGreaterOrEqualTo(2);

        hub.Unsubscribe();
        provider.EndTransmission();
    }

    [TestMethod]
    public void MultipleTradeTicksSameTimestamp_AllProcessedByAggregators()
    {
        // This test verifies the fix for issue #4 from code review
        TradeTickHub provider = new();
        TradeTickAggregatorHub aggregator = provider.ToTradeTickAggregatorHub(BarInterval.OneMinute);

        // Add three ticks at same timestamp with different execution IDs
        provider.Add(new TradeTick(
            DateTime.Parse("2023-11-09 10:00:00", invariantCulture),
            100m, 10m, "EXEC-001"));
        provider.Add(new TradeTick(
            DateTime.Parse("2023-11-09 10:00:00", invariantCulture),
            105m, 15m, "EXEC-002"));
        provider.Add(new TradeTick(
            DateTime.Parse("2023-11-09 10:00:00", invariantCulture),
            95m, 20m, "EXEC-003"));

        IReadOnlyList<IBar> results = aggregator.Results;

        results.Should().HaveCount(1);

        // All three ticks should be incorporated into the bar
        IBar bar = results[0];
        bar.High.Should().Be(105m); // Max of all three
        bar.Low.Should().Be(95m);   // Min of all three
        bar.Volume.Should().Be(45m); // Sum of all three (10 + 15 + 20)

        aggregator.Unsubscribe();
        provider.EndTransmission();
    }

    [TestMethod]
    public void TradeTickObserver_WithWarmupAndMultipleSameTimestamp_WorksCorrectly()
    {
        TradeTickHub provider = new();

        // prefill warmup window
        for (int i = 0; i < 20; i++)
        {
            provider.Add(new TradeTick(
                DateTime.Parse("2023-11-09", invariantCulture).AddMinutes(i),
                100m + i, 10m + i, $"EXEC-{i:000}"));
        }

        // initialize observer
        TradeTickHub observer = new(provider);

        // fetch initial results
        IReadOnlyList<ITradeTick> results = observer.Results.ToList();

        results.Should().HaveCount(20);

        // stream in-order duplicates (same timestamp different exec IDs)
        provider.Add(new TradeTick(
            DateTime.Parse("2023-11-09", invariantCulture).AddMinutes(20),
            120m, 30m, "EXEC-020"));
        provider.Add(new TradeTick(
            DateTime.Parse("2023-11-09", invariantCulture).AddMinutes(20),
            121m, 31m, "EXEC-021"));

        IReadOnlyList<ITradeTick> afterDuplicates = observer.Results.ToList();
        afterDuplicates.Count.Should().BeGreaterThanOrEqualTo(21);

        // add late historical tick (earlier DateTime than current tail)
        provider.Add(new TradeTick(
            DateTime.Parse("2023-11-09", invariantCulture).AddMinutes(15),
            115m, 25m, "EXEC-015-LATE"));

        IReadOnlyList<ITradeTick> afterLateArrival = observer.Results.ToList();

        // results should maintain ordering
        for (int i = 1; i < afterLateArrival.Count; i++)
        {
            afterLateArrival[i].Timestamp.Should().BeOnOrAfter(afterLateArrival[i - 1].Timestamp);
        }

        // add another historical tick
        provider.Add(new TradeTick(
            DateTime.Parse("2023-11-09", invariantCulture).AddMinutes(10),
            110m, 20m, "EXEC-010"));

        IReadOnlyList<ITradeTick> afterSecondAdd = observer.Results.ToList();

        // Verify strict ordering is maintained after each mutation
        for (int i = 1; i < afterSecondAdd.Count; i++)
        {
            afterSecondAdd[i].Timestamp.Should().BeOnOrAfter(afterSecondAdd[i - 1].Timestamp);
        }

        // Verify content validity for final results
        afterSecondAdd.Should().AllSatisfy(t => {
            t.Timestamp.Should().NotBe(default);
            t.Price.Should().BeGreaterThan(0);
            t.Volume.Should().BeGreaterThan(0);
        });

        // cleanup
        observer.Unsubscribe();
        provider.EndTransmission();
    }

    [TestMethod]
    public void WithCachePruning_MatchesSeriesExactly()
    {
        const int maxCacheSize = 50;
        const int totalTradeTicks = 100;

        // build tick data: 100 sequential ticks at 1-minute intervals
        DateTime start = new(2023, 11, 9, 10, 0, 0);
        List<TradeTick> allTradeTicks = Enumerable
            .Range(0, totalTradeTicks)
            .Select(i => new TradeTick(
                start.AddMinutes(i),
                100m + i,
                10m + i,
                $"EXEC-{i:000}"))
            .ToList();

        // Setup TradeTickHub with cache limit
        TradeTickHub hub = new(maxCacheSize);

        // Stream more ticks than cache can hold
        foreach (TradeTick tick in allTradeTicks)
        {
            hub.Add(tick);
        }

        // Verify cache was pruned to maxCacheSize
        hub.Cache.Should().HaveCount(maxCacheSize);

        // Verify correct ticks remain (the most recent ones)
        IReadOnlyList<ITradeTick> cachedTradeTicks = hub.Cache;
        IReadOnlyList<TradeTick> expectedTradeTicks = allTradeTicks
            .TakeLast(maxCacheSize)
            .ToList();

        cachedTradeTicks.Should().HaveCount(expectedTradeTicks.Count);

        for (int i = 0; i < expectedTradeTicks.Count; i++)
        {
            cachedTradeTicks[i].Timestamp.Should().Be(expectedTradeTicks[i].Timestamp);
            cachedTradeTicks[i].Price.Should().Be(expectedTradeTicks[i].Price);
            cachedTradeTicks[i].Volume.Should().Be(expectedTradeTicks[i].Volume);
        }

        hub.EndTransmission();
    }

    /// <summary>
    /// Test observer helper class for tracking tick notifications.
    /// </summary>
    private class TestTradeTickObserver : IStreamObserver<ITradeTick>
    {
        public Action<ITradeTick, bool, int?> OnAddAction { get; set; } = (_, _, _) => { };
        public Action<DateTime> OnRebuildAction { get; set; } = _ => { };
        public bool IsSubscribed { get; private set; } = true;

        public void OnAdd(ITradeTick item, bool notify, int? indexHint)
            => OnAddAction.Invoke(item, notify, indexHint);

        public void OnCompleted() => IsSubscribed = false;

        public void OnRebuild(DateTime fromTimestamp)
            => OnRebuildAction.Invoke(fromTimestamp);

        public void OnError(Exception exception) { }

        public void OnPrune(DateTime toTimestamp) { }

        public void Unsubscribe() => IsSubscribed = false;
    }
}
