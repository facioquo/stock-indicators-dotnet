namespace BufferLists;

[TestClass]
public class MarubozuBufferListTests : BufferListTestBase
{
    private const double minBodyPercent = 95;

    private static readonly IReadOnlyList<CandleResult> series
       = Bars.ToMarubozu(minBodyPercent);

    [TestMethod]
    public void AddBars_WithValidBars_IncrementsResults()
    {
        MarubozuList sut = new(minBodyPercent);

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
        MarubozuList sut = new(minBodyPercent) { Bars };

        sut.Should().HaveCount(Bars.Count);
        sut.IsExactly(series);
    }

    [TestMethod]
    public void BarsCtor_OnInstantiation_IncrementsResults()
    {
        MarubozuList sut = new(minBodyPercent, Bars);

        sut.Should().HaveCount(Bars.Count);
        sut.IsExactly(series);
    }

    [TestMethod]
    public override void Clear_WithState_ResetsState()
    {
        List<Bar> subset = Bars.Take(80).ToList();
        IReadOnlyList<CandleResult> expected = subset.ToMarubozu(minBodyPercent);

        MarubozuList sut = new(minBodyPercent, subset);

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

        MarubozuList sut = new(minBodyPercent) {
            MaxListSize = maxListSize
        };

        sut.Add(Bars);

        IReadOnlyList<CandleResult> expected = series
            .Skip(series.Count - maxListSize)
            .ToList();

        sut.Should().HaveCount(maxListSize);
        sut.IsExactly(expected);
    }
}
