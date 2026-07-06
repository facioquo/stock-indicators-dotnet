namespace BufferLists;

[TestClass]
public class StochRsiBufferListTests : BufferListTestBase, ITestChainBufferList
{
    private const int rsiPeriods = 14;
    private const int stochPeriods = 14;
    private const int signalPeriods = 3;
    private const int smoothPeriods = 1;

    private static readonly IReadOnlyList<IReusable> reusables
       = Bars
        .Cast<IReusable>()
        .ToList();

    private static readonly IReadOnlyList<StochRsiResult> series
       = Bars.ToStochRsi(rsiPeriods, stochPeriods, signalPeriods, smoothPeriods);

    [TestMethod]
    public void AddBar_IncrementsResults()
    {
        StochRsiList sut = new(rsiPeriods, stochPeriods, signalPeriods, smoothPeriods);

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
        StochRsiList sut = Bars.ToStochRsiList(rsiPeriods, stochPeriods, signalPeriods, smoothPeriods);

        sut.Should().HaveCount(Bars.Count);
        sut.IsExactly(series);
    }

    [TestMethod]
    public void BarsCtor_OnInstantiation_IncrementsResults()
    {
        StochRsiList sut = new(rsiPeriods, stochPeriods, signalPeriods, smoothPeriods, Bars);

        sut.Should().HaveCount(Bars.Count);
        sut.IsExactly(series);
    }

    [TestMethod]
    public void Results_AreAlwaysBounded()
    {
        StochRsiList sut = new(14, 14, 3, 1, Bars);
        sut.IsBetween(static x => x.StochRsi, 0, 100);
        sut.IsBetween(static x => x.Signal, 0, 100);
    }

    [TestMethod]
    public override void Clear_WithState_ResetsState()
    {
        List<Bar> subset = Bars.Take(80).ToList();
        IReadOnlyList<StochRsiResult> expected = subset.ToStochRsi(rsiPeriods, stochPeriods, signalPeriods, smoothPeriods);

        StochRsiList sut = new(rsiPeriods, stochPeriods, signalPeriods, smoothPeriods, subset);

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
        StochRsiList sut = new(rsiPeriods, stochPeriods, signalPeriods, smoothPeriods);

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
        StochRsiList sut = new(rsiPeriods, stochPeriods, signalPeriods, smoothPeriods) { reusables };

        sut.Should().HaveCount(Bars.Count);
        sut.IsExactly(series);
    }

    [TestMethod]
    public void AddDateAndValue_IncrementsResults()
    {
        StochRsiList sut = new(rsiPeriods, stochPeriods, signalPeriods, smoothPeriods);

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

        StochRsiList sut = new(rsiPeriods, stochPeriods, signalPeriods, smoothPeriods) {
            MaxListSize = maxListSize
        };

        sut.Add(Bars);

        IReadOnlyList<StochRsiResult> expected = series
            .Skip(series.Count - maxListSize)
            .ToList();

        sut.Should().HaveCount(maxListSize);
        sut.IsExactly(expected);
    }
}
