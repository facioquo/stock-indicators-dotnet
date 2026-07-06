namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Interface for Rate of Change (ROC) hub.
/// </summary>
public interface IRoc
{
    /// <summary>
    /// Gets the number of lookback periods.
    /// </summary>
    int LookbackPeriods { get; }
}
