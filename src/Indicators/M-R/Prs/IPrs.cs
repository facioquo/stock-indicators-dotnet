namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Interface for Price Relative Strength (PRS) streaming and buffered list.
/// </summary>
public interface IPrs
{
    /// <summary>
    /// Gets the number of periods for the PRS% lookback calculation.
    /// </summary>
    int LookbackPeriods { get; }
}
