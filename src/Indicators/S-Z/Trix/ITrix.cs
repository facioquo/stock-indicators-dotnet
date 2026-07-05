namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Interface for Triple Exponential Moving Average Oscillator (TRIX) calculations.
/// </summary>
public interface ITrix
{
    /// <summary>
    /// Gets the number of periods to look back for the calculation.
    /// </summary>
    int LookbackPeriods { get; }

    /// <summary>
    /// Gets the smoothing factor for the calculation.
    /// </summary>
    double K { get; }
}
