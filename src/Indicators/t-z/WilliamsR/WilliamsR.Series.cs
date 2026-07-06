namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Williams %R indicator.
/// </summary>
public static partial class WilliamsR
{
    /// <summary>
    /// Calculates the Williams %R for a series of bars.
    /// </summary>
    /// <param name="bars">Aggregate OHLCV price bars, time sorted.</param>
    /// <param name="lookbackPeriods">Quantity of periods in lookback window.</param>
    /// <returns>A list of WilliamsResult containing the Williams %R values.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="bars"/> is null.</exception>
    public static IReadOnlyList<WilliamsResult> ToWilliamsR(
        this IReadOnlyList<IBar> bars,
        int lookbackPeriods = 14)
        => bars
            .ToBarDList()
            .CalcWilliamsR(lookbackPeriods);

    /// <summary>
    /// Calculates the Williams %R for a series of bars.
    /// </summary>
    /// <param name="bars">Source list of bars.</param>
    /// <param name="lookbackPeriods">Quantity of periods in lookback window.</param>
    /// <returns>A list of WilliamsResult containing the Williams %R values.</returns>
    private static List<WilliamsResult> CalcWilliamsR(
        this List<BarD> bars,
        int lookbackPeriods)
    {
        // check parameter arguments
        Validate(lookbackPeriods);

        // convert Fast Stochastic to William %R
        return bars.CalcStoch(lookbackPeriods, 1, 1, 3, 2, MaType.SMA)
            .ConvertAll(static s => new WilliamsResult(
                Timestamp: s.Timestamp,
                WilliamsR: s.Oscillator - 100
             ))
;
    }

    /// <summary>
    /// Creates a buffer list for Williams %R calculations.
    /// </summary>
    /// <param name="bars">Aggregate OHLCV price bars, time sorted.</param>
    /// <param name="lookbackPeriods">Quantity of periods in lookback window.</param>
    /// <returns>A WilliamsRList instance.</returns>
    public static WilliamsRList ToWilliamsRList(
        this IReadOnlyList<IBar> bars,
        int lookbackPeriods = 14)
        => new(lookbackPeriods) { bars };
}
