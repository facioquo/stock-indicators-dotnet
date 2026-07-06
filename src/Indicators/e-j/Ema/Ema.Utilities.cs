namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Provides utility methods for Exponential Moving Average (EMA) calculations.
/// </summary>
public static partial class Ema
{
    /// <summary>
    /// Increments the EMA value using the smoothing factor.
    /// </summary>
    /// <param name="k">Smoothing factor.</param>
    /// <param name="lastEma">Last EMA value.</param>
    /// <param name="newPrice">New price value.</param>
    /// <returns>Incremented EMA value.</returns>
    public static double Increment(
        double k,
        double lastEma,
        double newPrice)
        => lastEma + (k * (newPrice - lastEma));

    /// <summary>
    /// Increments the EMA value using the lookback periods.
    /// </summary>
    /// <param name="lookbackPeriods">Quantity of periods in lookback window.</param>
    /// <param name="lastEma">Last EMA value.</param>
    /// <param name="newPrice">New price value.</param>
    /// <returns>Incremented EMA value.</returns>
    public static double Increment(
        int lookbackPeriods,
        double lastEma,
        double newPrice)
    {
        double k = 2d / (lookbackPeriods + 1);
        return Increment(k, lastEma, newPrice);
    }

    /// <summary>
    /// Increments the EMA value using the smoothing factor.
    /// </summary>
    /// <param name="k">Smoothing factor.</param>
    /// <param name="lastEma">Last EMA value.</param>
    /// <param name="newPrice">New price value.</param>
    /// <returns>Incremented EMA value, or null if the last EMA value is null.</returns>
    public static double? Increment(
        double k,
        double? lastEma,
        double newPrice)
        => lastEma + (k * (newPrice - lastEma));

    /// <summary>
    /// Removes the recommended warmup periods from the EMA results.
    /// </summary>
    /// <param name="results">List of EMA results.</param>
    /// <returns>A list of EMA results with warmup periods removed.</returns>
    public static IReadOnlyList<EmaResult> RemoveWarmupPeriods(
        this IReadOnlyList<EmaResult> results)
    {
        ArgumentNullException.ThrowIfNull(results);

        int n = results
          .FindIndex(static x => x.Ema != null) + 1;

        return results.Remove(n + 100);
    }

    /// <summary>
    /// Validates the lookback periods for EMA calculations.
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
            throw new ArgumentOutOfRangeException(
                nameof(lookbackPeriods), lookbackPeriods,
                "Lookback periods must be greater than 0 for EMA.");
        }
    }
}
