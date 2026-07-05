namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Double-precision Bar, for internal use only.
/// </summary>
/// <inheritdoc cref="IBar" />
[Serializable]
internal record BarD
(
    DateTime Timestamp,
    double Open,
    double High,
    double Low,
    double Close,
    double Volume
) : IReusable
{
    /// <inheritdoc/>
    [JsonIgnore]
    double IReusable.Value => Close;
}
