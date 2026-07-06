namespace BufferLists;

[TestClass]
public class PmoBufferListTests : BufferListTestBase, ITestChainBufferList, ITestCustomBufferListCache
{
    private const int timePeriods = 35;
    private const int smoothPeriods = 20;
    private const int signalPeriods = 10;

    private static readonly IReadOnlyList<IReusable> reusables
       = Bars
        .Cast<IReusable>()
        .ToList();

    private static readonly IReadOnlyList<PmoResult> series
       = Bars.ToPmo(timePeriods, smoothPeriods, signalPeriods);

    [TestMethod]
    public void AddDateAndValue_IncrementsResults()
    {
        PmoList sut = new(timePeriods, smoothPeriods, signalPeriods);

        foreach (IReusable item in reusables)
        {
            sut.Add(item.Timestamp, item.Value);
        }

        sut.Should().HaveCount(Bars.Count);
        sut.IsExactly(series);
    }

    [TestMethod]
    public void AddReusableItem_IncrementsResults()
    {
        PmoList sut = new(timePeriods, smoothPeriods, signalPeriods);

        foreach (IReusable item in reusables)
        {
            sut.Add(item);
        }

        sut.Should().HaveCount(Bars.Count);
        sut.IsExactly(series);
    }

    [TestMethod]
    public void AddReusableItemBatch_IncrementsResults()
    {
        PmoList sut = new(timePeriods, smoothPeriods, signalPeriods) { reusables };

        sut.Should().HaveCount(Bars.Count);
        sut.IsExactly(series);
    }

    [TestMethod]
    public void AddBar_IncrementsResults()
    {
        PmoList sut = new(timePeriods, smoothPeriods, signalPeriods);

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
        PmoList sut = Bars.ToPmoList(timePeriods, smoothPeriods, signalPeriods);

        sut.Should().HaveCount(Bars.Count);
        sut.IsExactly(series);
    }

    [TestMethod]
    public void BarsCtor_OnInstantiation_IncrementsResults()
    {
        PmoList sut = new(timePeriods, smoothPeriods, signalPeriods, Bars);

        sut.Should().HaveCount(Bars.Count);
        sut.IsExactly(series);
    }

    [TestMethod]
    public override void Clear_WithState_ResetsState()
    {
        List<Bar> subset = Bars.Take(80).ToList();
        IReadOnlyList<PmoResult> expected = subset.ToPmo(timePeriods, smoothPeriods, signalPeriods);

        PmoList sut = new(timePeriods, smoothPeriods, signalPeriods, subset);

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

        PmoList sut = new(timePeriods, smoothPeriods, signalPeriods) {
            MaxListSize = maxListSize
        };

        sut.Add(Bars);

        IReadOnlyList<PmoResult> expected
            = series.Skip(series.Count - maxListSize).ToList();

        sut.Should().HaveCount(maxListSize);
        sut.IsExactly(expected);
    }

    [TestMethod]
    public void CustomBuffer_OverMaxListSize_AutoAdjustsListAndBuffers()
    {
        const int maxListSize = 120;

        PmoList sut = new(timePeriods, smoothPeriods, signalPeriods) {
            MaxListSize = maxListSize
        };

        sut.Add(Bars);

        IReadOnlyList<PmoResult> expected
            = series.Skip(series.Count - maxListSize).ToList();

        sut.Should().HaveCount(maxListSize);
        sut.IsExactly(expected);

        // Add more bars to verify continued operation after pruning
        List<Bar> moreBars = Bars.TakeLast(50).ToList();
        sut.Add(moreBars);

        IReadOnlyList<PmoResult> allSeries = Bars.Concat(moreBars).ToList()
            .ToPmo(timePeriods, smoothPeriods, signalPeriods);

        IReadOnlyList<PmoResult> expectedAfterMore
            = allSeries.Skip(allSeries.Count - maxListSize).ToList();

        sut.Should().HaveCount(maxListSize);
        sut.IsExactly(expectedAfterMore);
    }
}
