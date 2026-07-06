namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Provides utility methods for the WMA (Weighted Moving Average) indicator.
/// </summary>
public static partial class Wma
{
    // parameter validation
    /// <summary>
    /// Validates the parameters for the WMA calculation.
    /// </summary>
    /// <param name="lookbackPeriods">Quantity of periods in lookback window.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when the lookback periods are less than or equal to 0.
    /// </exception>
    internal static void Validate(
        int lookbackPeriods)
    {
        // check parameter arguments
        if (lookbackPeriods <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(lookbackPeriods), lookbackPeriods,
                "Lookback periods must be greater than 0 for WMA.");
        }
    }

    /// <summary>
    /// Calculates a weighted moving average from the supplied buffer.
    /// </summary>
    /// <param name="buffer">Buffer containing the values to average.</param>
    /// <param name="periods">Number of periods in the weighted average.</param>
    /// <param name="divisor">Divisor used to normalize the weighted sum.</param>
    /// <returns>Weighted moving average, or <c>null</c> if insufficient data is present.</returns>
    internal static double? ComputeWeightedMovingAverage(Queue<double> buffer, int periods, double divisor)
    {
        if (buffer.Count < periods)
        {
            return null;
        }

        double wma = 0d;
        int weight = 1;

        // Calculate exactly like static series: divide inside the loop for each term
        // This ensures precision matches the static series implementation
        foreach (double value in buffer)
        {
            wma += value * weight / divisor;
            weight++;

            if (weight > periods)
            {
                break;
            }
        }

        return wma;
    }

    /// <summary>
    /// Calculates a weighted moving average for the specified index using reusable source values.
    /// </summary>
    /// <typeparam name="T">Reusable type within the source list.</typeparam>
    /// <param name="source">Source list of reusable values.</param>
    /// <param name="lookbackPeriods">Quantity of periods in lookback window.</param>
    /// <param name="endIndex">Ending index (inclusive) for the calculation.</param>
    /// <returns>Weighted moving average, or <see cref="double.NaN"/> if insufficient data is present.</returns>
    internal static double Increment<T>(
        IReadOnlyList<T> source,
        int lookbackPeriods,
        int endIndex)
        where T : IReusable
    {
        ArgumentNullException.ThrowIfNull(source);

        if (endIndex < lookbackPeriods - 1 || endIndex >= source.Count)
        {
            return double.NaN;
        }

        double divisor = (double)lookbackPeriods * (lookbackPeriods + 1) / 2d;

        double wma = 0d;
        int weight = 1;

        for (int i = endIndex - lookbackPeriods + 1; i <= endIndex; i++)
        {
            double value = source[i].Value;
            if (double.IsNaN(value))
            {
                return double.NaN;
            }

            wma += value * weight / divisor;
            weight++;
        }

        return wma;
    }
}
