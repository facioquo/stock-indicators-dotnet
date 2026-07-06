namespace BufferLists;

[TestClass]
public class DonchianBufferListTests : BufferListTestBase
{
    private const int lookbackPeriods = 20;

    private static readonly IReadOnlyList<DonchianResult> series
       = Bars.ToDonchian(lookbackPeriods);

    [TestMethod]
    public void AddBars_WithValidBars_IncrementsResults()
    {
        DonchianList sut = new(lookbackPeriods);

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
        DonchianList sut = Bars.ToDonchianList(lookbackPeriods);

        sut.Should().HaveCount(Bars.Count);
        sut.IsExactly(series);
    }

    [TestMethod]
    public void BarsCtor_OnInstantiation_IncrementsResults()
    {
        DonchianList sut = new(lookbackPeriods, Bars);

        sut.Should().HaveCount(Bars.Count);
        sut.IsExactly(series);
    }

    [TestMethod]
    public override void Clear_WithState_ResetsState()
    {
        List<Bar> subset = Bars.Take(80).ToList();
        IReadOnlyList<DonchianResult> expected = subset.ToDonchian(lookbackPeriods);

        DonchianList sut = new(lookbackPeriods, subset);

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

        DonchianList sut = new(lookbackPeriods) {
            MaxListSize = maxListSize
        };

        sut.Add(Bars);

        IReadOnlyList<DonchianResult> expected = series
            .Skip(series.Count - maxListSize)
            .ToList();

        sut.Should().HaveCount(maxListSize);
        sut.IsExactly(expected);
    }
}
