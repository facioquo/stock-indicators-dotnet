namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Interface for Pivot Points calculations.
/// </summary>
public interface IPivotPoints
{
    /// <summary>
    /// Gets the size of the window for pivot point calculation.
    /// </summary>
    BarInterval WindowSize { get; }

    /// <summary>
    /// Gets the type of pivot point calculation to use.
    /// </summary>
    PivotPointType PointType { get; }
}
