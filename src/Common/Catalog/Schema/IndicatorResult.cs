namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Represents the configuration for an indicator result.
/// </summary>
[Serializable]
public record IndicatorResult
{
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
}
