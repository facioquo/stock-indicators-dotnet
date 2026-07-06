namespace BufferLists;

[TestClass]
public class DojiBufferListTests : BufferListTestBase
{
    private const double maxPriceChangePercent = 0.1;

    private static readonly IReadOnlyList<CandleResult> series
       = Bars.ToDoji(maxPriceChangePercent);

    [TestMethod]
    public void AddBars_WithValidBars_IncrementsResults()
    {
        DojiList sut = new(maxPriceChangePercent);

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
        DojiList sut = Bars.ToDojiList(maxPriceChangePercent);

        sut.Should().HaveCount(Bars.Count);
        sut.IsExactly(series);
    }

    [TestMethod]
    public void BarsCtor_OnInstantiation_IncrementsResults()
    {
        DojiList sut = new(maxPriceChangePercent, Bars);

        sut.Should().HaveCount(Bars.Count);
        sut.IsExactly(series);
    }

    [TestMethod]
    public override void Clear_WithState_ResetsState()
    {
        List<Bar> subset = Bars.Take(80).ToList();
        IReadOnlyList<CandleResult> expected = subset.ToDoji(maxPriceChangePercent);

        DojiList sut = new(maxPriceChangePercent, subset);

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

        DojiList sut = new(maxPriceChangePercent) {
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
