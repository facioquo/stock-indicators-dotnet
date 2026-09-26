namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Specifies how a bar aggregation hub (<see cref="BarAggregatorHub"/> or
/// <see cref="TradeTickAggregatorHub"/>) handles a silent bucket — one with
/// no upstream input between the last emitted bar and the next active bucket.
/// </summary>
public enum GapFillMode
{
    /// <summary>
    /// Silent buckets are omitted from the output stream. This is the
    /// default behavior.
    /// </summary>
    None,

    /// <summary>
    /// Silent buckets are filled with zero-volume synthetic bars whose
    /// O/H/L/C all carry forward the prior bar's close.
    /// </summary>
    ForwardFill,

    /// <summary>
    /// Silent buckets are filled with zero-volume synthetic bars whose
    /// O/H/L/C are linearly interpolated between the prior bar's close and
    /// the next real bar's open (or price, for tick input), evenly spaced
    /// across the missing buckets. Interpolated prices are not rounded and
    /// may carry full <see cref="decimal"/> precision.
    /// </summary>
    Interpolate
}
