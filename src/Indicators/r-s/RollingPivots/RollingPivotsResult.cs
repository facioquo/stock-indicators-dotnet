namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Represents the result of a Rolling Pivots calculation.
/// </summary>
[Serializable]
public record RollingPivotsResult : IPivotPoint, IReusable
{
    /// <summary>
    /// Gets the timestamp of the Rolling Pivots result.
    /// </summary>
    public DateTime Timestamp { get; init; }

    /// <summary>
    /// Gets the pivot point (PP) value.
    /// </summary>
    public double? PP { get; init; }

    /// <summary>
    /// Gets the first support level (S1).
    /// </summary>
    public double? S1 { get; init; }

    /// <summary>
    /// Gets the second support level (S2).
    /// </summary>
    public double? S2 { get; init; }

    /// <summary>
    /// Gets the third support level (S3).
    /// </summary>
    public double? S3 { get; init; }

    /// <summary>
    /// Gets the fourth support level (S4).
    /// </summary>
    public double? S4 { get; init; }

    /// <summary>
    /// Gets the first resistance level (R1).
    /// </summary>
    public double? R1 { get; init; }

    /// <summary>
    /// Gets the second resistance level (R2).
    /// </summary>
    public double? R2 { get; init; }

    /// <summary>
    /// Gets the third resistance level (R3).
    /// </summary>
    public double? R3 { get; init; }

    /// <summary>
    /// Gets the fourth resistance level (R4).
    /// </summary>
    public double? R4 { get; init; }

    /// <inheritdoc/>
    [JsonIgnore]
    public double Value => PP.Null2NaN();
}
