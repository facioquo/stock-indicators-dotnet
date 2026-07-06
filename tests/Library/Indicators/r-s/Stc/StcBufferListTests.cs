namespace BufferLists;

[TestClass]
public class StcBufferListTests : BufferListTestBase, ITestChainBufferList
{
    private const int cyclePeriods = 9;
    private const int fastPeriods = 12;
    private const int slowPeriods = 26;

    private static readonly IReadOnlyList<IReusable> reusables
       = Bars
        .Cast<IReusable>()
        .ToList();

    private static readonly IReadOnlyList<StcResult> series
       = Bars.ToStc(cyclePeriods, fastPeriods, slowPeriods);

    [TestMethod]
    public void AddBar_IncrementsResults()
    {
        StcList sut = new(cyclePeriods, fastPeriods, slowPeriods);

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
        StcList sut = Bars.ToStcList(cyclePeriods, fastPeriods, slowPeriods);

        sut.Should().HaveCount(Bars.Count);
        sut.IsExactly(series);
    }

    [TestMethod]
    public void BarsCtor_OnInstantiation_IncrementsResults()
    {
        StcList sut = new(cyclePeriods, fastPeriods, slowPeriods, Bars);

        sut.Should().HaveCount(Bars.Count);
        sut.IsExactly(series);
    }

    [TestMethod]
    public void AddReusableItem_IncrementsResults()
    {
        StcList sut = new(cyclePeriods, fastPeriods, slowPeriods);

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
        StcList sut = new(cyclePeriods, fastPeriods, slowPeriods) { reusables };

        sut.Should().HaveCount(Bars.Count);
        sut.IsExactly(series);
    }

    [TestMethod]
    public void AddDateAndValue_IncrementsResults()
    {
        StcList sut = new(cyclePeriods, fastPeriods, slowPeriods);

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
        List<Bar> subset = Bars.Take(100).ToList();
        IReadOnlyList<StcResult> expected = subset.ToStc(cyclePeriods, fastPeriods, slowPeriods);

        StcList sut = new(cyclePeriods, fastPeriods, slowPeriods, subset);

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

        StcList sut = new(cyclePeriods, fastPeriods, slowPeriods) {
            MaxListSize = maxListSize
        };

        sut.Add(Bars);

        IReadOnlyList<StcResult> expected = series
            .Skip(series.Count - maxListSize)
            .ToList();

        sut.Should().HaveCount(maxListSize);
        sut.IsExactly(expected);
    }

    [TestMethod]
    public void Results_AreAlwaysBounded()
    {
        StcList sut = new(9, 12, 26, Bars);
        sut.IsBetween(static x => x.Stc, 0, 100);
    }
}
