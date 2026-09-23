namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Provides utility methods for the Price Momentum Oscillator (PMO).
/// </summary>
public static partial class Pmo
{
    /// <summary>
    /// Removes the warmup periods and the further periods PMO needs for its values to converge.
    /// </summary>
    /// <param name="results">PMO results to evaluate.</param>
    /// <returns>PMO results with the warmup periods removed.</returns>
    public static IReadOnlyList<PmoResult> RemoveWarmupPeriods(
        this IReadOnlyList<PmoResult> results)
        => results.RemoveBeforeFirstValue(
            static x => x.Pmo != null,
            static i => i + 251);

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
