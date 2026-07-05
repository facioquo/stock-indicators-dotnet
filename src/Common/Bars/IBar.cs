namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Bar interface for standard OHLCV aggregate period.
/// This is commonly known as a "bar" or "candle" and represents
/// an asset price range over a specific time range,
/// <para>
/// If implementing your own custom <c>TBar:IBar</c> type:
/// </para>
/// <para>
/// For chaining compatibility (<see cref="IReusable"/>
/// compliance), add the following <c>TBar</c> property
/// (pointer) to your <see cref="Close"/> price.
/// <code>
///    double IReusable.Value => (double)Close;
/// </code>
/// </para>
/// <para>
/// TIP: If you do not need a custom bar type,
/// use the built-in <see cref="Bar"/>.
/// </para>
/// </summary>
public interface IBar : IReusable
{
    /// <summary>
    /// Aggregate bar's first tick price
    /// </summary>
    decimal Open { get; }

    /// <summary>
    /// Aggregate bar's highest tick price
    /// </summary>
    decimal High { get; }

    /// <summary>
    /// Aggregate bar's lowest tick price
    /// </summary>
    decimal Low { get; }

    /// <summary>
    /// Aggregate bar's last tick price
    /// </summary>
    decimal Close { get; }

    /// <summary>
    /// Aggregate bar's tick volume
    /// </summary>
    decimal Volume { get; }
}
