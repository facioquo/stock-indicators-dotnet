namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Interface for Chaikin Oscillator calculations.
/// </summary>
public interface IChaikinOsc
{
    /// <summary>
    /// Gets the number of fast EMA periods.
    /// </summary>
    int FastPeriods { get; }

    /// <summary>
    /// Gets the number of slow EMA periods.
    /// </summary>
    int SlowPeriods { get; }
}
