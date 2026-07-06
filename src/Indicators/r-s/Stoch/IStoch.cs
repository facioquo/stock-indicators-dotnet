namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Interface for Stochastic Oscillator calculations.
/// </summary>
public interface IStoch
{
    /// <summary>
    /// Gets the number of periods to look back for the oscillator calculation.
    /// </summary>
    int LookbackPeriods { get; }

    /// <summary>
    /// Gets the number of periods for the signal line.
    /// </summary>
    int SignalPeriods { get; }

    /// <summary>
    /// Gets the number of periods for smoothing the oscillator.
    /// </summary>
    int SmoothPeriods { get; }

    /// <summary>
    /// Gets the K factor for the Stochastic calculation.
    /// </summary>
    double KFactor { get; }

    /// <summary>
    /// Gets the D factor for the Stochastic calculation.
    /// </summary>
    double DFactor { get; }

    /// <summary>
    /// Gets the type of moving average used for calculations.
    /// </summary>
    MaType MovingAverageType { get; }
}
