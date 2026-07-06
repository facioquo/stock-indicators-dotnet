namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Interface for Price Momentum Oscillator (PMO) calculations.
/// </summary>
public interface IPmo
{
    /// <summary>
    /// Gets the number of periods for the time span.
    /// </summary>
    int TimePeriods { get; }

    /// <summary>
    /// Gets the number of periods for smoothing.
    /// </summary>
    int SmoothPeriods { get; }

    /// <summary>
    /// Gets the number of periods for the signal line.
    /// </summary>
    int SignalPeriods { get; }
}
