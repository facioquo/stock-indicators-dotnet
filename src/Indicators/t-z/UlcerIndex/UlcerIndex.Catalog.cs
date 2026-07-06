namespace FacioQuo.Stock.Indicators;

public static partial class UlcerIndex
{
    /// <summary>
    /// Ulcer Index Common Base Listing
    /// </summary>
    internal static readonly IndicatorListing CommonListing =
        new CatalogListingBuilder()
            .WithName("Ulcer Index")
            .WithId("ULCER")
            .WithCategory(Category.PriceCharacteristic)
            .AddParameter<int>("lookbackPeriods", "Lookback Periods", description: "Number of periods for the Ulcer Index calculation", isRequired: false, defaultValue: 14, minimum: 1, maximum: 250)
            .AddResult("UlcerIndex", "Ulcer Index", ResultType.Default, isReusable: true)
            .Build();

    /// <summary>
    /// Ulcer Index Series Listing
    /// </summary>
    internal static readonly IndicatorListing SeriesListing =
        new CatalogListingBuilder(CommonListing)
            .WithStyle(Style.Series)
            .WithMethodName("ToUlcerIndex")
            .Build();

    /// <summary>
    /// Ulcer Index Stream Listing
    /// </summary>
    internal static readonly IndicatorListing StreamListing =
        new CatalogListingBuilder(CommonListing)
            .WithStyle(Style.Stream)
            .WithMethodName("ToUlcerIndexHub")
            .Build();

    /// <summary>
    /// Ulcer Index Buffer Listing
    /// </summary>
    internal static readonly IndicatorListing BufferListing =
        new CatalogListingBuilder(CommonListing)
            .WithStyle(Style.Buffer)
            .WithMethodName("ToUlcerIndexList")
            .Build();
}
