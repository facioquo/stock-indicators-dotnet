namespace BufferLists;

[TestClass]
public class KeltnerBufferListTests : BufferListTestBase
{
    private const int emaPeriods = 20;
    private const double multiplier = 2;
    private const int atrPeriods = 10;

    private static readonly IReadOnlyList<KeltnerResult> series
       = Bars.ToKeltner(emaPeriods, multiplier, atrPeriods);

    [TestMethod]
    public void AddBars_WithValidBars_IncrementsResults()
    {
        KeltnerList sut = new(emaPeriods, multiplier, atrPeriods);

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
        KeltnerList sut = Bars.ToKeltnerList(emaPeriods, multiplier, atrPeriods);

        sut.Should().HaveCount(Bars.Count);
        sut.IsExactly(series);
    }

    [TestMethod]
    public void BarsCtor_OnInstantiation_IncrementsResults()
    {
        KeltnerList sut = new(emaPeriods, multiplier, atrPeriods, Bars);

        sut.Should().HaveCount(Bars.Count);
        sut.IsExactly(series);
    }

    [TestMethod]
    public override void Clear_WithState_ResetsState()
    {
        List<Bar> subset = Bars.Take(80).ToList();
        IReadOnlyList<KeltnerResult> expected = subset.ToKeltner(emaPeriods, multiplier, atrPeriods);

        KeltnerList sut = new(emaPeriods, multiplier, atrPeriods, subset);

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

        KeltnerList sut = new(emaPeriods, multiplier, atrPeriods) {
            MaxListSize = maxListSize
        };

        sut.Add(Bars);

        IReadOnlyList<KeltnerResult> expected
            = series.Skip(series.Count - maxListSize).ToList();

        sut.Should().HaveCount(maxListSize);
        sut.IsExactly(expected);
    }
}
