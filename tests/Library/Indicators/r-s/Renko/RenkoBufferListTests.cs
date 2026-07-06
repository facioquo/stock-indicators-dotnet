namespace BufferLists;

[TestClass]
public class RenkoBufferListTests : BufferListTestBase, ITestBarBufferList
{
    private const decimal brickSize = 1.0m;
    private const EndType endType = EndType.Close;

    private static readonly IReadOnlyList<RenkoResult> series
       = Bars.ToRenko(brickSize, endType);

    [TestMethod]
    public void AddBar_IncrementsResults()
    {
        RenkoList sut = new(brickSize, endType);

        foreach (Bar bar in Bars)
        {
            sut.Add(bar);
        }

        sut.Should().HaveCount(series.Count);
        sut.IsExactly(series);
    }

    [TestMethod]
    public void AddBarsBatch_IncrementsResults()
    {
        RenkoList sut = new(brickSize, endType) { Bars };

        sut.Should().HaveCount(series.Count);
        sut.IsExactly(series);
    }

    [TestMethod]
    public void BarsCtor_OnInstantiation_IncrementsResults()
    {
        RenkoList sut = new(brickSize, endType, Bars);

        sut.Should().HaveCount(series.Count);
        sut.IsExactly(series);
    }

    [TestMethod]
    public override void Clear_WithState_ResetsState()
    {
        List<Bar> subset = Bars.Take(80).ToList();

        RenkoList sut = new(brickSize, endType, subset);

        sut.Should().HaveCount(subset.ToRenko(brickSize, endType).Count);

        sut.Clear();

        sut.Should().BeEmpty();

        foreach (Bar bar in subset)
        {
            sut.Add(bar);
        }

        IReadOnlyList<RenkoResult> expected = subset.ToRenko(brickSize, endType);

        sut.Should().HaveCount(expected.Count);
        sut.IsExactly(expected);
    }

    [TestMethod]
    public override void PruneList_OverMaxListSize_AutoAdjustsListAndBuffers()
    {
        const int maxListSize = 80;

        RenkoList sut = new(brickSize, endType) {
            MaxListSize = maxListSize
        };

        foreach (Bar bar in Bars)
        {
            sut.Add(bar);
        }

        IReadOnlyList<RenkoResult> expected
            = series.Skip(series.Count - maxListSize).ToList();

        sut.Should().HaveCount(maxListSize);
        sut.IsExactly(expected);
    }
}
