namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Operation applied between the values of two series at each matched timestamp.
/// </summary>
public enum PairOperation
{
    /// <summary>
    /// Adds the other value to the source value.
    /// </summary>
    Plus = 0,

    /// <summary>
    /// Subtracts the other value from the source value.
    /// </summary>
    Minus = 1,

    /// <summary>
    /// Multiplies the source value by the other value.
    /// </summary>
    Times = 2,

    /// <summary>
    /// Divides the source value by the other value; a 0 divisor yields an incalculable period.
    /// </summary>
    DividedBy = 3,

    /// <summary>
    /// Selects the larger of the two values.
    /// </summary>
    MaxWith = 4,

    /// <summary>
    /// Selects the smaller of the two values.
    /// </summary>
    MinWith = 5
}
