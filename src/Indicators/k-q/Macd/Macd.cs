namespace FacioQuo.Stock.Indicators;

/// <summary>
/// MACD (Moving Average Convergence Divergence) indicator.
/// </summary>
public static partial class Macd
{
    /// <summary>
    /// Converts a list of source values to MACD (Moving Average Convergence Divergence) results.
    /// </summary>
    /// <param name="source">List of source values to transform.</param>
    /// <param name="fastPeriods">Number of periods for the fast EMA. Default is 12.</param>
    /// <param name="slowPeriods">Number of periods for the slow EMA. Default is 26.</param>
    /// <param name="signalPeriods">Number of periods for the signal line. Default is 9.</param>
    /// <returns>A list of MACD results.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="source"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when any of the parameters are out of their valid range.</exception>
    public static IReadOnlyList<MacdResult> ToMacd(
        this IReadOnlyList<IReusable> source,
        int fastPeriods = 12,
        int slowPeriods = 26,
        int signalPeriods = 9)
    {
        // check parameter arguments
        ArgumentNullException.ThrowIfNull(source);
        Validate(fastPeriods, slowPeriods, signalPeriods);

        // initialize
        int length = source.Count;
        List<MacdResult> results = new(length);

        double lastEmaFast = double.NaN;
        double lastEmaSlow = double.NaN;
        double lastEmaMacd = double.NaN;

        double kFast = 2d / (fastPeriods + 1);
        double kSlow = 2d / (slowPeriods + 1);
        double kMacd = 2d / (signalPeriods + 1);

        // roll through source values
        for (int i = 0; i < length; i++)
        {
            // Fast EMA
            double emaFast
                = i >= fastPeriods - 1 && results[i - 1].FastEma is null
                ? Sma.Increment(source, fastPeriods, i)
                : Ema.Increment(kFast, lastEmaFast, source[i].Value);

            // Slow EMA
            double emaSlow
                = i >= slowPeriods - 1 && results[i - 1].SlowEma is null
                ? Sma.Increment(source, slowPeriods, i)
                : Ema.Increment(kSlow, lastEmaSlow, source[i].Value);

            // MACD
            double macd = emaFast - emaSlow;

            // Signal
            double signal;

            if (i >= signalPeriods + slowPeriods - 2 && results[i - 1].Signal is null)
            {
                double sum = macd;
                for (int p = i - signalPeriods + 1; p < i; p++)
                {
                    sum += results[p].Value;
                }

                signal = sum / signalPeriods;
            }
            else
            {
                signal = Ema.Increment(kMacd, lastEmaMacd, macd);
            }

            // results
            results.Add(new MacdResult(
                Timestamp: source[i].Timestamp,
                Macd: macd.NaN2Null(),
                Signal: signal.NaN2Null(),
                Histogram: (macd - signal).NaN2Null(),
                FastEma: emaFast.NaN2Null(),
                SlowEma: emaSlow.NaN2Null()));

            lastEmaMacd = signal;
            lastEmaFast = emaFast;
            lastEmaSlow = emaSlow;
        }

        return results;
    }
}
