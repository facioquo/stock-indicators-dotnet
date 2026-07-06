namespace BufferLists;

[TestClass]
public class IchimokuBufferListTests : BufferListTestBase, ITestBarBufferList, ITestCustomBufferListCache
{
    private const int tenkanPeriods = 9;
    private const int kijunPeriods = 26;
    private const int senkouBPeriods = 52;
    private const int senkouOffset = 26;
    private const int chikouOffset = 26;

    private static readonly IReadOnlyList<IchimokuResult> series
        = Bars.ToIchimoku(tenkanPeriods, kijunPeriods, senkouBPeriods, senkouOffset, chikouOffset);

    [TestMethod]
    public void AddBar_IncrementsResults()
    {
        IchimokuList sut = new(tenkanPeriods, kijunPeriods, senkouBPeriods, senkouOffset, chikouOffset);

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
        IchimokuList sut = Bars.ToIchimokuList(tenkanPeriods, kijunPeriods, senkouBPeriods, senkouOffset, chikouOffset);

        sut.Should().HaveCount(Bars.Count);
        sut.IsExactly(series);
    }

    [TestMethod]
    public void BarsCtor_OnInstantiation_IncrementsResults()
    {
        IchimokuList sut = new(tenkanPeriods, kijunPeriods, senkouBPeriods, senkouOffset, chikouOffset, Bars);

        sut.Should().HaveCount(Bars.Count);
        sut.IsExactly(series);
    }

    [TestMethod]
    public override void Clear_WithState_ResetsState()
    {
        List<Bar> subset = Bars.Take(80).ToList();
        IReadOnlyList<IchimokuResult> expected = subset.ToIchimoku(tenkanPeriods, kijunPeriods, senkouBPeriods, senkouOffset, chikouOffset);

        IchimokuList sut = new(tenkanPeriods, kijunPeriods, senkouBPeriods, senkouOffset, chikouOffset, subset);

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

        IchimokuList sut = new(tenkanPeriods, kijunPeriods, senkouBPeriods, senkouOffset, chikouOffset) {
            MaxListSize = maxListSize
        };

        sut.Add(Bars);

        IReadOnlyList<IchimokuResult> expected
            = series.Skip(series.Count - maxListSize).ToList();

        sut.Should().HaveCount(maxListSize);
        sut.IsExactly(expected);
    }

    [TestMethod]
    public void CustomBuffer_OverMaxListSize_AutoAdjustsListAndBuffers()
    {
        const int maxListSize = 150;
        const int barsSize = 1000;

        // Use a test data that exceeds all cache size thresholds
        List<Bar> bars = LongishBars
            .Take(barsSize)
            .ToList();

        // Expected results after pruning (tail end)
        IReadOnlyList<IchimokuResult> expected = bars
            .ToIchimoku(tenkanPeriods, kijunPeriods, senkouBPeriods, senkouOffset, chikouOffset)
            .Skip(barsSize - maxListSize)
            .ToList();

        // Generate buffer list
        IchimokuList sut = new(tenkanPeriods, kijunPeriods, senkouBPeriods, senkouOffset, chikouOffset, bars) {
            MaxListSize = maxListSize
        };

        // Verify expected results matching equivalent series values
        sut.Count.Should().Be(maxListSize);
        sut.IsExactly(expected);
    }
}
