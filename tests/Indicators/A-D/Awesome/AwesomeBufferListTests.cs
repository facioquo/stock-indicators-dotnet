namespace BufferLists;

[TestClass]
public class AwesomeBufferListTests : BufferListTestBase, ITestChainBufferList
{
    private const int fastPeriods = 5;
    private const int slowPeriods = 34;

    private static readonly IReadOnlyList<IReusable> reusables
        = Bars
            .ToBarPart(CandlePart.HL2)
            .ToList();

    private static readonly IReadOnlyList<AwesomeResult> series
        = Bars.ToAwesome(fastPeriods, slowPeriods);

    [TestMethod]
    public void AddDateAndValue_IncrementsResults()
    {
        AwesomeList sut = new(fastPeriods, slowPeriods);

        foreach (IReusable item in reusables)
        {
            sut.Add(item.Timestamp, item.Value);
        }

        sut.Should().HaveCount(Bars.Count);
        sut.IsExactly(series);
    }

    [TestMethod]
    public void AddReusableItem_IncrementsResults()
    {
        AwesomeList sut = new(fastPeriods, slowPeriods);

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
        AwesomeList sut = new(fastPeriods, slowPeriods) { reusables };

        sut.Should().HaveCount(Bars.Count);
        sut.IsExactly(series);
    }

    [TestMethod]
    public void AddBar_IncrementsResults()
    {
        AwesomeList sut = new(fastPeriods, slowPeriods);

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
        AwesomeList sut = new(fastPeriods, slowPeriods) { Bars };

        sut.Should().HaveCount(Bars.Count);
        sut.IsExactly(series);
    }

    [TestMethod]
    public void BarsCtor_OnInstantiation_IncrementsResults()
    {
        AwesomeList sut = new(fastPeriods, slowPeriods, Bars);

        sut.Should().HaveCount(Bars.Count);
        sut.IsExactly(series);
    }

    [TestMethod]
    public override void Clear_WithState_ResetsState()
    {
        List<Bar> subset = Bars.Take(80).ToList();
        IReadOnlyList<AwesomeResult> expected = subset.ToAwesome(fastPeriods, slowPeriods);

        AwesomeList sut = new(fastPeriods, slowPeriods, subset);

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

        AwesomeList sut = new(fastPeriods, slowPeriods) {
            MaxListSize = maxListSize
        };

        sut.Add(Bars);

        IReadOnlyList<AwesomeResult> expected
            = series.Skip(series.Count - maxListSize).ToList();

        sut.Should().HaveCount(maxListSize);
        sut.IsExactly(expected);
    }
}
