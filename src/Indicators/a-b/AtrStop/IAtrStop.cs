namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Interface for ATR Stop Hub.
/// </summary>
public interface IAtrStop
{
    /// <summary>
    /// Gets the number of periods to look back.
    /// </summary>
    int LookbackPeriods { get; }

    /// <summary>
    /// Gets the multiplier for the ATR.
    /// </summary>
    double Multiplier { get; }

    /// <summary>
    /// Gets the type of price to use for the calculation.
    /// </summary>
    EndType EndType { get; }
}
