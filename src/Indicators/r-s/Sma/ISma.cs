namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Interface for Simple Moving Average (SMA) hub.
/// </summary>
public interface ISma
{
    /// <summary>
    /// Gets the number of lookback periods.
    /// </summary>
    int LookbackPeriods { get; }
}
