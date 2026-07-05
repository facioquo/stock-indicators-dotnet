namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Interface for Ichimoku indicator configuration properties.
/// </summary>
public interface IIchimoku
{
    /// <summary>
    /// Gets the number of periods for the Tenkan-sen (conversion line).
    /// </summary>
    int TenkanPeriods { get; }

    /// <summary>
    /// Gets the number of periods for the Kijun-sen (base line).
    /// </summary>
    int KijunPeriods { get; }

    /// <summary>
    /// Gets the number of periods for the Senkou Span B (leading span B).
    /// </summary>
    int SenkouBPeriods { get; }

    /// <summary>
    /// Gets the number of periods for the Senkou offset.
    /// </summary>
    int SenkouOffset { get; }

    /// <summary>
    /// Gets the number of periods for the Chikou offset.
    /// </summary>
    int ChikouOffset { get; }
}
