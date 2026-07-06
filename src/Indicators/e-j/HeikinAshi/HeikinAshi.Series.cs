namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Heikin-Ashi indicator.
/// </summary>
public static partial class HeikinAshi
{
    /// <summary>
    /// Converts a list of bars to Heikin-Ashi results.
    /// </summary>
    /// <param name="bars">Aggregate OHLCV price bars, time sorted.</param>
    /// <returns>A list of Heikin-Ashi results.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the bars list is null.</exception>
    public static IReadOnlyList<HeikinAshiResult> ToHeikinAshi(
        this IReadOnlyList<IBar> bars)
    {
        ArgumentNullException.ThrowIfNull(bars);

        // initialize
        int length = bars.Count;
        List<HeikinAshiResult> results = new(length);

        decimal prevOpen = decimal.MinValue;
        decimal prevClose = decimal.MinValue;

        if (length > 0)
        {
            IBar q = bars[0];
            prevOpen = q.Open;
            prevClose = q.Close;
        }

        // roll through source values
        for (int i = 0; i < length; i++)
        {
            IBar q = bars[i];

            // close
            decimal close = (q.Open + q.High + q.Low + q.Close) / 4;

            // open
            decimal open = (prevOpen + prevClose) / 2;

            // high
            decimal[] arrH = [q.High, open, close];
            decimal high = arrH.Max();

            // low
            decimal[] arrL = [q.Low, open, close];
            decimal low = arrL.Min();

            results.Add(new HeikinAshiResult(
                Timestamp: q.Timestamp,
                Open: open,
                High: high,
                Low: low,
                Close: close,
                Volume: q.Volume));

            // save for next iteration
            prevOpen = open;
            prevClose = close;
        }

        return results;
    }
}
