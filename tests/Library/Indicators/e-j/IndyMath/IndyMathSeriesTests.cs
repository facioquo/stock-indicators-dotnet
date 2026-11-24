namespace StaticSeries;

[TestClass]
public class IndyMathSeriesTests : StaticSeriesTestBase
{
    private static readonly DateTime Start = new(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    private static List<TimeValue> Series(params double[] values)
        => [.. values.Select(static (v, i) => new TimeValue(Start.AddDays(i), v))];

    private static double?[] Values(IReadOnlyList<MathResult> results)
        => [.. results.Select(static x => x.Result)];

    [TestMethod]
    public override void DefaultParameters_ReturnsExpectedResults()
    {
        // catalog default parameters
        IReadOnlyList<MathResult> plus = Bars.Plus(1);
        IReadOnlyList<MathResult> percent = Bars.PlusPercent(2.5);
        IReadOnlyList<MathResult> spread = Bars.ToSma(10).Minus(Bars.ToSma(20));
        IReadOnlyList<MathResult> highest = Bars.RollingMax(20);

        plus.Should().HaveCount(502);
        percent.Should().HaveCount(502);
        spread.Should().HaveCount(502);
        highest.Should().HaveCount(502);

        plus[0].Result.Should().Be(Bars[0].Value + 1);
        plus[501].Result.Should().Be(Bars[501].Value + 1);
        percent[501].Result.Should().Be(Bars[501].Value + (Bars[501].Value * 2.5 / 100d));

        spread.Take(19).Should().OnlyContain(static x => x.Result == null);
        spread[19].Result.Should().NotBeNull();

        highest.Take(19).Should().OnlyContain(static x => x.Result == null);
        highest[19].Result.Should().Be(Bars.Take(20).Max(static x => x.Value));
        highest[501].Result.Should().Be(Bars.Skip(482).Max(static x => x.Value));
    }

    [TestMethod]
    public override void BadBars_DoesNotFail()
    {
        BadBars.Plus(BadBars).Should().HaveCount(502);
        BadBars.DividedBy(BadBars).Should().HaveCount(502);
        BadBars.PlusPercent(-10).Should().HaveCount(502);
        BadBars.RollingMax(15).Should().HaveCount(502);
        BadBars.RollingMin(15).Should().HaveCount(502);

        IReadOnlyList<MathResult> median = BadBars.RollingMedian(15);
        median.Should().HaveCount(502);
        median.Should().NotContain(static x => x.Result.HasValue && double.IsNaN(x.Result.Value));
    }

    [TestMethod]
    public override void NoBars_ReturnsEmpty()
    {
        Nobars.Plus(Nobars).Should().BeEmpty();
        Nobars.Times(3).Should().BeEmpty();
        Nobars.RollingMax(5).Should().BeEmpty();
        Nobars.RollingMedian(5).Should().BeEmpty();

        IReadOnlyList<MathResult> one = Onebar.RollingMin(5);
        one.Should().ContainSingle();
        one[0].Result.Should().BeNull();
    }

    [TestMethod]
    public void PairwiseOperations_MatchedSeries_ReturnExpectedValues()
    {
        List<TimeValue> a = Series(6, 8, -3, double.NaN);
        List<TimeValue> b = Series(2, 0, 4, 1);

        Values(a.Plus(b)).Should().Equal(8, 8, 1, null);
        Values(a.Minus(b)).Should().Equal(4, 8, -7, null);
        Values(a.Times(b)).Should().Equal(12, 0, -12, null);
        Values(a.DividedBy(b)).Should().Equal(3, null, -0.75, null);
        Values(a.MaxWith(b)).Should().Equal(6, 8, 4, null);
        Values(a.MinWith(b)).Should().Equal(2, 0, -3, null);

        // NaN in the other series also propagates to null
        Values(b.MaxWith(a)).Should().Equal(6, 8, 4, null);
        Values(b.MinWith(a)).Should().Equal(2, 0, -3, null);
        Values(b.Minus(a)).Should().Equal(-4, -8, 7, null);

        a.Plus(b).Select(static x => x.Timestamp)
            .Should().Equal(a.Select(static x => x.Timestamp));
    }

    [TestMethod]
    public void PairwiseOperations_ChainedIndicatorComposition_MatchesHandLoop()
    {
        // MACD − EMA(MACD) + SMA, the composition requested in Discussion #977
        IReadOnlyList<MacdResult> macd = Bars.ToMacd(12, 26, 9);
        IReadOnlyList<EmaResult> macdEma = macd.ToEma(9);
        IReadOnlyList<SmaResult> sma = Bars.ToSma(9);

        IReadOnlyList<MathResult> composed = macd.Minus(macdEma).Plus(sma);

        double?[] expected = [.. Enumerable.Range(0, Bars.Count)
            .Select(i => (macd[i].Value - macdEma[i].Value + sma[i].Value).NaN2Null())];

        Values(composed).Should().Equal(expected);
        composed.Select(static x => x.Timestamp)
            .Should().Equal(Bars.Select(static x => x.Timestamp));

        // MACD warms up for 25 periods, then EMA(9) needs 8 more
        composed.TakeWhile(static x => x.Result is null).Should().HaveCount(33);
        composed.Skip(33).Should().OnlyContain(static x => x.Result != null);
    }

    [TestMethod]
    public void ScalarOperations_FixedAmount_ReturnExpectedValues()
    {
        List<TimeValue> a = Series(10, -4, double.NaN);

        Values(a.Plus(2.5)).Should().Equal(12.5, -1.5, null);
        Values(a.Minus(2.5)).Should().Equal(7.5, -6.5, null);
        Values(a.Times(-2)).Should().Equal(-20, 8, null);
        Values(a.DividedBy(4)).Should().Equal(2.5, -1, null);

        a.Times(2).Select(static x => x.Timestamp)
            .Should().Equal(a.Select(static x => x.Timestamp));
    }

    [TestMethod]
    public void PairwiseOperations_BarsWithThemselves_ReturnIdentityValues()
    {
        Bars.Minus(Bars).Should().OnlyContain(static x => x.Result == 0);
        Values(Bars.MaxWith(Bars)).Should().Equal(Bars.Select(static x => (double?)x.Value));
        Bars.DividedBy(Bars).Should().OnlyContain(static x => x.Result == 1);
    }

    [TestMethod]
    public void PlusPercent_PositiveAndNegativeOffsets_ScaleValues()
    {
        List<TimeValue> a = Series(200, -50);

        IReadOnlyList<MathResult> up = a.PlusPercent(2.5);
        up[0].Result.Should().BeApproximately(205, Money6);
        up[1].Result.Should().BeApproximately(-51.25, Money6);

        // the offset form is exact here; a precomputed 1.025 factor lands one step short
        Series(100).PlusPercent(2.5)[0].Result.Should().Be(102.5);

        IReadOnlyList<MathResult> down = a.PlusPercent(-10);
        down[0].Result.Should().BeApproximately(180, Money6);
        down[1].Result.Should().BeApproximately(-45, Money6);
    }

    [TestMethod]
    public void RollingMaxMin_HandCalculated_ReturnExpectedValues()
    {
        List<TimeValue> a = Series(3, 1, 4, 1, 5, 9, 2, 6);

        Values(a.RollingMax(3)).Should().Equal(null, null, 4, 4, 5, 9, 9, 9);
        Values(a.RollingMin(3)).Should().Equal(null, null, 1, 1, 1, 1, 2, 2);
        Values(a.RollingMax(1)).Should().Equal(a.Select(static x => (double?)x.Value));
    }

    [TestMethod]
    public void RollingMedian_OddAndEvenWindows_ReturnExpectedValues()
    {
        List<TimeValue> a = Series(3, 1, 4, 1, 5, 9, 2, 6);

        Values(a.RollingMedian(3)).Should().Equal(null, null, 3, 1, 4, 5, 5, 6);
        Values(a.RollingMedian(4)).Should().Equal(null, null, null, 2, 2.5, 4.5, 3.5, 5.5);
    }

    [TestMethod]
    public void RollingMedian_ExtremeFiniteValues_DoesNotOverflow()
    {
        List<TimeValue> a = Series(double.MaxValue, double.MaxValue, -double.MaxValue, double.MaxValue);

        Values(a.RollingMedian(2)).Should().Equal(null, double.MaxValue, 0, 0);
    }

    [TestMethod]
    public void RollingMedian_InfiniteValues_PropagateWithoutOverflowFallback()
    {
        List<TimeValue> a = Series(double.PositiveInfinity, double.PositiveInfinity, double.NegativeInfinity);

        Values(a.RollingMedian(2)).Should().Equal(null, double.PositiveInfinity, null);
    }

    [TestMethod]
    public void RollingMedian_SubnormalValues_ReturnsExactMidpoint()
    {
        List<TimeValue> a = Series(double.Epsilon, double.Epsilon, double.Epsilon * 3);

        Values(a.RollingMedian(2)).Should().Equal(null, double.Epsilon, double.Epsilon * 2);
    }

    [TestMethod]
    public void RollingWindows_LookbackAtOrBeyondCount_ReturnWarmupOnly()
    {
        List<TimeValue> a = Series(3, 1, 4, 1, 5);

        // lookback equals count: only the final period has a full window
        Values(a.RollingMax(5)).Should().Equal(null, null, null, null, 5);
        Values(a.RollingMin(5)).Should().Equal(null, null, null, null, 1);
        Values(a.RollingMedian(5)).Should().Equal(null, null, null, null, 3);

        // lookback beyond count, including one that must not size buffers from the lookback
        foreach (int lookback in (int[])[6, int.MaxValue])
        {
            a.RollingMax(lookback).Should().OnlyContain(static x => x.Result == null);
            a.RollingMin(lookback).Should().OnlyContain(static x => x.Result == null);
            a.RollingMedian(lookback).Should().OnlyContain(static x => x.Result == null);
        }
    }

    [TestMethod]
    public void RollingWindows_NaNInWindow_ReturnNullUntilItExits()
    {
        List<TimeValue> a = Series(1, 2, double.NaN, 4, 5, 6);

        double?[] expected = [null, 2, null, null, 5, 6];
        Values(a.RollingMax(2)).Should().Equal(expected);
        Values(a.RollingMin(2)).Should().Equal(null, 1, null, null, 4, 5);
        Values(a.RollingMedian(2)).Should().Equal(null, 1.5, null, null, 4.5, 5.5);
    }

    [TestMethod]
    public void RollingWindows_DefaultBars_MatchBruteForce()
    {
        // SMA warmup NaNs, plus single, consecutive, and trailing NaNs mid-series
        int[] gaps = [30, 61, 62, 63, 200, 501];
        List<IReusable> source = [.. Bars.ToSma(5)
            .Select((x, i) => (IReusable)new TimeValue(x.Timestamp, gaps.Contains(i) ? double.NaN : x.Value))];

        foreach (int lookback in (int[])[1, 2, 3, 4, 5, 7, 20])
        {
            IReadOnlyList<MathResult> max = source.RollingMax(lookback);
            IReadOnlyList<MathResult> min = source.RollingMin(lookback);
            IReadOnlyList<MathResult> median = source.RollingMedian(lookback);

            max.Should().HaveCount(barsCount);

            for (int i = 0; i < barsCount; i++)
            {
                double[] window = i < lookback - 1
                    ? []
                    : [.. source.Skip(i - lookback + 1).Take(lookback).Select(static x => x.Value)];

                if (window.Length == 0 || window.Any(double.IsNaN))
                {
                    max[i].Result.Should().BeNull();
                    min[i].Result.Should().BeNull();
                    median[i].Result.Should().BeNull();
                    continue;
                }

                double[] sorted = [.. window.Order()];
                double expectedMedian = lookback % 2 == 1
                    ? sorted[lookback / 2]
                    : (sorted[(lookback / 2) - 1] + sorted[lookback / 2]) / 2d;

                max[i].Result.Should().Be(window.Max());
                min[i].Result.Should().Be(window.Min());
                median[i].Result.Should().Be(expectedMedian);
            }
        }
    }

    [TestMethod]
    public void Chaining_IndicatorResults_ProducesChainableSeries()
    {
        // fast SMA minus slow SMA, then smoothed
        IReadOnlyList<SmaResult> fast = Bars.ToSma(10);
        IReadOnlyList<SmaResult> slow = Bars.ToSma(20);

        IReadOnlyList<MathResult> spread = fast.Minus(slow);
        spread.Should().HaveCount(barsCount);
        spread[18].Result.Should().BeNull();
        spread[19].Result.Should().Be(fast[19].Sma!.Value - slow[19].Sma!.Value);

        IReadOnlyList<SmaResult> smoothed = spread.ToSma(5);
        smoothed[22].Sma.Should().BeNull();
        smoothed[23].Sma.Should().NotBeNull();
    }

    [TestMethod]
    public void PairwiseOperations_MismatchedLength_ThrowsInvalidBarsException()
        => FluentActions
            .Invoking(static () => Bars.Plus(Bars.Take(10).ToList()))
            .Should().ThrowExactly<InvalidBarsException>()
            .WithMessage("*lengths do not match*");

    [TestMethod]
    public void PairwiseOperations_MismatchedTimestamps_ThrowsInvalidBarsException()
    {
        List<TimeValue> a = Series(1, 2, 3);
        List<TimeValue> b = [a[0], new(a[1].Timestamp.AddHours(1), 2), a[2]];

        FluentActions
            .Invoking(() => a.DividedBy(b))
            .Should().ThrowExactly<InvalidBarsException>()
            .WithParameterName("otherSource")
            .WithMessage("*index 1*otherSource has*");
    }

    [TestMethod]
    public void Exceptions_NullSeries_ThrowsArgumentNullException()
    {
        List<TimeValue> a = Series(1);

        FluentActions.Invoking(static () => ((IReadOnlyList<IReusable>)null!).Plus(1))
            .Should().ThrowExactly<ArgumentNullException>().WithParameterName("source");
        FluentActions.Invoking(static () => ((IReadOnlyList<IReusable>)null!).DividedBy(double.NaN))
            .Should().ThrowExactly<ArgumentNullException>().WithParameterName("source");
        FluentActions.Invoking(() => ((IReadOnlyList<IReusable>)null!).Minus(a))
            .Should().ThrowExactly<ArgumentNullException>().WithParameterName("basisSource");
        FluentActions.Invoking(() => a.MaxWith(null!))
            .Should().ThrowExactly<ArgumentNullException>().WithParameterName("otherSource");
        FluentActions.Invoking(static () => ((IReadOnlyList<IReusable>)null!).RollingMin(2))
            .Should().ThrowExactly<ArgumentNullException>();
        FluentActions.Invoking(static () => ((IReadOnlyList<IReusable>)null!).RollingMedian(2))
            .Should().ThrowExactly<ArgumentNullException>();
    }

    [TestMethod]
    public void Exceptions_InvalidScalar_ThrowsArgumentOutOfRangeException()
    {
        List<TimeValue> a = Series(1);

        FluentActions.Invoking(() => a.DividedBy(0))
            .Should().ThrowExactly<ArgumentOutOfRangeException>().WithParameterName("divisor");
        FluentActions.Invoking(() => a.DividedBy(-0.0))
            .Should().ThrowExactly<ArgumentOutOfRangeException>().WithParameterName("divisor");
        FluentActions.Invoking(() => a.DividedBy(double.NaN))
            .Should().ThrowExactly<ArgumentOutOfRangeException>().WithParameterName("divisor");
        FluentActions.Invoking(() => a.DividedBy(double.PositiveInfinity))
            .Should().ThrowExactly<ArgumentOutOfRangeException>().WithParameterName("divisor");
        FluentActions.Invoking(() => a.Plus(double.NaN))
            .Should().ThrowExactly<ArgumentOutOfRangeException>().WithParameterName("amount");
        FluentActions.Invoking(() => a.Minus(double.PositiveInfinity))
            .Should().ThrowExactly<ArgumentOutOfRangeException>().WithParameterName("amount");
        FluentActions.Invoking(() => a.Times(double.NegativeInfinity))
            .Should().ThrowExactly<ArgumentOutOfRangeException>().WithParameterName("factor");
        FluentActions.Invoking(() => a.PlusPercent(double.NaN))
            .Should().ThrowExactly<ArgumentOutOfRangeException>().WithParameterName("percentOffset");
    }

    [TestMethod]
    public void Exceptions_InvalidLookback_ThrowsArgumentOutOfRangeException()
    {
        List<TimeValue> a = Series(1);

        FluentActions.Invoking(() => a.RollingMax(0))
            .Should().ThrowExactly<ArgumentOutOfRangeException>().WithParameterName("lookbackPeriods");
        FluentActions.Invoking(() => a.RollingMin(-1))
            .Should().ThrowExactly<ArgumentOutOfRangeException>().WithParameterName("lookbackPeriods");
        FluentActions.Invoking(() => a.RollingMedian(0))
            .Should().ThrowExactly<ArgumentOutOfRangeException>().WithParameterName("lookbackPeriods");
    }
}
