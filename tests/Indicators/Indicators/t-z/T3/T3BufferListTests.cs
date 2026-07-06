namespace BufferLists;

[TestClass]
public class T3BufferListTests : BufferListTestBase, ITestChainBufferList
{
    private const int lookbackPeriods = 5;
    private const double volumeFactor = 0.7;

    private static readonly IReadOnlyList<IReusable> reusables
       = Bars
        .Cast<IReusable>()
        .ToList();

    private static readonly IReadOnlyList<T3Result> series
       = Bars.ToT3(lookbackPeriods, volumeFactor);

    [TestMethod]
    public void AddBar_IncrementsResults()
    {
        T3List sut = new(lookbackPeriods, volumeFactor);

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
        T3List sut = new(lookbackPeriods, volumeFactor) { Bars };

        sut.Should().HaveCount(Bars.Count);
        sut.IsExactly(series);
    }

    [TestMethod]
    public void BarsCtor_OnInstantiation_IncrementsResults()
    {
        T3List sut = new(lookbackPeriods, volumeFactor, Bars);

        sut.Should().HaveCount(Bars.Count);
        sut.IsExactly(series);
    }

    [TestMethod]
    public void BarsCtorPartial_OnSplitInitialization_IncrementsResults()
    {
        // Test split initialization: half on construction, half after
        int splitPoint = Bars.Count / 2;
        List<Bar> firstHalf = Bars.Take(splitPoint).ToList();
        List<Bar> secondHalf = Bars.Skip(splitPoint).ToList();

        T3List sut = new(lookbackPeriods, volumeFactor, firstHalf);

        foreach (Bar q in secondHalf)
        {
            sut.Add(q);
        }

        sut.Should().HaveCount(Bars.Count);
        sut.IsExactly(series);
    }

    [TestMethod]
    public override void Clear_WithState_ResetsState()
    {
        List<Bar> subset = Bars.Take(80).ToList();
        IReadOnlyList<T3Result> expected = subset.ToT3(lookbackPeriods, volumeFactor);

        T3List sut = new(lookbackPeriods, volumeFactor, subset);

        sut.Should().HaveCount(subset.Count);
        sut.IsExactly(expected);

        sut.Clear();

        sut.Should().BeEmpty();

        sut.Add(subset);

        sut.Should().HaveCount(expected.Count);
        sut.IsExactly(expected);
    }

    [TestMethod]
    public void AddReusableItem_IncrementsResults()
    {
        T3List sut = new(lookbackPeriods, volumeFactor);

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
        T3List sut = new(lookbackPeriods, volumeFactor) { reusables };

        sut.Should().HaveCount(Bars.Count);
        sut.IsExactly(series);
    }

    [TestMethod]
    public void AddDateAndValue_IncrementsResults()
    {
        T3List sut = new(lookbackPeriods, volumeFactor);

        foreach (IReusable item in reusables)
        {
            sut.Add(item.Timestamp, item.Value);
        }

        sut.Should().HaveCount(Bars.Count);
        sut.IsExactly(series);
    }

    [TestMethod]
    public override void PruneList_OverMaxListSize_AutoAdjustsListAndBuffers()
    {
        const int maxListSize = 120;

        T3List sut = new(lookbackPeriods, volumeFactor) {
            MaxListSize = maxListSize
        };

        sut.Add(Bars);

        IReadOnlyList<T3Result> expected = series
            .Skip(series.Count - maxListSize)
            .ToList();

        sut.Should().HaveCount(maxListSize);
        sut.IsExactly(expected);
    }
}
