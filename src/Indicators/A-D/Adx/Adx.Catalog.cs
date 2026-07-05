namespace FacioQuo.Stock.Indicators;

public static partial class Adx
{
    /// <summary>
    /// ADX Common Base Listing
    /// </summary>
    internal static readonly IndicatorListing CommonListing =
        new CatalogListingBuilder()
            .WithName("Average Directional Index (ADX)")
            .WithId("ADX")
            .WithCategory(Category.PriceTrend)
            .AddParameter<int>("lookbackPeriods", "Lookback Periods", defaultValue: 14, minimum: 2, maximum: 250)
            .AddResult("Pdi", "+DI", ResultType.Default)
            .AddResult("Mdi", "-DI", ResultType.Default)
            .AddResult("Dx", "DX", ResultType.Default)
            .AddResult("Adx", "ADX", ResultType.Default, isReusable: true)
            .AddResult("Adxr", "ADXR", ResultType.Default)
            .Build();

    /// <summary>
    /// ADX Series Listing
    /// </summary>
    internal static readonly IndicatorListing SeriesListing =
        new CatalogListingBuilder(CommonListing)
            .WithStyle(Style.Series)
            .WithMethodName("ToAdx")
            .Build();

    /// <summary>
    /// ADX Stream Listing
    /// </summary>
    internal static readonly IndicatorListing StreamListing =
        new CatalogListingBuilder(CommonListing)
            .WithStyle(Style.Stream)
            .WithMethodName("ToAdxHub")
            .Build();

    /// <summary>
    /// ADX Buffer Listing
    /// </summary>
    internal static readonly IndicatorListing BufferListing =
        new CatalogListingBuilder(CommonListing)
            .WithStyle(Style.Buffer)
            .WithMethodName("ToAdxList")
            .Build();
}
