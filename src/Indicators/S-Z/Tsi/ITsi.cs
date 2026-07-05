namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Interface for True Strength Index (TSI) calculations.
/// </summary>
public interface ITsi
{
    /// <summary>
    /// Gets the number of periods for the lookback calculation.
    /// </summary>
    int LookbackPeriods { get; }

    /// <summary>
    /// Gets the number of periods for the smoothing calculation.
    /// </summary>
    int SmoothPeriods { get; }

    /// <summary>
    /// Gets the number of periods for the signal calculation.
    /// </summary>
    int SignalPeriods { get; }
}
