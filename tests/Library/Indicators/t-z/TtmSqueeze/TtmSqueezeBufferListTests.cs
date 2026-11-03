namespace BufferLists;

[TestClass]
public class TtmSqueezeBufferListTests : BufferListTestBase, ITestBarBufferList
{
    private const int lookbackPeriods = 20;
    private const double standardDeviations = 2;
    private const double multiplier = 1.5;

    private static readonly IReadOnlyList<TtmSqueezeResult> series
       = Bars.ToTtmSqueeze(lookbackPeriods, standardDeviations, multiplier);

    [TestMethod]
    public void AddBar_IncrementsResults()
    {
        TtmSqueezeList sut = new(lookbackPeriods, standardDeviations, multiplier);

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
        TtmSqueezeList sut = new(lookbackPeriods, standardDeviations, multiplier) { Bars };

        sut.Should().HaveCount(Bars.Count);
        sut.IsExactly(series);
    }

    [TestMethod]
    public void BarsCtor_OnInstantiation_IncrementsResults()
    {
        TtmSqueezeList sut = new(lookbackPeriods, standardDeviations, multiplier, Bars);

        sut.Should().HaveCount(Bars.Count);
        sut.IsExactly(series);
    }

    [TestMethod]
    public override void Clear_WithState_ResetsState()
    {
        List<Bar> subset = Bars.Take(80).ToList();
        IReadOnlyList<TtmSqueezeResult> expected = subset.ToTtmSqueeze(lookbackPeriods, standardDeviations, multiplier);

        TtmSqueezeList sut = new(lookbackPeriods, standardDeviations, multiplier, subset);

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

        TtmSqueezeList sut = new(lookbackPeriods, standardDeviations, multiplier) {
            MaxListSize = maxListSize
        };

        sut.Add(Bars);

        IReadOnlyList<TtmSqueezeResult> expected = series
            .Skip(series.Count - maxListSize)
            .ToList();

        sut.Should().HaveCount(maxListSize);
        sut.IsExactly(expected);
    }
}
