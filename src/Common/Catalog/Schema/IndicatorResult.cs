namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Represents the configuration for an indicator result.
/// </summary>
[Serializable]
public record IndicatorResult
{
    /// <summary>
    /// The <see cref="ChartPane"/> value for a result drawn on the price axis.
    /// </summary>
    public static string PricePane { get; } = "Price";

    /// <summary>
    /// Gets or sets the display name of the result.
    /// </summary>
    public required string DisplayName { get; init; }

    /// <summary>
    /// Gets or sets the data name of the result.
    /// </summary>
    public required string DataName { get; init; }

    /// <summary>
    /// Gets or sets the data type of the result.
    /// </summary>
    public required ResultType DataType { get; init; }

    /// <summary>
    /// Gets or sets a value indicating whether this result is the reusable output.
    /// </summary>
    /// <remarks>
    /// This value is only set to true when it is used to set the <see cref="IReusable.Value"/>.
    /// </remarks>
    public bool IsReusable { get; init; }

    /// <summary>
    /// Gets or sets the chart pane this result draws in.
    /// </summary>
    /// <remarks>
    /// <see cref="PricePane"/> draws on the price axis. Any other value names a separate pane,
    /// shared by every result of the listing with the same value, because those results share
    /// units; results with different values would flatten each other on one axis.
    /// <c>null</c> means the result is not charted, such as a boolean flag or a pattern code.
    /// </remarks>
    public string? ChartPane { get; init; }
}
