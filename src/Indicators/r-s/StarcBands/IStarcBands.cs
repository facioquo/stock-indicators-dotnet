namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Provides a common interface for STARC Bands implementations.
/// </summary>
public interface IStarcBands
{
    /// <summary>
    /// Gets the number of periods for the Simple Moving Average (SMA).
    /// </summary>
    int SmaPeriods { get; init; }

    /// <summary>
    /// Gets the multiplier for the Average True Range (ATR).
    /// </summary>
    double Multiplier { get; init; }

    /// <summary>
    /// Gets the number of periods for the ATR calculation.
    /// </summary>
    int AtrPeriods { get; init; }
}
