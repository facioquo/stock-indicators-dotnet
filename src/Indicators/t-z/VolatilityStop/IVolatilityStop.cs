namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Interface for Volatility Stop Hub.
/// </summary>
public interface IVolatilityStop
{
    /// <summary>
    /// Gets the number of periods to look back.
    /// </summary>
    int LookbackPeriods { get; }

    /// <summary>
    /// Gets the multiplier for the ATR.
    /// </summary>
    double Multiplier { get; }
}
