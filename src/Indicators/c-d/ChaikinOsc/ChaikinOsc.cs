namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Chaikin Oscillator on a series of bars indicator.
/// </summary>
public static partial class ChaikinOsc
{
    /// <summary>
    /// Calculates the Chaikin Oscillator for a series of bars.
    /// </summary>
    /// <param name="bars">Aggregate OHLCV price bars, time sorted.</param>
    /// <param name="fastPeriods">Number of periods to use for the fast EMA. Default is 3.</param>
    /// <param name="slowPeriods">Number of periods to use for the slow EMA. Default is 10.</param>
    /// <returns>A read-only list of <see cref="ChaikinOscResult"/> containing the Chaikin Oscillator calculation results.</returns>
    public static IReadOnlyList<ChaikinOscResult> ToChaikinOsc(
        this IReadOnlyList<IBar> bars,
        int fastPeriods = 3,
        int slowPeriods = 10)
    {
        // check parameter arguments
        ArgumentNullException.ThrowIfNull(bars);
        Validate(fastPeriods, slowPeriods);

        // initialize
        int length = bars.Count;
        List<ChaikinOscResult> results = new(length);

        // money flow
        IReadOnlyList<AdlResult> adlResults = bars.ToAdl();

        // fast/slow EMA of ADL
        IReadOnlyList<EmaResult> adlEmaSlow = adlResults.ToEma(slowPeriods);
        IReadOnlyList<EmaResult> adlEmaFast = adlResults.ToEma(fastPeriods);

        // roll through source values
        for (int i = 0; i < length; i++)
        {
            AdlResult a = adlResults[i];
            EmaResult f = adlEmaFast[i];
            EmaResult s = adlEmaSlow[i];

            results.Add(new(
                 Timestamp: a.Timestamp,
                 MoneyFlowMultiplier: a.MoneyFlowMultiplier,
                 MoneyFlowVolume: a.MoneyFlowVolume,
                 Adl: a.Adl,
                 Oscillator: f.Ema - s.Ema,
                 FastEma: f.Ema,
                 SlowEma: s.Ema
             ));
        }

        return results;
    }
}
