namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Interface representing pivot points.
/// </summary>
internal interface IPivotPoint
{
    /// <summary>
    /// Gets the fourth resistance level.
    /// </summary>
    double? R4 { get; }

    /// <summary>
    /// Gets the third resistance level.
    /// </summary>
    double? R3 { get; }

    /// <summary>
    /// Gets the second resistance level.
    /// </summary>
    double? R2 { get; }

    /// <summary>
    /// Gets the first resistance level.
    /// </summary>
    double? R1 { get; }

    /// <summary>
    /// Gets the pivot point.
    /// </summary>
    double? PP { get; }

    /// <summary>
    /// Gets the first support level.
    /// </summary>
    double? S1 { get; }

    /// <summary>
    /// Gets the second support level.
    /// </summary>
    double? S2 { get; }

    /// <summary>
    /// Gets the third support level.
    /// </summary>
    double? S3 { get; }

    /// <summary>
    /// Gets the fourth support level.
    /// </summary>
    double? S4 { get; }
}

/// <summary>
/// Represents the result of pivot points calculation.
/// </summary>
[Serializable]
public record PivotPointsResult : IPivotPoint, IReusable
{
    /// <summary>
    /// Gets the timestamp of the result.
    /// </summary>
    public DateTime Timestamp { get; init; }

    /// <inheritdoc/>
    public double? PP { get; init; }

    /// <inheritdoc/>
    public double? S1 { get; init; }
    /// <inheritdoc/>
    public double? S2 { get; init; }
    /// <inheritdoc/>
    public double? S3 { get; init; }
    /// <inheritdoc/>
    public double? S4 { get; init; }

    /// <inheritdoc/>
    public double? R1 { get; init; }
    /// <inheritdoc/>
    public double? R2 { get; init; }
    /// <inheritdoc/>
    public double? R3 { get; init; }
    /// <inheritdoc/>
    public double? R4 { get; init; }

    /// <inheritdoc/>
    [JsonIgnore]
    public double Value => PP.Null2NaN();
}

/// <summary>
/// Represents a window point for pivot points calculation.
/// </summary>
internal record WindowPoint : IPivotPoint
{
    /// <inheritdoc/>
    public double? PP { get; init; }

    /// <inheritdoc/>
    public double? S1 { get; init; }
    /// <inheritdoc/>
    public double? S2 { get; init; }
    /// <inheritdoc/>
    public double? S3 { get; init; }
    /// <inheritdoc/>
    public double? S4 { get; init; }

    /// <inheritdoc/>
    public double? R1 { get; init; }
    /// <inheritdoc/>
    public double? R2 { get; init; }
    /// <inheritdoc/>
    public double? R3 { get; init; }
    /// <inheritdoc/>
    public double? R4 { get; init; }
}
