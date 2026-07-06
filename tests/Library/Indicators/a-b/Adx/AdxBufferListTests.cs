namespace BufferLists;

[TestClass]
public class AdxBufferListTests : BufferListTestBase
{
    private const int lookbackPeriods = 14;

    private static readonly IReadOnlyList<AdxResult> series
       = Bars.ToAdx(lookbackPeriods);

    [TestMethod]
    public void Results_AreAlwaysBounded()
    {
        AdxList sut = new(14, Bars);
        sut.IsBetween(static x => x.Pdi, 0, 100);
        sut.IsBetween(static x => x.Mdi, 0, 100);
        sut.IsBetween(static x => x.Dx, 0, 100);
        sut.IsBetween(static x => x.Adx, 0, 100);
        sut.IsBetween(static x => x.Adxr, 0, 100);
    }

    [TestMethod]
    public void AddBars_WithValidBars_IncrementsResults()
    {
        AdxList sut = new(lookbackPeriods);

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
        AdxList sut = new(lookbackPeriods) { Bars };

        sut.Should().HaveCount(Bars.Count);
        sut.IsExactly(series);
    }

    [TestMethod]
    public void BarsCtor_OnInstantiation_IncrementsResults()
    {
        AdxList sut = new(lookbackPeriods, Bars);

        sut.Should().HaveCount(Bars.Count);
        sut.IsExactly(series);
    }

    [TestMethod]
    public override void Clear_WithState_ResetsState()
    {
        List<Bar> subset = Bars.Take(80).ToList();
        IReadOnlyList<AdxResult> expected = subset.ToAdx(lookbackPeriods);

        AdxList sut = new(lookbackPeriods, subset);

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
        const int maxListSize = 100;

        AdxList sut = new(lookbackPeriods) {
            MaxListSize = maxListSize
        };

        sut.Add(Bars);

        IReadOnlyList<AdxResult> expected = series
            .Skip(series.Count - maxListSize)
            .ToList();

        sut.Should().HaveCount(maxListSize);
        sut.IsExactly(expected);
    }
}
