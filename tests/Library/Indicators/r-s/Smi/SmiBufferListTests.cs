namespace BufferLists;

[TestClass]
public class SmiBufferListTests : BufferListTestBase, ITestBarBufferList
{
    private const int lookbackPeriods = 13;
    private const int firstSmoothPeriods = 25;
    private const int secondSmoothPeriods = 2;
    private const int signalPeriods = 3;

    private static readonly IReadOnlyList<SmiResult> series
       = Bars.ToSmi(lookbackPeriods, firstSmoothPeriods, secondSmoothPeriods, signalPeriods);

    [TestMethod]
    public void AddBar_IncrementsResults()
    {
        SmiList sut = new(lookbackPeriods, firstSmoothPeriods, secondSmoothPeriods, signalPeriods);

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
        SmiList sut = new(lookbackPeriods, firstSmoothPeriods, secondSmoothPeriods, signalPeriods) { Bars };

        sut.Should().HaveCount(Bars.Count);
        sut.IsExactly(series);
    }

    [TestMethod]
    public void BarsCtor_OnInstantiation_IncrementsResults()
    {
        SmiList sut = new(lookbackPeriods, firstSmoothPeriods, secondSmoothPeriods, signalPeriods, Bars);

        sut.Should().HaveCount(Bars.Count);
        sut.IsExactly(series);
    }

    [TestMethod]
    public override void Clear_WithState_ResetsState()
    {
        List<Bar> subset = Bars.Take(80).ToList();
        IReadOnlyList<SmiResult> expected = subset.ToSmi(lookbackPeriods, firstSmoothPeriods, secondSmoothPeriods, signalPeriods);

        SmiList sut = new(lookbackPeriods, firstSmoothPeriods, secondSmoothPeriods, signalPeriods, subset);

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
        const int maxListSize = 100;

        SmiList sut = new(lookbackPeriods, firstSmoothPeriods, secondSmoothPeriods, signalPeriods) {
            MaxListSize = maxListSize
        };

        sut.Add(Bars);

        IReadOnlyList<SmiResult> expected = series
            .Skip(series.Count - maxListSize)
            .ToList();

        sut.Should().HaveCount(maxListSize);
        sut.IsExactly(expected);
    }
}
