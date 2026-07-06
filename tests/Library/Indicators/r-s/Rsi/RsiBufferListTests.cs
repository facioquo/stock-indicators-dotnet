namespace BufferLists;

[TestClass]
public class RsiBufferListTests : BufferListTestBase, ITestChainBufferList
{
    private const int lookbackPeriods = 14;

    private static readonly IReadOnlyList<IReusable> reusables
       = Bars
        .Cast<IReusable>()
        .ToList();

    private static readonly IReadOnlyList<RsiResult> series
       = Bars.ToRsi(lookbackPeriods);

    [TestMethod]
    public void Boundary_WithRandomBars_StaysWithinBounds()
    {
        IReadOnlyList<RsiResult> sut = Data
            .GetRandom(2500)
            .ToRsiList(14);

        sut.IsBetween(static x => x.Rsi, 0d, 100d);
    }

    [TestMethod]
    public void AddBar_IncrementsResults()
    {
        RsiList sut = new(lookbackPeriods);

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
        RsiList sut = Bars.ToRsiList(lookbackPeriods);

        sut.Should().HaveCount(Bars.Count);
        sut.IsExactly(series);
    }

    [TestMethod]
    public void BarsCtor_OnInstantiation_IncrementsResults()
    {
        RsiList sut = new(lookbackPeriods, Bars);

        sut.Should().HaveCount(Bars.Count);
        sut.IsExactly(series);
    }

    [TestMethod]
    public void AddReusableItem_IncrementsResults()
    {
        RsiList sut = new(lookbackPeriods);

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
        RsiList sut = new(lookbackPeriods) { reusables };

        sut.Should().HaveCount(Bars.Count);
        sut.IsExactly(series);
    }

    [TestMethod]
    public void AddDateAndValue_IncrementsResults()
    {
        RsiList sut = new(lookbackPeriods);

        foreach (IReusable item in reusables)
        {
            sut.Add(item.Timestamp, item.Value);
        }

        sut.Should().HaveCount(Bars.Count);
        sut.IsExactly(series);
    }

    [TestMethod]
    public void Results_WithAnyInput_AreAlwaysBounded()
    {
        RsiList sut = new(14, Bars);
        sut.IsBetween(static x => x.Rsi, 0, 100);
    }

    [TestMethod]
    public override void Clear_WithState_ResetsState()
    {
        List<Bar> subset = Bars.Take(80).ToList();
        IReadOnlyList<RsiResult> expected = subset.ToRsi(lookbackPeriods);

        RsiList sut = new(lookbackPeriods, subset);

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

        RsiList sut = new(lookbackPeriods) {
            MaxListSize = maxListSize
        };

        sut.Add(Bars);

        IReadOnlyList<RsiResult> expected = series
            .Skip(series.Count - maxListSize)
            .ToList();

        sut.Should().HaveCount(maxListSize);
        sut.IsExactly(expected);
    }
}
