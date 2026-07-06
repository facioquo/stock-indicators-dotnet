namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Interface for Rate of Change with Bands (RocWb) indicator
/// </summary>
public interface IRocWb
{
    /// <summary>
    /// Gets the number of lookback periods.
    /// </summary>
    int LookbackPeriods { get; }

    /// <summary>
    /// Gets the number of EMA periods.
    /// </summary>
    int EmaPeriods { get; }

    /// <summary>
    /// Gets the number of standard deviation periods.
    /// </summary>
    int StdDevPeriods { get; }
}
