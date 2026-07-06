namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Interface for Ultimate Oscillator Hub.
/// </summary>
public interface IUltimate
{
    /// <summary>
    /// Gets the short period for Ultimate Oscillator calculation.
    /// </summary>
    int ShortPeriods { get; }

    /// <summary>
    /// Gets the middle period for Ultimate Oscillator calculation.
    /// </summary>
    int MiddlePeriods { get; }

    /// <summary>
    /// Gets the long period for Ultimate Oscillator calculation.
    /// </summary>
    int LongPeriods { get; }
}
