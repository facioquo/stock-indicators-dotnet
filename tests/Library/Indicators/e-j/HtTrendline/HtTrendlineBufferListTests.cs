namespace BufferLists;

[TestClass]
public class HtTrendlineBufferListTests : BufferListTestBase, ITestChainBufferList, ITestCustomBufferListCache
{
    private static readonly IReadOnlyList<IReusable> reusables
       = Bars
        .Cast<IReusable>()
        .ToList();

    private static readonly IReadOnlyList<HtlResult> series
       = Bars.ToHtTrendline();

    [TestMethod]
    public void AddBar_IncrementsResults()
    {
        HtTrendlineList sut = new();

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
        HtTrendlineList sut = new() { Bars };

        sut.Should().HaveCount(Bars.Count);
        sut.IsExactly(series);
    }

    [TestMethod]
    public void BarsCtor_OnInstantiation_IncrementsResults()
    {
        HtTrendlineList sut = new(Bars);

        sut.Should().HaveCount(Bars.Count);
        sut.IsExactly(series);
    }

    [TestMethod]
    public void AddReusableItem_IncrementsResults()
    {
        HtTrendlineList sut = new();

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
        HtTrendlineList sut = new() { reusables };

        sut.Should().HaveCount(Bars.Count);
        sut.IsExactly(series);
    }

    [TestMethod]
    public void AddDateAndValue_IncrementsResults()
    {
        HtTrendlineList sut = new();

        foreach (IReusable item in reusables)
        {
            sut.Add(item.Timestamp, item.Hl2OrValue());
        }

        sut.Should().HaveCount(Bars.Count);
        sut.IsExactly(series);
    }

    [TestMethod]
    public override void Clear_WithState_ResetsState()
    {
        List<Bar> subset = Bars.Take(80).ToList();
        IReadOnlyList<HtlResult> expected = subset.ToHtTrendline();

        HtTrendlineList sut = new(subset);

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

        HtTrendlineList sut = new() { MaxListSize = maxListSize };

        sut.Add(Bars);

        IReadOnlyList<HtlResult> expected = series
            .Skip(series.Count - maxListSize)
            .ToList();

        sut.Should().HaveCount(maxListSize);
        sut.IsExactly(expected);
    }

    [TestMethod]
    public void CustomBuffer_OverMaxListSize_AutoAdjustsListAndBuffers()
    {
        const int maxListSize = 200;
        const int barsSize = 750;

        // Use test data that exceeds custom cache thresholds
        List<Bar> bars = LongishBars
            .Take(barsSize)
            .ToList();

        // Expected results after pruning (tail end)
        IReadOnlyList<HtlResult> expected = bars
            .ToHtTrendline()
            .Skip(barsSize - maxListSize)
            .ToList();

        // Generate buffer list
        HtTrendlineList sut = new(bars) { MaxListSize = maxListSize };

        sut.Should().HaveCount(maxListSize);
        sut.IsExactly(expected);

        // Add more bars to verify continued operation after pruning
        List<Bar> moreBars = LongishBars.Skip(barsSize).Take(50).ToList();
        sut.Add(moreBars);

        IReadOnlyList<HtlResult> allSeries = bars
            .Concat(moreBars)
            .ToList()
            .ToHtTrendline();

        IReadOnlyList<HtlResult> finalExpected = allSeries
            .Skip(allSeries.Count - maxListSize)
            .ToList();

        sut.Should().HaveCount(maxListSize);
        sut.IsExactly(finalExpected);
    }
}
