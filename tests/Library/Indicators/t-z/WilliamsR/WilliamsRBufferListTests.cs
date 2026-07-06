namespace BufferLists;

[TestClass]
public class WilliamsRBufferListTests : BufferListTestBase
{
    private const int lookbackPeriods = 14;

    private static readonly IReadOnlyList<WilliamsResult> series
       = Bars.ToWilliamsR(lookbackPeriods);

    [TestMethod]
    public void Boundary_WithRandomBars_StaysWithinBounds()
    {
        IReadOnlyList<WilliamsResult> sut = Data
            .GetRandom(2500)
            .ToWilliamsRList(14);

        sut.IsBetween(static x => x.WilliamsR, -100d, 0d);
    }

    [TestMethod]
    public void AddBars_WithValidBars_IncrementsResults()
    {
        WilliamsRList sut = new(lookbackPeriods);

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
        WilliamsRList sut = new(lookbackPeriods) { Bars };

        sut.Should().HaveCount(Bars.Count);
        sut.IsExactly(series);
    }

    [TestMethod]
    public void BarsCtor_OnInstantiation_IncrementsResults()
    {
        WilliamsRList sut = new(lookbackPeriods, Bars);

        sut.Should().HaveCount(Bars.Count);
        sut.IsExactly(series);
    }

    [TestMethod]
    public void Results_AreAlwaysBounded()
    {
        WilliamsRList sut = new(14, Bars);
        sut.IsBetween(static x => x.WilliamsR, -100, 0);
    }

    [TestMethod]
    public override void Clear_WithState_ResetsState()
    {
        List<Bar> subset = Bars.Take(80).ToList();

        WilliamsRList sut = new(lookbackPeriods, subset);

        sut.Should().HaveCount(subset.Count);

        sut.Clear();

        sut.Should().BeEmpty();

        foreach (Bar bar in subset)
        {
            sut.Add(bar);
        }

        IReadOnlyList<WilliamsResult> expected = subset.ToWilliamsR(lookbackPeriods);

        sut.Should().HaveCount(expected.Count);
        sut.IsExactly(expected);
    }

    [TestMethod]
    public void IncrementalConsistency_VersusBatch_MatchesExactly()
    {
        // Test that incremental addition produces same sut as batch
        WilliamsRList incremental = new(lookbackPeriods);
        WilliamsRList batch = new(lookbackPeriods) { Bars };

        foreach (Bar bar in Bars)
        {
            incremental.Add(bar);
        }

        incremental.Should().HaveCount(batch.Count);
        incremental.IsExactly(batch);
    }

    [TestMethod]
    public void ParameterValidation_InvalidLookback_ThrowsArgumentOutOfRangeException()
    {
        // Test parameter validation
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(static () => new WilliamsRList(0));
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(static () => new WilliamsRList(-1));
    }

    [TestMethod]
    public void BoundaryConditions_WithMinimalData_ReturnsExpectedResult()
    {
        // Test with minimal data
        WilliamsRList sut = new(5);
        List<Bar> minimal = Bars.Take(5).ToList();

        foreach (Bar bar in minimal)
        {
            sut.Add(bar);
        }

        sut.Should().HaveCount(minimal.Count);

        // Should have null values for initial periods
        for (int i = 0; i < 4; i++)
        {
            sut[i].WilliamsR.Should().BeNull();
        }

        // Should have a value at the lookback period boundary
        sut[4].WilliamsR.Should().NotBeNull();
    }

    [TestMethod]
    public void BufferListExtension_VersusConstructor_MatchesExactly()
    {
        // Test extension method
        WilliamsRList fromExtension = Bars.ToWilliamsRList(lookbackPeriods);
        WilliamsRList fromConstructor = new(lookbackPeriods) { Bars };

        fromExtension.Should().HaveCount(fromConstructor.Count);
        fromExtension.IsExactly(fromConstructor);
    }

    [TestMethod]
    public override void PruneList_OverMaxListSize_AutoAdjustsListAndBuffers()
    {
        const int maxListSize = 120;

        WilliamsRList sut = new(lookbackPeriods) {
            MaxListSize = maxListSize
        };

        foreach (Bar bar in Bars)
        {
            sut.Add(bar);
        }

        IReadOnlyList<WilliamsResult> expected
            = series.Skip(series.Count - maxListSize).ToList();

        sut.Should().HaveCount(maxListSize);
        sut.IsExactly(expected);
    }
}
