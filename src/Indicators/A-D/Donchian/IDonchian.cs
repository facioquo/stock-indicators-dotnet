namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Interface for Donchian Channels calculations.
/// </summary>
public interface IDonchian
{
    /// <summary>
    /// Gets the number of lookback periods.
    /// </summary>
    int LookbackPeriods { get; }
}
