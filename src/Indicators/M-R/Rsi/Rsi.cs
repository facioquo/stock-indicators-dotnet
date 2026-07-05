namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Relative Strength Index (RSI) indicator.
/// </summary>
public static partial class Rsi
{
    /// <summary>
    /// Converts a list of reusable values to a list of RSI results.
    /// </summary>
    /// <param name="source">List of reusable values.</param>
    /// <param name="lookbackPeriods">Number of periods to look back for the RSI calculation.</param>
    /// <returns>A list of RSI results.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="source"/> is null.</exception>
    public static IReadOnlyList<RsiResult> ToRsi(
        this IReadOnlyList<IReusable> source,
        int lookbackPeriods = 14)
    {
        // check parameter arguments
        ArgumentNullException.ThrowIfNull(source);
        Validate(lookbackPeriods);

        // initialize
        int length = source.Count;
        double avgGain = double.NaN;
        double avgLoss = double.NaN;

        List<RsiResult> results = new(length);
        double[] gain = new double[length]; // gain
        double[] loss = new double[length]; // loss

        if (length == 0)
        {
            return results;
        }

        double prevValue = source[0].Value;

        // roll through source values
        for (int i = 0; i < length; i++)
        {
            IReusable s = source[i];

            if (double.IsNaN(s.Value) || double.IsNaN(prevValue))
            {
                gain[i] = loss[i] = double.NaN;
            }
            else
            {
                gain[i] = s.Value > prevValue ? s.Value - prevValue : 0;
                loss[i] = s.Value < prevValue ? prevValue - s.Value : 0;
            }

            double? rsi = null;
            prevValue = s.Value;

            // re/initialize average gain
            if (i >= lookbackPeriods && (double.IsNaN(avgGain) || double.IsNaN(avgLoss)))
            {
                double sumGain = 0;
                double sumLoss = 0;

                for (int p = i - lookbackPeriods + 1; p <= i; p++)
                {
                    sumGain += gain[p];
                    sumLoss += loss[p];
                }

                avgGain = sumGain / lookbackPeriods;
                avgLoss = sumLoss / lookbackPeriods;

                rsi = !double.IsNaN(avgGain / avgLoss)
                      ? avgLoss > 0 ? 100 - (100 / (1 + (avgGain / avgLoss))) : 100
                      : null;
            }

            // calculate RSI normally
            else if (i > lookbackPeriods)
            {
                avgGain = ((avgGain * (lookbackPeriods - 1)) + gain[i]) / lookbackPeriods;
                avgLoss = ((avgLoss * (lookbackPeriods - 1)) + loss[i]) / lookbackPeriods;

                if (avgLoss > 0)
                {
                    double rs = avgGain / avgLoss;
                    rsi = 100 - (100 / (1 + rs));
                }
                else
                {
                    rsi = 100;
                }
            }

            RsiResult r = new(
                Timestamp: s.Timestamp,
                Rsi: rsi);

            results.Add(r);
        }

        return results;
    }
}
