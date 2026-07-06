namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Simple Moving Average (SMA) with extended analysis indicator.
/// </summary>
public static partial class SmaAnalysis
{
    /// <summary>
    /// Converts a source list to a list of SMA analysis results.
    /// </summary>
    /// <param name="source">Source list to analyze.</param>
    /// <param name="lookbackPeriods">Number of periods to look back for the SMA calculation.</param>
    /// <returns>A read-only list of SMA analysis results.</returns>
    public static IReadOnlyList<SmaAnalysisResult> ToSmaAnalysis(
        this IReadOnlyList<IReusable> source,
        int lookbackPeriods)
    {
        // check parameter arguments
        ArgumentNullException.ThrowIfNull(source);
        Sma.Validate(lookbackPeriods);

        // initialize
        int length = source.Count;
        List<SmaAnalysisResult> results = new(length);
        Queue<double> buffer = new(lookbackPeriods);

        // roll through source values
        for (int i = 0; i < length; i++)
        {
            IReusable s = source[i];

            // advance the rolling window and emit once it is full
            buffer.Update(lookbackPeriods, s.Value);

            double sma = buffer.Average(lookbackPeriods);

            if (double.IsNaN(sma))
            {
                results.Add(new SmaAnalysisResult(Timestamp: s.Timestamp));
                continue;
            }

            // analysis metrics over the same rolling window of raw values
            double sumMad = 0;
            double sumMse = 0;
            double sumMape = 0;

            foreach (double val in buffer)
            {
                double diff = val - sma;
                sumMad += Math.Abs(diff);
                sumMse += diff * diff;
                sumMape += val == 0 ? double.NaN : Math.Abs(diff) / val;
            }

            results.Add(new SmaAnalysisResult(
                Timestamp: s.Timestamp,
                Sma: sma.NaN2Null(),
                Mad: (sumMad / lookbackPeriods).NaN2Null(),
                Mse: (sumMse / lookbackPeriods).NaN2Null(),
                Mape: (sumMape / lookbackPeriods).NaN2Null()));
        }

        return results;
    }
}
