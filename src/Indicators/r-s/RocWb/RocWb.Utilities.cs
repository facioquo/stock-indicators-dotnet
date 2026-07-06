namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Provides utility methods for Rate of Change with Bands (RocWb) calculations.
/// </summary>
public static partial class RocWb
{
    /// <summary>
    /// Removes the recommended warmup periods from the RocWb results.
    /// </summary>
    /// <inheritdoc cref="Reusable.RemoveWarmupPeriods{T}(IReadOnlyList{T})"/>
    public static IReadOnlyList<RocWbResult> RemoveWarmupPeriods(
        this IReadOnlyList<RocWbResult> results)
    {
        ArgumentNullException.ThrowIfNull(results);

        int n = results
            .FindIndex(static x => x.RocEma != null) + 1;

        return results.Remove(n + 100);
    }

    /// <summary>
    /// Validates the parameters for RocWb calculations.
    /// </summary>
    /// <param name="lookbackPeriods">Quantity of periods in lookback window.</param>
    /// <param name="emaPeriods">Number of periods for the exponential moving average calculation.</param>
    /// <param name="stdDevPeriods">Number of periods for the standard deviation calculation.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when any of the parameters are out of range.</exception>
    internal static void Validate(
        int lookbackPeriods,
        int emaPeriods,
        int stdDevPeriods)
    {
        // check parameter arguments
        if (lookbackPeriods <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(lookbackPeriods), lookbackPeriods,
                "Lookback periods must be greater than 0 for ROC with Bands.");
        }

        if (emaPeriods <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(emaPeriods), emaPeriods,
                "EMA periods must be greater than 0 for ROC.");
        }

        if (stdDevPeriods <= 0 || stdDevPeriods > lookbackPeriods)
        {
            throw new ArgumentOutOfRangeException(nameof(stdDevPeriods), stdDevPeriods,
                "Standard deviation periods must be greater than 0 and less than lookback period for ROC with Bands.");
        }
    }
}
