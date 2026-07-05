namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Interface for Williams Fractal calculations.
/// </summary>
public interface IFractal
{
    /// <summary>
    /// Gets the number of periods to look back for the calculation.
    /// </summary>
    int LeftSpan { get; }

    /// <summary>
    /// Gets the number of periods to look forward for the calculation.
    /// </summary>
    int RightSpan { get; }

    /// <summary>
    /// Gets the type of price to use for the calculation.
    /// </summary>
    EndType EndType { get; }
}
