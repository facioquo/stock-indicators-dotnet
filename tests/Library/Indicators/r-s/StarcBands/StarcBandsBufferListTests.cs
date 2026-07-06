namespace BufferLists;

[TestClass]
public class StarcBandsBufferListTests : BufferListTestBase, ITestBarBufferList
{
    private const int smaPeriods = 5;
    private const double multiplier = 2;
    private const int atrPeriods = 10;

    private static readonly IReadOnlyList<StarcBandsResult> series
       = Bars.ToStarcBands(smaPeriods, multiplier, atrPeriods);

    [TestMethod]
    public void AddBar_IncrementsResults()
    {
        StarcBandsList sut = new(smaPeriods, multiplier, atrPeriods);

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
        StarcBandsList sut = new(smaPeriods, multiplier, atrPeriods) { Bars };

        sut.Should().HaveCount(Bars.Count);
        sut.IsExactly(series);
    }

    [TestMethod]
    public void BarsCtor_OnInstantiation_IncrementsResults()
    {
        StarcBandsList sut = new(smaPeriods, multiplier, atrPeriods, Bars);

        sut.Should().HaveCount(Bars.Count);
        sut.IsExactly(series);
    }

    [TestMethod]
    public override void Clear_WithState_ResetsState()
    {
        List<Bar> subset = Bars.Take(80).ToList();
        IReadOnlyList<StarcBandsResult> expected = subset.ToStarcBands(smaPeriods, multiplier, atrPeriods);

        StarcBandsList sut = new(smaPeriods, multiplier, atrPeriods, subset);

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
        const int maxListSize = 100;

        StarcBandsList sut = new(smaPeriods, multiplier, atrPeriods) {
            MaxListSize = maxListSize
        };

        sut.Add(Bars);

        IReadOnlyList<StarcBandsResult> expected = series
            .Skip(series.Count - maxListSize)
            .ToList();

        sut.Should().HaveCount(maxListSize);
        sut.IsExactly(expected);
    }
}
