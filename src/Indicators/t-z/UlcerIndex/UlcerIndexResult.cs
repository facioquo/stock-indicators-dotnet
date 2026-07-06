namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Represents the result of an Ulcer Index calculation.
/// </summary>
/// <param name="Timestamp">Timestamp of the data point.</param>
/// <param name="UlcerIndex">Value of the Ulcer Index at this point.</param>
[Serializable]
public record UlcerIndexResult
(
    DateTime Timestamp,
    double? UlcerIndex
) : IReusable
{
    /// <inheritdoc/>
    [JsonIgnore]
    public double Value => UlcerIndex.Null2NaN();

    /// <inheritdoc/>
    [Obsolete("Rename UI to UlcerIndex")]
    public double? UI => UlcerIndex;
}
