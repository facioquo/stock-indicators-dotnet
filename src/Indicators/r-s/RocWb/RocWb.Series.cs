namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Rate of Change with Bands (RocWb) series indicator.
/// </summary>
public static partial class RocWb
{
    /// <summary>
    /// Converts a list of reusable values to a list of RocWb results.
    /// </summary>
    /// <param name="source">List of reusable values.</param>
    /// <param name="lookbackPeriods">Number of periods to look back for the ROC calculation.</param>
    /// <param name="emaPeriods">Number of periods for the exponential moving average calculation.</param>
    /// <param name="stdDevPeriods">Number of periods for the standard deviation calculation.</param>
    /// <returns>A list of RocWb results.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="source"/> is null.</exception>
    public static IReadOnlyList<RocWbResult> ToRocWb(
        this IReadOnlyList<IReusable> source,
        int lookbackPeriods = 20,
        int emaPeriods = 5,
        int stdDevPeriods = 5)
    {
        // check parameter arguments
        ArgumentNullException.ThrowIfNull(source);
        Validate(lookbackPeriods, emaPeriods, stdDevPeriods);

        // initialize
        int length = source.Count;
        List<RocWbResult> results = new(length);

        double k = 2d / (emaPeriods + 1);
        double prevEma = double.NaN;

        IReadOnlyList<IReusable> ogRoc = source
            .ToRoc(lookbackPeriods);

        double[] rocSq = ogRoc
            .Select(static x => x.Value * x.Value)
            .ToArray();

        double[] ema = new double[length];

        // roll through results
        for (int i = 0; i < length; i++)
        {
            IReusable roc = ogRoc[i];

            // exponential moving average
            if (double.IsNaN(prevEma) && i >= emaPeriods)
            {
                double sum = 0;
                for (int p = i - emaPeriods + 1; p <= i; p++)
                {
                    sum += ogRoc[p].Value;
                }

                ema[i] = sum / emaPeriods;
            }

            // normal EMA
            else
            {
                ema[i] = Ema.Increment(k, prevEma, roc.Value);
            }

            prevEma = ema[i];

            // ROC deviation
            double? rocDev = null;

            if (i >= stdDevPeriods)
            {
                double sum = 0;
                for (int p = i - stdDevPeriods + 1; p <= i; p++)
                {
                    sum += rocSq[p];
                }

                rocDev = Math.Sqrt(sum / stdDevPeriods).NaN2Null();
            }

            results.Add(new(
                Timestamp: roc.Timestamp,
                Roc: roc.Value.NaN2Null(),
                RocEma: ema[i].NaN2Null(),
                UpperBand: rocDev,
                LowerBand: -rocDev));
        }

        return results;
    }
}
