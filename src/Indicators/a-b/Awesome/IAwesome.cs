namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Interface for Awesome Oscillator streaming and buffered list.
/// </summary>
public interface IAwesome
{
    /// <summary>
    /// Gets the number of periods for the fast moving average.
    /// </summary>
    int FastPeriods { get; }

    /// <summary>
    /// Gets the number of periods for the slow moving average.
    /// </summary>
    int SlowPeriods { get; }
}
