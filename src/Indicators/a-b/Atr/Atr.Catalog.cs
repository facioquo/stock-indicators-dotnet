namespace FacioQuo.Stock.Indicators;

public static partial class Atr
{
    /// <summary>
    /// ATR Common Base Listing
    /// </summary>
    internal static readonly IndicatorListing CommonListing =
        new CatalogListingBuilder()
            .WithName("Average True Range (ATR)")
            .WithId("ATR")
            .WithCategory(Category.PriceCharacteristic)
            .AddParameter<int>("lookbackPeriods", "Lookback Periods", defaultValue: 14, minimum: 2, maximum: 250)
            .AddResult("Tr", "True Range", ResultType.Default)
            .AddResult("Atr", "ATR", ResultType.Default)
            .AddResult("Atrp", "ATR %", ResultType.Default, isReusable: true)
            .Build();

    /// <summary>
    /// ATR Series Listing
    /// </summary>
    internal static readonly IndicatorListing SeriesListing =
        new CatalogListingBuilder(CommonListing)
            .WithStyle(Style.Series)
            .WithMethodName("ToAtr")
            .Build();

    /// <summary>
    /// ATR Stream Listing
    /// </summary>
    internal static readonly IndicatorListing StreamListing =
        new CatalogListingBuilder(CommonListing)
            .WithStyle(Style.Stream)
            .WithMethodName("ToAtrHub")
            .Build();

    /// <summary>
    /// ATR Buffer Listing
    /// </summary>
    internal static readonly IndicatorListing BufferListing =
        new CatalogListingBuilder(CommonListing)
            .WithStyle(Style.Buffer)
            .WithMethodName("ToAtrList")
            .Build();
}
