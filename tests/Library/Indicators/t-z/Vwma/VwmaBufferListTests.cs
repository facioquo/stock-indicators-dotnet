namespace BufferLists;

[TestClass]
public class VwmaBufferListTests : BufferListTestBase
{
    private const int lookbackPeriods = 10;

    private static readonly IReadOnlyList<VwmaResult> series
       = Bars.ToVwma(lookbackPeriods);

    [TestMethod]
    public void AddBars_WithValidBars_IncrementsResults()
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
    public void AddBarsBatch_WithValidBars_IncrementsResults()
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
}
