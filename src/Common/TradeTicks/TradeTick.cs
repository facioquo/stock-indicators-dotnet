namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Built-in TradeTick type, representing a raw market tick data point.
/// </summary>
/// <param name="Timestamp">
/// Date/time of the tick
/// </param>
/// <param name="Price">
/// TradeTick price
/// </param>
/// <param name="Volume">
/// TradeTick volume (quantity traded)
/// </param>
/// <param name="ExecutionId">
/// Optional unique execution ID for duplicate detection
/// </param>
[Serializable]
public record TradeTick
(
    DateTime Timestamp,
    decimal Price,
    decimal Volume,
    string? ExecutionId = null
) : ITradeTick
{
    /// <inheritdoc/>
    [JsonIgnore]
    public double Value => (double)Price;

    /// <summary>
    /// Initializes a new instance of the <see cref="TradeTick"/> class.
    /// <remarks>
    /// This parameterless constructor exists for serialization/deserialization
    /// scenarios. Use the primary constructor with explicit values for normal usage.
    /// </remarks>
    /// </summary>
    public TradeTick()
        : this(default, default, default, null) { }
}
