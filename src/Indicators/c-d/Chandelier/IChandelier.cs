namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Interface for Chandelier Exit calculations.
/// </summary>
public interface IChandelier
{
    /// <summary>
    /// Gets the number of lookback periods.
    /// </summary>
    int LookbackPeriods { get; }

    /// <summary>
    /// Gets the ATR multiplier.
    /// </summary>
    double Multiplier { get; }

    /// <summary>
    /// Gets the direction type (Long or Short).
    /// </summary>
    Direction Type { get; }
}
