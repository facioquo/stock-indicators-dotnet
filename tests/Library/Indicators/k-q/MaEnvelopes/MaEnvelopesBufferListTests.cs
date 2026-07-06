namespace BufferLists;

[TestClass]
public class MaEnvelopesBufferListTests : BufferListTestBase, ITestChainBufferList
{
    private const int lookbackPeriods = 20;
    private const double percentOffset = 2.5;

    private static readonly IReadOnlyList<IReusable> reusables
       = Bars
        .Cast<IReusable>()
        .ToList();

    private static readonly IReadOnlyList<MaEnvelopeResult> series
       = Bars.ToMaEnvelopes(lookbackPeriods, percentOffset);

    [TestMethod]
    public void AddBar_IncrementsResults()
    {
        MaEnvelopesList sut = new(lookbackPeriods, percentOffset);

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
        MaEnvelopesList sut = Bars.ToMaEnvelopesList(lookbackPeriods, percentOffset);

        sut.Should().HaveCount(Bars.Count);
        sut.IsExactly(series);
    }

    [TestMethod]
    public void BarsCtor_OnInstantiation_IncrementsResults()
    {
        MaEnvelopesList sut = new(lookbackPeriods, percentOffset, MaType.SMA, Bars);

        sut.Should().HaveCount(Bars.Count);
        sut.IsExactly(series);
    }

    [TestMethod]
    public void AddReusableItem_IncrementsResults()
    {
        MaEnvelopesList sut = new(lookbackPeriods, percentOffset);

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
        MaEnvelopesList sut = new(lookbackPeriods, percentOffset) { reusables };

        sut.Should().HaveCount(Bars.Count);
        sut.IsExactly(series);
    }

    [TestMethod]
    public void AddDateAndValue_IncrementsResults()
    {
        MaEnvelopesList sut = new(lookbackPeriods, percentOffset);

        foreach (IReusable item in reusables)
        {
            sut.Add(item.Timestamp, item.Value);
        }

        sut.Should().HaveCount(Bars.Count);
        sut.IsExactly(series);
    }

    [TestMethod]
    public void WithEmaType_AsMaType_ReturnsExpectedResult()
    {
        IReadOnlyList<MaEnvelopeResult> expected = Bars.ToMaEnvelopes(
            lookbackPeriods, percentOffset, MaType.EMA);

        MaEnvelopesList sut = Bars.ToMaEnvelopesList(
            lookbackPeriods, percentOffset, MaType.EMA);

        sut.Should().HaveCount(Bars.Count);
        sut.IsExactly(expected);
    }

    [TestMethod]
    public override void Clear_WithState_ResetsState()
    {
        List<Bar> subset = Bars.Take(80).ToList();
        IReadOnlyList<MaEnvelopeResult> expected = subset.ToMaEnvelopes(lookbackPeriods, percentOffset);

        MaEnvelopesList sut = new(lookbackPeriods, percentOffset, MaType.SMA, subset);

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

        MaEnvelopesList sut = new(lookbackPeriods, percentOffset) {
            MaxListSize = maxListSize
        };

        sut.Add(Bars);

        IReadOnlyList<MaEnvelopeResult> expected = series
            .Skip(series.Count - maxListSize)
            .ToList();

        sut.Should().HaveCount(maxListSize);
        sut.IsExactly(expected);
    }
}
