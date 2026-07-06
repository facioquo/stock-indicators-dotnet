namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Interface for SuperTrend Hub.
/// </summary>
public interface ISuperTrend
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
