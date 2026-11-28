namespace BufferLists;

[TestClass]
public class VwmaBufferListTests : BufferListTestBase, ITestBarBufferList
{
    private const int lookbackPeriods = 10;

    private static readonly IReadOnlyList<VwmaResult> series
       = Bars.ToVwma(lookbackPeriods);

    [TestMethod]
    public void AddBar_IncrementsResults()
    {
        VwmaList sut = new(lookbackPeriods);

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
        VwmaList sut = new(lookbackPeriods) { Bars };

        sut.Should().HaveCount(Bars.Count);
        sut.IsExactly(series);
    }

    [TestMethod]
    public void BarsCtor_OnInstantiation_IncrementsResults()
    {
        VwmaList sut = new(lookbackPeriods, Bars);

        sut.Should().HaveCount(Bars.Count);
        sut.IsExactly(series);
    }

    [TestMethod]
    public override void Clear_WithState_ResetsState()
    {
        List<Bar> subset = Bars.Take(80).ToList();

        VwmaList sut = new(lookbackPeriods, subset);

        sut.Should().HaveCount(subset.Count);

        sut.Clear();

        sut.Should().BeEmpty();

        foreach (Bar bar in subset)
        {
            sut.Add(bar);
        }

        IReadOnlyList<VwmaResult> expected = subset.ToVwma(lookbackPeriods);

        sut.Should().HaveCount(expected.Count);
        sut.IsExactly(expected);
    }

    [TestMethod]
    public override void PruneList_OverMaxListSize_AutoAdjustsListAndBuffers()
    {
        const int maxListSize = 120;

        VwmaList sut = new(lookbackPeriods) {
            MaxListSize = maxListSize
        };

        foreach (Bar bar in Bars)
        {
            sut.Add(bar);
        }

        IReadOnlyList<VwmaResult> expected
            = series.Skip(series.Count - maxListSize).ToList();

        sut.Should().HaveCount(maxListSize);
        sut.IsExactly(expected);
    }

    [TestMethod]
    [DataRow(CandlePart.Open)]
    [DataRow(CandlePart.High)]
    [DataRow(CandlePart.Low)]
    [DataRow(CandlePart.HLC3)]
    [DataRow(CandlePart.HL2)]
    [DataRow(CandlePart.OC2)]
    [DataRow(CandlePart.OHL3)]
    [DataRow(CandlePart.OHLC4)]
    public void ToVwmaList_SelectedCandlePart_MatchesSeriesExactly(CandlePart candlePart)
    {
        IReadOnlyList<VwmaResult> expected = Bars.ToVwma(14, candlePart);
        VwmaList actual = new(14, candlePart, Bars);
        actual.IsExactly(expected);
        Bars.ToVwmaList(14, candlePart).IsExactly(expected);

        actual.Clear();
        actual.Add(Bars);
        actual.IsExactly(expected);
    }

    [TestMethod]
    public void Add_PreselectedPrice_DoesNotApplyCandlePartAgain()
    {
        DateTime day = new(2026, 1, 1);
        VwmaList list = new(2, CandlePart.High);
        list.Add(day, 10, 2);
        list.Add(day.AddDays(1), 20, 1);
        list[1].Vwma.Should().BeApproximately(40d / 3d, 0.000000000001);
    }
    [TestMethod]
    public void SelectedPart_NameIncludesNonDefaultPrice()
        => new VwmaList(14, CandlePart.HL2).Name.Should().Be("VWMA(14,HL2)");

    [TestMethod]
    public void VolumeAsPrice_Throws()
        => FluentActions.Invoking(() => new VwmaList(14, CandlePart.Volume))
            .Should().ThrowExactly<ArgumentOutOfRangeException>().WithParameterName("candlePart");
}
