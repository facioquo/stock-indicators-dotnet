namespace BufferLists;

[TestClass]
public class RocWbBufferListTests : BufferListTestBase, ITestChainBufferList
{
    private const int lookbackPeriods = 20;
    private const int emaPeriods = 3;
    private const int stdDevPeriods = 20;

    private static readonly IReadOnlyList<IReusable> reusables
       = Bars
        .Cast<IReusable>()
        .ToList();

    private static readonly IReadOnlyList<RocWbResult> series
       = Bars.ToRocWb(lookbackPeriods, emaPeriods, stdDevPeriods);

    [TestMethod]
    public void AddBar_IncrementsResults()
    {
        RocWbList sut = new(lookbackPeriods, emaPeriods, stdDevPeriods);

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
        RocWbList sut = new(lookbackPeriods, emaPeriods, stdDevPeriods) { Bars };

        sut.Should().HaveCount(Bars.Count);
        sut.IsExactly(series);
    }

    [TestMethod]
    public void BarsCtor_OnInstantiation_IncrementsResults()
    {
        RocWbList sut = new(lookbackPeriods, emaPeriods, stdDevPeriods, Bars);

        sut.Should().HaveCount(Bars.Count);
        sut.IsExactly(series);
    }

    [TestMethod]
    public override void Clear_WithState_ResetsState()
    {
        List<Bar> subset = Bars.Take(80).ToList();
        IReadOnlyList<RocWbResult> expected = subset.ToRocWb(lookbackPeriods, emaPeriods, stdDevPeriods);

        RocWbList sut = new(lookbackPeriods, emaPeriods, stdDevPeriods, subset);

        sut.Should().HaveCount(subset.Count);
        sut.IsExactly(expected);

        sut.Clear();

        sut.Should().BeEmpty();

        sut.Add(subset);

        sut.Should().HaveCount(expected.Count);
        sut.IsExactly(expected);
    }

    [TestMethod]
    public void AddReusableItem_IncrementsResults()
    {
        RocWbList sut = new(lookbackPeriods, emaPeriods, stdDevPeriods);

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
        RocWbList sut = new(lookbackPeriods, emaPeriods, stdDevPeriods) { reusables };

        sut.Should().HaveCount(Bars.Count);
        sut.IsExactly(series);
    }

    [TestMethod]
    public void AddDateAndValue_IncrementsResults()
    {
        RocWbList sut = new(lookbackPeriods, emaPeriods, stdDevPeriods);

        foreach (IReusable item in reusables)
        {
            sut.Add(item.Timestamp, item.Value);
        }

        sut.Should().HaveCount(Bars.Count);
        sut.IsExactly(series);
    }

    [TestMethod]
    public override void PruneList_OverMaxListSize_AutoAdjustsListAndBuffers()
    {
        const int maxListSize = 120;

        RocWbList sut = new(lookbackPeriods, emaPeriods, stdDevPeriods) {
            MaxListSize = maxListSize
        };

        sut.Add(Bars);

        IReadOnlyList<RocWbResult> expected = series
            .Skip(series.Count - maxListSize)
            .ToList();

        sut.Should().HaveCount(maxListSize);
        sut.IsExactly(expected);
    }
}
