namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Arithmetic applied between each value in a series and a fixed operand.
/// </summary>
public enum ScalarOperation
{
    /// <summary>
    /// Adds the operand to each value.
    /// </summary>
    Plus = 0,

    /// <summary>
    /// Subtracts the operand from each value.
    /// </summary>
    Minus = 1,

    /// <summary>
    /// Multiplies each value by the operand.
    /// </summary>
    Times = 2,

    /// <summary>
    /// Divides each value by the operand, which must not be 0.
    /// </summary>
    DividedBy = 3,

    /// <summary>
    /// Offsets each value by the operand as a percent of itself, where 2.5 adds 2.5%.
    /// </summary>
    PlusPercent = 4
}
