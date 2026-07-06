namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Represents the result of a Stochastic Oscillator calculation.
/// </summary>
/// <param name="Timestamp">Timestamp of the data point.</param>
/// <param name="Oscillator">Stochastic Oscillator (%K) at this point.</param>
/// <param name="Signal">The %D signal at this point.</param>
/// <param name="PercentJ">The %J at this point.</param>
[Serializable]
public record StochResult
(
    DateTime Timestamp,
    double? Oscillator,
    double? Signal,
    double? PercentJ
) : IReusable
{
    /// <inheritdoc/>
    [JsonIgnore]
    public double Value => Oscillator.Null2NaN();

    // aliases

    /// <summary>
    /// Gets the value of the Stochastic Oscillator (%K) at this point.
    /// It is an alias of the <see cref="Oscillator"/> property.
    /// </summary>
    public double? K => Oscillator;

    /// <summary>
    /// Gets the signal line value (%D) at this point.
    /// It is an alias of the <see cref="Signal"/> property.
    /// </summary>
    public double? D => Signal;

    /// <summary>
    /// Gets the value of the %J line at this point.
    /// It is an alias of the <see cref="PercentJ"/> property.
    /// </summary>
    public double? J => PercentJ;
}
