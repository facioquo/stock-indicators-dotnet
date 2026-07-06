namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Interface for Fractal Chaos Bands (FCB) calculations.
/// </summary>
public interface IFcb
{
    /// <summary>
    /// Gets the window span for the calculation.
    /// </summary>
    int WindowSpan { get; }
}
