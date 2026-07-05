namespace FacioQuo.Stock.Indicators;

public static partial class Ichimoku
{
    /// <summary>
    /// ICHIMOKU Common Base Listing
    /// </summary>
    internal static readonly IndicatorListing CommonListing =
        new CatalogListingBuilder()
            .WithName("Ichimoku Cloud")
            .WithId("ICHIMOKU")
            .WithCategory(Category.PriceTrend)
            .AddParameter<int>("tenkanPeriods", "Tenkan Periods", defaultValue: 9, minimum: 1, maximum: 250)
            .AddParameter<int>("kijunPeriods", "Kijun Periods", defaultValue: 26, minimum: 2, maximum: 250)
            .AddParameter<int>("senkouBPeriods", "Senkou B Periods", defaultValue: 52, minimum: 3, maximum: 250)
            .AddResult("TenkanSen", "Tenkan-sen", ResultType.Default, isReusable: true)
            .AddResult("KijunSen", "Kijun-sen", ResultType.Default)
            .AddResult("SenkouSpanA", "Senkou Span A", ResultType.Default)
            .AddResult("SenkouSpanB", "Senkou Span B", ResultType.Default)
            .AddResult("ChikouSpan", "Chikou Span", ResultType.Default)
            .Build();

    /// <summary>
    /// ICHIMOKU Series Listing
    /// </summary>
    internal static readonly IndicatorListing SeriesListing =
        new CatalogListingBuilder(CommonListing)
            .WithStyle(Style.Series)
            .WithMethodName("ToIchimoku")
            .Build();

    /// <summary>
    /// ICHIMOKU Stream Listing
    /// </summary>
    internal static readonly IndicatorListing StreamListing =
        new CatalogListingBuilder(CommonListing)
            .WithStyle(Style.Stream)
            .WithMethodName("ToIchimokuHub")
            .Build();

    /// <summary>
    /// ICHIMOKU Buffer Listing
    /// </summary>
    internal static readonly IndicatorListing BufferListing =
        new CatalogListingBuilder(CommonListing)
            .WithStyle(Style.Buffer)
            .WithMethodName("ToIchimokuList")
            .Build();
}
