namespace BufferLists;

[TestClass]
public class PivotPointsBufferListTests : BufferListTestBase, ITestBarBufferList
{
    private const BarInterval windowSize = BarInterval.Month;
    private const PivotPointType pointType = PivotPointType.Standard;

    private static readonly IReadOnlyList<PivotPointsResult> series
       = Bars.ToPivotPoints(windowSize, pointType);

    [TestMethod]
    public void AddBar_IncrementsResults()
    {
        PivotPointsList sut = new(windowSize, pointType);

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
        PivotPointsList sut = Bars.ToPivotPointsList(windowSize, pointType);

        sut.Should().HaveCount(Bars.Count);
        sut.IsExactly(series);
    }

    [TestMethod]
    public void BarsCtor_OnInstantiation_IncrementsResults()
    {
        PivotPointsList sut = new(windowSize, pointType, Bars);

        sut.Should().HaveCount(Bars.Count);
        sut.IsExactly(series);
    }

    [TestMethod]
    public override void Clear_WithState_ResetsState()
    {
        List<Bar> subset = Bars.Take(80).ToList();
        IReadOnlyList<PivotPointsResult> expected = subset.ToPivotPoints(windowSize, pointType);

        PivotPointsList sut = new(windowSize, pointType, subset);

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

        PivotPointsList sut = new(windowSize, pointType) {
            MaxListSize = maxListSize
        };

        sut.Add(Bars);

        IReadOnlyList<PivotPointsResult> expected
            = series.Skip(series.Count - maxListSize).ToList();

        sut.Should().HaveCount(maxListSize);
        sut.IsExactly(expected);
    }
}
