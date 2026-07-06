namespace FacioQuo.Stock.Indicators;

public static partial class AtrStop
{
    /// <summary>
    /// ATR-STOP Common Base Listing
    /// </summary>
    internal static readonly IndicatorListing CommonListing =
        new CatalogListingBuilder()
            .WithName("ATR Trailing Stop")
            .WithId("ATR-STOP")
            .WithCategory(Category.PriceTrend)
            .AddParameter<int>("lookbackPeriods", "Lookback Periods", defaultValue: 21, minimum: 1, maximum: 50)
            .AddParameter<double>("multiplier", "Multiplier", defaultValue: 3.0, minimum: 0.1, maximum: 10.0)
            .AddEnumParameter<EndType>("endType", "End Type", defaultValue: EndType.Close)
            .AddResult("AtrStop", "ATR Trailing Stop", ResultType.Default, isReusable: true)
            .AddResult("BuyStop", "Buy Stop", ResultType.Default)
            .AddResult("SellStop", "Sell Stop", ResultType.Default)
            .AddResult("Atr", "ATR", ResultType.Default)
            .Build();

    /// <summary>
    /// ATR-STOP Series Listing
    /// </summary>
    internal static readonly IndicatorListing SeriesListing =
        new CatalogListingBuilder(CommonListing)
            .WithStyle(Style.Series)
            .WithMethodName("ToAtrStop")
            .Build();

    /// <summary>
    /// ATR-STOP Stream Listing
    /// </summary>
    internal static readonly IndicatorListing StreamListing =
        new CatalogListingBuilder(CommonListing)
            .WithStyle(Style.Stream)
            .WithMethodName("ToAtrStopHub")
            .Build();

    /// <summary>
    /// ATR-STOP Buffer Listing
    /// </summary>
    internal static readonly IndicatorListing BufferListing =
        new CatalogListingBuilder(CommonListing)
            .WithStyle(Style.Buffer)
            .WithMethodName("ToAtrStopList")
            .Build();
}
