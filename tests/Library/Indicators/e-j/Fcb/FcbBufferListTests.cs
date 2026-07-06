namespace BufferLists;

[TestClass]
public class FcbBufferListTests : BufferListTestBase
{
    private const int windowSpan = 2;

    private static readonly IReadOnlyList<FcbResult> series
       = Bars.ToFcb(windowSpan);

    [TestMethod]
    public void AddBars_WithValidBars_IncrementsResults()
    {
        FcbList sut = new(windowSpan);

        foreach (Bar q in Bars) { sut.Add(q); }

        sut.Should().HaveCount(Bars.Count);
        sut.IsExactly(series);
    }

    [TestMethod]
    public void AddBarsBatch_WithValidBars_IncrementsResults()
    {
        FcbList sut = new(windowSpan) { Bars };

        IReadOnlyList<FcbResult> series
            = Bars.ToFcb(windowSpan);

        sut.Should().HaveCount(Bars.Count);
        sut.IsExactly(series);
    }

    [TestMethod]
    public void BarsCtor_OnInstantiation_IncrementsResults()
    {
        FcbList sut = new(windowSpan, Bars);

        sut.Should().HaveCount(Bars.Count);
        sut.IsExactly(series);
    }

    [TestMethod]
    public override void PruneList_OverMaxListSize_AutoAdjustsListAndBuffers()
    {
        const int maxListSize = 120;

        FcbList sut = new(windowSpan) {
            MaxListSize = maxListSize
        };

        sut.Add(Bars);

        IReadOnlyList<FcbResult> expected
            = series.Skip(series.Count - maxListSize).ToList();

        sut.Should().HaveCount(maxListSize);
        sut.IsExactly(expected);
    }

    [TestMethod]
    public override void Clear_WithState_ResetsState()
    {
        List<Bar> subset = Bars.Take(80).ToList();
        IReadOnlyList<FcbResult> expected = subset.ToFcb(windowSpan);

        FcbList sut = new(windowSpan, subset);

        sut.Should().HaveCount(subset.Count);
        sut.IsExactly(expected);

        sut.Clear();

        sut.Should().BeEmpty();

        sut.Add(subset);

        sut.Should().HaveCount(expected.Count);
        sut.IsExactly(expected);
    }
}
