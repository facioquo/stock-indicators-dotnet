namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Values of two series matched at the same timestamp.
/// </summary>
/// <remarks>
/// Like <see cref="TimeValue"/>, this is a chainable transport record: an
/// incalculable value is NaN, not <see langword="null"/>.
/// </remarks>
/// <param name="Timestamp">Date and time both series share.</param>
/// <param name="Value">Value of the source series; the chainable value.</param>
/// <param name="Other">Value of the other series at the same timestamp.</param>
[Serializable]
public record PairedValue
(
    DateTime Timestamp,
    double Value,
    double Other
) : IReusable
{
    /// <inheritdoc/>
    public double Value { get; } = Value;
}
