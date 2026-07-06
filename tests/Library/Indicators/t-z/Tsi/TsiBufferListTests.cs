namespace BufferLists;

[TestClass]
public class TsiBufferListTests : BufferListTestBase, ITestChainBufferList
{
    private const int lookbackPeriods = 25;
    private const int smoothPeriods = 13;
    private const int signalPeriods = 7;

    private static readonly IReadOnlyList<IReusable> reusables
       = Bars.Cast<IReusable>().ToList();

    private static readonly IReadOnlyList<TsiResult> series
       = Bars.ToTsi(lookbackPeriods, smoothPeriods, signalPeriods);

    [TestMethod]
    public void Results_AreAlwaysBounded()
    {
        TsiList sut = new(25, 13, 7, Bars);
        sut.IsBetween(static x => x.Tsi, -100, 100);
        sut.IsBetween(static x => x.Signal, -100, 100);
    }

    [TestMethod]
    public void AddBar_IncrementsResults()
    {
        TsiList sut = new(lookbackPeriods, smoothPeriods, signalPeriods);

        foreach (Bar q in Bars) { sut.Add(q); }

        sut.Should().HaveCount(Bars.Count);
        sut.IsExactly(series);
    }

    [TestMethod]
    public void AddBarsBatch_IncrementsResults()
    {
        TsiList sut = Bars.ToTsiList(lookbackPeriods, smoothPeriods, signalPeriods);

        sut.Should().HaveCount(Bars.Count);
        sut.IsExactly(series);
    }

    [TestMethod]
    public void BarsCtor_OnInstantiation_IncrementsResults()
    {
        TsiList sut = new(lookbackPeriods, smoothPeriods, signalPeriods, Bars);

        sut.Should().HaveCount(Bars.Count);
        sut.IsExactly(series);
    }

    [TestMethod]
    public void AddReusableItem_IncrementsResults()
    {
        TsiList sut = new(lookbackPeriods, smoothPeriods, signalPeriods);

        foreach (IReusable item in reusables) { sut.Add(item); }

        sut.Should().HaveCount(Bars.Count);
        sut.IsExactly(series);
    }

    [TestMethod]
    public void AddReusableItemBatch_IncrementsResults()
    {
        TsiList sut = new(lookbackPeriods, smoothPeriods, signalPeriods) { reusables };

        sut.Should().HaveCount(Bars.Count);
        sut.IsExactly(series);
    }

    [TestMethod]
    public void AddDateAndValue_IncrementsResults()
    {
        TsiList sut = new(lookbackPeriods, smoothPeriods, signalPeriods);

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

        TsiList sut = new(lookbackPeriods, smoothPeriods, signalPeriods) {
            MaxListSize = maxListSize
        };

        sut.Add(Bars);

        IReadOnlyList<TsiResult> expected
            = series.Skip(series.Count - maxListSize).ToList();

        sut.Should().HaveCount(maxListSize);
        sut.IsExactly(expected);
    }

    [TestMethod]
    public override void Clear_WithState_ResetsState()
    {
        List<Bar> subset = Bars.Take(80).ToList();
        IReadOnlyList<TsiResult> expected = subset.ToTsi(lookbackPeriods, smoothPeriods, signalPeriods);

        TsiList sut = new(lookbackPeriods, smoothPeriods, signalPeriods, subset);

        sut.Should().HaveCount(subset.Count);
        sut.IsExactly(expected);

        sut.Clear();

        sut.Should().BeEmpty();

        sut.Add(subset);

        sut.Should().HaveCount(expected.Count);
        sut.IsExactly(expected);
    }
}
