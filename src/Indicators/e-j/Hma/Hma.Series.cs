namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Hull Moving Average (HMA) indicator.
/// </summary>
public static partial class Hma
{
    /// <summary>
    /// Converts a list of time-series values to Hull Moving Average (HMA) results.
    /// </summary>
    /// <param name="source">List of time-series values to transform.</param>
    /// <param name="lookbackPeriods">Quantity of periods in lookback window.</param>
    /// <returns>A list of HMA results.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="source"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the lookback periods are less than 2.</exception>
    public static IReadOnlyList<HmaResult> ToHma(
        this IReadOnlyList<IReusable> source,
        int lookbackPeriods = 14)
    {
        // check parameter arguments
        ArgumentNullException.ThrowIfNull(source);
        Validate(lookbackPeriods);

        // initialize
        int length = source.Count;
        int shiftQty = lookbackPeriods - 1;
        List<IReusable> synthHistory = [];

        IReadOnlyList<WmaResult> wmaN1
            = source.ToWma(lookbackPeriods);

        IReadOnlyList<WmaResult> wmaN2
            = source.ToWma(lookbackPeriods / 2);

        // roll through source values, to get interim synthetic bars
        for (int i = 0; i < length; i++)
        {
            IReusable s = source[i];

            WmaResult w1 = wmaN1[i];
            WmaResult w2 = wmaN2[i];

            if (i < shiftQty)
            {
                continue;
            }

            TimeValue sh = new(
                s.Timestamp,
                (w2.Wma.Null2NaN() * 2d) - w1.Wma.Null2NaN());

            synthHistory.Add(sh);
        }

        // add back truncated null results
        int sqN = (int)Math.Sqrt(lookbackPeriods);

        List<HmaResult> results = source
            .Take(shiftQty)
            .Select(static x => new HmaResult(x.Timestamp))
            .ToList();

        // calculate final HMA = WMA with period SQRT(n)
        List<HmaResult> hmaResults = synthHistory.ToWma(sqN)
            .Select(static x => new HmaResult(
                Timestamp: x.Timestamp,
                Hma: x.Wma
             ))
            .ToList();

        // add WMA to results
        results.AddRange(hmaResults);

        return results.ToSortedList();
    }
}
