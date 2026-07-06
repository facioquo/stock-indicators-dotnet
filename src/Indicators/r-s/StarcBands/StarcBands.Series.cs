namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Provides methods for STARC Band indicator calculations.
/// </summary>
public static partial class StarcBands
{
    /// <summary>
    /// Converts a series of bars to STARC Bands.
    /// </summary>
    /// <param name="bars">Source series of bars.</param>
    /// <param name="smaPeriods">Number of periods for the Simple Moving Average (SMA).</param>
    /// <param name="multiplier">Multiplier for the Average True Range (ATR).</param>
    /// <param name="atrPeriods">Number of periods for the ATR calculation.</param>
    /// <returns>A list of <see cref="StarcBandsResult"/> containing the STARC Bands values.</returns>
    public static IReadOnlyList<StarcBandsResult> ToStarcBands(
        this IReadOnlyList<IBar> bars,
        int smaPeriods = 5,
        double multiplier = 2,
        int atrPeriods = 10)
        => bars
            .ToBarDList()
            .CalcStarcBands(smaPeriods, multiplier, atrPeriods);

    /// <summary>
    /// Calculates the STARC Bands for a series of bars.
    /// </summary>
    /// <param name="bars">Source list of bars.</param>
    /// <param name="smaPeriods">Number of periods for the Simple Moving Average (SMA).</param>
    /// <param name="multiplier">Multiplier for the Average True Range (ATR).</param>
    /// <param name="atrPeriods">Number of periods for the ATR calculation.</param>
    /// <returns>A list of <see cref="StarcBandsResult"/> containing the STARC Bands values.</returns>
    private static List<StarcBandsResult> CalcStarcBands(
        this List<BarD> bars,
        int smaPeriods,
        double multiplier,
        int atrPeriods)
    {
        // check parameter arguments
        Validate(smaPeriods, multiplier, atrPeriods);

        // initialize
        int length = bars.Count;
        List<StarcBandsResult> results = new(length);
        List<AtrResult> atrResults = bars.CalcAtr(atrPeriods);
        IReadOnlyList<SmaResult> smaResults = bars.ToSma(smaPeriods);

        // roll through source values
        for (int i = 0; i < length; i++)
        {
            SmaResult s = smaResults[i];
            AtrResult a = atrResults[i];

            results.Add(new(
                Timestamp: s.Timestamp,
                UpperBand: s.Sma + (multiplier * a.Atr),
                Centerline: s.Sma,
                LowerBand: s.Sma - (multiplier * a.Atr)));
        }

        return results;
    }
}
