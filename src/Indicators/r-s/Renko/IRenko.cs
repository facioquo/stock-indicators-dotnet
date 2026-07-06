namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Streaming hub for generating Renko chart results.
/// </summary>
public interface IRenko
{
    /// <summary>
    /// Gets the size of each Renko brick.
    /// </summary>
    decimal BrickSize { get; }

    /// <summary>
    /// Gets the price candle end type used to determine when threshold
    /// is met to generate new bricks.
    /// </summary>
    EndType EndType { get; }
}
