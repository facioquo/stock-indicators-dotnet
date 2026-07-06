namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Provides methods for generating Renko chart series using ATR (Average True Range).
/// </summary>
public static partial class RenkoAtr
{
    /// <summary>
    /// Converts a list of bars to a list of Renko chart results using ATR for brick size.
    /// </summary>
    /// <param name="bars">Aggregate OHLCV price bars, time sorted.</param>
    /// <param name="atrPeriods">Number of periods for calculating ATR.</param>
    /// <param name="endType">Price candle end type to use as the brick threshold.</param>
    /// <returns>A list of Renko chart results.</returns>
    public static IReadOnlyList<RenkoResult> ToRenkoAtr(
        this IReadOnlyList<IBar> bars,
        int atrPeriods = 14,
        EndType endType = EndType.Close)
    {
        // initialize
        List<AtrResult> atrResults = bars
            .ToBarDList()
            .CalcAtr(atrPeriods);

        AtrResult? last = atrResults.Count > 0 ? atrResults[^1] : null;
        decimal brickSize = (decimal?)last?.Atr ?? 0;

        return brickSize == 0
          ? []
          : bars.ToRenko(brickSize, endType);
    }
}
