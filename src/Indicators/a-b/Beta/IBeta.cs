namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Interface for Beta coefficient calculations.
/// </summary>
public interface IBeta
{
    /// <summary>
    /// Gets the number of periods to look back for the calculation.
    /// </summary>
    int LookbackPeriods { get; }

    /// <summary>
    /// Gets the type of Beta calculation.
    /// </summary>
    BetaType Type { get; }
}
