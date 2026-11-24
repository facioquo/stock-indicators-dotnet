namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Interface for Price Relative Strength (PRS) streaming and buffered list.
/// </summary>
public interface IPrs
{
    /// <summary>
    /// Gets the number of periods for the PRS% lookback calculation.
    /// </summary>
    /// <remarks>0 when no <see cref="PrsResult.PrsPercent"/> is calculated.</remarks>
    int LookbackPeriods { get; }
}
