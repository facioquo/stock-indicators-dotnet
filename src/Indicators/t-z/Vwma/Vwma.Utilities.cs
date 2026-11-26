namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Provides utility methods for the VWMA (Volume Weighted Moving Average) indicator.
/// </summary>
public static partial class Vwma
{
    // parameter validation
    /// <summary>
    /// Validates the parameters for the VWMA calculation.
    /// </summary>
    /// <param name="lookbackPeriods">Quantity of periods in lookback window.</param>
    /// <param name="candlePart">Price component to weight by volume.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when the lookback periods are less than or equal to 0.
    /// </exception>
    internal static void Validate(
        int lookbackPeriods, CandlePart candlePart)
    {
        BarParts.Validate(candlePart);
        if (candlePart == CandlePart.Volume)
        {
            throw new ArgumentOutOfRangeException(nameof(candlePart), candlePart,
                "VWMA requires a price component.");
        }

        if (lookbackPeriods <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(lookbackPeriods), lookbackPeriods,
                "Lookback periods must be greater than 0 for Vwma.");
        }
    }
}
