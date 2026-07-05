namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Interface for T3 Moving Average calculations.
/// </summary>
public interface IT3
{
    /// <summary>
    /// Gets the number of periods to look back for the calculation.
    /// </summary>
    int LookbackPeriods { get; }

    /// <summary>
    /// Gets the volume factor for the calculation.
    /// </summary>
    double VolumeFactor { get; }

    /// <summary>
    /// Gets the smoothing factor for the calculation.
    /// </summary>
    double K { get; }

    /// <summary>
    /// Gets the first coefficient for the calculation.
    /// </summary>
    double C1 { get; }

    /// <summary>
    /// Gets the second coefficient for the calculation.
    /// </summary>
    double C2 { get; }

    /// <summary>
    /// Gets the third coefficient for the calculation.
    /// </summary>
    double C3 { get; }

    /// <summary>
    /// Gets the fourth coefficient for the calculation.
    /// </summary>
    double C4 { get; }
}
