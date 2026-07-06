namespace BufferLists;

[TestClass]
public class FractalBufferListTests : BufferListTestBase, ITestBarBufferList
{
    private const int windowSpan = 2;
    private const EndType endType = EndType.HighLow;

    private static readonly IReadOnlyList<FractalResult> series
       = Bars.ToFractal(windowSpan, endType);

    [TestMethod]
    public void AddBar_IncrementsResults()
    {
        FractalList sut = new(windowSpan, endType);

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
        FractalList sut = new(windowSpan, endType) { Bars };

        sut.Should().HaveCount(Bars.Count);
        sut.IsExactly(series);
    }

    [TestMethod]
    public void BarsCtor_OnInstantiation_IncrementsResults()
    {
        FractalList sut = new(windowSpan, Bars, endType);

        sut.Should().HaveCount(Bars.Count);
        sut.IsExactly(series);
    }

    [TestMethod]
    public override void Clear_WithState_ResetsState()
    {
        List<Bar> subset = Bars.Take(80).ToList();
        IReadOnlyList<FractalResult> expected = subset.ToFractal(windowSpan, endType);

        FractalList sut = new(windowSpan, subset, endType);

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

        FractalList sut = new(windowSpan, endType) {
            MaxListSize = maxListSize
        };

        sut.Add(Bars);

        IReadOnlyList<FractalResult> expected = series
            .Skip(series.Count - maxListSize)
            .ToList();

        sut.Should().HaveCount(maxListSize);
        sut.IsExactly(expected);
    }

    [TestMethod]
    public void DifferentSpans_WithMultipleSpanValues_ProducesCorrectResults()
    {
        const int leftSpan = 2;
        const int rightSpan = 4;

        IReadOnlyList<FractalResult> expected = Bars.ToFractal(leftSpan, rightSpan, endType);
        FractalList sut = new(leftSpan, rightSpan, Bars, endType);

        sut.Should().HaveCount(Bars.Count);
        sut.IsExactly(expected);
    }

    [TestMethod]
    public void CloseEndType_WithCloseTypeSetting_ProducesCorrectResults()
    {
        const EndType closeType = EndType.Close;

        IReadOnlyList<FractalResult> expected = Bars.ToFractal(windowSpan, closeType);
        FractalList sut = new(windowSpan, Bars, closeType);

        sut.Should().HaveCount(Bars.Count);
        sut.IsExactly(expected);
    }
}
