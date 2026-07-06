namespace BufferLists;

[TestClass]
public class DpoBufferListTests : BufferListTestBase, ITestChainBufferList, ITestCustomBufferListCache
{
    private const int lookbackPeriods = 20;

    private static readonly IReadOnlyList<IReusable> reusables
       = Bars
        .Cast<IReusable>()
        .ToList();

    private static readonly IReadOnlyList<DpoResult> series
       = Bars.ToDpo(lookbackPeriods);

    [TestMethod]
    public void AddDateAndValue_IncrementsResults()
    {
        DpoList sut = new(lookbackPeriods);

        foreach (IReusable item in reusables)
        {
            sut.Add(item.Timestamp, item.Value);
        }

        // DPO maintains 1:1 correspondence - retroactively updates as lookahead becomes available
        sut.Should().HaveCount(Bars.Count);
        sut.IsExactly(series);
    }

    [TestMethod]
    public void AddReusableItem_IncrementsResults()
    {
        DpoList sut = new(lookbackPeriods);

        foreach (IReusable item in reusables)
        {
            sut.Add(item);
        }

        // DPO maintains 1:1 correspondence - retroactively updates as lookahead becomes available
        sut.Should().HaveCount(Bars.Count);
        sut.IsExactly(series);
    }

    [TestMethod]
    public void AddReusableItemBatch_IncrementsResults()
    {
        DpoList sut = new(lookbackPeriods) { reusables };

        // DPO maintains 1:1 correspondence - retroactively updates as lookahead becomes available
        sut.Should().HaveCount(Bars.Count);
        sut.IsExactly(series);
    }

    [TestMethod]
    public void AddBar_IncrementsResults()
    {
        DpoList sut = new(lookbackPeriods);

        foreach (Bar bar in Bars)
        {
            sut.Add(bar);
        }

        // DPO maintains 1:1 correspondence - retroactively updates as lookahead becomes available
        sut.Should().HaveCount(Bars.Count);
        sut.IsExactly(series);
    }

    [TestMethod]
    public void AddBarsBatch_IncrementsResults()
    {
        DpoList sut = Bars.ToDpoList(lookbackPeriods);

        // DPO maintains 1:1 correspondence - retroactively updates as lookahead becomes available
        sut.Should().HaveCount(Bars.Count);
        sut.IsExactly(series);
    }

    [TestMethod]
    public void BarsCtor_OnInstantiation_IncrementsResults()
    {
        DpoList sut = new(lookbackPeriods, Bars);

        // DPO maintains 1:1 correspondence - retroactively updates as lookahead becomes available
        sut.Should().HaveCount(Bars.Count);
        sut.IsExactly(series);
    }

    [TestMethod]
    public override void Clear_WithState_ResetsState()
    {
        List<Bar> subset = Bars.Take(80).ToList();
        IReadOnlyList<DpoResult> expected = subset.ToDpo(lookbackPeriods);

        DpoList sut = new(lookbackPeriods, subset);

        // DPO maintains 1:1 correspondence - retroactively updates as lookahead becomes available
        sut.Should().HaveCount(subset.Count);
        sut.IsExactly(expected);

        sut.Clear();

        sut.Should().BeEmpty();

        sut.Add(subset);

        sut.Should().HaveCount(subset.Count);
        sut.IsExactly(expected);
    }

    [TestMethod]
    public override void PruneList_OverMaxListSize_AutoAdjustsListAndBuffers()
    {
        const int maxListSize = 120;

        DpoList sut = new(lookbackPeriods) {
            MaxListSize = maxListSize
        };

        sut.Add(Bars);

        // DPO maintains 1:1 correspondence with maxListSize pruning
        int expectedCount = Math.Min(maxListSize, Bars.Count);

        sut.Should().HaveCount(expectedCount);

        // Compare with the last expectedCount results from series
        IReadOnlyList<DpoResult> expected = series
            .Skip(Bars.Count - expectedCount)
            .Take(expectedCount)
            .ToList();

        sut.IsExactly(expected);
    }

    [TestMethod]
    public void CustomBuffer_OverMaxListSize_AutoAdjustsListAndBuffers()
    {
        const int maxListSize = 120;

        DpoList sut = new(lookbackPeriods) {
            MaxListSize = maxListSize
        };

        sut.Add(Bars);

        // DPO maintains 1:1 correspondence with maxListSize pruning
        int expectedCount = Math.Min(maxListSize, Bars.Count);

        sut.Should().HaveCount(expectedCount);

        // Compare with the last expectedCount results from series
        IReadOnlyList<DpoResult> expected = series
            .Skip(Bars.Count - expectedCount)
            .Take(expectedCount)
            .ToList();

        sut.IsExactly(expected);
    }
}
