namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Provides utility methods for the VWAP (Volume Weighted Average Price) indicator.
/// </summary>
public static partial class Vwap
{
    // parameter validation
    /// <summary>
    /// Validates the parameters for the VWAP calculation.
    /// </summary>
    /// <param name="bars">Aggregate OHLCV price bars, time sorted.</param>
    /// <param name="startDate">Optional start date for the VWAP calculation.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when the start date is earlier than the first bar's timestamp.
    /// </exception>
    internal static void Validate(
        IReadOnlyList<BarD> bars,
        DateTime? startDate)
    {
        // nothing to do for 0 length
        if (bars.Count == 0)
        {
            return;
        }

        // check parameter arguments (intentionally after bars check)
        if (startDate < bars[0].Timestamp)
        {
            throw new ArgumentOutOfRangeException(nameof(startDate), startDate,
                "Start Timestamp must be within the bars range for VWAP.");
        }
    }
}
