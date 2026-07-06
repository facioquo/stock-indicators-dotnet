namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Interface for Keltner Channel hub.
/// </summary>
public interface IKeltner
{
    /// <summary>
    /// Gets the number of periods for the EMA.
    /// </summary>
    int EmaPeriods { get; }

    /// <summary>
    /// Gets the multiplier for the ATR.
    /// </summary>
    double Multiplier { get; }

    /// <summary>
    /// Gets the number of periods for the ATR.
    /// </summary>
    int AtrPeriods { get; }
}
