namespace BufferLists;

[TestClass]
public class AroonBufferListTests : BufferListTestBase
{
    private const int lookbackPeriods = 25;

    private static readonly IReadOnlyList<AroonResult> series
        = Bars.ToAroon(lookbackPeriods);

    [TestMethod]
    public void Boundary_WithRandomBars_StaysWithinBounds()
    {
        IReadOnlyList<AroonResult> sut = Data
            .GetRandom(2500)
            .ToAroonList(25);

        sut.IsBetween(static x => x.AroonUp, 0d, 100d);
        sut.IsBetween(static x => x.AroonDown, 0d, 100d);
        sut.IsBetween(static x => x.Oscillator, -100d, 100d);
    }

    [TestMethod]
    public void Results_AreAlwaysBounded()
    {
        AroonList sut = new(25, Bars);
        sut.IsBetween(static x => x.AroonUp, 0, 100);
        sut.IsBetween(static x => x.AroonDown, 0, 100);
        sut.IsBetween(static x => x.Oscillator, -100, 100);
    }

    [TestMethod]
    public void AddBars_WithValidBars_IncrementsResults()
    {
        AroonList sut = new(lookbackPeriods);

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
        AroonList sut = new(lookbackPeriods) { Bars };

        sut.Should().HaveCount(Bars.Count);
        sut.IsExactly(series);
    }

    [TestMethod]
    public void BarsCtor_OnInstantiation_IncrementsResults()
    {
        AroonList sut = new(lookbackPeriods, Bars);

        sut.Should().HaveCount(Bars.Count);
        sut.IsExactly(series);
    }

    [TestMethod]
    public override void Clear_WithState_ResetsState()
    {
        List<Bar> subset = Bars.Take(80).ToList();
        IReadOnlyList<AroonResult> expected = subset.ToAroon(lookbackPeriods);

        AroonList sut = new(lookbackPeriods, subset);

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

        AroonList sut = new(lookbackPeriods) {
            MaxListSize = maxListSize
        };

        sut.Add(Bars);

        IReadOnlyList<AroonResult> expected
            = series.Skip(series.Count - maxListSize).ToList();

        sut.Should().HaveCount(maxListSize);
        sut.IsExactly(expected);
    }
}
