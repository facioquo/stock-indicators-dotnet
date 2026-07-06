namespace Observables;

/// <summary>
/// Tests to validate bounds-checking on Cache and ProviderCache access.
/// These tests attempt to reproduce potential IndexOutOfRangeException vulnerabilities
/// as described in Issue #1917.
/// </summary>
[TestClass]
public class BoundsCheckingTests : TestBase
{
    /// <summary>
    /// Test Case 1: Initialize AtrStopHub with exactly LookbackPeriods bars.
    /// This tests the boundary condition where warmup period just completes.
    /// </summary>
    [TestMethod]
    public void AtrStopHub_WithExactlyLookbackPeriods_ShouldNotThrow()
    {
        // Arrange - use exactly 21 bars (default LookbackPeriods for AtrStop)
        const int lookbackPeriods = 21;
        BarHub barHub = new();

        // Act - add exactly lookbackPeriods bars
        barHub.Add(Bars.Take(lookbackPeriods));

        // Create hub after data is added
        AtrStopHub observer = barHub.ToAtrStopHub(lookbackPeriods);

        // Assert - should not throw and should have correct count
        observer.Results.Should().HaveCount(lookbackPeriods);

        // Cleanup
        observer.Unsubscribe();
        barHub.EndTransmission();
    }

    /// <summary>
    /// Test Case 2: Initialize AtrStopHub with one less than LookbackPeriods bars,
    /// then add one more to reach exactly the boundary.
    /// </summary>
    [TestMethod]
    public void AtrStopHub_AddBarToReachBoundary_ShouldNotThrow()
    {
        // Arrange - use one less than lookbackPeriods
        const int lookbackPeriods = 21;
        BarHub barHub = new();

        // Initialize with one less than lookback
        barHub.Add(Bars.Take(lookbackPeriods - 1));
        AtrStopHub observer = barHub.ToAtrStopHub(lookbackPeriods);

        // Act - add the bar that reaches exactly lookbackPeriods
        barHub.Add(Bars[lookbackPeriods - 1]);

        // Assert - should not throw and should have correct count
        observer.Results.Should().HaveCount(lookbackPeriods);

        // Cleanup
        observer.Unsubscribe();
        barHub.EndTransmission();
    }

    /// <summary>
    /// Test Case 3: Remove a bar from the middle and trigger rebuild.
    /// This tests RollbackState when i > LookbackPeriods.
    /// </summary>
    [TestMethod]
    public void AtrStopHub_RemoveBarAndRebuild_ShouldNotThrow()
    {
        // Arrange
        const int lookbackPeriods = 21;
        BarHub barHub = new();
        barHub.Add(Bars.Take(50));

        AtrStopHub observer = barHub.ToAtrStopHub(lookbackPeriods);

        // Act - remove a bar in the middle (after warmup period)
        // This should trigger RollbackState with i > LookbackPeriods
        barHub.RemoveAt(30);

        // Assert - should not throw
        observer.Results.Should().HaveCount(49);

        // Cleanup
        observer.Unsubscribe();
        barHub.EndTransmission();
    }

    /// <summary>
    /// Test Case 4: Add a late arrival bar that triggers rebuild.
    /// This tests the scenario where rebuild starts from a middle index.
    /// </summary>
    [TestMethod]
    public void AtrStopHub_AddLateArrival_ShouldNotThrow()
    {
        // Arrange
        const int lookbackPeriods = 21;
        BarHub barHub = new();

        // Add bars but skip one in the middle
        for (int i = 0; i < 50; i++)
        {
            if (i == 30)
            {
                continue;  // Skip index 30
            }

            barHub.Add(Bars[i]);
        }

        AtrStopHub observer = barHub.ToAtrStopHub(lookbackPeriods);
        observer.Results.Should().HaveCount(49);

        // Act - insert the late arrival
        barHub.Add(Bars[30]);

        // Assert - should not throw and should have all bars
        observer.Results.Should().HaveCount(50);

        // Cleanup
        observer.Unsubscribe();
        barHub.EndTransmission();
    }

    /// <summary>
    /// Test Case 5: Test with minimum valid LookbackPeriods for AtrStop (2).
    /// </summary>
    [TestMethod]
    public void AtrStopHub_WithMinimumLookbackPeriods_ShouldNotThrow()
    {
        // Arrange - use minimum valid lookback period of 2 for AtrStop
        const int lookbackPeriods = 2;
        BarHub barHub = new();
        barHub.Add(Bars.Take(10));

        // Act - create hub with minimum lookback
        AtrStopHub observer = barHub.ToAtrStopHub(lookbackPeriods);

        // Assert - should not throw
        observer.Results.Should().HaveCount(10);

        // Cleanup
        observer.Unsubscribe();
        barHub.EndTransmission();
    }

    /// <summary>
    /// Test Case 6: Rapid add/remove operations around warmup boundary.
    /// </summary>
    [TestMethod]
    public void AtrStopHub_RapidOperationsAroundBoundary_ShouldNotThrow()
    {
        // Arrange
        const int lookbackPeriods = 21;
        BarHub barHub = new();

        // Pre-fill with warmup data (26 bars initially)
        barHub.Add(Bars.Take(lookbackPeriods + 5));
        AtrStopHub observer = barHub.ToAtrStopHub(lookbackPeriods);
        int initialCount = observer.Results.Count;  // Should be 26

        // Act - add more bars (15 more)
        for (int i = lookbackPeriods + 5; i < lookbackPeriods + 20; i++)
        {
            barHub.Add(Bars[i]);
        }

        int afterAddCount = observer.Results.Count;  // Should be 41

        // Remove some bars (2 removals)
        barHub.RemoveAt(25);
        barHub.RemoveAt(20);

        // Assert - should not throw and count should be afterAddCount - 2
        observer.Results.Should().HaveCount(afterAddCount - 2);

        // Cleanup
        observer.Unsubscribe();
        barHub.EndTransmission();
    }

    /// <summary>
    /// Test Case 7: Test ATR Hub (simpler case with same pattern).
    /// </summary>
    [TestMethod]
    public void AtrHub_RemoveAndRebuild_ShouldNotThrow()
    {
        // Arrange
        const int lookbackPeriods = 14;
        BarHub barHub = new();
        barHub.Add(Bars.Take(50));

        AtrHub observer = barHub.ToAtrHub(lookbackPeriods);

        // Act - remove a bar after warmup
        barHub.RemoveAt(30);

        // Assert - should not throw
        observer.Results.Should().HaveCount(49);

        // Cleanup
        observer.Unsubscribe();
        barHub.EndTransmission();
    }

    /// <summary>
    /// Test Case 8: Test Keltner Hub (uses both Cache and ProviderCache[i-1]).
    /// </summary>
    [TestMethod]
    public void KeltnerHub_RemoveAndRebuild_ShouldNotThrow()
    {
        // Arrange
        const int emaPeriods = 20;
        const int atrPeriods = 10;
        BarHub barHub = new();
        barHub.Add(Bars.Take(50));

        KeltnerHub observer = barHub.ToKeltnerHub(emaPeriods, 2, atrPeriods);

        // Act - remove a bar after warmup
        barHub.RemoveAt(35);

        // Assert - should not throw
        observer.Results.Should().HaveCount(49);

        // Cleanup
        observer.Unsubscribe();
        barHub.EndTransmission();
    }

    /// <summary>
    /// Test Case 9: Test Stochastic Hub (has complex cache access patterns).
    /// </summary>
    [TestMethod]
    public void StochHub_RemoveAndRebuild_ShouldNotThrow()
    {
        // Arrange
        const int lookbackPeriods = 14;
        BarHub barHub = new();
        barHub.Add(Bars.Take(50));

        StochHub observer = barHub.ToStochHub(lookbackPeriods);

        // Act - remove a bar after warmup
        barHub.RemoveAt(30);

        // Assert - should not throw
        observer.Results.Should().HaveCount(49);

        // Cleanup
        observer.Unsubscribe();
        barHub.EndTransmission();
    }

    /// <summary>
    /// Test Case 10: Test empty provider scenario.
    /// </summary>
    [TestMethod]
    public void AtrStopHub_EmptyProvider_ShouldNotThrow()
    {
        // Arrange
        BarHub barHub = new();

        // Act - create hub with empty provider
        AtrStopHub observer = barHub.ToAtrStopHub();

        // Assert - should not throw and should be empty
        observer.Results.Should().BeEmpty();

        // Cleanup
        observer.Unsubscribe();
        barHub.EndTransmission();
    }

    /// <summary>
    /// Test Case 11: Test single bar scenario.
    /// </summary>
    [TestMethod]
    public void AtrStopHub_SingleBar_ShouldNotThrow()
    {
        // Arrange
        BarHub barHub = new();
        barHub.Add(Bars.Take(1));

        // Act - create hub with single bar
        AtrStopHub observer = barHub.ToAtrStopHub();

        // Assert - should not throw
        observer.Results.Should().HaveCount(1);

        // Cleanup
        observer.Unsubscribe();
        barHub.EndTransmission();
    }

    /// <summary>
    /// Test Case 12: Test rebuild from middle with pruned cache.
    /// Tests the scenario where MaxCacheSize causes pruning before a rebuild.
    /// </summary>
    [TestMethod]
    public void AtrStopHub_PrunedCacheWithRebuild_ShouldNotThrow()
    {
        // Arrange - use small max cache size
        const int maxCacheSize = 40;
        const int lookbackPeriods = 21;
        BarHub barHub = new(maxCacheSize);
        AtrStopHub observer = barHub.ToAtrStopHub(lookbackPeriods);

        // Add enough bars to trigger pruning
        barHub.Add(Bars.Take(60));

        // At this point, cache should be pruned to maxCacheSize
        observer.Results.Should().HaveCount(maxCacheSize);

        // Act - remove a bar and trigger rebuild
        const int removeIndex = 30;
        barHub.RemoveAt(removeIndex);

        // Assert - should not throw
        observer.Results.Should().HaveCount(maxCacheSize - 1);

        // Cleanup
        observer.Unsubscribe();
        barHub.EndTransmission();
    }

    /// <summary>
    /// A subscribed (non-root) hub is driven by its provider, so reinitializing
    /// it directly is rejected. Reinitialize the root hub (or rebuild) instead.
    /// </summary>
    [TestMethod]
    public void AtrStopHub_ReinitializeOnSubscribedHub_Throws()
    {
        // Arrange
        const int lookbackPeriods = 21;
        BarHub barHub = new();
        barHub.Add(Bars.Take(50));

        AtrStopHub observer = barHub.ToAtrStopHub(lookbackPeriods);
        observer.Results.Should().HaveCount(50);

        // Act / Assert - reinitializing a subscribed hub is forbidden
        Assert.ThrowsExactly<InvalidOperationException>(observer.Reinitialize);

        // Cleanup
        observer.Unsubscribe();
        barHub.EndTransmission();
    }

    /// <summary>
    /// Test Case 14: Test multiple late arrivals in sequence.
    /// </summary>
    [TestMethod]
    public void AtrStopHub_MultipleLateArrivals_ShouldNotThrow()
    {
        // Arrange
        const int lookbackPeriods = 21;
        BarHub barHub = new();

        // Add bars but skip several in the middle
        int[] skipIndices = [25, 30, 35];
        for (int i = 0; i < 50; i++)
        {
            if (skipIndices.Contains(i))
            {
                continue;
            }

            barHub.Add(Bars[i]);
        }

        AtrStopHub observer = barHub.ToAtrStopHub(lookbackPeriods);
        observer.Results.Should().HaveCount(47);

        // Act - insert late arrivals in reverse order
        foreach (int idx in skipIndices.Reverse())
        {
            barHub.Add(Bars[idx]);
        }

        // Assert - should not throw and should have all bars
        observer.Results.Should().HaveCount(50);

        // Cleanup
        observer.Unsubscribe();
        barHub.EndTransmission();
    }

    /// <summary>
    /// Test Case 15: Test SuperTrend Hub (complex state-dependent indicator).
    /// </summary>
    [TestMethod]
    public void SuperTrendHub_RemoveAndRebuild_ShouldNotThrow()
    {
        // Arrange
        const int atrPeriods = 10;
        BarHub barHub = new();
        barHub.Add(Bars.Take(50));

        SuperTrendHub observer = barHub.ToSuperTrendHub(atrPeriods);

        // Act - remove a bar after warmup
        barHub.RemoveAt(30);

        // Assert - should not throw
        observer.Results.Should().HaveCount(49);

        // Cleanup
        observer.Unsubscribe();
        barHub.EndTransmission();
    }

    /// <summary>
    /// Test Case 16: Initialize observer BEFORE adding data to provider.
    /// This tests the scenario where provider is empty at observer creation.
    /// </summary>
    [TestMethod]
    public void AtrStopHub_InitializeBeforeData_ShouldNotThrow()
    {
        // Arrange
        const int lookbackPeriods = 21;
        BarHub barHub = new();

        // Create observer with empty provider
        AtrStopHub observer = barHub.ToAtrStopHub(lookbackPeriods);
        observer.Results.Should().BeEmpty();

        // Act - now add data
        barHub.Add(Bars.Take(50));

        // Assert - should not throw and should have all bars
        observer.Results.Should().HaveCount(50);

        // Cleanup
        observer.Unsubscribe();
        barHub.EndTransmission();
    }

    /// <summary>
    /// Test Case 17: Add bars one by one, then remove one at the boundary.
    /// </summary>
    [TestMethod]
    public void AtrStopHub_AddOneByOneThenRemoveAtBoundary_ShouldNotThrow()
    {
        // Arrange
        const int lookbackPeriods = 21;
        BarHub barHub = new();
        AtrStopHub observer = barHub.ToAtrStopHub(lookbackPeriods);

        // Add bars one by one
        for (int i = 0; i < lookbackPeriods + 5; i++)
        {
            barHub.Add(Bars[i]);
        }

        // Act - remove exactly at the boundary (index = lookbackPeriods)
        barHub.RemoveAt(lookbackPeriods);

        // Assert - should not throw
        observer.Results.Should().HaveCount(lookbackPeriods + 4);

        // Cleanup
        observer.Unsubscribe();
        barHub.EndTransmission();
    }

    /// <summary>
    /// Test Case 18: Test Rebuild() method directly.
    /// </summary>
    [TestMethod]
    public void AtrStopHub_DirectRebuild_ShouldNotThrow()
    {
        // Arrange
        const int lookbackPeriods = 21;
        BarHub barHub = new();
        barHub.Add(Bars.Take(50));
        AtrStopHub observer = barHub.ToAtrStopHub(lookbackPeriods);

        // Act - call Rebuild directly
        observer.Rebuild();

        // Assert - should not throw and should have same count
        observer.Results.Should().HaveCount(50);

        // Cleanup
        observer.Unsubscribe();
        barHub.EndTransmission();
    }

    /// <summary>
    /// Test Case 19: Test Rebuild from a specific timestamp.
    /// </summary>
    [TestMethod]
    public void AtrStopHub_RebuildFromTimestamp_ShouldNotThrow()
    {
        // Arrange
        const int lookbackPeriods = 21;
        BarHub barHub = new();
        barHub.Add(Bars.Take(50));
        AtrStopHub observer = barHub.ToAtrStopHub(lookbackPeriods);

        // Get a timestamp in the middle (after warmup)
        DateTime rebuildTimestamp = Bars[30].Timestamp;

        // Act - rebuild from specific timestamp
        observer.Rebuild(rebuildTimestamp);

        // Assert - should not throw and should have same count
        observer.Results.Should().HaveCount(50);

        // Cleanup
        observer.Unsubscribe();
        barHub.EndTransmission();
    }

    /// <summary>
    /// Test Case 20: Chained StreamHubs - test cascading rebuilds.
    /// </summary>
    [TestMethod]
    public void ChainedHubs_RemoveAndRebuild_ShouldNotThrow()
    {
        // Arrange
        BarHub barHub = new();
        barHub.Add(Bars.Take(50));

        // Create a chain: BarHub -> AtrHub -> SmaHub
        AtrHub atrHub = barHub.ToAtrHub(14);
        SmaHub smaHub = atrHub.ToSmaHub(10);

        // Act - remove a bar, which should cascade rebuild through the chain
        barHub.RemoveAt(35);

        // Assert - should not throw
        atrHub.Results.Should().HaveCount(49);
        smaHub.Results.Should().HaveCount(49);

        // Cleanup
        smaHub.Unsubscribe();
        atrHub.Unsubscribe();
        barHub.EndTransmission();
    }

    /// <summary>
    /// Test Case 21: Test with very small data set (less than warmup period).
    /// </summary>
    [TestMethod]
    public void AtrStopHub_SmallDataSet_ShouldNotThrow()
    {
        // Arrange
        const int lookbackPeriods = 21;
        BarHub barHub = new();

        // Add only 5 bars (less than lookback period)
        barHub.Add(Bars.Take(5));
        AtrStopHub observer = barHub.ToAtrStopHub(lookbackPeriods);

        // Act - try to remove from small dataset
        barHub.RemoveAt(2);

        // Assert - should not throw
        observer.Results.Should().HaveCount(4);

        // Cleanup
        observer.Unsubscribe();
        barHub.EndTransmission();
    }

    /// <summary>
    /// Test Case 22: Remove all bars after warmup.
    /// </summary>
    [TestMethod]
    public void AtrStopHub_RemoveAllAfterWarmup_ShouldNotThrow()
    {
        // Arrange
        const int lookbackPeriods = 21;
        BarHub barHub = new();
        barHub.Add(Bars.Take(30));
        AtrStopHub observer = barHub.ToAtrStopHub(lookbackPeriods);

        // Act - remove all bars after warmup period (indices 21-29)
        for (int i = 29; i >= lookbackPeriods; i--)
        {
            barHub.RemoveAt(i);
        }

        // Assert - should not throw and should have only warmup bars left
        observer.Results.Should().HaveCount(lookbackPeriods);

        // Cleanup
        observer.Unsubscribe();
        barHub.EndTransmission();
    }

    /// <summary>
    /// Test Case 23: Remove bars from the beginning.
    /// </summary>
    [TestMethod]
    public void AtrStopHub_RemoveFromBeginning_ShouldNotThrow()
    {
        // Arrange
        const int lookbackPeriods = 21;
        BarHub barHub = new();
        barHub.Add(Bars.Take(50));
        AtrStopHub observer = barHub.ToAtrStopHub(lookbackPeriods);

        // Act - remove first few bars
        for (int i = 0; i < 5; i++)
        {
            barHub.RemoveAt(0);
        }

        // Assert - should not throw
        observer.Results.Should().HaveCount(45);

        // Cleanup
        observer.Unsubscribe();
        barHub.EndTransmission();
    }

    /// <summary>
    /// Test Case 24: Test VolatilityStop Hub (similar pattern to AtrStop).
    /// </summary>
    [TestMethod]
    public void VolatilityStopHub_RemoveAndRebuild_ShouldNotThrow()
    {
        // Arrange
        const int lookbackPeriods = 7;
        BarHub barHub = new();
        barHub.Add(Bars.Take(50));

        VolatilityStopHub observer = barHub.ToVolatilityStopHub(lookbackPeriods);

        // Act - remove a bar after warmup
        barHub.RemoveAt(30);

        // Assert - should not throw
        observer.Results.Should().HaveCount(49);

        // Cleanup
        observer.Unsubscribe();
        barHub.EndTransmission();
    }

    /// <summary>
    /// Test Case 25: Test Chandelier Hub.
    /// </summary>
    [TestMethod]
    public void ChandelierHub_RemoveAndRebuild_ShouldNotThrow()
    {
        // Arrange
        const int lookbackPeriods = 22;
        BarHub barHub = new();
        barHub.Add(Bars.Take(50));

        ChandelierHub observer = barHub.ToChandelierHub(lookbackPeriods);

        // Act - remove a bar after warmup
        barHub.RemoveAt(35);

        // Assert - should not throw
        observer.Results.Should().HaveCount(49);

        // Cleanup
        observer.Unsubscribe();
        barHub.EndTransmission();
    }

    /// <summary>
    /// Test Case 26: Test Renko Hub (asymmetric - can produce 0 or many bricks per bar).
    /// </summary>
    [TestMethod]
    public void RenkoHub_RemoveAndRebuild_ShouldNotThrow()
    {
        // Arrange
        const decimal brickSize = 2.5m;
        BarHub barHub = new();
        barHub.Add(Bars.Take(100));

        RenkoHub observer = barHub.ToRenkoHub(brickSize);
        int initialBrickCount = observer.Results.Count;

        // Act - remove a bar in the middle
        barHub.RemoveAt(50);

        // Assert - should not throw
        // Renko count may vary since it's asymmetric
        observer.Results.Should().NotBeNull();

        // Cleanup
        observer.Unsubscribe();
        barHub.EndTransmission();
    }

    /// <summary>
    /// Test Case 27: Test Renko Hub with small brick size (more bricks).
    /// </summary>
    [TestMethod]
    public void RenkoHub_SmallBrickSize_RemoveAndRebuild_ShouldNotThrow()
    {
        // Arrange
        const decimal brickSize = 0.5m;
        BarHub barHub = new();
        barHub.Add(Bars.Take(50));

        RenkoHub observer = barHub.ToRenkoHub(brickSize);

        // Act - remove a bar
        barHub.RemoveAt(25);

        // Assert - should not throw
        observer.Results.Should().NotBeNull();

        // Cleanup
        observer.Unsubscribe();
        barHub.EndTransmission();
    }

    /// <summary>
    /// RemoveRange(timestamp) on a subscribed (non-root) hub is rejected; the
    /// hub is driven by its provider. Remove from the root hub instead.
    /// </summary>
    [TestMethod]
    public void AtrStopHub_RemoveRangeOnSubscribedHub_Throws()
    {
        // Arrange
        const int lookbackPeriods = 21;
        BarHub barHub = new();
        barHub.Add(Bars.Take(50));
        AtrStopHub observer = barHub.ToAtrStopHub(lookbackPeriods);

        // Get timestamp to remove from
        DateTime removeFromTimestamp = Bars[30].Timestamp;

        // Act / Assert - mutating a subscribed hub is forbidden
        Assert.ThrowsExactly<InvalidOperationException>(
            () => observer.RemoveRange(removeFromTimestamp, notify: true));

        // hub is unchanged
        observer.Results.Should().HaveCount(50);

        // Cleanup
        observer.Unsubscribe();
        barHub.EndTransmission();
    }

    /// <summary>
    /// RemoveRange(index) on a subscribed (non-root) hub is rejected; the hub
    /// is driven by its provider. Remove from the root hub instead.
    /// </summary>
    [TestMethod]
    public void AtrStopHub_RemoveRangeByIndexOnSubscribedHub_Throws()
    {
        // Arrange
        const int lookbackPeriods = 21;
        BarHub barHub = new();
        barHub.Add(Bars.Take(50));
        AtrStopHub observer = barHub.ToAtrStopHub(lookbackPeriods);

        // Act / Assert - mutating a subscribed hub is forbidden
        Assert.ThrowsExactly<InvalidOperationException>(
            () => observer.RemoveRange(30, notify: true));

        // hub is unchanged
        observer.Results.Should().HaveCount(50);

        // Cleanup
        observer.Unsubscribe();
        barHub.EndTransmission();
    }

    /// <summary>
    /// RemoveAt on a subscribed (non-root) hub is rejected; the hub is driven
    /// by its provider. Remove from the root hub instead.
    /// </summary>
    [TestMethod]
    public void AtrStopHub_RemoveAtOnSubscribedHub_Throws()
    {
        // Arrange
        const int lookbackPeriods = 21;
        BarHub barHub = new();
        barHub.Add(Bars.Take(50));
        AtrStopHub observer = barHub.ToAtrStopHub(lookbackPeriods);

        // Act / Assert - mutating a subscribed hub is forbidden
        Assert.ThrowsExactly<InvalidOperationException>(
            () => observer.RemoveAt(30));

        // hub is unchanged
        observer.Results.Should().HaveCount(50);

        // Cleanup
        observer.Unsubscribe();
        barHub.EndTransmission();
    }

    /// <summary>
    /// Test Case 31: A removal cascading through a deep chain must leave every
    /// level bit-for-bit equal to the equivalent batch Series chain (not just
    /// the right count) — the rollback engine is value-exact end to end.
    /// </summary>
    [TestMethod]
    public void DeeplyChainedHubs_RemoveAndRebuild_MatchesSeries()
    {
        // Arrange
        List<Bar> bars = Bars.Take(100).ToList();
        BarHub barHub = new();
        barHub.Add(bars);

        // Create a deep chain: BarHub -> AtrHub -> EmaHub -> SmaHub
        AtrHub atrHub = barHub.ToAtrHub(14);
        EmaHub emaHub = atrHub.ToEmaHub(10);
        SmaHub smaHub = emaHub.ToSmaHub(5);

        // Act - remove a bar, triggering cascade through all levels
        const int removeIndex = 60;
        barHub.RemoveAt(removeIndex);

        // Assert - every level matches the equivalent batch chain on the revised bars
        List<Bar> revised = [.. bars];
        revised.RemoveAt(removeIndex);

        IReadOnlyList<AtrResult> expectedAtr = revised.ToAtr(14);
        IReadOnlyList<EmaResult> expectedEma = expectedAtr.ToEma(10);
        IReadOnlyList<SmaResult> expectedSma = expectedEma.ToSma(5);

        atrHub.Results.Should().HaveCount(99);
        atrHub.Results.IsExactly(expectedAtr);
        emaHub.Results.IsExactly(expectedEma);
        smaHub.Results.IsExactly(expectedSma);

        // Cleanup
        smaHub.Unsubscribe();
        emaHub.Unsubscribe();
        atrHub.Unsubscribe();
        barHub.EndTransmission();
    }

    /// <summary>
    /// Test Case 32: Test concurrent operations (add and remove).
    /// </summary>
    [TestMethod]
    public void AtrStopHub_ConcurrentAddAndRemove_ShouldNotThrow()
    {
        // Arrange
        const int lookbackPeriods = 21;
        BarHub barHub = new();
        barHub.Add(Bars.Take(40));
        AtrStopHub observer = barHub.ToAtrStopHub(lookbackPeriods);

        // Act - interleaved add and remove operations
        for (int i = 40; i < 60; i++)
        {
            barHub.Add(Bars[i]);
            if (i % 5 == 0 && barHub.Bars.Count > lookbackPeriods + 5)
            {
                // Remove from the middle occasionally
                barHub.RemoveAt(lookbackPeriods + 2);
            }
        }

        // Assert - should not throw
        observer.Results.Should().NotBeNull();
        observer.Results.Should().NotBeEmpty();

        // Cleanup
        observer.Unsubscribe();
        barHub.EndTransmission();
    }

    /// <summary>
    /// Test Case 33: Attempt to trigger RollbackState edge case.
    /// Tests scenario where RollbackState is called with i > LookbackPeriods
    /// but Cache might be in an unexpected state.
    /// </summary>
    [TestMethod]
    public void AtrStopHub_RollbackStateEdgeCase_ShouldNotThrow()
    {
        // Arrange
        const int lookbackPeriods = 10;  // Small lookback for easier testing
        BarHub barHub = new();

        // Add just enough bars to get past lookback
        barHub.Add(Bars.Take(15));
        AtrStopHub observer = barHub.ToAtrStopHub(lookbackPeriods);

        // Verify initial state
        observer.Results.Should().HaveCount(15);

        // Act - remove a bar after warmup, triggering RollbackState
        // This should call RollbackState with i > lookbackPeriods
        barHub.RemoveAt(12);

        // Assert - should not throw
        observer.Results.Should().HaveCount(14);

        // Cleanup
        observer.Unsubscribe();
        barHub.EndTransmission();
    }

    /// <summary>
    /// Test Case 34: Test with BadBars data (edge case data).
    /// </summary>
    [TestMethod]
    public void AtrStopHub_WithBadBars_ShouldNotThrow()
    {
        // Arrange - BadBars may have unusual values
        const int lookbackPeriods = 21;
        BarHub barHub = new();
        barHub.Add(BadBars);

        // Act - create hub
        AtrStopHub observer = barHub.ToAtrStopHub(lookbackPeriods);

        // Assert - should not throw (count may differ from input due to duplicates)
        observer.Results.Should().NotBeNull();
        observer.Results.Should().NotBeEmpty();

        // Cleanup
        observer.Unsubscribe();
        barHub.EndTransmission();
    }

    /// <summary>
    /// Test Case 35: Test rebuild at exactly i = LookbackPeriods.
    /// </summary>
    [TestMethod]
    public void AtrStopHub_RebuildAtExactlyLookback_ShouldNotThrow()
    {
        // Arrange
        const int lookbackPeriods = 21;
        BarHub barHub = new();
        barHub.Add(Bars.Take(50));
        AtrStopHub observer = barHub.ToAtrStopHub(lookbackPeriods);

        // Get timestamp at exactly lookback
        DateTime rebuildTimestamp = Bars[lookbackPeriods].Timestamp;

        // Act - rebuild from exactly lookback
        observer.Rebuild(rebuildTimestamp);

        // Assert - should not throw
        observer.Results.Should().HaveCount(50);

        // Cleanup
        observer.Unsubscribe();
        barHub.EndTransmission();
    }
}
