namespace BufferLists;

[TestClass]
public class RollingPivotsBufferListTests : BufferListTestBase, ITestBarBufferList
{
    private const int windowPeriods = 20;
    private const int offsetPeriods = 0;
    private const PivotPointType pointType = PivotPointType.Standard;

    private static readonly IReadOnlyList<RollingPivotsResult> series
       = Bars.ToRollingPivots(windowPeriods, offsetPeriods, pointType);

    [TestMethod]
    public void AddBar_IncrementsResults()
    {
        RollingPivotsList sut = new(windowPeriods, offsetPeriods, pointType);

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
        RollingPivotsList sut = new(windowPeriods, offsetPeriods, pointType) { Bars };

        sut.Should().HaveCount(Bars.Count);
        sut.IsExactly(series);
    }

    [TestMethod]
    public void BarsCtor_OnInstantiation_IncrementsResults()
    {
        RollingPivotsList sut = new(windowPeriods, offsetPeriods, pointType, Bars);

        sut.Should().HaveCount(Bars.Count);
        sut.IsExactly(series);
    }

    [TestMethod]
    public override void Clear_WithState_ResetsState()
    {
        List<Bar> subset = Bars.Take(80).ToList();
        IReadOnlyList<RollingPivotsResult> expected = subset.ToRollingPivots(windowPeriods, offsetPeriods, pointType);

        RollingPivotsList sut = new(windowPeriods, offsetPeriods, pointType, subset);

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

        RollingPivotsList sut = new(windowPeriods, offsetPeriods, pointType) {
            MaxListSize = maxListSize
        };

        sut.Add(Bars);

        IReadOnlyList<RollingPivotsResult> expected = series
            .Skip(series.Count - maxListSize)
            .ToList();

        sut.Should().HaveCount(maxListSize);
        sut.IsExactly(expected);
    }

    [TestMethod]
    public void AlternativeParameters_WithFibonacci_MatchesSeriesExactly()
    {
        const int altWindowPeriods = 10;
        const int altOffsetPeriods = 2;
        const PivotPointType altPointType = PivotPointType.Fibonacci;

        IReadOnlyList<RollingPivotsResult> expected = Bars.ToRollingPivots(altWindowPeriods, altOffsetPeriods, altPointType);
        RollingPivotsList sut = new(altWindowPeriods, altOffsetPeriods, altPointType) { Bars };

        sut.Should().HaveCount(Bars.Count);
        sut.IsExactly(expected);
    }

    [TestMethod]
    public void PropertiesAreCorrect_AfterInstantiation_AreSetCorrectly()
    {
        RollingPivotsList sut = new(windowPeriods, offsetPeriods, pointType);

        sut.WindowPeriods.Should().Be(windowPeriods);
        sut.OffsetPeriods.Should().Be(offsetPeriods);
        sut.PointType.Should().Be(pointType);
    }
}
