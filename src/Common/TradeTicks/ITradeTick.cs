namespace FacioQuo.Stock.Indicators;

/// <summary>
/// TradeTick interface for raw trade-tick data.
/// This represents a single executed trade with price and volume at a specific timestamp.
/// </summary>
public interface ITradeTick : IReusable
{
    /// <summary>
    /// TradeTick price
    /// </summary>
    decimal Price { get; }

    /// <summary>
    /// TradeTick volume (quantity traded)
    /// </summary>
    decimal Volume { get; }

    /// <summary>
    /// Optional unique execution ID for duplicate detection.
    /// When null, duplicates are assessed by timestamp only.
    /// </summary>
    string? ExecutionId { get; }
}
