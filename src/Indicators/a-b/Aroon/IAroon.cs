namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Interface for Aroon Oscillator streaming and buffered list.
/// </summary>
public interface IAroon
{
    /// <summary>
    /// Gets the number of periods to look back for the calculation.
    /// </summary>
    int LookbackPeriods { get; }
}
