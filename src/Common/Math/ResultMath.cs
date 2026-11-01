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
public static class ResultMath
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
        ValidateFinite(amount, nameof(amount));
        return source.Transform(v => v + amount);
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
        ValidateFinite(percentOffset, nameof(percentOffset));
        double ratio = 1d + (percentOffset / 100d);
        return source.Transform(v => v * ratio);
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
        ValidateFinite(amount, nameof(amount));
        return source.Transform(v => v - amount);
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
        ValidateFinite(factor, nameof(factor));
        return source.Transform(v => v * factor);
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
        ValidateFinite(divisor, nameof(divisor));

        if (divisor == 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(divisor), divisor, "The divisor value must not be 0.");
        }

        return source.Transform(v => v / divisor);
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
        => source.RollingExtreme(lookbackPeriods, isMax: true);

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
        => source.RollingExtreme(lookbackPeriods, isMax: false);

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
    /// <exception cref="ArgumentNullException">Thrown when the series is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the lookback periods are less than 1.</exception>
    public static IReadOnlyList<TimeValue> RollingMedian(
        this IReadOnlyList<IReusable> source,
        int lookbackPeriods)
    {
        ArgumentNullException.ThrowIfNull(source);
        ValidateLookback(lookbackPeriods);

        int length = source.Count;
        List<TimeValue> results = new(length);

        // window values kept in ascending order (NaN sorts first);
        // holds one extra value between each insert and its paired removal
        List<double> window = new(Math.Min(lookbackPeriods, length) + 1);
        int nanCount = 0;
        int mid = lookbackPeriods / 2;
        bool isEven = lookbackPeriods % 2 == 0;

        for (int i = 0; i < length; i++)
        {
            double value = source[i].Value;
            Insert(window, value);

            if (double.IsNaN(value))
            {
                nanCount++;
            }

            if (i >= lookbackPeriods)
            {
                double outgoing = source[i - lookbackPeriods].Value;
                window.RemoveAt(window.BinarySearch(outgoing));

                if (double.IsNaN(outgoing))
                {
                    nanCount--;
                }
            }

            double median = i < lookbackPeriods - 1 || nanCount > 0
                ? double.NaN
                : isEven
                    ? (window[mid - 1] / 2d) + (window[mid] / 2d) // avoids overflow near double.MaxValue
                    : window[mid];

            results.Add(new TimeValue(source[i].Timestamp, median));
        }

        return results;
    }

    /// <summary>
    /// Applies a single-value operation to every value in a series.
    /// </summary>
    /// <param name="source">Chainable results, time sorted.</param>
    /// <param name="operation">Operation applied to each value.</param>
    /// <returns>Time series of operation results.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the series is null.</exception>
    private static List<TimeValue> Transform(
        this IReadOnlyList<IReusable> source,
        Func<double, double> operation)
    {
        ArgumentNullException.ThrowIfNull(source);

        int length = source.Count;
        List<TimeValue> results = new(length);

        for (int i = 0; i < length; i++)
        {
            IReusable s = source[i];
            results.Add(new TimeValue(s.Timestamp, operation(s.Value)));
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
    /// Calculates a rolling highest or lowest value with a monotonic index deque, in O(n).
    /// </summary>
    /// <param name="source">Chainable results, time sorted.</param>
    /// <param name="lookbackPeriods">Number of periods in the window, including the current one.</param>
    /// <param name="isMax">Whether to track the highest (true) or lowest (false) value.</param>
    /// <returns>Time series of rolling extreme values.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the series is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the lookback periods are less than 1.</exception>
    private static List<TimeValue> RollingExtreme(
        this IReadOnlyList<IReusable> source,
        int lookbackPeriods,
        bool isMax)
    {
        ArgumentNullException.ThrowIfNull(source);
        ValidateLookback(lookbackPeriods);

        int length = source.Count;
        List<TimeValue> results = new(length);

        // ring of candidate-extreme indexes, oldest at head; never holds more than one window
        int capacity = Math.Max(1, Math.Min(lookbackPeriods, length));
        int[] deque = new int[capacity];
        int head = 0;
        int count = 0;
        int lastNaN = -1;

        for (int i = 0; i < length; i++)
        {
            double value = source[i].Value;

            if (double.IsNaN(value))
            {
                lastNaN = i;
                count = 0;
            }
            else
            {
                // evict the expired head first, so the push below always has room
                if (count > 0 && deque[head] <= i - lookbackPeriods)
                {
                    head = (head + 1) % capacity;
                    count--;
                }

                while (count > 0)
                {
                    double back = source[deque[(head + count - 1) % capacity]].Value;

                    if (isMax ? back > value : back < value)
                    {
                        break;
                    }

                    count--;
                }

                deque[(head + count) % capacity] = i;
                count++;
            }

            double extreme = i < lookbackPeriods - 1 || lastNaN > i - lookbackPeriods
                ? double.NaN
                : source[deque[head]].Value;

            results.Add(new TimeValue(source[i].Timestamp, extreme));
        }

        return results;
    }

    /// <summary>
    /// Inserts a value into an ascending list, keeping it sorted.
    /// </summary>
    /// <param name="sorted">Ascending list of values.</param>
    /// <param name="value">Value to insert.</param>
    private static void Insert(List<double> sorted, double value)
    {
        int index = sorted.BinarySearch(value);
        sorted.Insert(index < 0 ? ~index : index, value);
    }

    /// <summary>
    /// Validates that a scalar operand is a finite number.
    /// </summary>
    /// <param name="value">Scalar operand.</param>
    /// <param name="paramName">Name of the operand's parameter.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the value is NaN or infinite.</exception>
    private static void ValidateFinite(double value, string paramName)
    {
        if (!double.IsFinite(value))
        {
            throw new ArgumentOutOfRangeException(
                paramName, value, $"The {paramName} value must be a finite number.");
        }
    }

    /// <summary>
    /// Validates the rolling window size.
    /// </summary>
    /// <param name="lookbackPeriods">Number of periods in the window.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the lookback periods are less than 1.</exception>
    private static void ValidateLookback(int lookbackPeriods)
    {
        if (lookbackPeriods < 1)
        {
            throw new ArgumentOutOfRangeException(
                nameof(lookbackPeriods), lookbackPeriods,
                "Lookback periods must be greater than 0.");
        }
    }
}
