namespace FacioQuo.Stock.Indicators;

/// <summary>
/// True Range (TR) from a list of bars indicator.
/// </summary>
public static partial class Tr
{
    /// <summary>
    /// Converts a list of bars to a list of True Range (TR) results.
    /// </summary>
    /// <param name="bars">Aggregate OHLCV price bars, time sorted.</param>
    /// <returns>A list of True Range (TR) results.</returns>
    public static IReadOnlyList<TrResult> ToTr(
    this IReadOnlyList<IBar> bars)
    => bars
        .ToBarDList()
        .CalcTr();

    /// <summary>
    /// Calculates the True Range (TR) for a list of bars.
    /// </summary>
    /// <param name="bars">Source list of bars.</param>
    /// <returns>A list of True Range (TR) results.</returns>
    private static List<TrResult> CalcTr(
        this List<BarD> bars)
    {
        // initialize
        int length = bars.Count;
        TrResult[] results = new TrResult[length];

        // skip first period
        if (length > 0)
        {
            results[0] = new TrResult(bars[0].Timestamp, null);
        }

        // roll through source values
        for (int i = 1; i < length; i++)
        {
            BarD q = bars[i];

            results[i] = new TrResult(
                Timestamp: q.Timestamp,
                Tr: Increment(q.High, q.Low, bars[i - 1].Close));
        }

        return [.. results];
    }
}
