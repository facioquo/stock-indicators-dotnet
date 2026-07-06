namespace BufferLists;

/// <summary>
/// Covers <see cref="BufferList{TResult}"/> guard clauses and maintenance
/// behaviors that per-indicator contract tests exercise only indirectly:
/// the <c>MaxListSize</c> setter validation and prune-on-set behavior, and
/// the <c>UpdateInternal</c> bounds checks used by repainting indicators.
/// </summary>
[TestClass]
public class BufferListValidation : TestBase
{
    [TestMethod]
    public void MaxListSize_ZeroOrNegative_Throws()
    {
        SmaList sut = new(5);

        Action setZero = () => sut.MaxListSize = 0;
        Action setNegative = () => sut.MaxListSize = -1;

        setZero.Should().Throw<ArgumentOutOfRangeException>();
        setNegative.Should().Throw<ArgumentOutOfRangeException>();
    }

    [TestMethod]
    public void MaxListSize_SetBelowCurrentCount_PrunesImmediately()
    {
        SmaList sut = new(5);
        sut.Add(Bars.Take(50).ToList());
        sut.Should().HaveCount(50);

        sut.MaxListSize = 20;

        // oldest entries are pruned on assignment, newest are retained
        sut.Should().HaveCount(20);
        sut[^1].Timestamp.Should().Be(Bars[49].Timestamp);
        sut[0].Timestamp.Should().Be(Bars[30].Timestamp);
    }

    [TestMethod]
    public void MaxListSize_SetAboveCurrentCount_DoesNotPrune()
    {
        SmaList sut = new(5);
        sut.Add(Bars.Take(50).ToList());

        sut.MaxListSize = 60;

        sut.Should().HaveCount(50);
    }

    [TestMethod]
    public void UpdateInternal_WithinBounds_ReplacesItem()
    {
        TestList sut = new();
        sut.AddItem(new SmaResult(Bars[0].Timestamp, 1d));
        sut.AddItem(new SmaResult(Bars[1].Timestamp, 2d));

        sut.UpdateItem(1, new SmaResult(Bars[1].Timestamp, 99d));

        sut[1].Sma.Should().Be(99d);
        sut.Should().HaveCount(2);
    }

    [TestMethod]
    public void UpdateInternal_OutOfBounds_Throws()
    {
        TestList sut = new();
        sut.AddItem(new SmaResult(Bars[0].Timestamp, 1d));

        Action updateNegative = () => sut.UpdateItem(-1, new SmaResult(Bars[0].Timestamp, 5d));
        Action updateBeyondEnd = () => sut.UpdateItem(1, new SmaResult(Bars[0].Timestamp, 5d));

        updateNegative.Should().Throw<ArgumentOutOfRangeException>();
        updateBeyondEnd.Should().Throw<ArgumentOutOfRangeException>();
    }

    /// <summary>
    /// Minimal concrete list exposing the protected internals under test.
    /// </summary>
    private sealed class TestList : BufferList<SmaResult>
    {
        public TestList() => Name = "TEST";

        public void AddItem(SmaResult item) => AddInternal(item);

        public void UpdateItem(int index, SmaResult item) => UpdateInternal(index, item);
    }
}
