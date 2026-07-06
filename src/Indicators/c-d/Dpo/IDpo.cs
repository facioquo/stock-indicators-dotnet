namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Defines the contract for Detrended Price Oscillator (DPO) indicators.
/// </summary>
public interface IDpo
{
    /// <summary>
    /// Gets the number of periods to look back for the calculation.
    /// </summary>
    int LookbackPeriods { get; }
}
