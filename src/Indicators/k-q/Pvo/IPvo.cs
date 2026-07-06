namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Interface for Percentage Volume Oscillator (PVO) calculations.
/// </summary>
public interface IPvo
{
    /// <summary>
    /// Gets the number of periods for the fast EMA.
    /// </summary>
    int FastPeriods { get; }

    /// <summary>
    /// Gets the number of periods for the slow EMA.
    /// </summary>
    int SlowPeriods { get; }

    /// <summary>
    /// Gets the number of periods for the signal line.
    /// </summary>
    int SignalPeriods { get; }
}
