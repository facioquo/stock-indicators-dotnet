namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Represents the result of an Ichimoku Cloud calculation.
/// </summary>
/// <param name="Timestamp">Timestamp of the result.</param>
/// <param name="TenkanSen">Value of the Tenkan-sen (conversion line).</param>
/// <param name="KijunSen">Value of the Kijun-sen (base line).</param>
/// <param name="SenkouSpanA">Value of the Senkou Span A (leading span A).</param>
/// <param name="SenkouSpanB">Value of the Senkou Span B (leading span B).</param>
/// <param name="ChikouSpan">Value of the Chikou Span (lagging span).</param>
[Serializable]
public record IchimokuResult
(
   DateTime Timestamp,
   double? TenkanSen,   // conversion line
   double? KijunSen,    // base line
   double? SenkouSpanA, // leading span A
   double? SenkouSpanB, // leading span B
   double? ChikouSpan   // lagging span
) : ISeries;
