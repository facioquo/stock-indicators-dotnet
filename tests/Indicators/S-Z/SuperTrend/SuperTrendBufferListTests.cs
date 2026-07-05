namespace BufferLists;

[TestClass]
public class SuperTrendBufferListTests : BufferListTestBase, ITestBarBufferList
{
    private const int lookbackPeriods = 10;
    private const double multiplier = 3;

    private static readonly IReadOnlyList<SuperTrendResult> series
       = Bars.ToSuperTrend(lookbackPeriods, multiplier);

    [TestMethod]
    public void AddBar_IncrementsResults()
    {
        SuperTrendList sut = new(lookbackPeriods, multiplier);

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
        SuperTrendList sut = new(lookbackPeriods, multiplier) { Bars };

        sut.Should().HaveCount(Bars.Count);
        sut.IsExactly(series);
    }

    [TestMethod]
    public void BarsCtor_OnInstantiation_IncrementsResults()
    {
        SuperTrendList sut = new(lookbackPeriods, multiplier, Bars);

        sut.Should().HaveCount(Bars.Count);
        sut.IsExactly(series);
    }

    [TestMethod]
    public override void Clear_WithState_ResetsState()
    {
        List<Bar> subset = Bars.Take(80).ToList();
        IReadOnlyList<SuperTrendResult> expected = subset.ToSuperTrend(lookbackPeriods, multiplier);

        SuperTrendList sut = new(lookbackPeriods, multiplier, subset);

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

        SuperTrendList sut = new(lookbackPeriods, multiplier) {
            MaxListSize = maxListSize
        };

        sut.Add(Bars);

        IReadOnlyList<SuperTrendResult> expected = series
            .Skip(series.Count - maxListSize)
            .ToList();

        sut.Should().HaveCount(maxListSize);
        sut.IsExactly(expected);
    }
}
