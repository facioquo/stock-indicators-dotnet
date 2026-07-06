namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Provides a common interface for Stochastic Momentum Index (SMI) implementations.
/// </summary>
public interface ISmi
{
    /// <summary>
    /// Gets the number of periods for the lookback window.
    /// </summary>
    int LookbackPeriods { get; init; }

    /// <summary>
    /// Gets the number of periods for the first smoothing.
    /// </summary>
    int FirstSmoothPeriods { get; init; }

    /// <summary>
    /// Gets the number of periods for the second smoothing.
    /// </summary>
    int SecondSmoothPeriods { get; init; }

    /// <summary>
    /// Gets the number of periods for the signal line smoothing.
    /// </summary>
    int SignalPeriods { get; init; }
}
