namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Specifies the type of Beta calculation.
/// </summary>
public enum BetaType
{
    /// <summary>
    /// Standard Beta only
    /// </summary>
    Standard = 0,

    /// <summary>
    /// Beta+ updside only
    /// </summary>
    Up = 1,

    /// <summary>
    /// Beta- downside only
    /// </summary>
    Down = 2,

    /// <summary>
    /// Calculation all Beta types
    /// </summary>
    All = 3
}
