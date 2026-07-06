namespace FacioQuo.Stock.Indicators;

/// <summary>
/// VWAP (Volume Weighted Average Price) indicator.
/// </summary>
public static partial class Vwap
{
    /// <summary>
    /// Calculates the VWAP for a series of bars starting from a specific date.
    /// </summary>
    /// <param name="bars">Aggregate OHLCV price bars, time sorted.</param>
    /// <param name="startDate">Start date for the VWAP calculation.</param>
    /// <returns>A list of VwapResult containing the VWAP values.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="bars"/> is null.</exception>
    public static IReadOnlyList<VwapResult> ToVwap(
        this IReadOnlyList<IBar> bars,
        DateTime startDate)
        => bars
            .ToBarDList()
            .CalcVwap(startDate);

    /// <summary>
    /// Calculates the VWAP for a series of bars.
    /// </summary>
    /// <param name="bars">Aggregate OHLCV price bars, time sorted.</param>
    /// <returns>A list of VwapResult containing the VWAP values.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="bars"/> is null.</exception>
    public static IReadOnlyList<VwapResult> ToVwap(
        this IReadOnlyList<IBar> bars)
        => bars?.Count is null or 0
            ? []
            : bars
                .ToBarDList()
                .CalcVwap(bars[0].Timestamp);

    /// <summary>
    /// Calculates the VWAP for a series of bars.
    /// </summary>
    /// <param name="bars">Source list of bars.</param>
    /// <param name="startDate">Optional start date for the VWAP calculation. If not provided, the calculation starts from the first bar.</param>
    /// <returns>A list of VwapResult containing the VWAP values.</returns>
    private static List<VwapResult> CalcVwap(
        this List<BarD> bars,
        DateTime startDate)
    {
        // If caller passed default(DateTime) treat as unspecified and start at first bar
        if (startDate == default && bars.Count > 0)
        {
            startDate = bars[0].Timestamp;
        }

        // check parameter arguments
        Validate(bars, startDate);

        // initialize
        int length = bars.Count;
        List<VwapResult> results = new(length);

        if (length == 0)
        {
            return results;
        }

        double? cumVolume = 0;
        double? cumVolumeTp = 0;

        // roll through source values
        for (int i = 0; i < length; i++)
        {
            BarD q = bars[i];

            double? v = q.Volume;
            double? h = q.High;
            double? l = q.Low;
            double? c = q.Close;

            double? vwap;

            if (q.Timestamp >= startDate)
            {
                cumVolume += v;
                cumVolumeTp += v * (h + l + c) / 3;

                vwap = cumVolume != 0 ? cumVolumeTp / cumVolume : null;
            }
            else
            {
                vwap = null;
            }

            results.Add(new(
                Timestamp: q.Timestamp,
                Vwap: vwap));
        }

        return results;
    }
}
