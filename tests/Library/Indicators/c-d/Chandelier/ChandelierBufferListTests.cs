namespace BufferLists;

[TestClass]
public class ChandelierBufferListTests : BufferListTestBase
{
    private const int lookbackPeriods = 22;
    private const double multiplier = 3;
    private const Direction type = Direction.Long;

    private static readonly IReadOnlyList<ChandelierResult> series
       = Bars.ToChandelier(lookbackPeriods, multiplier, type);

    [TestMethod]
    public void AddBars_WithValidBars_IncrementsResults()
    {
        ChandelierList sut = new(lookbackPeriods, multiplier, type);

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
        ChandelierList sut = new(lookbackPeriods, multiplier, type) { Bars };

        sut.Should().HaveCount(Bars.Count);
        sut.IsExactly(series);
    }

    [TestMethod]
    public void BarsCtor_OnInstantiation_IncrementsResults()
    {
        ChandelierList sut = new(lookbackPeriods, multiplier, type, Bars);

        sut.Should().HaveCount(Bars.Count);
        sut.IsExactly(series);
    }

    [TestMethod]
    public override void Clear_WithState_ResetsState()
    {
        List<Bar> subset = Bars.Take(80).ToList();
        IReadOnlyList<ChandelierResult> expected = subset.ToChandelier(lookbackPeriods, multiplier, type);

        ChandelierList sut = new(lookbackPeriods, multiplier, type, subset);

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

        ChandelierList sut = new(lookbackPeriods, multiplier, type) {
            MaxListSize = maxListSize
        };

        sut.Add(Bars);

        IReadOnlyList<ChandelierResult> expected = series
            .Skip(series.Count - maxListSize)
            .ToList();

        sut.Should().HaveCount(maxListSize);
        sut.IsExactly(expected);
    }
}
