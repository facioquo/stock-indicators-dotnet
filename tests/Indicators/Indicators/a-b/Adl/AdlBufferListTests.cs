namespace BufferLists;

[TestClass]
public class AdlBufferListTests : BufferListTestBase
{
    private static readonly IReadOnlyList<AdlResult> series
       = Bars.ToAdl();

    [TestMethod]
    public void AddBars_WithValidBars_IncrementsResults()
    {
        AdlList sut = [];

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
        AdlList sut = new() { Bars };

        sut.Should().HaveCount(Bars.Count);
        sut.IsExactly(series);
    }

    [TestMethod]
    public void BarsCtor_OnInstantiation_IncrementsResults()
    {
        AdlList sut = new(Bars);

        sut.Should().HaveCount(Bars.Count);
        sut.IsExactly(series);
    }

    [TestMethod]
    public void BarsCtorPartial_OnSplitInstantiation_IncrementsResults()
    {
        // Test split initialization: half on construction, half after
        int splitPoint = Bars.Count / 2;
        List<Bar> firstHalf = Bars.Take(splitPoint).ToList();
        List<Bar> secondHalf = Bars.Skip(splitPoint).ToList();

        AdlList sut = new(firstHalf);

        foreach (Bar bar in secondHalf)
        {
            sut.Add(bar);
        }

        sut.Should().HaveCount(Bars.Count);
        sut.IsExactly(series);
    }

    [TestMethod]
    public override void Clear_WithState_ResetsState()
    {
        List<Bar> subset = Bars.Take(80).ToList();
        IReadOnlyList<AdlResult> expected = subset.ToAdl();

        AdlList sut = new(subset);

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

        AdlList sut = new() {
            MaxListSize = maxListSize
        };

        sut.Add(Bars);

        IReadOnlyList<AdlResult> expected = series
            .Skip(series.Count - maxListSize)
            .ToList();

        sut.Should().HaveCount(maxListSize);
        sut.IsExactly(expected);
    }
}
