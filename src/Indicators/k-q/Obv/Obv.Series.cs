namespace FacioQuo.Stock.Indicators;

/// <summary>
/// On-Balance Volume (OBV) indicator.
/// </summary>
public static partial class Obv
{
    /// <summary>
    /// Converts a list of bars to OBV results.
    /// </summary>
    /// <param name="bars">Aggregate OHLCV price bars, time sorted.</param>
    /// <returns>A list of OBV results.</returns>
    public static IReadOnlyList<ObvResult> ToObv(
        this IReadOnlyList<IBar> bars)
        => bars
            .ToBarDList()
            .CalcObv();

    /// <summary>
    /// Calculates the OBV for a list of bars.
    /// </summary>
    /// <param name="bars">Source list of bars.</param>
    /// <returns>A list of OBV results.</returns>
    private static List<ObvResult> CalcObv(
        this List<BarD> bars)
    {
        // initialize
        int length = bars.Count;
        List<ObvResult> results = new(length);

        double prevClose = double.NaN;

        // roll through source values
        for (int i = 0; i < length; i++)
        {
            BarD q = bars[i];

            ObvResult r = Increment(
                q.Timestamp,
                q.Close,
                q.Volume,
                prevClose,
                i > 0 ? results[i - 1].Obv : 0);

            results.Add(r);
            prevClose = q.Close;
        }

        return results;
    }
}
