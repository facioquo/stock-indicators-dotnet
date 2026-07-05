namespace BufferLists;

[TestClass]
public class FisherTransformBufferListTests : BufferListTestBase, ITestChainBufferList
{
    private const int lookbackPeriods = 10;

    private static readonly IReadOnlyList<IReusable> reusables
       = Bars
        .Cast<IReusable>()
        .ToList();

    private static readonly IReadOnlyList<FisherTransformResult> series
       = Bars.ToFisherTransform(lookbackPeriods);

    [TestMethod]
    public void AddBar_IncrementsResults()
    {
        FisherTransformList sut = new(lookbackPeriods);

        foreach (Bar q in Bars) { sut.Add(q); }

        sut.Should().HaveCount(Bars.Count);
        sut.IsExactly(series);
    }

    [TestMethod]
    public void AddBarsBatch_IncrementsResults()
    {
        FisherTransformList sut = new(lookbackPeriods) { Bars };

        IReadOnlyList<FisherTransformResult> series
            = Bars.ToFisherTransform(lookbackPeriods);

        sut.Should().HaveCount(Bars.Count);
        sut.IsExactly(series);
    }

    [TestMethod]
    public void BarsCtor_OnInstantiation_IncrementsResults()
    {
        FisherTransformList sut = new(lookbackPeriods, Bars);

        sut.Should().HaveCount(Bars.Count);
        sut.IsExactly(series);
    }

    [TestMethod]
    public override void PruneList_OverMaxListSize_AutoAdjustsListAndBuffers()
    {
        const int maxListSize = 120;

        FisherTransformList sut = new(lookbackPeriods) {
            MaxListSize = maxListSize
        };

        sut.Add(Bars);

        IReadOnlyList<FisherTransformResult> expected
            = series.Skip(series.Count - maxListSize).ToList();

        sut.Should().HaveCount(maxListSize);
        sut.IsExactly(expected);
    }

    [TestMethod]
    public override void Clear_WithState_ResetsState()
    {
        List<Bar> subset = Bars.Take(80).ToList();
        IReadOnlyList<FisherTransformResult> expected = subset.ToFisherTransform(lookbackPeriods);

        FisherTransformList sut = new(lookbackPeriods, subset);

        sut.Should().HaveCount(subset.Count);
        sut.IsExactly(expected);

        sut.Clear();

        sut.Should().BeEmpty();

        sut.Add(subset);

        sut.Should().HaveCount(expected.Count);
        sut.IsExactly(expected);
    }

    [TestMethod]
    public void AddDateAndValue_IncrementsResults()
    {
        FisherTransformList sut = new(lookbackPeriods);

        foreach (IReusable item in reusables)
        {
            sut.Add(item.Timestamp, item.Value);
        }

        // For FisherTransform with IReusable, we're using Close values
        // whereas with IBar we use HL2, so we need to compare to reusable series
        IReadOnlyList<FisherTransformResult> reusableSeries = reusables.ToFisherTransform(lookbackPeriods);

        sut.Should().HaveCount(Bars.Count);
        sut.IsExactly(reusableSeries);
    }

    [TestMethod]
    public void AddReusableItem_IncrementsResults()
    {
        FisherTransformList sut = new(lookbackPeriods);

        foreach (IReusable item in reusables)
        {
            sut.Add(item);
        }

        // FisherTransform with IReusable derived from IBar should use HL2, same as IBar
        sut.Should().HaveCount(Bars.Count);
        sut.IsExactly(series);
    }

    [TestMethod]
    public void AddReusableItemBatch_IncrementsResults()
    {
        FisherTransformList sut = new(lookbackPeriods) { reusables };

        // FisherTransform with IReusable derived from IBar should use HL2, same as IBar
        sut.Should().HaveCount(Bars.Count);
        sut.IsExactly(series);
    }
}
