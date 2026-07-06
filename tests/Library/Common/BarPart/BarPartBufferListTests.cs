namespace BufferLists;

[TestClass]
public class BarPartBufferListTests : BufferListTestBase, ITestBarBufferList
{
    private const CandlePart candlePart = CandlePart.Close;

    private static readonly IReadOnlyList<TimeValue> series
       = Bars.ToBarPart(candlePart);

    [TestMethod]
    public void AddBar_IncrementsResults()
    {
        BarPartList sut = new(candlePart);

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
        BarPartList sut = Bars.ToBarPartList(candlePart);

        sut.Should().HaveCount(Bars.Count);
        sut.IsExactly(series);
    }

    [TestMethod]
    public void BarsCtor_OnInstantiation_IncrementsResults()
    {
        BarPartList sut = new(candlePart, Bars);

        sut.Should().HaveCount(Bars.Count);
        sut.IsExactly(series);
    }

    [TestMethod]
    public override void Clear_WithState_ResetsState()
    {
        List<Bar> subset = Bars.Take(80).ToList();
        IReadOnlyList<TimeValue> expected = subset.ToBarPart(candlePart);

        BarPartList sut = new(candlePart, subset);

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

        BarPartList sut = new(candlePart) {
            MaxListSize = maxListSize
        };

        sut.Add(Bars);

        IReadOnlyList<TimeValue> expected = series
            .Skip(series.Count - maxListSize)
            .ToList();

        sut.Should().HaveCount(maxListSize);
        sut.IsExactly(expected);
    }

    [TestMethod]
    public void MultipleCandleParts()
    {
        // Test different candle parts
        IReadOnlyList<TimeValue> openSeries = Bars.ToBarPart(CandlePart.Open);
        BarPartList openList = new(CandlePart.Open, Bars);
        openList.IsExactly(openSeries);

        IReadOnlyList<TimeValue> highSeries = Bars.ToBarPart(CandlePart.High);
        BarPartList highList = new(CandlePart.High, Bars);
        highList.IsExactly(highSeries);

        IReadOnlyList<TimeValue> hl2Series = Bars.ToBarPart(CandlePart.HL2);
        BarPartList hl2List = new(CandlePart.HL2, Bars);
        hl2List.IsExactly(hl2Series);
    }
}
