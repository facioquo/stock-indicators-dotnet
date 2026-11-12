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
    /// <exception cref="ArgumentOutOfRangeException">The ATR periods are less than or equal to 1.</exception>
    public static IReadOnlyList<RenkoResult> ToRenkoAtr(
        this IReadOnlyList<IBar> bars,
        int atrPeriods = 14,
        EndType endType = EndType.Close)
    {
        // checked here so the error names this method's parameter, not ATR's
        if (atrPeriods <= 1)
        {
            throw new ArgumentOutOfRangeException(nameof(atrPeriods), atrPeriods,
                "ATR periods must be greater than 1 for Renko ATR.");
        }

        // initialize
        List<AtrResult> atrResults = bars
            .ToBarDList()
            .CalcAtr(atrPeriods);

        AtrResult? last = atrResults.Count > 0 ? atrResults[^1] : null;
        decimal brickSize = DeMath.ToDecimal(last?.Atr) ?? 0;

        return brickSize == 0
          ? []
          : bars.ToRenko(brickSize, endType);
    }
}
