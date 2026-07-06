namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Rate of Change (ROC) indicator.
/// </summary>
public static partial class Roc
{
    /// <summary>
    /// Converts a list of reusable values to a list of ROC results.
    /// </summary>
    /// <param name="source">List of reusable values.</param>
    /// <param name="lookbackPeriods">Number of periods to look back for the ROC calculation.</param>
    /// <returns>A list of ROC results.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="source"/> is null.</exception>
    public static IReadOnlyList<RocResult> ToRoc(
        this IReadOnlyList<IReusable> source,
        int lookbackPeriods = 14)
    {
        // check parameter arguments
        ArgumentNullException.ThrowIfNull(source);
        Validate(lookbackPeriods);

        // initialize
        int length = source.Count;
        List<RocResult> results = new(length);

        // roll through source values
        for (int i = 0; i < length; i++)
        {
            IReusable s = source[i];

            double roc;
            double momentum;

            if (i + 1 > lookbackPeriods)
            {
                IReusable back = source[i - lookbackPeriods];

                momentum = s.Value - back.Value;

                roc = back.Value == 0
                    ? double.NaN
                    : 100d * momentum / back.Value;
            }
            else
            {
                momentum = double.NaN;
                roc = double.NaN;
            }

            RocResult r = new(
                Timestamp: s.Timestamp,
                Momentum: momentum.NaN2Null(),
                Roc: roc.NaN2Null());

            results.Add(r);
        }

        return results;
    }
}
