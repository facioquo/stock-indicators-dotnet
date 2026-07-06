namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Chaikin Money Flow (CMF) on a series of bars indicator.
/// </summary>
public static partial class Cmf
{
    /// <summary>
    /// Calculates the Chaikin Money Flow (CMF) for a series of bars.
    /// </summary>
    /// <param name="bars">Aggregate OHLCV price bars, time sorted.</param>
    /// <param name="lookbackPeriods">Quantity of periods in lookback window.</param>
    /// <returns>A read-only list of <see cref="CmfResult"/> containing the CMF calculation results.</returns>
    public static IReadOnlyList<CmfResult> ToCmf(
        this IReadOnlyList<IBar> bars,
        int lookbackPeriods = 20)
        => bars
            .ToSortedList()
            .CalcCmf(lookbackPeriods);

    /// <summary>
    /// Calculates the Chaikin Money Flow (CMF) for a series of bars.
    /// </summary>
    /// <param name="source">Source list of bars.</param>
    /// <param name="lookbackPeriods">Quantity of periods in lookback window.</param>
    /// <returns>A list of <see cref="CmfResult"/> containing the CMF calculation results.</returns>
    private static List<CmfResult> CalcCmf(
        this IReadOnlyList<IBar> source,
        int lookbackPeriods)
    {
        // get volume array
        double[] volume
            = source.Select(static v => (double)v.Volume).ToArray();

        // check parameter arguments
        Validate(lookbackPeriods);

        // initialize
        int length = volume.Length;
        List<CmfResult> results = new(length);
        IReadOnlyList<AdlResult> adlResults = source.ToAdl();

        // roll through source values
        for (int i = 0; i < length; i++)
        {
            AdlResult adl = adlResults[i];
            double? cmf = null;

            if (i >= lookbackPeriods - 1)
            {
                double? sumMfv = 0;
                double? sumVol = 0;

                for (int p = i + 1 - lookbackPeriods; p <= i; p++)
                {
                    sumVol += volume[p];

                    AdlResult d = adlResults[p];
                    sumMfv += d.MoneyFlowVolume;
                }

                double? avgMfv = sumMfv / lookbackPeriods;
                double? avgVol = sumVol / lookbackPeriods;

                if (avgVol != 0)
                {
                    cmf = avgMfv / avgVol;
                }
            }

            results.Add(new(
                Timestamp: adl.Timestamp,
                MoneyFlowMultiplier: adl.MoneyFlowMultiplier,
                MoneyFlowVolume: adl.MoneyFlowVolume,
                Cmf: cmf));
        }

        return results;
    }
}
