namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Interface for Chaikin Money Flow (CMF) calculations.
/// </summary>
public interface ICmf
{
    /// <summary>
    /// Gets the number of lookback periods.
    /// </summary>
    int LookbackPeriods { get; }
}
