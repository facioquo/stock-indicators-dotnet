namespace BufferLists;

[TestClass]
public class KvoBufferListTests : BufferListTestBase
{
    private const int fastPeriods = 34;
    private const int slowPeriods = 55;
    private const int signalPeriods = 13;

    private static readonly IReadOnlyList<KvoResult> series
       = Bars.ToKvo(fastPeriods, slowPeriods, signalPeriods);

    [TestMethod]
    public void AddBars_WithValidBars_IncrementsResults()
    {
        KvoList sut = new(fastPeriods, slowPeriods, signalPeriods);

        foreach (Bar bar in Bars)
        {
            sut.Add(bar);
        }

        sut.Should().HaveCount(Bars.Count);
        sut.IsExactly(series);
    }

    [TestMethod]
    public void AddBarsBatch_WithValidBars_IncrementsResults()
    {
        KvoList sut = Bars.ToKvoList(fastPeriods, slowPeriods, signalPeriods);

        sut.Should().HaveCount(Bars.Count);
        sut.IsExactly(series);
    }

    [TestMethod]
    public void BarsCtor_OnInstantiation_IncrementsResults()
    {
        KvoList sut = new(fastPeriods, slowPeriods, signalPeriods, Bars);

        sut.Should().HaveCount(Bars.Count);
        sut.IsExactly(series);
    }

    [TestMethod]
    public override void Clear_WithState_ResetsState()
    {
        List<Bar> subset = Bars.Take(80).ToList();
        IReadOnlyList<KvoResult> expected = subset.ToKvo(fastPeriods, slowPeriods, signalPeriods);

        KvoList sut = new(fastPeriods, slowPeriods, signalPeriods, subset);

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

        KvoList sut = new(fastPeriods, slowPeriods, signalPeriods) {
            MaxListSize = maxListSize
        };

        sut.Add(Bars);

        IReadOnlyList<KvoResult> expected
            = series.Skip(series.Count - maxListSize).ToList();

        sut.Should().HaveCount(maxListSize);
        sut.IsExactly(expected);
    }
}
