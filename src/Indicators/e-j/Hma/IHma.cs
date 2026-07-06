namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Interface for Hull Moving Average (HMA) calculations.
/// </summary>
public interface IHma
{
    /// <summary>
    /// Gets the number of periods to look back for the calculation.
    /// </summary>
    int LookbackPeriods { get; }
}
