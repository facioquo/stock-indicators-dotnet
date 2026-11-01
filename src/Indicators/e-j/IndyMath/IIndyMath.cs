namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Interface for IndyMath arithmetic with a fixed operand.
/// </summary>
public interface IScalarMath
{
    /// <summary>
    /// Gets the arithmetic applied to each value.
    /// </summary>
    ScalarOperation Operation { get; }

    /// <summary>
    /// Gets the fixed operand, as given.
    /// </summary>
    double Operand { get; }
}

/// <summary>
/// Interface for IndyMath rolling-window calculations.
/// </summary>
public interface IRollingMath
{
    /// <summary>
    /// Gets the number of periods in the window, including the current one.
    /// </summary>
    int LookbackPeriods { get; }
}
