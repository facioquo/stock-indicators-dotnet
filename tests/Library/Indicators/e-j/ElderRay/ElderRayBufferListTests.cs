namespace BufferLists;

[TestClass]
public class ElderRayBufferListTests : BufferListTestBase
{
    private const int lookbackPeriods = 13;

    private static readonly IReadOnlyList<ElderRayResult> series
       = Bars.ToElderRay(lookbackPeriods);

    [TestMethod]
    public void AddBars_WithValidBars_IncrementsResults()
    {
        ElderRayList sut = new(lookbackPeriods);

        foreach (Bar q in Bars) { sut.Add(q); }

        sut.Should().HaveCount(Bars.Count);
        sut.IsExactly(series);
    }

    [TestMethod]
    public void AddBarsBatch_WithValidBars_IncrementsResults()
    {
        ElderRayList sut = new(lookbackPeriods) { Bars };

        IReadOnlyList<ElderRayResult> series
            = Bars.ToElderRay(lookbackPeriods);

        sut.Should().HaveCount(Bars.Count);
        sut.IsExactly(series);
    }

    [TestMethod]
    public void BarsCtor_OnInstantiation_IncrementsResults()
    {
        ElderRayList sut = new(lookbackPeriods, Bars);

        sut.Should().HaveCount(Bars.Count);
        sut.IsExactly(series);
    }

    [TestMethod]
    public override void PruneList_OverMaxListSize_AutoAdjustsListAndBuffers()
    {
        const int maxListSize = 120;

        ElderRayList sut = new(lookbackPeriods) {
            MaxListSize = maxListSize
        };

        sut.Add(Bars);

        IReadOnlyList<ElderRayResult> expected
            = series.Skip(series.Count - maxListSize).ToList();

        sut.Should().HaveCount(maxListSize);
        sut.IsExactly(expected);
    }

    [TestMethod]
    public override void Clear_WithState_ResetsState()
    {
        List<Bar> subset = Bars.Take(80).ToList();
        IReadOnlyList<ElderRayResult> expected = subset.ToElderRay(lookbackPeriods);

        ElderRayList sut = new(lookbackPeriods, subset);

        sut.Should().HaveCount(subset.Count);
        sut.IsExactly(expected);

        sut.Clear();

        sut.Should().BeEmpty();

        sut.Add(subset);

        sut.Should().HaveCount(expected.Count);
        sut.IsExactly(expected);
    }
}
