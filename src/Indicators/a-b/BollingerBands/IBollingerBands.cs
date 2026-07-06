namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Interface for Bollinger Bands hub.
/// </summary>
public interface IBollingerBands
{
    /// <summary>
    /// Gets the number of lookback periods.
    /// </summary>
    int LookbackPeriods { get; }

    /// <summary>
    /// Gets the number of standard deviations.
    /// </summary>
    double StandardDeviations { get; }
}
