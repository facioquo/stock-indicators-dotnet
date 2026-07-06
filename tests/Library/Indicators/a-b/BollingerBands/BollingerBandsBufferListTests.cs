namespace BufferLists;

[TestClass]
public class BollingerBandsBufferListTests : BufferListTestBase, ITestChainBufferList
{
    private const int lookbackPeriods = 20;
    private const double standardDeviations = 2;

    private static readonly IReadOnlyList<IReusable> reusables
        = Bars
            .Cast<IReusable>()
            .ToList();

    private static readonly IReadOnlyList<BollingerBandsResult> series
        = Bars.ToBollingerBands(lookbackPeriods, standardDeviations);

    [TestMethod]
    public void AddBar_IncrementsResults()
    {
        BollingerBandsList sut = new(lookbackPeriods, standardDeviations);

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
        BollingerBandsList sut = new(lookbackPeriods, standardDeviations) { Bars };

        sut.Should().HaveCount(Bars.Count);
        sut.IsExactly(series);
    }

    [TestMethod]
    public void BarsCtor_OnInstantiation_IncrementsResults()
    {
        BollingerBandsList sut = new(lookbackPeriods, standardDeviations, Bars);

        sut.Should().HaveCount(Bars.Count);
        sut.IsExactly(series);
    }

    [TestMethod]
    public void AddReusableItem_IncrementsResults()
    {
        BollingerBandsList sut = new(lookbackPeriods, standardDeviations);

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
        BollingerBandsList sut = new(lookbackPeriods, standardDeviations) { reusables };

        sut.Should().HaveCount(Bars.Count);
        sut.IsExactly(series);
    }

    [TestMethod]
    public void AddDateAndValue_IncrementsResults()
    {
        BollingerBandsList sut = new(lookbackPeriods, standardDeviations);

        foreach (IReusable item in reusables)
        {
            sut.Add(item.Timestamp, item.Value);
        }

        sut.Should().HaveCount(Bars.Count);
        sut.IsExactly(series);
    }

    [TestMethod]
    public override void Clear_WithState_ResetsState()
    {
        List<Bar> subset = Bars.Take(80).ToList();
        IReadOnlyList<BollingerBandsResult> expected = subset.ToBollingerBands(lookbackPeriods, standardDeviations);

        BollingerBandsList sut = new(lookbackPeriods, standardDeviations, subset);

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

        BollingerBandsList sut = new(lookbackPeriods, standardDeviations) {
            MaxListSize = maxListSize
        };

        sut.Add(Bars);

        IReadOnlyList<BollingerBandsResult> expected = series
            .Skip(series.Count - maxListSize)
            .ToList();

        sut.Should().HaveCount(maxListSize);
        sut.IsExactly(expected);
    }
}
