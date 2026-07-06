namespace Behavioral;

[TestClass]
public class MinCacheSizeTests : TestBase
{
    [TestMethod]
    public void MinCacheSize_InitializedToZero()
    {
        // Arrange & Act
        BarHub barHub = new(maxCacheSize: 100);

        // Assert
        barHub.MinCacheSize.Should().Be(0, "BarHub should start with MinCacheSize of 0");
    }

    [TestMethod]
    public void MinCacheSize_PropagatesFromSubscriber()
    {
        // Arrange
        BarHub barHub = new(maxCacheSize: 100);
        barHub.MinCacheSize.Should().Be(0);

        // Act - Subscribe an SMA hub which requires warmup
        SmaHub smaHub = barHub.ToSmaHub(20);

        // Assert
        barHub.MinCacheSize.Should().BeGreaterThan(0, "BarHub should inherit MinCacheSize from SMA subscriber");
        barHub.MinCacheSize.Should().Be(smaHub.MinCacheSize, "BarHub MinCacheSize should match subscriber's requirement");
    }

    [TestMethod]
    public void MinCacheSize_TracksMaximumFromMultipleSubscribers()
    {
        // Arrange
        BarHub barHub = new(maxCacheSize: 200);

        // Act - Subscribe multiple hubs with different warmup requirements
        SmaHub smaHub10 = barHub.ToSmaHub(10);
        SmaHub smaHub20 = barHub.ToSmaHub(20);
        SmaHub smaHub50 = barHub.ToSmaHub(50);

        // Assert
        int maxSubscriberMin = Math.Max(smaHub10.MinCacheSize, Math.Max(smaHub20.MinCacheSize, smaHub50.MinCacheSize));
        barHub.MinCacheSize.Should().Be(maxSubscriberMin, "BarHub should track maximum MinCacheSize from all subscribers");
    }

    [TestMethod]
    public void MinCacheSize_ReEvaluatedOnUnsubscribe()
    {
        // Arrange
        BarHub barHub = new(maxCacheSize: 200);
        SmaHub smaHub20 = barHub.ToSmaHub(20);
        SmaHub smaHub50 = barHub.ToSmaHub(50);
        int initialMinCacheSize = barHub.MinCacheSize;

        // Act - Unsubscribe the hub with larger requirement
        smaHub50.Unsubscribe();

        // Assert
        barHub.MinCacheSize.Should().BeLessOrEqualTo(initialMinCacheSize, "MinCacheSize should be re-evaluated after unsubscribe");
        barHub.MinCacheSize.Should().Be(smaHub20.MinCacheSize, "MinCacheSize should match remaining subscriber");
    }

    [TestMethod]
    public void MinCacheSize_ReducedToZeroWhenAllUnsubscribed()
    {
        // Arrange
        BarHub barHub = new(maxCacheSize: 200);
        SmaHub smaHub20 = barHub.ToSmaHub(20);
        barHub.MinCacheSize.Should().BeGreaterThan(0);

        // Act
        smaHub20.Unsubscribe();

        // Assert
        barHub.MinCacheSize.Should().Be(0, "MinCacheSize should be 0 when no subscribers remain");
    }

    [TestMethod]
    public void RejectInsertionsBeforeCacheTimeline()
    {
        // Arrange
        IReadOnlyList<Bar> bars = Data.GetDefault();
        BarHub barHub = new(maxCacheSize: 50);

        // Add first 100 bars (cache will have last 50)
        for (int i = 0; i < 100; i++)
        {
            barHub.Add(bars[i]);
        }

        int initialCacheSize = barHub.Results.Count;
        DateTime firstCachedTimestamp = barHub.Results[0].Timestamp;

        // Act - Try to add a bar before the current cache timeline
        Bar oldBar = bars[10]; // This should be before the cache
        barHub.Add(oldBar);

        // Assert
        barHub.Results.Count.Should().Be(initialCacheSize, "Cache size should not change");
        barHub.Results[0].Timestamp.Should().Be(firstCachedTimestamp, "First cached item should not change");
    }

    [TestMethod]
    public void ApplyRevisionsBeforeMinCacheSize()
    {
        // Arrange
        IReadOnlyList<Bar> bars = Data.GetDefault();
        BarHub barHub = new(maxCacheSize: 200);

        // Subscribe an indicator that requires warmup
        _ = barHub.ToSmaHub(20);

        // Add bars to build cache
        for (int i = 0; i < 100; i++)
        {
            barHub.Add(bars[i]);
        }

        int minCacheSize = barHub.MinCacheSize;
        minCacheSize.Should().BeGreaterThan(0, "MinCacheSize should be set from SMA warmup requirement");

        int initialCacheSize = barHub.Results.Count;

        // Act - Submit a same-timestamp revision at an index inside the warmup window
        // (before MinCacheSize). A revision of an already-cached bar must NOT be silently
        // dropped: it routes through the replace+rebuild path so the cache stays faithful
        // to the source and downstream indicator state is recomputed from that point.
        int reviseIndex = minCacheSize - 1;
        IBar originalBar = barHub.Results[reviseIndex];
        Bar revisedBar = new(
            originalBar.Timestamp,
            originalBar.Open * 0.99m,  // Different value to make the revision observable
            originalBar.High,
            originalBar.Low,
            originalBar.Close,
            originalBar.Volume);

        barHub.Add(revisedBar);

        // Assert - The revision is applied. Count is unchanged because a same-timestamp
        // revision replaces the existing entry rather than inserting a new one.
        barHub.Results.Count.Should().Be(initialCacheSize, "Same-timestamp revision replaces, it does not grow the cache");
        barHub.Results[reviseIndex].Open.Should().Be(originalBar.Open * 0.99m, "Revision before MinCacheSize should be applied, not dropped");
    }

    [TestMethod]
    public void AllowInsertionsAfterMinCacheSize()
    {
        // Arrange
        IReadOnlyList<Bar> bars = Data.GetDefault();
        BarHub barHub = new(maxCacheSize: 200);

        // Subscribe an indicator that requires warmup
        _ = barHub.ToSmaHub(20);

        // Add bars to build cache
        for (int i = 0; i < 100; i++)
        {
            barHub.Add(bars[i]);
        }

        int minCacheSize = barHub.MinCacheSize;
        int insertIndex = minCacheSize + 5;

        // Ensure we have enough bars
        insertIndex.Should().BeLessThan(barHub.Results.Count, "Need sufficient cache for safe insertion");

        // Act - Insert a bar at a safe position (after MinCacheSize)
        // Create a revision with same timestamp but different value
        DateTime targetTimestamp = barHub.Results[insertIndex].Timestamp;
        decimal originalOpen = barHub.Results[insertIndex].Open;
        IBar cachedBar = barHub.Results[insertIndex];
        Bar insertBar = new(
            targetTimestamp,
            originalOpen * 1.01m,  // Changed value
            cachedBar.High,
            cachedBar.Low,
            cachedBar.Close,
            cachedBar.Volume);

        barHub.Add(insertBar);

        // Assert - The bar revision should be accepted
        barHub.Results.Should().Contain(q => q.Timestamp == targetTimestamp);
        IBar updatedBar = barHub.Results.First(q => q.Timestamp == targetTimestamp);
        updatedBar.Open.Should().Be(originalOpen * 1.01m, "Revision should have updated the Open value");
    }

    [TestMethod]
    public void MinCacheSize_ChainedIndicatorsPropagate()
    {
        // Arrange
        BarHub barHub = new(maxCacheSize: 300);

        // Act - Create a chain: BarHub -> SMA(20) -> EMA(10)
        SmaHub smaHub = barHub.ToSmaHub(20);
        EmaHub emaHub = smaHub.ToEmaHub(10);

        // Assert
        // BarHub should have the maximum MinCacheSize requirement from the chain
        barHub.MinCacheSize.Should().BeGreaterThan(0, "BarHub should inherit MinCacheSize from subscribers");
        smaHub.MinCacheSize.Should().BeGreaterThan(0, "SmaHub should have its own MinCacheSize requirement");
        emaHub.MinCacheSize.Should().BeGreaterThan(0, "EmaHub should have MinCacheSize propagated to it");

        // Verify propagation through the chain
        barHub.MinCacheSize.Should().BeGreaterOrEqualTo(smaHub.MinCacheSize, "BarHub should track SmaHub requirement");
        smaHub.MinCacheSize.Should().BeGreaterOrEqualTo(emaHub.MinCacheSize, "SmaHub should track EmaHub requirement");
        barHub.MinCacheSize.Should().BeGreaterOrEqualTo(emaHub.MinCacheSize, "BarHub should track EmaHub requirement through chain");
    }
}
