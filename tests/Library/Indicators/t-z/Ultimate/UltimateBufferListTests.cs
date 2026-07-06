namespace BufferLists;

[TestClass]
public class UltimateBufferListTests : BufferListTestBase
{
    private const int shortPeriods = 7;
    private const int middlePeriods = 14;
    private const int longPeriods = 28;

    private static readonly IReadOnlyList<UltimateResult> series
       = Bars.ToUltimate(shortPeriods, middlePeriods, longPeriods);

    [TestMethod]
    public void Boundary_WithRandomBars_StaysWithinBounds()
    {
        IReadOnlyList<UltimateResult> sut = Data
            .GetRandom(2500)
            .ToUltimateList(7, 14, 28);

        sut.IsBetween(static x => x.Ultimate, 0d, 100d);
    }

    [TestMethod]
    public void Results_AreAlwaysBounded()
    {
        UltimateList sut = new(7, 14, 28, Bars);
        sut.IsBetween(static x => x.Ultimate, 0, 100);
    }

    [TestMethod]
    public void AddBars_WithValidBars_IncrementsResults()
    {
        UltimateList sut = new(shortPeriods, middlePeriods, longPeriods);

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
        UltimateList sut = new(shortPeriods, middlePeriods, longPeriods) { Bars };

        sut.Should().HaveCount(Bars.Count);
        sut.IsExactly(series);
    }

    [TestMethod]
    public void BarsCtor_OnInstantiation_IncrementsResults()
    {
        UltimateList sut = new(shortPeriods, middlePeriods, longPeriods, Bars);

        sut.Should().HaveCount(Bars.Count);
        sut.IsExactly(series);
    }

    [TestMethod]
    public override void Clear_WithState_ResetsState()
    {
        List<Bar> subset = Bars.Take(80).ToList();

        UltimateList sut = new(shortPeriods, middlePeriods, longPeriods, subset);

        sut.Should().HaveCount(subset.Count);

        sut.Clear();

        sut.Should().BeEmpty();

        foreach (Bar bar in subset)
        {
            sut.Add(bar);
        }

        IReadOnlyList<UltimateResult> expected = subset.ToUltimate(shortPeriods, middlePeriods, longPeriods);

        sut.Should().HaveCount(expected.Count);
        sut.IsExactly(expected);
    }

    [TestMethod]
    public override void PruneList_OverMaxListSize_AutoAdjustsListAndBuffers()
    {
        const int maxListSize = 120;

        UltimateList sut = new(shortPeriods, middlePeriods, longPeriods) {
            MaxListSize = maxListSize
        };

        foreach (Bar bar in Bars)
        {
            sut.Add(bar);
        }

        IReadOnlyList<UltimateResult> expected
            = series.Skip(series.Count - maxListSize).ToList();

        sut.Should().HaveCount(maxListSize);
        sut.IsExactly(expected);
    }
}
