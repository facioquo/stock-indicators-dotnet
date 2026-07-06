namespace BufferLists;

[TestClass]
public class AtrStopBufferListTests : BufferListTestBase
{
    private const int lookbackPeriods = 21;
    private const double multiplier = 3;
    private const EndType endType = EndType.Close;

    private static readonly IReadOnlyList<AtrStopResult> series
        = Bars.ToAtrStop(lookbackPeriods, multiplier, endType);

    [TestMethod]
    public void AddBars_WithValidBars_IncrementsResults()
    {
        AtrStopList sut = new(lookbackPeriods, multiplier, endType);

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
        AtrStopList sut = new(lookbackPeriods, multiplier, endType) { Bars };

        sut.Should().HaveCount(Bars.Count);
        sut.IsExactly(series);
    }

    [TestMethod]
    public void BarsCtor_OnInstantiation_IncrementsResults()
    {
        AtrStopList sut = new(lookbackPeriods, multiplier, endType, Bars);

        sut.Should().HaveCount(Bars.Count);
        sut.IsExactly(series);
    }

    [TestMethod]
    public override void Clear_WithState_ResetsState()
    {
        List<Bar> subset = Bars.Take(80).ToList();
        IReadOnlyList<AtrStopResult> expected = subset.ToAtrStop(lookbackPeriods, multiplier, endType);

        AtrStopList sut = new(lookbackPeriods, multiplier, endType, subset);

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

        AtrStopList sut = new(lookbackPeriods, multiplier, endType) {
            MaxListSize = maxListSize
        };

        sut.Add(Bars);

        IReadOnlyList<AtrStopResult> expected
            = series.Skip(series.Count - maxListSize).ToList();

        sut.Should().HaveCount(maxListSize);
        sut.IsExactly(expected);
    }
}
