namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Provides utility methods for the Stochastic RSI indicator.
/// </summary>
public static partial class StochRsi
{
    /// <summary>
    /// Removes the recommended warmup periods from the results.
    /// </summary>
    /// <param name="results">List of Stochastic RSI results.</param>
    /// <returns>A list of Stochastic RSI results with warmup periods removed.</returns>
    public static IReadOnlyList<StochRsiResult> RemoveWarmupPeriods(
        this IReadOnlyList<StochRsiResult> results)
    {
        ArgumentNullException.ThrowIfNull(results);

        int n = results
            .FindIndex(static x => x.StochRsi != null) + 2;

        return results.Remove(n + 100);
    }

    /// <summary>
    /// Returns the minimum number of source items required to produce a full
    /// Stochastic RSI result (with signal and smooth), used to validate cache size settings.
    /// Signal and smooth SMAs are applied sequentially, so their warmup costs are additive.
    /// When smoothPeriods is 1, the smoothing step is a pass-through and only costs 1 item.
    /// </summary>
    /// <param name="rsiPeriods">Number of periods for RSI.</param>
    /// <param name="stochPeriods">Number of periods for Stochastic %K window.</param>
    /// <param name="signalPeriods">Number of periods for the signal (D) line.</param>
    /// <param name="smoothPeriods">Number of periods for %K smoothing.</param>
    /// <returns>Minimum warmup period count.</returns>
    public static int WarmupPeriod(
        int rsiPeriods,
        int stochPeriods,
        int signalPeriods,
        int smoothPeriods)
        => (rsiPeriods * 2) + stochPeriods + signalPeriods + smoothPeriods - (smoothPeriods > 1 ? 2 : 1);

    /// <summary>
    /// Validates the parameters for Stochastic RSI calculation.
    /// </summary>
    /// <param name="rsiPeriods">Number of periods for the RSI calculation.</param>
    /// <param name="stochPeriods">Number of periods for the Stochastic calculation.</param>
    /// <param name="signalPeriods">Number of periods for the signal line.</param>
    /// <param name="smoothPeriods">Number of periods for smoothing.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when a parameter is out of range.</exception>
    internal static void Validate(
        int rsiPeriods,
        int stochPeriods,
        int signalPeriods,
        int smoothPeriods)
    {
        // check parameter arguments
        if (rsiPeriods <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(rsiPeriods), rsiPeriods,
                "RSI periods must be greater than 0 for Stochastic RSI.");
        }

        if (stochPeriods <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(stochPeriods), stochPeriods,
                "STOCH periods must be greater than 0 for Stochastic RSI.");
        }

        if (signalPeriods <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(signalPeriods), signalPeriods,
                "Signal periods must be greater than 0 for Stochastic RSI.");
        }

        if (smoothPeriods <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(smoothPeriods), smoothPeriods,
                "Smooth periods must be greater than 0 for Stochastic RSI.");
        }
    }
}
