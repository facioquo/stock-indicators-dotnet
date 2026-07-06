namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Interface for Average True Range (ATR) Hub.
/// </summary>
public interface IAtr
{
    /// <summary>
    /// Gets the lookback periods for ATR calculation.
    /// </summary>
    int LookbackPeriods { get; }
}
