namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Represents the result of a Hilbert Transform Trendline (HTL) calculation.
/// </summary>
/// <param name="Timestamp">Timestamp of the result.</param>
/// <param name="DcPeriods">Dominant cycle periods.</param>
/// <param name="Trendline">Value of the trendline.</param>
/// <param name="SmoothPrice">Smoothed price.</param>
[Serializable]
public record HtlResult
(
    DateTime Timestamp,
    int? DcPeriods,
    double? Trendline,
    double? SmoothPrice
) : IReusable
{
    /// <inheritdoc/>
    [JsonIgnore]
    public double Value => Trendline.Null2NaN();
}
