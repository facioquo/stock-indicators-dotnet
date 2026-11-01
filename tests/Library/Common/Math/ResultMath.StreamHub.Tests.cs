namespace StreamHubs;

/// <summary>
/// Series-parity tests shared by every result math streaming hub.
/// </summary>
/// <typeparam name="THub">Hub under test.</typeparam>
public abstract class ResultMathHubTestBase<THub> : StreamHubTestBase, ITestChainObserver, ITestChainProvider
    where THub : ChainHub<IReusable, TimeValue>
{
    /// <summary>
    /// Creates the hub under test on a provider.
    /// </summary>
    protected abstract THub NewHub(IChainProvider<IReusable> provider);

    /// <summary>
    /// Calculates the canonical Series results for the same operation.
    /// </summary>
    protected abstract IReadOnlyList<TimeValue> Series(IReadOnlyList<IReusable> source);

    /// <summary>
    /// Gets the hub name the operation under test should report.
    /// </summary>
    protected abstract string ExpectedName { get; }

    [TestMethod]
    public void BarObserver_WithWarmupLateArrivalAndRemoval_MatchesSeriesExactly()
    {
        BarHub barHub = new();
        barHub.Add(Bars.Take(20));

        THub observer = NewHub(barHub);
        IReadOnlyList<TimeValue> sut = observer.Results;

        for (int i = 20; i < barsCount; i++)
        {
            // skip one (add later)
            if (i == 80) { continue; }

            Bar q = Bars[i];
            barHub.Add(q);

            // resend duplicate bars
            if (i is > 100 and < 105) { barHub.Add(q); }
        }

        // late arrival triggers observer rebuild, should equal series
        barHub.Add(Bars[80]);
        sut.IsExactly(Series(Bars));

        // delete, should equal series (revised)
        barHub.RemoveAt(removeAtIndex);
        sut.IsExactly(Series(RevisedBars));
        sut.Should().HaveCount(barsCount - 1);

        observer.Unsubscribe();
        barHub.EndTransmission();
    }

    [TestMethod]
    public void WithCachePruning_MatchesSeriesExactly()
    {
        const int maxCacheSize = 50;
        const int totalBars = 100;

        IReadOnlyList<Bar> bars = Bars.Take(totalBars).ToList();
        IReadOnlyList<TimeValue> expected = Series(bars)
            .TakeLast(maxCacheSize)
            .ToList();

        BarHub barHub = new(maxCacheSize);
        THub observer = NewHub(barHub);

        barHub.Add(bars);

        barHub.Bars.Should().HaveCount(maxCacheSize);
        observer.Results.Should().HaveCount(maxCacheSize);
        observer.Results.IsExactly(expected);

        observer.Unsubscribe();
        barHub.EndTransmission();
    }

    [TestMethod]
    public void ChainObserver_ChainedProvider_MatchesSeriesExactly()
    {
        const int smaPeriods = 5;

        // SMA warmup feeds NaN values into the hub under test
        BarHub barHub = new();
        THub observer = NewHub(barHub.ToSmaHub(smaPeriods));

        for (int i = 0; i < barsCount; i++)
        {
            // skip one (add later)
            if (i == 80) { continue; }

            barHub.Add(Bars[i]);
        }

        barHub.Add(Bars[80]);
        barHub.RemoveAt(removeAtIndex);

        IReadOnlyList<TimeValue> sut = observer.Results;
        sut.Should().HaveCount(barsCount - 1);
        sut.IsExactly(Series(RevisedBars.ToSma(smaPeriods)));

        observer.Unsubscribe();
        barHub.EndTransmission();
    }

    [TestMethod]
    public void ChainProvider_MatchesSeriesExactly()
    {
        const int emaPeriods = 12;

        BarHub barHub = new();
        EmaHub observer = NewHub(barHub).ToEmaHub(emaPeriods);

        for (int i = 0; i < barsCount; i++)
        {
            // skip one (add later)
            if (i == 80) { continue; }

            Bar q = Bars[i];
            barHub.Add(q);

            // resend duplicate bars
            if (i is > 100 and < 105) { barHub.Add(q); }
        }

        barHub.Add(Bars[80]);
        barHub.RemoveAt(removeAtIndex);

        IReadOnlyList<EmaResult> sut = observer.Results;
        sut.Should().HaveCount(barsCount - 1);
        sut.IsExactly(Series(RevisedBars).ToEma(emaPeriods));

        observer.Unsubscribe();
        barHub.EndTransmission();
    }

    [TestMethod]
    public void NaNInRollbackWindow_LateArrivalAndRemoval_MatchesSeriesExactly()
    {
        // zero bars make ROC(1) emit NaN mid-series: single, consecutive, and late gaps
        int[] zeroes = [30, 61, 62, 63, 200];
        List<Bar> bars = [.. Bars.Select((b, i) => zeroes.Contains(i)
            ? b with { Open = 0, High = 0, Low = 0, Close = 0 }
            : b)];

        BarHub barHub = new();
        THub observer = NewHub(barHub.ToRocHub(1));

        for (int i = 0; i < barsCount; i++)
        {
            // skip one just after the NaN run, so its rebuild window replays the NaNs
            if (i == 66) { continue; }

            barHub.Add(bars[i]);
        }

        barHub.Add(bars[66]);
        observer.Results.IsExactly(Series(bars.ToRoc(1)));

        // removal whose replay window also holds the NaN run
        barHub.RemoveAt(70);
        bars.RemoveAt(70);
        observer.Results.IsExactly(Series(bars.ToRoc(1)));

        observer.Unsubscribe();
        barHub.EndTransmission();
    }

    [TestMethod]
    public void SameTimestampCorrection_BeforeWindowFills_MatchesSeriesExactly()
    {
        // correcting an early bar rolls back to a restore index below the lookback
        const int correctionIndex = 5;

        List<Bar> bars = [.. Bars];
        Bar original = bars[correctionIndex];
        Bar corrected = original with {
            Open = original.Open * 1.05m,
            High = original.High * 1.05m,
            Low = original.Low * 1.05m,
            Close = original.Close * 1.05m
        };

        BarHub barHub = new();
        THub observer = NewHub(barHub);

        barHub.Add(bars);
        barHub.Add(corrected);

        bars[correctionIndex] = corrected;
        observer.Results.Should().HaveCount(barsCount);
        observer.Results.IsExactly(Series(bars));

        observer.Unsubscribe();
        barHub.EndTransmission();
    }

    [TestMethod]
    public void Exceptions_NullProvider_ThrowsArgumentNullException()
        => FluentActions.Invoking(() => NewHub(null!))
            .Should().ThrowExactly<ArgumentNullException>().WithParameterName("chainProvider");

    [TestMethod]
    public override void ToStringOverride_ReturnsExpectedName()
    {
        THub hub = NewHub(new BarHub());
        hub.ToString().Should().Be(ExpectedName);
    }
}

[TestClass]
public class ScalarMathHubTests : ResultMathHubTestBase<ScalarMathHub>
{
    protected override string ExpectedName => "PLUSPERCENT(-2.5)";

    protected override ScalarMathHub NewHub(IChainProvider<IReusable> provider)
        => provider.ToPlusPercentHub(-2.5);

    protected override IReadOnlyList<TimeValue> Series(IReadOnlyList<IReusable> source)
        => source.PlusPercent(-2.5);

    [TestMethod]
    public void EveryOperation_StreamedBars_MatchesSeriesExactly()
    {
        BarHub barHub = new();
        ScalarMathHub plus = barHub.ToPlusHub(2.5);
        ScalarMathHub minus = barHub.ToMinusHub(2.5);
        ScalarMathHub times = barHub.ToTimesHub(-3);
        ScalarMathHub divided = barHub.ToDividedByHub(7);

        barHub.Add(Bars);

        plus.Results.IsExactly(Bars.Plus(2.5));
        minus.Results.IsExactly(Bars.Minus(2.5));
        times.Results.IsExactly(Bars.Times(-3));
        divided.Results.IsExactly(Bars.DividedBy(7));

        divided.Operation.Should().Be(ScalarOperation.DividedBy);
        divided.Operand.Should().Be(7);

        barHub.EndTransmission();
    }

    [TestMethod]
    public void Exceptions_InvalidOperand_ThrowsArgumentOutOfRangeException()
    {
        BarHub barHub = new();

        FluentActions.Invoking(() => barHub.ToDividedByHub(0))
            .Should().ThrowExactly<ArgumentOutOfRangeException>().WithParameterName("divisor");
        FluentActions.Invoking(() => barHub.ToPlusHub(double.NaN))
            .Should().ThrowExactly<ArgumentOutOfRangeException>().WithParameterName("amount");
    }
}

[TestClass]
public class RollingMaxHubTests : ResultMathHubTestBase<RollingMaxHub>
{
    private const int lookbackPeriods = 20;

    protected override string ExpectedName => "ROLLINGMAX(20)";

    protected override RollingMaxHub NewHub(IChainProvider<IReusable> provider)
        => provider.ToRollingMaxHub(lookbackPeriods);

    protected override IReadOnlyList<TimeValue> Series(IReadOnlyList<IReusable> source)
        => source.RollingMax(lookbackPeriods);

    [TestMethod]
    public void Exceptions_InvalidLookback_ThrowsArgumentOutOfRangeException()
    {
        FluentActions.Invoking(static () => new BarHub().ToRollingMaxHub(0))
            .Should().ThrowExactly<ArgumentOutOfRangeException>().WithParameterName("lookbackPeriods");

        // the window must fit in the provider's cache
        FluentActions.Invoking(static () => new BarHub(50).ToRollingMaxHub(51))
            .Should().ThrowExactly<ArgumentOutOfRangeException>();
    }
}

[TestClass]
public class RollingMinHubTests : ResultMathHubTestBase<RollingMinHub>
{
    private const int lookbackPeriods = 20;

    protected override string ExpectedName => "ROLLINGMIN(20)";

    protected override RollingMinHub NewHub(IChainProvider<IReusable> provider)
        => provider.ToRollingMinHub(lookbackPeriods);

    protected override IReadOnlyList<TimeValue> Series(IReadOnlyList<IReusable> source)
        => source.RollingMin(lookbackPeriods);

    [TestMethod]
    public void Properties_AfterConstruction_ReflectArguments()
        => new BarHub().ToRollingMinHub(14).LookbackPeriods.Should().Be(14);
}

[TestClass]
public class RollingMedianHubTests : ResultMathHubTestBase<RollingMedianHub>
{
    private const int lookbackPeriods = 20;

    protected override string ExpectedName => "ROLLINGMEDIAN(20)";

    protected override RollingMedianHub NewHub(IChainProvider<IReusable> provider)
        => provider.ToRollingMedianHub(lookbackPeriods);

    protected override IReadOnlyList<TimeValue> Series(IReadOnlyList<IReusable> source)
        => source.RollingMedian(lookbackPeriods);

    [TestMethod]
    public void OddLookback_StreamedBars_MatchesSeriesExactly()
    {
        BarHub barHub = new();
        RollingMedianHub hub = barHub.ToRollingMedianHub(7);

        barHub.Add(Bars);

        hub.Results.IsExactly(Bars.RollingMedian(7));
        barHub.EndTransmission();
    }
}
