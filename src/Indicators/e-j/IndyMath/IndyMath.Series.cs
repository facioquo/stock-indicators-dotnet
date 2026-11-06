namespace FacioQuo.Stock.Indicators;

/// <summary>
/// IndyMath: arithmetic and rolling-window math on chainable indicator results.
/// </summary>
/// <remarks>
/// <para>
/// Each method returns a new list of <see cref="MathResult"/> results, one per source result,
/// so the output chains into any indicator that accepts <see cref="IReusable"/> values.
/// An incalculable period, such as a warmup period of either input, is <see langword="null"/>.
/// </para>
/// <para>
/// Methods that combine two series require both to hold the same number of results with
/// identical timestamps at every position; a mismatch throws instead of pairing values
/// from different periods.
/// </para>
/// </remarks>
public static partial class IndyMath
{
    /// <summary>
    /// Adds the values of two series, period by period.
    /// </summary>
    /// <param name="basisSource">Chainable results, time sorted.</param>
    /// <param name="otherSource">Chainable results with timestamps matching <paramref name="basisSource"/>.</param>
    /// <returns>Time series of <c>basisSource + otherSource</c> values.</returns>
    /// <exception cref="ArgumentNullException">Thrown when either series is null.</exception>
    /// <exception cref="InvalidBarsException">Thrown when the series lengths or timestamps do not match.</exception>
    public static IReadOnlyList<MathResult> Plus(
        this IReadOnlyList<IReusable> basisSource,
        IReadOnlyList<IReusable> otherSource)
        => basisSource.Combine(otherSource, PairOperation.Plus);

    /// <summary>
    /// Adds a fixed amount to every value in a series.
    /// </summary>
    /// <param name="source">Chainable results, time sorted.</param>
    /// <param name="amount">Finite amount to add.</param>
    /// <returns>Time series of <c>source + amount</c> values.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the series is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the amount is not finite.</exception>
    public static IReadOnlyList<MathResult> Plus(
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
    /// <returns>Time series of <c>source + source × percentOffset / 100</c> values.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the series is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the percent offset is not finite.</exception>
    public static IReadOnlyList<MathResult> PlusPercent(
        this IReadOnlyList<IReusable> source,
        double percentOffset)
    {
        ArgumentNullException.ThrowIfNull(source);
        return source.Transform(new ScalarKernel(ScalarOperation.PlusPercent, percentOffset));
    }

    /// <summary>
    /// Subtracts the values of a second series from a first series, period by period.
    /// </summary>
    /// <param name="basisSource">Chainable results, time sorted.</param>
    /// <param name="otherSource">Chainable results with timestamps matching <paramref name="basisSource"/>.</param>
    /// <returns>Time series of <c>basisSource − otherSource</c> values.</returns>
    /// <exception cref="ArgumentNullException">Thrown when either series is null.</exception>
    /// <exception cref="InvalidBarsException">Thrown when the series lengths or timestamps do not match.</exception>
    public static IReadOnlyList<MathResult> Minus(
        this IReadOnlyList<IReusable> basisSource,
        IReadOnlyList<IReusable> otherSource)
        => basisSource.Combine(otherSource, PairOperation.Minus);

    /// <summary>
    /// Subtracts a fixed amount from every value in a series.
    /// </summary>
    /// <param name="source">Chainable results, time sorted.</param>
    /// <param name="amount">Finite amount to subtract.</param>
    /// <returns>Time series of <c>source − amount</c> values.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the series is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the amount is not finite.</exception>
    public static IReadOnlyList<MathResult> Minus(
        this IReadOnlyList<IReusable> source,
        double amount)
    {
        ArgumentNullException.ThrowIfNull(source);
        return source.Transform(new ScalarKernel(ScalarOperation.Minus, amount));
    }

    /// <summary>
    /// Multiplies the values of two series, period by period.
    /// </summary>
    /// <param name="basisSource">Chainable results, time sorted.</param>
    /// <param name="otherSource">Chainable results with timestamps matching <paramref name="basisSource"/>.</param>
    /// <returns>Time series of <c>basisSource × otherSource</c> values.</returns>
    /// <exception cref="ArgumentNullException">Thrown when either series is null.</exception>
    /// <exception cref="InvalidBarsException">Thrown when the series lengths or timestamps do not match.</exception>
    public static IReadOnlyList<MathResult> Times(
        this IReadOnlyList<IReusable> basisSource,
        IReadOnlyList<IReusable> otherSource)
        => basisSource.Combine(otherSource, PairOperation.Times);

    /// <summary>
    /// Multiplies every value in a series by a fixed factor.
    /// </summary>
    /// <param name="source">Chainable results, time sorted.</param>
    /// <param name="factor">Finite multiplier.</param>
    /// <returns>Time series of <c>source × factor</c> values.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the series is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the factor is not finite.</exception>
    public static IReadOnlyList<MathResult> Times(
        this IReadOnlyList<IReusable> source,
        double factor)
    {
        ArgumentNullException.ThrowIfNull(source);
        return source.Transform(new ScalarKernel(ScalarOperation.Times, factor));
    }

    /// <summary>
    /// Divides the values of a first series by a second series, period by period.
    /// </summary>
    /// <param name="basisSource">Chainable results (dividends), time sorted.</param>
    /// <param name="otherSource">Chainable results (divisors) with timestamps matching <paramref name="basisSource"/>.</param>
    /// <returns>
    /// Time series of <c>basisSource ÷ otherSource</c> values; <see langword="null"/> where the divisor is 0.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when either series is null.</exception>
    /// <exception cref="InvalidBarsException">Thrown when the series lengths or timestamps do not match.</exception>
    public static IReadOnlyList<MathResult> DividedBy(
        this IReadOnlyList<IReusable> basisSource,
        IReadOnlyList<IReusable> otherSource)
        => basisSource.Combine(otherSource, PairOperation.DividedBy);

    /// <summary>
    /// Divides every value in a series by a fixed divisor.
    /// </summary>
    /// <param name="source">Chainable results, time sorted.</param>
    /// <param name="divisor">Finite, non-zero divisor.</param>
    /// <returns>Time series of <c>source ÷ divisor</c> values.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the series is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the divisor is 0 or not finite.</exception>
    public static IReadOnlyList<MathResult> DividedBy(
        this IReadOnlyList<IReusable> source,
        double divisor)
    {
        ArgumentNullException.ThrowIfNull(source);
        return source.Transform(new ScalarKernel(ScalarOperation.DividedBy, divisor));
    }

    /// <summary>
    /// Selects the larger value of two series, period by period.
    /// </summary>
    /// <param name="basisSource">Chainable results, time sorted.</param>
    /// <param name="otherSource">Chainable results with timestamps matching <paramref name="basisSource"/>.</param>
    /// <returns>Time series of the larger value at each period.</returns>
    /// <exception cref="ArgumentNullException">Thrown when either series is null.</exception>
    /// <exception cref="InvalidBarsException">Thrown when the series lengths or timestamps do not match.</exception>
    public static IReadOnlyList<MathResult> MaxWith(
        this IReadOnlyList<IReusable> basisSource,
        IReadOnlyList<IReusable> otherSource)
        => basisSource.Combine(otherSource, PairOperation.MaxWith);

    /// <summary>
    /// Selects the smaller value of two series, period by period.
    /// </summary>
    /// <param name="basisSource">Chainable results, time sorted.</param>
    /// <param name="otherSource">Chainable results with timestamps matching <paramref name="basisSource"/>.</param>
    /// <returns>Time series of the smaller value at each period.</returns>
    /// <exception cref="ArgumentNullException">Thrown when either series is null.</exception>
    /// <exception cref="InvalidBarsException">Thrown when the series lengths or timestamps do not match.</exception>
    public static IReadOnlyList<MathResult> MinWith(
        this IReadOnlyList<IReusable> basisSource,
        IReadOnlyList<IReusable> otherSource)
        => basisSource.Combine(otherSource, PairOperation.MinWith);

    /// <summary>
    /// Calculates the highest value over a rolling lookback window.
    /// </summary>
    /// <param name="source">Chainable results, time sorted.</param>
    /// <param name="lookbackPeriods">Number of periods in the window, including the current one.</param>
    /// <returns>
    /// Time series of rolling highest values; <see langword="null"/> for the first
    /// <c>lookbackPeriods − 1</c> periods and wherever the window holds an incalculable value.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when the series is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the lookback periods are less than 1.</exception>
    public static IReadOnlyList<MathResult> RollingMax(
        this IReadOnlyList<IReusable> source,
        int lookbackPeriods)
    {
        ArgumentNullException.ThrowIfNull(source);
        return source.Map(new RollingExtremeWindow(lookbackPeriods, isMax: true).Add);
    }

    /// <summary>
    /// Calculates the lowest value over a rolling lookback window.
    /// </summary>
    /// <param name="source">Chainable results, time sorted.</param>
    /// <param name="lookbackPeriods">Number of periods in the window, including the current one.</param>
    /// <returns>
    /// Time series of rolling lowest values; <see langword="null"/> for the first
    /// <c>lookbackPeriods − 1</c> periods and wherever the window holds an incalculable value.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when the series is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the lookback periods are less than 1.</exception>
    public static IReadOnlyList<MathResult> RollingMin(
        this IReadOnlyList<IReusable> source,
        int lookbackPeriods)
    {
        ArgumentNullException.ThrowIfNull(source);
        return source.Map(new RollingExtremeWindow(lookbackPeriods, isMax: false).Add);
    }

    /// <summary>
    /// Calculates the median value over a rolling lookback window.
    /// </summary>
    /// <param name="source">Chainable results, time sorted.</param>
    /// <param name="lookbackPeriods">Number of periods in the window, including the current one.</param>
    /// <returns>
    /// Time series of rolling median values, averaging the two middle values when the window
    /// is even; <see langword="null"/> for the first <c>lookbackPeriods − 1</c> periods and
    /// wherever the window holds an incalculable value.
    /// </returns>
    /// <remarks>
    /// The window is kept sorted, so each period costs O(log k) comparisons plus an O(k) shift,
    /// and the series costs O(n·k) for a lookback of k periods.
    /// </remarks>
    /// <exception cref="ArgumentNullException">Thrown when the series is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the lookback periods are less than 1.</exception>
    public static IReadOnlyList<MathResult> RollingMedian(
        this IReadOnlyList<IReusable> source,
        int lookbackPeriods)
    {
        ArgumentNullException.ThrowIfNull(source);
        return source.Map(new RollingMedianWindow(lookbackPeriods).Add);
    }

    /// <summary>
    /// Applies a single-value operation to every value in a series.
    /// </summary>
    /// <param name="source">Chainable results, time sorted.</param>
    /// <param name="kernel">Validated operation applied to each value.</param>
    /// <returns>Time series of operation results.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the series is null.</exception>
    private static List<MathResult> Transform(
        this IReadOnlyList<IReusable> source,
        ScalarKernel kernel)
    {
        ArgumentNullException.ThrowIfNull(source);
        return source.Map(kernel.Apply);
    }

    /// <summary>
    /// Applies a two-value operation to two timestamp-matched series.
    /// </summary>
    /// <param name="basisSource">Chainable results, time sorted.</param>
    /// <param name="otherSource">Chainable results with timestamps matching <paramref name="basisSource"/>.</param>
    /// <param name="operation">Operation applied to each pair of values.</param>
    /// <returns>Time series of operation results.</returns>
    /// <exception cref="ArgumentNullException">Thrown when either series is null.</exception>
    /// <exception cref="InvalidBarsException">Thrown when the series lengths or timestamps do not match.</exception>
    private static List<MathResult> Combine(
        this IReadOnlyList<IReusable> basisSource,
        IReadOnlyList<IReusable> otherSource,
        PairOperation operation)
    {
        ArgumentNullException.ThrowIfNull(basisSource);
        ArgumentNullException.ThrowIfNull(otherSource);

        int length = basisSource.Count;

        if (otherSource.Count != length)
        {
            throw new InvalidBarsException(
                nameof(otherSource), otherSource.Count,
                $"Series lengths do not match: basisSource has {length} results and otherSource has {otherSource.Count}.  "
              + "Calculate both series from the same bars, and trim both to the same range before combining.");
        }

        List<MathResult> results = new(length);

        for (int i = 0; i < length; i++)
        {
            IReusable s = basisSource[i];
            IReusable o = otherSource[i];

            if (s.Timestamp != o.Timestamp)
            {
                throw new InvalidBarsException(
                    nameof(otherSource), o.Timestamp,
                    $"Timestamp sequence does not match at index {i}: basisSource has {s.Timestamp:o} and otherSource has {o.Timestamp:o}.  "
                  + "Calculate both series from the same bars, and trim both to the same range before combining.");
            }

            results.Add(new MathResult(s.Timestamp, PairKernel.Apply(operation, s.Value, o.Value).NaN2Null()));
        }

        return results;
    }

    /// <summary>
    /// Maps every value in a series through a function, keeping each timestamp.
    /// </summary>
    /// <param name="source">Chainable results, time sorted.</param>
    /// <param name="map">Function from the next value to its result; a rolling window's step is one.</param>
    /// <returns>Time series of results.</returns>
    private static List<MathResult> Map(
        this IReadOnlyList<IReusable> source,
        Func<double, double> map)
    {
        int length = source.Count;
        List<MathResult> results = new(length);

        for (int i = 0; i < length; i++)
        {
            IReusable s = source[i];
            results.Add(new MathResult(s.Timestamp, map(s.Value).NaN2Null()));
        }

        return results;
    }
}
