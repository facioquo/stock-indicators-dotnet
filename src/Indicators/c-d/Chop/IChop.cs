namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Interface for Choppiness Index (CHOP) calculations.
/// </summary>
public interface IChop
{
    /// <summary>
    /// Gets the number of lookback periods.
    /// </summary>
    int LookbackPeriods { get; }
}
