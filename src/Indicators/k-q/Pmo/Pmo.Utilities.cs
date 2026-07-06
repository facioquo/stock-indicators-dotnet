namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Provides utility methods for the Price Momentum Oscillator (PMO).
/// </summary>
public static partial class Pmo
{
    /// <summary>
    /// Removes the recommended warmup periods from the PMO results.
    /// </summary>
    /// <inheritdoc cref="Reusable.RemoveWarmupPeriods{T}(IReadOnlyList{T})"/>
    public static IReadOnlyList<PmoResult> RemoveWarmupPeriods(
        this IReadOnlyList<PmoResult> results)
    {
        ArgumentNullException.ThrowIfNull(results);

        int ts = results
            .FindIndex(static x => x.Pmo != null) + 1;

        return results.Remove(ts + 250);
    }

    /// <summary>
    /// Validates the parameters for PMO calculations.
    /// </summary>
    /// <param name="timePeriods">Number of periods for the time span.</param>
    /// <param name="smoothPeriods">Number of periods for smoothing.</param>
    /// <param name="signalPeriods">Number of periods for the signal line.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when any parameter is out of range.</exception>
    internal static void Validate(
        int timePeriods,
        int smoothPeriods,
        int signalPeriods)
    {
        // check parameter arguments
        if (timePeriods <= 1)
        {
            throw new ArgumentOutOfRangeException(nameof(timePeriods), timePeriods,
                "Time periods must be greater than 1 for PMO.");
        }

        if (smoothPeriods <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(smoothPeriods), smoothPeriods,
                "Smoothing periods must be greater than 0 for PMO.");
        }

        if (signalPeriods <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(signalPeriods), signalPeriods,
                "Signal periods must be greater than 0 for PMO.");
        }
    }
}
