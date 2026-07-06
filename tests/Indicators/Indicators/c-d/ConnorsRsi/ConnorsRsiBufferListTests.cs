namespace BufferLists;

[TestClass]
public class ConnorsRsiBufferListTests : BufferListTestBase, ITestChainBufferList
{
    private const int rsiPeriods = 3;
    private const int streakPeriods = 2;
    private const int rankPeriods = 100;

    private static readonly IReadOnlyList<IReusable> reusables
       = Bars
        .Cast<IReusable>()
        .ToList();

    private static readonly IReadOnlyList<ConnorsRsiResult> series
       = Bars.ToConnorsRsi(rsiPeriods, streakPeriods, rankPeriods);

    [TestMethod]
    public void Boundary_WithRandomBars_StaysWithinBounds()
    {
        IReadOnlyList<ConnorsRsiResult> sut = Data
            .GetRandom(2500)
            .ToConnorsRsiList(3, 2, 100);

        sut.IsBetween(static x => x.ConnorsRsi, 0d, 100d);
    }

    [TestMethod]
    public void AddBar_IncrementsResults()
    {
        ConnorsRsiList sut = new(rsiPeriods, streakPeriods, rankPeriods);

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
        ConnorsRsiList sut = Bars.ToConnorsRsiList(rsiPeriods, streakPeriods, rankPeriods);

        sut.Should().HaveCount(Bars.Count);
        sut.IsExactly(series);
    }

    [TestMethod]
    public void BarsCtor_OnInstantiation_IncrementsResults()
    {
        ConnorsRsiList sut = new(rsiPeriods, streakPeriods, rankPeriods, Bars);

        sut.Should().HaveCount(Bars.Count);
        sut.IsExactly(series);
    }

    [TestMethod]
    public void AddReusableItem_IncrementsResults()
    {
        ConnorsRsiList sut = new(rsiPeriods, streakPeriods, rankPeriods);

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
        ConnorsRsiList sut = new(rsiPeriods, streakPeriods, rankPeriods) { reusables };

        sut.Should().HaveCount(Bars.Count);
        sut.IsExactly(series);
    }

    [TestMethod]
    public void AddDateAndValue_IncrementsResults()
    {
        ConnorsRsiList sut = new(rsiPeriods, streakPeriods, rankPeriods);

        foreach (IReusable item in reusables)
        {
            sut.Add(item.Timestamp, item.Value);
        }

        sut.Should().HaveCount(Bars.Count);
        sut.IsExactly(series);
    }

    [TestMethod]
    public void Results_AreAlwaysBounded()
    {
        ConnorsRsiList sut = new(3, 2, 100, Bars);
        sut.IsBetween(static x => x.ConnorsRsi, 0, 100);
    }

    [TestMethod]
    public override void Clear_WithState_ResetsState()
    {
        List<Bar> subset = Bars.Take(80).ToList();
        IReadOnlyList<ConnorsRsiResult> expected = subset.ToConnorsRsi(rsiPeriods, streakPeriods, rankPeriods);

        ConnorsRsiList sut = new(rsiPeriods, streakPeriods, rankPeriods, subset);

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

        ConnorsRsiList sut = new(rsiPeriods, streakPeriods, rankPeriods) {
            MaxListSize = maxListSize
        };

        sut.Add(Bars);

        IReadOnlyList<ConnorsRsiResult> expected = series
            .Skip(series.Count - maxListSize)
            .ToList();

        sut.Should().HaveCount(maxListSize);
        sut.IsExactly(expected);
    }
}
