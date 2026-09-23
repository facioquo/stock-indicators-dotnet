namespace FacioQuo.Stock.Indicators;

// KLINGER VOLUME OSCILLATOR (UTILIITES)

public static partial class Kvo
{
    /// <summary>
    /// Removes the warmup periods and the further periods Klinger Volume Oscillator needs for its values to converge.
    /// </summary>
    /// <param name="results">Klinger Volume Oscillator results to evaluate.</param>
    /// <returns>Klinger Volume Oscillator results with the warmup periods removed.</returns>
    public static IReadOnlyList<KvoResult> RemoveWarmupPeriods(
        this IReadOnlyList<KvoResult> results)
        => results.RemoveBeforeFirstValue(
            static x => x.Oscillator != null,
            static i => i + 149);

    /// <summary>
    /// Validates the parameters for the KVO (Klinger Volume Oscillator) calculation.
    /// </summary>
    /// <param name="fastPeriods">Number of periods for the fast EMA.</param>
    /// <param name="slowPeriods">Number of periods for the slow EMA.</param>
    /// <param name="signalPeriods">Number of periods for the signal line.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when any of the parameters are out of their valid range.
    /// </exception>
    internal static void Validate(
        int fastPeriods,
        int slowPeriods,
        int signalPeriods)
    {
        // check parameter arguments
        if (fastPeriods <= 2)
        {
            throw new ArgumentOutOfRangeException(nameof(fastPeriods), fastPeriods,
                "Fast (short) Periods must be greater than 2 for Klinger Oscillator.");
        }

        if (slowPeriods <= fastPeriods)
        {
            throw new ArgumentOutOfRangeException(nameof(slowPeriods), slowPeriods,
                "Slow (long) Periods must be greater than Fast Periods for Klinger Oscillator.");
        }

        if (signalPeriods <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(signalPeriods), signalPeriods,
                "Signal Periods must be greater than 0 for Klinger Oscillator.");
        }
    }

    /// <summary>
    /// State structure for KVO streaming calculations.
    /// </summary>
    internal class KvoState
    {
        public double PrevHlc { get; set; }
        public double PrevTrend { get; set; }
        public double PrevDm { get; set; }
        public double PrevCm { get; set; }
        public double PrevVfFastEma { get; set; }
        public double PrevVfSlowEma { get; set; }
        public double SumVf { get; set; }
    }
}
