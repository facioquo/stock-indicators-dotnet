namespace BufferLists;

[TestClass]
public class PivotsBufferListTests : BufferListTestBase, ITestBarBufferList
{
    private const int leftSpan = 4;
    private const int rightSpan = 4;
    private const int maxTrendPeriods = 20;

    private static readonly IReadOnlyList<PivotsResult> series
       = Bars.ToPivots(leftSpan, rightSpan, maxTrendPeriods);

    [TestMethod]
    public void AddBar_IncrementsResults()
    {
        PivotsList sut = new(leftSpan, rightSpan, maxTrendPeriods);

        foreach (Bar bar in Bars)
        {
            sut.Add(bar);
        }

        sut.Should().HaveCount(Bars.Count);
        sut.IsExactly(series);
    }

    [TestMethod]
    public void AddBarsBatch_IncrementsResults()
    {
        PivotsList sut = Bars.ToPivotsList(leftSpan, rightSpan, maxTrendPeriods);

        sut.Should().HaveCount(Bars.Count);
        sut.IsExactly(series);
    }

    [TestMethod]
    public void BarsCtor_OnInstantiation_IncrementsResults()
    {
        PivotsList sut = new(leftSpan, rightSpan, maxTrendPeriods, EndType.HighLow, Bars);

        sut.Should().HaveCount(Bars.Count);
        sut.IsExactly(series);
    }

    [TestMethod]
    public override void Clear_WithState_ResetsState()
    {
        List<Bar> subset = Bars.Take(80).ToList();
        IReadOnlyList<PivotsResult> expected = subset.ToPivots(leftSpan, rightSpan, maxTrendPeriods);

        PivotsList sut = new(leftSpan, rightSpan, maxTrendPeriods, EndType.HighLow, subset);

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

        PivotsList sut = new(leftSpan, rightSpan, maxTrendPeriods) {
            MaxListSize = maxListSize
        };

        sut.Add(Bars);

        IReadOnlyList<PivotsResult> expected
            = series.Skip(series.Count - maxListSize).ToList();

        sut.Should().HaveCount(maxListSize);
        sut.IsExactly(expected);
    }
}
