namespace FacioQuo.Stock.Indicators;

public static partial class RenkoAtr
{
    /// <summary>
    /// Renko (ATR) Common Base Listing
    /// </summary>
    internal static readonly IndicatorListing CommonListing =
        new CatalogListingBuilder()
            .WithName("Renko (ATR)")
            .WithId("RENKO-ATR")
            .WithCategory(Category.PriceTrend)
            .AddParameter<int>("atrPeriods", "ATR Periods", description: "Number of periods for the ATR calculation", isRequired: false, defaultValue: 14, minimum: 1, maximum: 100)
            .AddEnumParameter<EndType>("endType", "End Type", description: "Type of price to use for the calculation", isRequired: false, defaultValue: EndType.Close)
            .AddResult(nameof(RenkoResult.Open), "Open", IndicatorResult.PricePane, ResultType.Default)
            .AddResult(nameof(RenkoResult.High), "High", IndicatorResult.PricePane, ResultType.Default)
            .AddResult(nameof(RenkoResult.Low), "Low", IndicatorResult.PricePane, ResultType.Default)
            .AddResult(nameof(RenkoResult.Close), "Close", IndicatorResult.PricePane, ResultType.Default, isReusable: true)
            .AddResult(nameof(RenkoResult.Volume), "Volume", "Volume", ResultType.Default)
            .AddResult(nameof(RenkoResult.IsUp), "Is Up", null, ResultType.Default)
            .Build();

    /// <summary>
    /// Renko (ATR) Series Listing
    /// </summary>
    internal static readonly IndicatorListing SeriesListing =
        new CatalogListingBuilder(CommonListing)
            .WithStyle(Style.Series)
            .WithMethodName("ToRenkoAtr")
            .Build();

    // BufferList and StreamHub not implemented - would require buffering all bars
    // and recalculating entire Renko series on each add to maintain ATR accuracy.
    // Series-only implementation maintained for correctness.
}
