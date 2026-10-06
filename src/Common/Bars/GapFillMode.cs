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
    None = 0,

    /// <summary>
    /// Silent buckets are filled with zero-volume synthetic bars whose
    /// O/H/L/C all carry forward the prior bar's close.
    /// </summary>
    ForwardFill = 1,

    /// <summary>
    /// Silent buckets are filled with zero-volume synthetic bars whose
    /// O/H/L/C are linearly interpolated between the prior bar's close and
    /// the next real bar's open (or price, for tick input), evenly spaced
    /// across the missing buckets. Interpolated prices are not rounded and
    /// may carry full <see cref="decimal"/> precision. For ticks the end
    /// anchor is the first tick, by time, in the bucket after the gap. Each
    /// synthesized bar uses a later input, so its value is not knowable at
    /// its own timestamp and it is emitted only once that input arrives.
    /// </summary>
    Interpolate = 2
}
