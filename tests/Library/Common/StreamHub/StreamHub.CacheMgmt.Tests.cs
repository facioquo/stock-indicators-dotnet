namespace Observables;

[TestClass]
public class CacheManagement : TestBase
{
    [TestMethod]
    public void Remove()
    {
        BarHub barHub = new();
        SmaHub observer = barHub.ToSmaHub(20);

        List<Bar> bars = Bars.Take(21).ToList();

        barHub.Add(bars);

        Console.WriteLine(observer.Results.ToStringOut());

        // Verify StreamHub matches Series for same input
        IReadOnlyList<SmaResult> seriesBeforeRemove = bars.ToSma(20);
        observer.Results[19].Sma.Should().Be(seriesBeforeRemove[19].Sma);

        // Create new bar list with the removed item (more efficient than LINQ Where)
        List<Bar> barsAfterRemove = [.. bars];
        barsAfterRemove.RemoveAt(14);

        barHub.RemoveAt(14);
        barHub.EndTransmission();

        Console.WriteLine(observer.Results.ToStringOut());

        // After removal, we have 20 bars, period is 20, so SMA starts at index 19
        // StreamHub result at index 19 should match Series result at index 19 (last element)
        IReadOnlyList<SmaResult> seriesAfterRemove = barsAfterRemove.ToSma(20);
        observer.Results[19].Sma.Should().Be(seriesAfterRemove[19].Sma);
    }

    /// <summary>
    /// late arrival
    /// </summary>
    [TestMethod]
    public void ActAddOld()
    {
        int length = Bars.Count;

        // add base bars
        BarHub barHub = new();

        BarPartHub observer = barHub
            .ToBarPartHub(CandlePart.Close);

        // emulate incremental bars
        for (int i = 0; i < length; i++)
        {
            // skip one
            if (i == 100)
            {
                continue;
            }

            Bar q = Bars[i];
            barHub.Add(q);
        }

        // add late
        barHub.Add(Bars[100]);

        // assert same as original
        for (int i = 0; i < length; i++)
        {
            Bar q = Bars[i];
            TimeValue r = observer.Cache[i];

            // compare bar to result cache
            r.Timestamp.Should().Be(q.Timestamp);
            r.Value.Should().Be(q.Value);
        }

        // close observations
        barHub.EndTransmission();
    }

    [TestMethod]
    public void Overflowing()
    {
        // initialize
        BarHub barHub = new();

        Bar dup = new(
            Timestamp: DateTime.Now,
            Open: 1.00m,
            High: 2.00m,
            Low: 0.50m,
            Close: 1.75m,
            Volume: 1000);

        BarPartHub observer = barHub
            .ToBarPartHub(CandlePart.Close);

        // overflowing, under threshold
        for (int i = 0; i <= 100; i++)
        {
            barHub.Add(dup);
        }

        // assert: no fault, no overflow (yet)

        barHub.Bars.Should().HaveCount(1);
        observer.Results.Should().HaveCount(1);
        barHub.IsFaulted.Should().BeFalse();
        barHub.OverflowCount.Should().Be(100);
        barHub.HasObservers.Should().BeTrue();

        barHub.EndTransmission();
    }

    [TestMethod]
    public void OverflowedAndReset()
    {
        // initialize
        BarHub barHub = new();

        Bar dup = new(
            Timestamp: DateTime.Now,
            Open: 1.00m,
            High: 2.00m,
            Low: 0.50m,
            Close: 1.75m,
            Volume: 1000);

        BarPartHub observer = barHub
            .ToBarPartHub(CandlePart.Close);

        // overflowed, over threshold
        Assert.ThrowsExactly<OverflowException>(
            () => {

                for (int i = 0; i <= 101; i++)
                {
                    barHub.Add(dup);
                }
            });

        // assert: faulted

        barHub.Bars.Should().HaveCount(1);
        observer.Results.Should().HaveCount(1);
        barHub.IsFaulted.Should().BeTrue();
        barHub.OverflowCount.Should().Be(101);
        barHub.HasObservers.Should().BeTrue();

        // act: reset

        barHub.ResetFault();

        for (int i = 0; i < 100; i++)
        {
            barHub.Add(dup);
        }

        // assert: no fault, no overflow (yet)

        barHub.Bars.Should().HaveCount(1);
        observer.Results.Should().HaveCount(1);
        barHub.IsFaulted.Should().BeFalse();
        barHub.OverflowCount.Should().Be(100);
        barHub.HasObservers.Should().BeTrue(); // not lost

        barHub.EndTransmission();
    }

    [TestMethod]
    public void MaxCacheSize()
    {
        const int maxCacheSize = 30;

        // initialize
        BarHub barHub = new(maxCacheSize);
        SmaHub observer = barHub.ToSmaHub(20);

        // sets max cache size
        barHub.MaxCacheSize.Should().Be(maxCacheSize);

        // inherits max cache size
        observer.MaxCacheSize.Should().Be(maxCacheSize);
    }

    [TestMethod]
    public void PrunedCache()
    {
        const int maxCacheSize = 30;

        // initialize
        BarHub barHub = new(maxCacheSize);
        SmaHub observer = barHub.ToSmaHub(20);
        IReadOnlyList<SmaResult> seriesList = Bars.ToSma(20);

        // add bars
        barHub.Add(Bars.Take(maxCacheSize));

        // assert: cache size is full size
        barHub.Bars.Should().HaveCount(maxCacheSize);
        observer.Results.Should().HaveCount(maxCacheSize);

        // add more bars to exceed max cache size
        barHub.Add(Bars.Skip(maxCacheSize).Take(10));

        // assert: cache size is pruned
        barHub.Results.Should().HaveCount(maxCacheSize);
        observer.Results.Should().HaveCount(maxCacheSize);

        // assert: correct values remain
        barHub.Bars.IsExactly(
            Bars.Skip(10).Take(maxCacheSize));

        observer.Results.IsExactly(
            seriesList.Skip(10).Take(maxCacheSize));
    }

    [TestMethod]
    public void PrunedAsymmetric()
    {
        const int maxCacheSize = 30;
        const decimal brickSize = 2.5m;
        const EndType endType = EndType.Close;

        // initialize
        BarHub barHub = new(maxCacheSize);
        RenkoHub observer = barHub.ToRenkoHub(brickSize, endType);
        IReadOnlyList<RenkoResult> seriesList = Bars.ToRenko(brickSize, endType);

        // add bars (Renko produces asymmetric results - can be 0 or many bricks per bar)
        barHub.Add(Bars.Take(maxCacheSize));

        // assert: cache size is at or under max (Renko may produce fewer results than bars)
        barHub.Bars.Should().HaveCount(maxCacheSize);
        observer.Results.Should().HaveCountLessThanOrEqualTo(maxCacheSize);

        // add more bars to exceed max cache size
        barHub.Add(Bars.Skip(maxCacheSize).Take(10));

        // assert: bar cache is pruned to max size
        barHub.Bars.Should().HaveCount(maxCacheSize);

        // assert: Renko cache is pruned by date, not count
        // (should contain all Renko bricks from the most recent maxCacheSize bars)
        DateTime oldestBarDate = barHub.Bars[0].Timestamp;
        observer.Results.Should().OnlyContain(r => r.Timestamp >= oldestBarDate,
            "Renko bricks should be pruned by date to match the oldest bar in cache");

        barHub.EndTransmission();
    }

    /// <summary>
    /// Verifies that exposed cache references cannot be cast to mutable lists.
    /// This prevents users from bypassing safe StreamHub methods.
    /// </summary>
    [TestMethod]
    public void CacheReferencesAreImmutable()
    {
        BarHub barHub = new();
        SmaHub observer = barHub.ToSmaHub(20);

        List<Bar> bars = Bars.Take(25).ToList();
        barHub.Add(bars);

        // verify StreamHub.Results cannot be cast to mutable list
        IReadOnlyList<SmaResult> results = observer.Results;
        bool canCastResults = results is List<SmaResult>;
        canCastResults.Should().BeFalse("Results should not be castable to List<T>");

        // verify BarHub.Bars cannot be cast to mutable list
        IReadOnlyList<IBar> barsRef = barHub.Bars;
        bool canCastBars = barsRef is List<IBar>;
        canCastBars.Should().BeFalse("Bars should not be castable to List<T>");

        barHub.EndTransmission();
    }

    /// <summary>
    /// Verifies that adding a bar with the same timestamp replaces the existing bar
    /// instead of clearing the cache (standalone BarHub vulnerability fix).
    /// </summary>
    [TestMethod]
    public void UpdateBarWithSameTimestamp()
    {
        BarHub barHub = new();
        BarPartHub observer = barHub.ToBarPartHub(CandlePart.Close);

        DateTime timestamp = new(2020, 1, 1, 10, 0, 0);

        // add initial bar
        Bar q1 = new(timestamp, 100m, 105m, 95m, 102m, 1000);
        barHub.Add(q1);

        barHub.Bars.Should().HaveCount(1);
        barHub.Bars[0].Close.Should().Be(102m);
        observer.Results.Should().HaveCount(1);
        observer.Results[0].Value.Should().Be(102);

        // add updated bar with same timestamp but different values
        // should replace the existing bar and notify observers to rebuild
        Bar q2 = new(timestamp, 100m, 110m, 90m, 108m, 1500);
        barHub.Add(q2);

        // BarHub should still have 1 bar with updated values
        barHub.Bars.Should().HaveCount(1);
        barHub.Bars[0].Close.Should().Be(108m);

        // observer should rebuild from BarHub's cache with updated values
        observer.Results.Should().HaveCount(1);
        observer.Results[0].Value.Should().Be(108);

        barHub.EndTransmission();
    }
}
