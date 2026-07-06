namespace BufferLists;

[TestClass]
public class PvoBufferListTests : BufferListTestBase, ITestBarBufferList
{
    private const int fastPeriods = 12;
    private const int slowPeriods = 26;
    private const int signalPeriods = 9;

    private static readonly IReadOnlyList<PvoResult> series
       = Bars.ToPvo(fastPeriods, slowPeriods, signalPeriods);

    [TestMethod]
    public void AddBar_IncrementsResults()
    {
        PvoList sut = new(fastPeriods, slowPeriods, signalPeriods);

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
        PvoList sut = Bars.ToPvoList(fastPeriods, slowPeriods, signalPeriods);

        sut.Should().HaveCount(Bars.Count);
        sut.IsExactly(series);
    }

    [TestMethod]
    public void BarsCtor_OnInstantiation_IncrementsResults()
    {
        PvoList sut = new(fastPeriods, slowPeriods, signalPeriods, Bars);

        sut.Should().HaveCount(Bars.Count);
        sut.IsExactly(series);
    }

    [TestMethod]
    public override void Clear_WithState_ResetsState()
    {
        List<Bar> subset = Bars.Take(80).ToList();
        IReadOnlyList<PvoResult> expected = subset.ToPvo(fastPeriods, slowPeriods, signalPeriods);

        PvoList sut = new(fastPeriods, slowPeriods, signalPeriods, subset);

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

        PvoList sut = new(fastPeriods, slowPeriods, signalPeriods) {
            MaxListSize = maxListSize
        };

        sut.Add(Bars);

        IReadOnlyList<PvoResult> expected
            = series.Skip(series.Count - maxListSize).ToList();

        sut.Should().HaveCount(maxListSize);
        sut.IsExactly(expected);
    }
}
