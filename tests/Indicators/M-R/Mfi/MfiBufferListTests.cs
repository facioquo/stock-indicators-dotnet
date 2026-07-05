namespace BufferLists;

[TestClass]
public class MfiBufferListTests : BufferListTestBase
{
    private const int lookbackPeriods = 14;

    private static readonly IReadOnlyList<MfiResult> series
       = Bars.ToMfi(lookbackPeriods);

    [TestMethod]
    public void Boundary_WithRandomBars_StaysWithinBounds()
    {
        IReadOnlyList<MfiResult> sut = Data
            .GetRandom(2500)
            .ToMfiList(14);

        sut.IsBetween(static x => x.Mfi, 0d, 100d);
    }

    [TestMethod]
    public void Results_WithAnyInput_AreAlwaysBounded()
    {
        MfiList sut = new(14, Bars);
        sut.IsBetween(static x => x.Mfi, 0, 100);
    }

    [TestMethod]
    public void AddBars_WithValidBars_IncrementsResults()
    {
        MfiList sut = new(lookbackPeriods);

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
        MfiList sut = new(lookbackPeriods) { Bars };

        sut.Should().HaveCount(Bars.Count);
        sut.IsExactly(series);
    }

    [TestMethod]
    public void BarsCtor_OnInstantiation_IncrementsResults()
    {
        MfiList sut = new(lookbackPeriods, Bars);

        sut.Should().HaveCount(Bars.Count);
        sut.IsExactly(series);
    }

    [TestMethod]
    public override void Clear_WithState_ResetsState()
    {
        List<Bar> subset = Bars.Take(80).ToList();
        IReadOnlyList<MfiResult> expected = subset.ToMfi(lookbackPeriods);

        MfiList sut = new(lookbackPeriods, subset);

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

        MfiList sut = new(lookbackPeriods) {
            MaxListSize = maxListSize
        };

        sut.Add(Bars);

        IReadOnlyList<MfiResult> expected = series
            .Skip(series.Count - maxListSize)
            .ToList();

        sut.Should().HaveCount(maxListSize);
        sut.IsExactly(expected);
    }
}
