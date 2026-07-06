namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Built-in Bar type, representing an OHLCV aggregate price period.
/// </summary>
/// <param name="Timestamp">
/// Close date/time of the aggregate
/// </param>
/// <param name="Open">
/// Aggregate bar's first tick price
/// </param>
/// <param name="High">
/// Aggregate bar's highest tick price
/// </param>
/// <param name="Low">
/// Aggregate bar's lowest tick price
/// </param>
/// <param name="Close">
/// Aggregate bar's last tick price
/// </param>
/// <param name="Volume">
/// Aggregate bar's tick volume
/// </param>
/// <inheritdoc cref="IBar"/>
[Serializable]
public record Bar
(
    DateTime Timestamp,
    decimal Open,
    decimal High,
    decimal Low,
    decimal Close,
    decimal Volume
) : IBar
{
    /// <inheritdoc/>
    [JsonIgnore]
    public double Value => (double)Close;

    /// <inheritdoc/>
    [Obsolete("Use 'Timestamp' property instead.")]
    public DateTime Date
    {
        get => Timestamp;
        init => Timestamp = value;
    }
}
