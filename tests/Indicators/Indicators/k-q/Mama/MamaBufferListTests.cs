namespace BufferLists;

[TestClass]
public class MamaBufferListTests : BufferListTestBase, ITestChainBufferList, ITestCustomBufferListCache
{
    private const double fastLimit = 0.5;
    private const double slowLimit = 0.05;

    private static readonly IReadOnlyList<IReusable> reusables
       = Bars
        .Use(CandlePart.HL2)  // HL2 values (not Close) for comparables
        .Cast<IReusable>()
        .ToList();

    private static readonly IReadOnlyList<MamaResult> series
       = Bars.ToMama(fastLimit, slowLimit);

    [TestMethod]
    public void AddBar_IncrementsResults()
    {
        MamaList sut = new(fastLimit, slowLimit);

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
        MamaList sut = new(fastLimit, slowLimit) { Bars };

        sut.Should().HaveCount(Bars.Count);
        sut.IsExactly(series);
    }

    [TestMethod]
    public void BarsCtor_OnInstantiation_IncrementsResults()
    {
        MamaList sut = new(fastLimit, slowLimit, Bars);

        sut.Should().HaveCount(Bars.Count);
        sut.IsExactly(series);
    }

    [TestMethod]
    public void AddReusableItem_IncrementsResults()
    {
        MamaList sut = new(fastLimit, slowLimit);

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
        MamaList sut = new(fastLimit, slowLimit) { reusables };

        sut.Should().HaveCount(Bars.Count);
        sut.IsExactly(series);
    }

    [TestMethod]
    public void AddDateAndValue_IncrementsResults()
    {
        MamaList sut = new(fastLimit, slowLimit);

        foreach (IReusable item in reusables)
        {
            sut.Add(item.Timestamp, item.Value);
        }

        sut.Should().HaveCount(Bars.Count);
        sut.IsExactly(series);
    }

    [TestMethod]
    public override void Clear_WithState_ResetsState()
    {
        List<Bar> subset = Bars.Take(80).ToList();
        IReadOnlyList<MamaResult> expected = subset.ToMama(fastLimit, slowLimit);

        MamaList sut = new(fastLimit, slowLimit, subset);

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
        const int maxListSize = 150;

        MamaList sut = new(fastLimit, slowLimit) {
            MaxListSize = maxListSize
        };

        sut.Add(Bars);

        IReadOnlyList<MamaResult> expected = series
            .Skip(series.Count - maxListSize)
            .ToList();

        sut.Should().HaveCount(maxListSize);
        sut.IsExactly(expected);
    }

    [TestMethod]
    public void CustomBuffer_OverMaxListSize_AutoAdjustsListAndBuffers()
    {
        const int maxListSize = 200;
        const int barsSize = 1250;

        // Use a test data that exceeds all cache size thresholds
        List<Bar> bars = LongishBars
            .Take(barsSize)
            .ToList();

        // Expected results after pruning (tail end)
        IReadOnlyList<MamaResult> expected = bars
            .ToMama(fastLimit, slowLimit)
            .Skip(barsSize - maxListSize)
            .ToList();

        // Generate buffer list
        MamaList sut = new(fastLimit, slowLimit, bars) {
            MaxListSize = maxListSize
        };

        // Verify expected results matching equivalent series values
        sut.Count.Should().Be(maxListSize);
        sut.IsExactly(expected);
    }
}
