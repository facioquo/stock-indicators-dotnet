namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Chainable <see cref="IReusable"/> record type.
/// </summary>
/// <param name="Timestamp">Date and time of record.</param>
/// <param name="Value">Value that is chainable.</param>
/// <remarks>
/// <para>
/// Unlike most records that return <see langword="null"/>,
/// the <see cref="Value"/> property will return <see cref="double.NaN"/>
/// when incalculable.
/// </para>
/// <para>
/// You may optionally check <see cref="double.IsNaN(double)"/>
/// or use <see cref="NullMath.NaN2Null(double)"/> to convert back to <see langword="null"/>.
/// </para>
/// </remarks>
[Serializable]
public record TimeValue
(
    DateTime Timestamp,
    double Value
) : IReusable
{
    /// <inheritdoc/>
    public double Value { get; } = Value;
}
