namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Interface for Smoothed Moving Average (SMMA) hub.
/// </summary>
public interface ISmma
{
    /// <summary>
    /// Gets the number of lookback periods.
    /// </summary>
    int LookbackPeriods { get; }
}
