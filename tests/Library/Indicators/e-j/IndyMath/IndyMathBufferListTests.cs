namespace BufferLists;

/// <summary>
/// Series-parity tests shared by every IndyMath buffer list.
/// </summary>
/// <typeparam name="TList">Buffer list under test.</typeparam>
public abstract class IndyMathBufferListTestBase<TList> : BufferListTestBase, ITestChainBufferList
    where TList : BufferList<MathResult>
{
    // SMA warmup NaNs, plus single, consecutive, and trailing NaNs mid-series
    private static readonly int[] gaps = [30, 61, 62, 63, 200, 501];

    private static readonly IReadOnlyList<IReusable> reusables
        = [.. Bars.ToSma(5).Select(static (x, i) =>
            (IReusable)new TimeValue(x.Timestamp, gaps.Contains(i) ? double.NaN : x.Value))];

    private static IIncrementFromChain Chain(TList list) => (IIncrementFromChain)list;

    /// <summary>
    /// Creates an empty list under test.
    /// </summary>
    protected abstract TList NewList();

    /// <summary>
    /// Creates a list under test pre-populated through its chaining constructor.
    /// </summary>
    protected abstract TList NewList(IReadOnlyList<IReusable> values);

    /// <summary>
    /// Calculates the canonical Series results for the same operation.
    /// </summary>
    protected abstract IReadOnlyList<MathResult> Series(IReadOnlyList<IReusable> source);

    [TestMethod]
    public void AddBar_IncrementsResults()
    {
        TList sut = NewList();

        foreach (Bar bar in Bars)
        {
            Chain(sut).Add(bar);
        }

        sut.Should().HaveCount(Bars.Count);
        sut.IsExactly(Series(Bars));
    }

    [TestMethod]
    public void AddBarsBatch_IncrementsResults()
    {
        TList sut = NewList();
        Chain(sut).Add(Bars);

        sut.Should().HaveCount(Bars.Count);
        sut.IsExactly(Series(Bars));
    }

    [TestMethod]
    public void BarsCtor_OnInstantiation_IncrementsResults()
    {
        TList sut = NewList(Bars);

        sut.Should().HaveCount(Bars.Count);
        sut.IsExactly(Series(Bars));
    }

    [TestMethod]
    public void AddReusableItem_IncrementsResults()
    {
        TList sut = NewList();

        foreach (IReusable item in reusables)
        {
            Chain(sut).Add(item);
        }

        sut.Should().HaveCount(reusables.Count);
        sut.IsExactly(Series(reusables));
    }

    [TestMethod]
    public void AddReusableItemBatch_IncrementsResults()
    {
        TList sut = NewList();
        Chain(sut).Add(reusables);

        sut.Should().HaveCount(reusables.Count);
        sut.IsExactly(Series(reusables));
    }

    [TestMethod]
    public void AddDateAndValue_IncrementsResults()
    {
        TList sut = NewList();

        foreach (IReusable item in reusables)
        {
            Chain(sut).Add(item.Timestamp, item.Value);
        }

        sut.Should().HaveCount(reusables.Count);
        sut.IsExactly(Series(reusables));
    }

    [TestMethod]
    public void AddNulls_ThrowsArgumentNullException()
    {
        TList sut = NewList();

        FluentActions.Invoking(() => Chain(sut).Add((IReusable)null!))
            .Should().ThrowExactly<ArgumentNullException>();
        FluentActions.Invoking(() => Chain(sut).Add((IReadOnlyList<IReusable>)null!))
            .Should().ThrowExactly<ArgumentNullException>();
    }

    [TestMethod]
    public override void Clear_WithState_ResetsState()
    {
        List<IReusable> subset = [.. reusables.Take(80)];
        IReadOnlyList<MathResult> expected = Series(subset);

        TList sut = NewList(subset);
        sut.IsExactly(expected);

        sut.Clear();
        sut.Should().BeEmpty();

        Chain(sut).Add(subset);
        sut.Should().HaveCount(expected.Count);
        sut.IsExactly(expected);
    }

    [TestMethod]
    public void Clear_ThenFiniteStart_RestartsWarmup()
    {
        // seed with a prefix that ends on a finite value, then replay a finite start,
        // so any position, NaN marker, or window value Clear() leaves behind shows up
        TList sut = NewList([.. reusables.Take(100)]);
        sut.Clear();

        Chain(sut).Add(Bars);
        sut.IsExactly(Series(Bars));
    }

    [TestMethod]
    public override void PruneList_OverMaxListSize_AutoAdjustsListAndBuffers()
    {
        const int maxListSize = 100;

        TList sut = NewList();
        sut.MaxListSize = maxListSize;
        Chain(sut).Add(reusables);

        IReadOnlyList<MathResult> expected = Series(reusables)
            .Skip(reusables.Count - maxListSize)
            .ToList();

        sut.Should().HaveCount(maxListSize);
        sut.IsExactly(expected);
    }
}

[TestClass]
public class RollingMaxBufferListTests : IndyMathBufferListTestBase<RollingMaxList>
{
    private const int lookbackPeriods = 20;

    protected override RollingMaxList NewList() => new(lookbackPeriods);

    protected override RollingMaxList NewList(IReadOnlyList<IReusable> values) => new(lookbackPeriods, values);

    protected override IReadOnlyList<MathResult> Series(IReadOnlyList<IReusable> source)
        => source.RollingMax(lookbackPeriods);

    [TestMethod]
    public void Lookbacks_AcrossRingGrowth_MatchSeriesExactly()
    {
        // 16-slot initial ring grows past 16 and is capped by lookbacks beyond the count
        foreach (int lookback in (int[])[1, 2, 16, 17, 40, 600, int.MaxValue])
        {
            Bars.ToRollingMaxList(lookback).IsExactly(Bars.RollingMax(lookback));
        }

        // a falling series keeps every value in the deque, forcing growth
        List<TimeValue> falling = [.. Bars.Select(static (b, i) => new TimeValue(b.Timestamp, -i))];
        falling.ToRollingMaxList(50).IsExactly(falling.RollingMax(50));
    }

    [TestMethod]
    public void Properties_AfterConstruction_ReflectArguments()
    {
        RollingMaxList sut = new(14);
        sut.LookbackPeriods.Should().Be(14);
        sut.ToString().Should().Be("ROLLINGMAX(14)");
    }

    [TestMethod]
    public void Exceptions_InvalidLookback_ThrowsArgumentOutOfRangeException()
        => FluentActions.Invoking(static () => new RollingMaxList(0))
            .Should().ThrowExactly<ArgumentOutOfRangeException>().WithParameterName("lookbackPeriods");
}

[TestClass]
public class RollingMinBufferListTests : IndyMathBufferListTestBase<RollingMinList>
{
    private const int lookbackPeriods = 20;

    protected override RollingMinList NewList() => new(lookbackPeriods);

    protected override RollingMinList NewList(IReadOnlyList<IReusable> values) => new(lookbackPeriods, values);

    protected override IReadOnlyList<MathResult> Series(IReadOnlyList<IReusable> source)
        => source.RollingMin(lookbackPeriods);

    [TestMethod]
    public void Lookbacks_AcrossRingGrowth_MatchSeriesExactly()
    {
        foreach (int lookback in (int[])[1, 2, 16, 17, 40, 600, int.MaxValue])
        {
            Bars.ToRollingMinList(lookback).IsExactly(Bars.RollingMin(lookback));
        }

        // a rising series keeps every value in the deque, forcing growth
        List<TimeValue> rising = [.. Bars.Select(static (b, i) => new TimeValue(b.Timestamp, i))];
        rising.ToRollingMinList(50).IsExactly(rising.RollingMin(50));
    }

    [TestMethod]
    public void Properties_AfterConstruction_ReflectArguments()
    {
        RollingMinList sut = new(14);
        sut.LookbackPeriods.Should().Be(14);
        sut.ToString().Should().Be("ROLLINGMIN(14)");
    }

    [TestMethod]
    public void Exceptions_InvalidLookback_ThrowsArgumentOutOfRangeException()
        => FluentActions.Invoking(static () => new RollingMinList(-1))
            .Should().ThrowExactly<ArgumentOutOfRangeException>().WithParameterName("lookbackPeriods");
}

[TestClass]
public class RollingMedianBufferListTests : IndyMathBufferListTestBase<RollingMedianList>
{
    private const int lookbackPeriods = 20;

    protected override RollingMedianList NewList() => new(lookbackPeriods);

    protected override RollingMedianList NewList(IReadOnlyList<IReusable> values) => new(lookbackPeriods, values);

    protected override IReadOnlyList<MathResult> Series(IReadOnlyList<IReusable> source)
        => source.RollingMedian(lookbackPeriods);

    [TestMethod]
    public void Lookbacks_OddEvenAndBeyondCount_MatchSeriesExactly()
    {
        foreach (int lookback in (int[])[1, 2, 7, 600, int.MaxValue])
        {
            Bars.ToRollingMedianList(lookback).IsExactly(Bars.RollingMedian(lookback));
        }
    }

    [TestMethod]
    public void Properties_AfterConstruction_ReflectArguments()
    {
        RollingMedianList sut = new(14);
        sut.LookbackPeriods.Should().Be(14);
        sut.ToString().Should().Be("ROLLINGMEDIAN(14)");
    }

    [TestMethod]
    public void Exceptions_InvalidLookback_ThrowsArgumentOutOfRangeException()
        => FluentActions.Invoking(static () => new RollingMedianList(0))
            .Should().ThrowExactly<ArgumentOutOfRangeException>().WithParameterName("lookbackPeriods");
}
