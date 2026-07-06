namespace BufferLists;

[TestClass]
public class VolatilityStopBufferListTests : BufferListTestBase, ITestBarBufferList
{
    private const int lookbackPeriods = 14;
    private const double multiplier = 3;
    private static readonly IReadOnlyList<VolatilityStopResult> expected
        = Bars.ToVolatilityStop(lookbackPeriods, multiplier);

    [TestMethod]
    public void AddBar_IncrementsResults()
    {
        VolatilityStopList sut = new(lookbackPeriods, multiplier);

        foreach (Bar q in Bars)
        {
            sut.Add(q);
        }

        sut.Should().HaveCount(Bars.Count);
        sut.IsExactly(expected);
    }

    [TestMethod]
    public void AddBarsBatch_IncrementsResults()
    {
        VolatilityStopList sut = Bars.ToVolatilityStopList(lookbackPeriods, multiplier);

        sut.Should().HaveCount(Bars.Count);
        sut.IsExactly(expected);
    }

    [TestMethod]
    public void BarsCtor_OnInstantiation_IncrementsResults()
    {
        VolatilityStopList sut = new(lookbackPeriods, multiplier, Bars);

        sut.Should().HaveCount(Bars.Count);
        sut.IsExactly(expected);
    }

    [TestMethod]
    public override void Clear_WithState_ResetsState()
    {
        List<Bar> subset = Bars.Take(80).ToList();
        IReadOnlyList<VolatilityStopResult> expected = subset.ToVolatilityStop(lookbackPeriods, multiplier);

        VolatilityStopList sut = new(lookbackPeriods, multiplier, subset);

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
        VolatilityStopList sut = new(lookbackPeriods, multiplier) {
            MaxListSize = 100
        };

        sut.Add(Bars);

        sut.Should().HaveCount(100);

        // Verify the last 100 items match series results
        IReadOnlyList<VolatilityStopResult> expectedLast100 = expected.Skip(expected.Count - 100).ToList();

        sut.IsExactly(expectedLast100);
    }
}
