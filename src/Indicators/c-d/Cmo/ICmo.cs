namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Interface for Chande Momentum Oscillator (CMO) calculations.
/// </summary>
public interface ICmo
{
    /// <summary>
    /// Gets the number of periods to look back for the calculation.
    /// </summary>
    int LookbackPeriods { get; }
}
