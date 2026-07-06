namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Interface for Arnaud Legoux Moving Average (ALMA) calculations.
/// </summary>
public interface IAlma
{
    /// <summary>
    /// Gets the number of periods to look back for the calculation.
    /// </summary>
    int LookbackPeriods { get; }

    /// <summary>
    /// Gets the offset parameter for the ALMA calculation.
    /// </summary>
    double Offset { get; }

    /// <summary>
    /// Gets the sigma parameter for the ALMA calculation.
    /// </summary>
    double Sigma { get; }
}
