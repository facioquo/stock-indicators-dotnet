namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Arithmetic and rolling-window math on chainable indicator results.
/// </summary>
/// <remarks>
/// <para>
/// Each method returns a new list of <see cref="TimeValue"/> results, one per source result,
/// so the output chains into any indicator that accepts <see cref="IReusable"/> values.
/// </para>
/// <para>
/// Methods that combine two series require both to hold the same number of results with
/// identical timestamps at every position; a mismatch throws instead of pairing values
/// from different periods. <see cref="double.NaN"/> source values propagate to the output.
/// </para>
/// </remarks>
public static partial class ResultMath
{
    /// <summary>
    /// Adds the values of two series, period by period.
    /// </summary>
    /// <param name="source">Chainable results, time sorted.</param>
    /// <param name="other">Chainable results with timestamps matching <paramref name="source"/>.</param>
    /// <returns>Time series of <c>source + other</c> values.</returns>
    /// <exception cref="ArgumentNullException">Thrown when either series is null.</exception>
    /// <exception cref="InvalidBarsException">Thrown when the series lengths or timestamps do not match.</exception>
    public static IReadOnlyList<TimeValue> Plus(
        this IReadOnlyList<IReusable> source,
        IReadOnlyList<IReusable> other)
        => source.Combine(other, static (a, b) => a + b);

    /// <summary>
    /// Adds a fixed amount to every value in a series.
    /// </summary>
    /// <param name="source">Chainable results, time sorted.</param>
    /// <param name="amount">Finite amount to add.</param>
    /// <returns>Time series of <c>source + amount</c> values.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the series is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the amount is not finite.</exception>
    public static IReadOnlyList<TimeValue> Plus(
        this IReadOnlyList<IReusable> source,
        double amount)
    {
        ArgumentNullException.ThrowIfNull(source);
        return source.Transform(new ScalarKernel(ScalarOperation.Plus, amount));
    }

    /// <summary>
    /// Offsets every value in a series by a percentage of itself.
    /// </summary>
    /// <param name="source">Chainable results, time sorted.</param>
    /// <param name="percentOffset">
    /// Finite percent offset, where 2.5 adds 2.5% and -2.5 subtracts 2.5%.
    /// </param>
    /// <returns>Time series of <c>source × (1 + percentOffset / 100)</c> values.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the series is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the percent offset is not finite.</exception>
    public static IReadOnlyList<TimeValue> PlusPercent(
        this IReadOnlyList<IReusable> source,
        double percentOffset)
    {
        ArgumentNullException.ThrowIfNull(source);
        return source.Transform(new ScalarKernel(ScalarOperation.PlusPercent, percentOffset));
    }

    /// <summary>
    /// Subtracts the values of a second series from a first series, period by period.
    /// </summary>
    /// <param name="source">Chainable results, time sorted.</param>
    /// <param name="other">Chainable results with timestamps matching <paramref name="source"/>.</param>
    /// <returns>Time series of <c>source − other</c> values.</returns>
    /// <exception cref="ArgumentNullException">Thrown when either series is null.</exception>
    /// <exception cref="InvalidBarsException">Thrown when the series lengths or timestamps do not match.</exception>
    public static IReadOnlyList<TimeValue> Minus(
        this IReadOnlyList<IReusable> source,
        IReadOnlyList<IReusable> other)
        => source.Combine(other, static (a, b) => a - b);

    /// <summary>
    /// Subtracts a fixed amount from every value in a series.
    /// </summary>
    /// <param name="source">Chainable results, time sorted.</param>
    /// <param name="amount">Finite amount to subtract.</param>
    /// <returns>Time series of <c>source − amount</c> values.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the series is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the amount is not finite.</exception>
    public static IReadOnlyList<TimeValue> Minus(
        this IReadOnlyList<IReusable> source,
        double amount)
    {
        ArgumentNullException.ThrowIfNull(source);
        return source.Transform(new ScalarKernel(ScalarOperation.Minus, amount));
    }

    /// <summary>
    /// Multiplies the values of two series, period by period.
    /// </summary>
    /// <param name="source">Chainable results, time sorted.</param>
    /// <param name="other">Chainable results with timestamps matching <paramref name="source"/>.</param>
    /// <returns>Time series of <c>source × other</c> values.</returns>
    /// <exception cref="ArgumentNullException">Thrown when either series is null.</exception>
    /// <exception cref="InvalidBarsException">Thrown when the series lengths or timestamps do not match.</exception>
    public static IReadOnlyList<TimeValue> Times(
        this IReadOnlyList<IReusable> source,
        IReadOnlyList<IReusable> other)
        => source.Combine(other, static (a, b) => a * b);

    /// <summary>
    /// Multiplies every value in a series by a fixed factor.
    /// </summary>
    /// <param name="source">Chainable results, time sorted.</param>
    /// <param name="factor">Finite multiplier.</param>
    /// <returns>Time series of <c>source × factor</c> values.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the series is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the factor is not finite.</exception>
    public static IReadOnlyList<TimeValue> Times(
        this IReadOnlyList<IReusable> source,
        double factor)
    {
        ArgumentNullException.ThrowIfNull(source);
        return source.Transform(new ScalarKernel(ScalarOperation.Times, factor));
    }

    /// <summary>
    /// Divides the values of a first series by a second series, period by period.
    /// </summary>
    /// <param name="source">Chainable results (dividends), time sorted.</param>
    /// <param name="other">Chainable results (divisors) with timestamps matching <paramref name="source"/>.</param>
    /// <returns>
    /// Time series of <c>source ÷ other</c> values; <see cref="double.NaN"/> where the divisor is 0.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when either series is null.</exception>
    /// <exception cref="InvalidBarsException">Thrown when the series lengths or timestamps do not match.</exception>
    public static IReadOnlyList<TimeValue> DividedBy(
        this IReadOnlyList<IReusable> source,
        IReadOnlyList<IReusable> other)
        => source.Combine(other, static (a, b) => b != 0 ? a / b : double.NaN);

    /// <summary>
    /// Divides every value in a series by a fixed divisor.
    /// </summary>
    /// <param name="source">Chainable results, time sorted.</param>
    /// <param name="divisor">Finite, non-zero divisor.</param>
    /// <returns>Time series of <c>source ÷ divisor</c> values.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the series is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the divisor is 0 or not finite.</exception>
    public static IReadOnlyList<TimeValue> DividedBy(
        this IReadOnlyList<IReusable> source,
        double divisor)
    {
        ArgumentNullException.ThrowIfNull(source);
        return source.Transform(new ScalarKernel(ScalarOperation.DividedBy, divisor));
    }

    /// <summary>
    /// Selects the larger value of two series, period by period.
    /// </summary>
    /// <param name="source">Chainable results, time sorted.</param>
    /// <param name="other">Chainable results with timestamps matching <paramref name="source"/>.</param>
    /// <returns>Time series of the larger value at each period.</returns>
    /// <exception cref="ArgumentNullException">Thrown when either series is null.</exception>
    /// <exception cref="InvalidBarsException">Thrown when the series lengths or timestamps do not match.</exception>
    public static IReadOnlyList<TimeValue> MaxWith(
        this IReadOnlyList<IReusable> source,
        IReadOnlyList<IReusable> other)
        => source.Combine(other, Math.Max);

    /// <summary>
    /// Selects the smaller value of two series, period by period.
    /// </summary>
    /// <param name="source">Chainable results, time sorted.</param>
    /// <param name="other">Chainable results with timestamps matching <paramref name="source"/>.</param>
    /// <returns>Time series of the smaller value at each period.</returns>
    /// <exception cref="ArgumentNullException">Thrown when either series is null.</exception>
    /// <exception cref="InvalidBarsException">Thrown when the series lengths or timestamps do not match.</exception>
    public static IReadOnlyList<TimeValue> MinWith(
        this IReadOnlyList<IReusable> source,
        IReadOnlyList<IReusable> other)
        => source.Combine(other, Math.Min);

    /// <summary>
    /// Calculates the highest value over a rolling lookback window.
    /// </summary>
    /// <param name="source">Chainable results, time sorted.</param>
    /// <param name="lookbackPeriods">Number of periods in the window, including the current one.</param>
    /// <returns>
    /// Time series of rolling highest values; <see cref="double.NaN"/> for the first
    /// <c>lookbackPeriods − 1</c> periods and wherever the window holds a NaN value.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when the series is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the lookback periods are less than 1.</exception>
    public static IReadOnlyList<TimeValue> RollingMax(
        this IReadOnlyList<IReusable> source,
        int lookbackPeriods)
    {
        ArgumentNullException.ThrowIfNull(source);
        return source.Rolling(new RollingExtremeWindow(lookbackPeriods, isMax: true).Add);
    }

    /// <summary>
    /// Calculates the lowest value over a rolling lookback window.
    /// </summary>
    /// <param name="source">Chainable results, time sorted.</param>
    /// <param name="lookbackPeriods">Number of periods in the window, including the current one.</param>
    /// <returns>
    /// Time series of rolling lowest values; <see cref="double.NaN"/> for the first
    /// <c>lookbackPeriods − 1</c> periods and wherever the window holds a NaN value.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when the series is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the lookback periods are less than 1.</exception>
    public static IReadOnlyList<TimeValue> RollingMin(
        this IReadOnlyList<IReusable> source,
        int lookbackPeriods)
    {
        ArgumentNullException.ThrowIfNull(source);
        return source.Rolling(new RollingExtremeWindow(lookbackPeriods, isMax: false).Add);
    }

    /// <summary>
    /// Calculates the median value over a rolling lookback window.
    /// </summary>
    /// <param name="source">Chainable results, time sorted.</param>
    /// <param name="lookbackPeriods">Number of periods in the window, including the current one.</param>
    /// <returns>
    /// Time series of rolling median values, averaging the two middle values when the window
    /// is even; <see cref="double.NaN"/> for the first <c>lookbackPeriods − 1</c> periods and
    /// wherever the window holds a NaN value.
    /// </returns>
    /// <remarks>
    /// The window is kept sorted, so each period costs O(log k) comparisons plus an O(k) shift,
    /// and the series costs O(n·k) for a lookback of k periods.
    /// </remarks>
    /// <exception cref="ArgumentNullException">Thrown when the series is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the lookback periods are less than 1.</exception>
    public static IReadOnlyList<TimeValue> RollingMedian(
        this IReadOnlyList<IReusable> source,
        int lookbackPeriods)
    {
        ArgumentNullException.ThrowIfNull(source);
        return source.Rolling(new RollingMedianWindow(lookbackPeriods).Add);
    }

    /// <summary>
    /// Applies a single-value operation to every value in a series.
    /// </summary>
    /// <param name="source">Chainable results, time sorted.</param>
    /// <param name="kernel">Validated operation applied to each value.</param>
    /// <returns>Time series of operation results.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the series is null.</exception>
    private static List<TimeValue> Transform(
        this IReadOnlyList<IReusable> source,
        ScalarKernel kernel)
    {
        ArgumentNullException.ThrowIfNull(source);

        int length = source.Count;
        List<TimeValue> results = new(length);

        for (int i = 0; i < length; i++)
        {
            IReusable s = source[i];
            results.Add(new TimeValue(s.Timestamp, kernel.Apply(s.Value)));
        }

        return results;
    }

    /// <summary>
    /// Applies a two-value operation to two timestamp-matched series.
    /// </summary>
    /// <param name="source">Chainable results, time sorted.</param>
    /// <param name="other">Chainable results with timestamps matching <paramref name="source"/>.</param>
    /// <param name="operation">Operation applied to each pair of values.</param>
    /// <returns>Time series of operation results.</returns>
    /// <exception cref="ArgumentNullException">Thrown when either series is null.</exception>
    /// <exception cref="InvalidBarsException">Thrown when the series lengths or timestamps do not match.</exception>
    private static List<TimeValue> Combine(
        this IReadOnlyList<IReusable> source,
        IReadOnlyList<IReusable> other,
        Func<double, double, double> operation)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(other);

        int length = source.Count;

        if (other.Count != length)
        {
            throw new InvalidBarsException(
                nameof(other), other.Count,
                $"Series lengths do not match: source has {length} results and other has {other.Count}.  "
              + "Calculate both series from the same bars, and trim both to the same range before combining.");
        }

        List<TimeValue> results = new(length);

        for (int i = 0; i < length; i++)
        {
            IReusable s = source[i];
            IReusable o = other[i];

            if (s.Timestamp != o.Timestamp)
            {
                throw new InvalidBarsException(
                    nameof(other), o.Timestamp,
                    $"Timestamp sequence does not match at index {i}: source has {s.Timestamp:o} and other has {o.Timestamp:o}.  "
                  + "Calculate both series from the same bars, and trim both to the same range before combining.");
            }

            results.Add(new TimeValue(s.Timestamp, operation(s.Value, o.Value)));
        }

        return results;
    }

    /// <summary>
    /// Advances a rolling window over every value in a series.
    /// </summary>
    /// <param name="source">Chainable results, time sorted.</param>
    /// <param name="advance">Window step that takes the next value and returns the window result.</param>
    /// <returns>Time series of window results.</returns>
    private static List<TimeValue> Rolling(
        this IReadOnlyList<IReusable> source,
        Func<double, double> advance)
    {
        int length = source.Count;
        List<TimeValue> results = new(length);

        for (int i = 0; i < length; i++)
        {
            IReusable s = source[i];
            results.Add(new TimeValue(s.Timestamp, advance(s.Value)));
        }

        return results;
    }

    /// <summary>
    /// Validates the rolling window size.
    /// </summary>
    /// <param name="lookbackPeriods">Number of periods in the window.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the lookback periods are less than 1.</exception>
    internal static void ValidateLookback(int lookbackPeriods)
    {
        if (lookbackPeriods < 1)
        {
            throw new ArgumentOutOfRangeException(
                nameof(lookbackPeriods), lookbackPeriods,
                "Lookback periods must be greater than 0.");
        }
    }
}
