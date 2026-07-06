namespace BufferLists;

[TestClass]
public class CmfBufferListTests : BufferListTestBase
{
    private const int lookbackPeriods = 20;

    private static readonly IReadOnlyList<CmfResult> series
       = Bars.ToCmf(lookbackPeriods);

    [TestMethod]
    public void AddBars_WithValidBars_IncrementsResults()
    {
        CmfList sut = new(lookbackPeriods);

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
        CmfList sut = new(lookbackPeriods) { Bars };

        sut.Should().HaveCount(Bars.Count);
        sut.IsExactly(series);
    }

    [TestMethod]
    public void BarsCtor_OnInstantiation_IncrementsResults()
    {
        CmfList sut = new(lookbackPeriods, Bars);

        sut.Should().HaveCount(Bars.Count);
        sut.IsExactly(series);
    }

    [TestMethod]
    public void Results_AreAlwaysBounded()
    {
        CmfList sut = new(20, Bars);
        sut.IsBetween(static x => x.Cmf, -1, 1);
    }

    [TestMethod]
    public override void Clear_WithState_ResetsState()
    {
        List<Bar> subset = Bars.Take(80).ToList();
        IReadOnlyList<CmfResult> expected = subset.ToCmf(lookbackPeriods);

        CmfList sut = new(lookbackPeriods, subset);

        sut.Should().HaveCount(subset.Count);
        sut.IsExactly(expected);

        sut.Clear();

        sut.Should().BeEmpty();

        sut.Add(subset);

        sut.Should().HaveCount(expected.Count);
        sut.IsExactly(expected);
    }

    [TestMethod]
    public override void PruneList_OverMaxListSize_AutoAdjustsListAndBuffers()
    {
        const int maxListSize = 120;

        CmfList sut = new(lookbackPeriods) {
            MaxListSize = maxListSize
        };

        sut.Add(Bars);

        IReadOnlyList<CmfResult> expected = series
            .Skip(series.Count - maxListSize)
            .ToList();

        sut.Should().HaveCount(maxListSize);
        sut.IsExactly(expected);
    }
}
