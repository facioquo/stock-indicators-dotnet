namespace BufferLists;

[TestClass]
public class ChaikinOscBufferListTests : BufferListTestBase
{
    private const int fastPeriods = 3;
    private const int slowPeriods = 10;

    private static readonly IReadOnlyList<ChaikinOscResult> series
       = Bars.ToChaikinOsc(fastPeriods, slowPeriods);

    [TestMethod]
    public void AddBars_WithValidBars_IncrementsResults()
    {
        ChaikinOscList sut = new(fastPeriods, slowPeriods);

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
        ChaikinOscList sut = new(fastPeriods, slowPeriods) { Bars };

        sut.Should().HaveCount(Bars.Count);
        sut.IsExactly(series);
    }

    [TestMethod]
    public void BarsCtor_OnInstantiation_IncrementsResults()
    {
        ChaikinOscList sut = new(fastPeriods, slowPeriods, Bars);

        sut.Should().HaveCount(Bars.Count);
        sut.IsExactly(series);
    }

    [TestMethod]
    public override void Clear_WithState_ResetsState()
    {
        List<Bar> subset = Bars.Take(80).ToList();
        IReadOnlyList<ChaikinOscResult> expected = subset.ToChaikinOsc(fastPeriods, slowPeriods);

        ChaikinOscList sut = new(fastPeriods, slowPeriods, subset);

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

        ChaikinOscList sut = new(fastPeriods, slowPeriods) {
            MaxListSize = maxListSize
        };

        sut.Add(Bars);

        IReadOnlyList<ChaikinOscResult> expected = series
            .Skip(series.Count - maxListSize)
            .ToList();

        sut.Should().HaveCount(maxListSize);
        sut.IsExactly(expected);
    }
}
