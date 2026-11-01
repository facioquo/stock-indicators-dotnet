namespace Utilities;

[TestClass]
public class ResultMaths : TestBaseWithPrecision
{
    private static readonly DateTime Start = new(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    private static List<TimeValue> Series(params double[] values)
        => [.. values.Select(static (v, i) => new TimeValue(Start.AddDays(i), v))];

    private static double[] Values(IReadOnlyList<TimeValue> results)
        => [.. results.Select(static x => x.Value)];

    [TestMethod]
    public void PairwiseOperations_MatchedSeries_ReturnExpectedValues()
    {
        List<TimeValue> a = Series(6, 8, -3, double.NaN);
        List<TimeValue> b = Series(2, 0, 4, 1);

        Values(a.Plus(b)).Should().Equal(8, 8, 1, double.NaN);
        Values(a.Minus(b)).Should().Equal(4, 8, -7, double.NaN);
        Values(a.Times(b)).Should().Equal(12, 0, -12, double.NaN);
        Values(a.DividedBy(b)).Should().Equal(3, double.NaN, -0.75, double.NaN);
        Values(a.MaxWith(b)).Should().Equal(6, 8, 4, double.NaN);
        Values(a.MinWith(b)).Should().Equal(2, 0, -3, double.NaN);

        a.Plus(b).Select(static x => x.Timestamp)
            .Should().Equal(a.Select(static x => x.Timestamp));
    }

    [TestMethod]
    public void ScalarOperations_FixedAmount_ReturnExpectedValues()
    {
        List<TimeValue> a = Series(10, -4, double.NaN);

        Values(a.Plus(2.5)).Should().Equal(12.5, -1.5, double.NaN);
        Values(a.Minus(2.5)).Should().Equal(7.5, -6.5, double.NaN);
        Values(a.Times(-2)).Should().Equal(-20, 8, double.NaN);
        Values(a.DividedBy(4)).Should().Equal(2.5, -1, double.NaN);

        a.Times(2).Select(static x => x.Timestamp)
            .Should().Equal(a.Select(static x => x.Timestamp));
    }

    [TestMethod]
    public void PairwiseOperations_BarsWithThemselves_ReturnIdentityValues()
    {
        Bars.Minus(Bars).Should().OnlyContain(static x => x.Value == 0);
        Values(Bars.MaxWith(Bars)).Should().Equal(Bars.Select(static x => x.Value));
        Bars.DividedBy(Bars).Should().OnlyContain(static x => x.Value == 1);
    }

    [TestMethod]
    public void PlusPercent_PositiveAndNegativeOffsets_ScaleValues()
    {
        List<TimeValue> a = Series(200, -50);

        IReadOnlyList<TimeValue> up = a.PlusPercent(2.5);
        up[0].Value.Should().BeApproximately(205, Money6);
        up[1].Value.Should().BeApproximately(-51.25, Money6);

        IReadOnlyList<TimeValue> down = a.PlusPercent(-10);
        down[0].Value.Should().BeApproximately(180, Money6);
        down[1].Value.Should().BeApproximately(-45, Money6);
    }

    [TestMethod]
    public void RollingMaxMin_HandCalculated_ReturnExpectedValues()
    {
        List<TimeValue> a = Series(3, 1, 4, 1, 5, 9, 2, 6);

        Values(a.RollingMax(3)).Should().Equal(double.NaN, double.NaN, 4, 4, 5, 9, 9, 9);
        Values(a.RollingMin(3)).Should().Equal(double.NaN, double.NaN, 1, 1, 1, 1, 2, 2);
        Values(a.RollingMax(1)).Should().Equal(Values(a));
    }

    [TestMethod]
    public void RollingMedian_OddAndEvenWindows_ReturnExpectedValues()
    {
        List<TimeValue> a = Series(3, 1, 4, 1, 5, 9, 2, 6);

        Values(a.RollingMedian(3)).Should().Equal(double.NaN, double.NaN, 3, 1, 4, 5, 5, 6);
        Values(a.RollingMedian(4)).Should().Equal(double.NaN, double.NaN, double.NaN, 2, 2.5, 4.5, 3.5, 5.5);
    }

    [TestMethod]
    public void RollingMedian_ExtremeFiniteValues_DoesNotOverflow()
    {
        List<TimeValue> a = Series(double.MaxValue, double.MaxValue, -double.MaxValue, double.MaxValue);

        Values(a.RollingMedian(2)).Should().Equal(double.NaN, double.MaxValue, 0, 0);
    }

    [TestMethod]
    public void RollingWindows_LookbackAtOrBeyondCount_ReturnWarmupOnly()
    {
        List<TimeValue> a = Series(3, 1, 4, 1, 5);

        // lookback equals count: only the final period has a full window
        Values(a.RollingMax(5)).Should().Equal(double.NaN, double.NaN, double.NaN, double.NaN, 5);
        Values(a.RollingMin(5)).Should().Equal(double.NaN, double.NaN, double.NaN, double.NaN, 1);
        Values(a.RollingMedian(5)).Should().Equal(double.NaN, double.NaN, double.NaN, double.NaN, 3);

        // lookback beyond count, including one that must not size buffers from the lookback
        foreach (int lookback in (int[])[6, int.MaxValue])
        {
            a.RollingMax(lookback).Should().OnlyContain(static x => double.IsNaN(x.Value));
            a.RollingMin(lookback).Should().OnlyContain(static x => double.IsNaN(x.Value));
            a.RollingMedian(lookback).Should().OnlyContain(static x => double.IsNaN(x.Value));
        }
    }

    [TestMethod]
    public void RollingWindows_NaNInWindow_ReturnNaNUntilItExits()
    {
        List<TimeValue> a = Series(1, 2, double.NaN, 4, 5, 6);

        double[] expected = [double.NaN, 2, double.NaN, double.NaN, 5, 6];
        Values(a.RollingMax(2)).Should().Equal(expected);
        Values(a.RollingMin(2)).Should().Equal(double.NaN, 1, double.NaN, double.NaN, 4, 5);
        Values(a.RollingMedian(2)).Should().Equal(double.NaN, 1.5, double.NaN, double.NaN, 4.5, 5.5);
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
            IReadOnlyList<TimeValue> max = source.RollingMax(lookback);
            IReadOnlyList<TimeValue> min = source.RollingMin(lookback);
            IReadOnlyList<TimeValue> median = source.RollingMedian(lookback);

            max.Should().HaveCount(barsCount);

            for (int i = 0; i < barsCount; i++)
            {
                double[] window = i < lookback - 1
                    ? []
                    : [.. source.Skip(i - lookback + 1).Take(lookback).Select(static x => x.Value)];

                if (window.Length == 0 || window.Any(double.IsNaN))
                {
                    max[i].Value.Should().Be(double.NaN);
                    min[i].Value.Should().Be(double.NaN);
                    median[i].Value.Should().Be(double.NaN);
                    continue;
                }

                double[] sorted = [.. window.Order()];
                double expectedMedian = lookback % 2 == 1
                    ? sorted[lookback / 2]
                    : (sorted[(lookback / 2) - 1] + sorted[lookback / 2]) / 2d;

                max[i].Value.Should().Be(window.Max());
                min[i].Value.Should().Be(window.Min());
                median[i].Value.Should().Be(expectedMedian);
            }
        }
    }

    [TestMethod]
    public void Chaining_IndicatorResults_ProducesChainableSeries()
    {
        // fast SMA minus slow SMA, then smoothed
        IReadOnlyList<SmaResult> fast = Bars.ToSma(10);
        IReadOnlyList<SmaResult> slow = Bars.ToSma(20);

        IReadOnlyList<TimeValue> spread = fast.Minus(slow);
        spread.Should().HaveCount(barsCount);
        spread[18].Value.Should().Be(double.NaN);
        spread[19].Value.Should().Be(fast[19].Sma!.Value - slow[19].Sma!.Value);

        IReadOnlyList<SmaResult> smoothed = spread.ToSma(5);
        smoothed[22].Sma.Should().BeNull();
        smoothed[23].Sma.Should().NotBeNull();
    }

    [TestMethod]
    public void AllOperations_EmptySeries_ReturnEmpty()
    {
        List<TimeValue> empty = [];

        empty.Plus(empty).Should().BeEmpty();
        empty.Times(3).Should().BeEmpty();
        empty.RollingMax(5).Should().BeEmpty();
        empty.RollingMedian(5).Should().BeEmpty();
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
            .WithParameterName("other")
            .WithMessage("*index 1*other has*");
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
            .Should().ThrowExactly<ArgumentNullException>().WithParameterName("source");
        FluentActions.Invoking(() => a.MaxWith(null!))
            .Should().ThrowExactly<ArgumentNullException>().WithParameterName("other");
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
