namespace BufferLists;

[TestClass]
public class HeikinAshiBufferListTests : BufferListTestBase
{
    private static readonly IReadOnlyList<HeikinAshiResult> series
       = Bars.ToHeikinAshi();

    [TestMethod]
    public void AddBars_WithValidBars_IncrementsResults()
    {
        HeikinAshiList sut = [];

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
        HeikinAshiList sut = Bars.ToHeikinAshiList();

        sut.Should().HaveCount(Bars.Count);
        sut.IsExactly(series);
    }

    [TestMethod]
    public void BarsCtor_OnInstantiation_IncrementsResults()
    {
        HeikinAshiList sut = new(Bars);

        sut.Should().HaveCount(Bars.Count);
        sut.IsExactly(series);
    }

    [TestMethod]
    public override void Clear_WithState_ResetsState()
    {
        List<Bar> subset = Bars.Take(80).ToList();
        IReadOnlyList<HeikinAshiResult> expected = subset.ToHeikinAshi();

        HeikinAshiList sut = new(subset);

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

        HeikinAshiList sut = new() {
            MaxListSize = maxListSize
        };

        sut.Add(Bars);

        IReadOnlyList<HeikinAshiResult> expected
            = series.Skip(series.Count - maxListSize).ToList();

        sut.Should().HaveCount(maxListSize);
        sut.IsExactly(expected);
    }
}
