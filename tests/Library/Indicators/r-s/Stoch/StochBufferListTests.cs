namespace BufferLists;

[TestClass]
public class StochBufferListTests : BufferListTestBase
{
    private const int lookbackPeriods = 14;
    private const int signalPeriods = 3;
    private const int smoothPeriods = 3;
    private const double kFactor = 3;
    private const double dFactor = 2;
    private const MaType movingAverageType = MaType.SMA;

    private static readonly IReadOnlyList<StochResult> series
       = Bars.ToStoch(lookbackPeriods, signalPeriods, smoothPeriods);

    [TestMethod]
    public void Boundary_WithRandomBars_StaysWithinBounds()
    {
        // %J is unbounded by design so not asserted.
        IReadOnlyList<StochResult> sut = Data
            .GetRandom(2500)
            .ToStochList(14, 3, 3);

        sut.IsBetween(static x => x.Oscillator, 0d, 100d);
        sut.IsBetween(static x => x.Signal, 0d, 100d);
    }

    [TestMethod]
    public void AddBars_WithValidBars_IncrementsResults()
    {
        StochList sut = new(lookbackPeriods, signalPeriods, smoothPeriods);

        foreach (Bar bar in Bars)
        {
            sut.Add(bar);
        }

        sut.Should().HaveCount(Bars.Count);
        sut.IsExactly(series);
    }

    [TestMethod]
    public void AddBarsBatch_WithValidBars_IncrementsResults()
    {
        StochList sut = new(lookbackPeriods, signalPeriods, smoothPeriods) { Bars };

        sut.Should().HaveCount(Bars.Count);
        sut.IsExactly(series);
    }

    [TestMethod]
    public void BarsCtor_OnInstantiation_IncrementsResults()
    {
        StochList sut = new(lookbackPeriods, signalPeriods, smoothPeriods, kFactor, dFactor, movingAverageType, Bars);

        sut.Should().HaveCount(Bars.Count);
        sut.IsExactly(series);
    }

    [TestMethod]
    public override void Clear_WithState_ResetsState()
    {
        List<Bar> subset = Bars.Take(80).ToList();

        StochList sut = new(lookbackPeriods, signalPeriods, smoothPeriods, kFactor, dFactor, movingAverageType, subset);

        sut.Should().HaveCount(subset.Count);

        sut.Clear();

        sut.Should().BeEmpty();

        foreach (Bar bar in subset)
        {
            sut.Add(bar);
        }

        IReadOnlyList<StochResult> expected = subset.ToStoch(lookbackPeriods, signalPeriods, smoothPeriods);

        sut.Should().HaveCount(expected.Count);
        sut.IsExactly(expected);
    }

    [TestMethod]
    public void ExtendedParameters_WithCustomMaType_ReturnsExpectedResult()
    {
        const double kFactor = 5;
        const double dFactor = 4;
        const MaType movingAverageType = MaType.SMMA;

        StochList sut = new(lookbackPeriods, signalPeriods, smoothPeriods, kFactor, dFactor, movingAverageType) { Bars };

        IReadOnlyList<StochResult> expected = Bars.ToStoch(lookbackPeriods, signalPeriods, smoothPeriods, kFactor, dFactor, movingAverageType);

        sut.Should().HaveCount(Bars.Count);
        sut.IsExactly(expected);
    }

    [TestMethod]
    public void Results_AreAlwaysBounded()
    {
        StochList sut = new(14, 3, 3, 1, 1, MaType.SMA, Bars);
        sut.IsBetween(static x => x.Oscillator, 0, 100);
        sut.IsBetween(static x => x.Signal, 0, 100);
    }

    [TestMethod]
    public void IncrementalConsistency_VersusBatch_MatchesExactly()
    {
        // Test that incremental addition produces same sut as batch
        StochList incremental = new(lookbackPeriods, signalPeriods, smoothPeriods);
        StochList batch = new(lookbackPeriods, signalPeriods, smoothPeriods) { Bars };

        foreach (Bar bar in Bars)
        {
            incremental.Add(bar);
        }

        incremental.Should().HaveCount(batch.Count);
        incremental.IsExactly(batch);
    }

    [TestMethod]
    public void ParameterValidation_InvalidParameters_ThrowsArgumentOutOfRangeException()
    {
        // Test parameter validation
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(static () => new StochList(0));
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(static () => new StochList(14, 0));
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(static () => new StochList(14, 3, 0));
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(static () => new StochList(14, 3, 3, 0, 2, MaType.SMA));
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(static () => new StochList(14, 3, 3, 3, 0, MaType.SMA));
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(static () => new StochList(14, 3, 3, 3, 2, MaType.ALMA));
    }

    [TestMethod]
    public void BoundaryConditions_WithMinimalData_ReturnsExpectedResult()
    {
        // Test with minimal data
        StochList sut = new(5, 3, 3);
        List<Bar> minimal = Bars.Take(5).ToList();

        foreach (Bar bar in minimal)
        {
            sut.Add(bar);
        }

        sut.Should().HaveCount(minimal.Count);

        // Should have null values for initial periods
        for (int i = 0; i < 4; i++)
        {
            sut[i].Oscillator.Should().BeNull();
        }
    }

    [TestMethod]
    public void BufferListExtension_VersusConstructor_MatchesExactly()
    {
        // Test extension method
        StochList fromExtension = Bars.ToStochList(lookbackPeriods, signalPeriods, smoothPeriods);
        StochList fromConstructor = new(lookbackPeriods, signalPeriods, smoothPeriods) { Bars };

        fromExtension.IsExactly(fromConstructor);
    }

    [TestMethod]
    public override void PruneList_OverMaxListSize_AutoAdjustsListAndBuffers()
    {
        const int maxListSize = 120;

        StochList sut = new(lookbackPeriods, signalPeriods, smoothPeriods) {
            MaxListSize = maxListSize
        };

        foreach (Bar bar in Bars)
        {
            sut.Add(bar);
        }

        IReadOnlyList<StochResult> expected
            = series.Skip(series.Count - maxListSize).ToList();

        sut.Should().HaveCount(maxListSize);
        sut.IsExactly(expected);
    }
}
