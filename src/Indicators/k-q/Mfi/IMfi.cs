namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Interface for Money Flow Index (MFI) calculations.
/// </summary>
public interface IMfi
{
    /// <summary>
    /// Gets the number of lookback periods.
    /// </summary>
    int LookbackPeriods { get; }
}
