namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Interface for Vortex Indicator Hub.
/// </summary>
public interface IVortex
{
    /// <summary>
    /// Gets the lookback periods for Vortex calculation.
    /// </summary>
    int LookbackPeriods { get; }
}
