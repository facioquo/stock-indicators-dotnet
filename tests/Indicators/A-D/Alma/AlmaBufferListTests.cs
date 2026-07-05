namespace BufferLists;

[TestClass]
public class AlmaBufferListTests : BufferListTestBase, ITestChainBufferList
{
    private const int lookbackPeriods = 10;
    private const double offset = 0.85;
    private const double sigma = 6;

    private static readonly IReadOnlyList<IReusable> reusables
       = Bars
        .Cast<IReusable>()
        .ToList();

    private static readonly IReadOnlyList<AlmaResult> series
       = Bars.ToAlma(lookbackPeriods, offset, sigma);

    [TestMethod]
    public void AddBar_IncrementsResults()
    {
        AlmaList sut = new(lookbackPeriods, offset, sigma);

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
        AlmaList sut = Bars.ToAlmaList(lookbackPeriods, offset, sigma);

        sut.Should().HaveCount(Bars.Count);
        sut.IsExactly(series);
    }

    [TestMethod]
    public void BarsCtor_OnInstantiation_IncrementsResults()
    {
        AlmaList sut = new(lookbackPeriods, offset, sigma, Bars);

        sut.Should().HaveCount(Bars.Count);
        sut.IsExactly(series);
    }

    [TestMethod]
    public void AddReusableItem_IncrementsResults()
    {
        AlmaList sut = new(lookbackPeriods, offset, sigma);

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
        AlmaList sut = new(lookbackPeriods, offset, sigma) { reusables };

        sut.Should().HaveCount(Bars.Count);
        sut.IsExactly(series);
    }

    [TestMethod]
    public void AddDateAndValue_IncrementsResults()
    {
        AlmaList sut = new(lookbackPeriods, offset, sigma);

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
        IReadOnlyList<AlmaResult> expected = subset.ToAlma(lookbackPeriods, offset, sigma);

        AlmaList sut = new(lookbackPeriods, offset, sigma, subset);

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

        AlmaList sut = new(lookbackPeriods, offset, sigma) {
            MaxListSize = maxListSize
        };

        sut.Add(Bars);

        IReadOnlyList<AlmaResult> expected = series
            .Skip(series.Count - maxListSize)
            .ToList();

        sut.Should().HaveCount(maxListSize);
        sut.IsExactly(expected);
    }
}
