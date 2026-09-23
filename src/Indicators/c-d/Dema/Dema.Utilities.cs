namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Provides utility methods for the DEMA (Double Exponential Moving Average) indicator.
/// </summary>
public static partial class Dema
{
    /// <summary>
    /// Calculates DEMA from two EMA values.
    /// </summary>
    /// <param name="ema1">First EMA value (EMA of price).</param>
    /// <param name="ema2">Second EMA value (EMA of EMA1).</param>
    /// <returns>DEMA value.</returns>
    public static double Calculate(double ema1, double ema2)
        => (2d * ema1) - ema2;

    /// <summary>
    /// Calculates DEMA from two EMA values, handling nullable inputs.
    /// </summary>
    /// <param name="ema1">First EMA value (EMA of price).</param>
    /// <param name="ema2">Second EMA value (EMA of EMA1).</param>
    /// <returns>DEMA value, or null if either input is null.</returns>
    public static double? Calculate(double? ema1, double? ema2)
        => (ema1.HasValue && ema2.HasValue) ? Calculate(ema1.Value, ema2.Value) : null;

    /// <summary>
    /// Removes the warmup periods and the further periods DEMA needs for its values to converge.
    /// </summary>
    /// <param name="results">DEMA results to evaluate.</param>
    /// <returns>DEMA results with the warmup periods removed.</returns>
    public static IReadOnlyList<DemaResult> RemoveWarmupPeriods(
        this IReadOnlyList<DemaResult> results)
        => results.RemoveBeforeFirstValue(
            static x => x.Dema != null,
            static i => (2 * i) + 102);

    /// <summary>
    /// Validates the parameters for the DEMA calculation.
    /// </summary>
    /// <param name="lookbackPeriods">Quantity of periods in lookback window.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the lookback periods are less than or equal to 0.</exception>
    internal static void Validate(
        int lookbackPeriods)
    {
        // check parameter arguments
        if (lookbackPeriods <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(lookbackPeriods), lookbackPeriods,
                "Lookback periods must be greater than 0 for DEMA.");
        }
    }
}
