namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Triple Exponential Moving Average (TEMA) series indicator.
/// </summary>
public static partial class Tema
{
    /// <summary>
    /// Calculates the TEMA series for the given source data.
    /// </summary>
    /// <param name="source">Source data to calculate the TEMA for.</param>
    /// <param name="lookbackPeriods">Number of lookback periods for the TEMA calculation.</param>
    /// <returns>A read-only list of TEMA results.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the source data is null.</exception>
    public static IReadOnlyList<TemaResult> ToTema(
        this IReadOnlyList<IReusable> source,
        int lookbackPeriods)
    {
        // check parameter arguments
        ArgumentNullException.ThrowIfNull(source);
        Validate(lookbackPeriods);

        // initialize
        int length = source.Count;
        List<TemaResult> results = new(length);

        double k = 2d / (lookbackPeriods + 1);
        double lastEma1 = double.NaN;
        double lastEma2 = double.NaN;
        double lastEma3 = double.NaN;

        // roll through source values
        for (int i = 0; i < length; i++)
        {
            IReusable s = source[i];

            // skip incalculable periods
            if (i < lookbackPeriods - 1)
            {
                results.Add(new(s.Timestamp));
                continue;
            }

            double ema1;
            double ema2;
            double ema3;

            // when no prior EMA, reset as SMA
            if (double.IsNaN(lastEma3))
            {
                double sum = 0;
                for (int p = i - lookbackPeriods + 1; p <= i; p++)
                {
                    IReusable ps = source[p];
                    sum += ps.Value;
                }

                ema1 = ema2 = ema3 = sum / lookbackPeriods;
            }

            // normal TEMA
            else
            {
                ema1 = lastEma1 + (k * (s.Value - lastEma1));
                ema2 = lastEma2 + (k * (ema1 - lastEma2));
                ema3 = lastEma3 + (k * (ema2 - lastEma3));
            }

            results.Add(new TemaResult(
                Timestamp: s.Timestamp,
                Tema: ((3 * ema1) - (3 * ema2) + ema3).NaN2Null()) {
                Ema1 = ema1,
                Ema2 = ema2,
                Ema3 = ema3
            });

            lastEma1 = ema1;
            lastEma2 = ema2;
            lastEma3 = ema3;
        }

        return results;
    }
}
