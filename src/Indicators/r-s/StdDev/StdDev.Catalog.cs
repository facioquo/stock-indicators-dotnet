namespace FacioQuo.Stock.Indicators;

public static partial class StdDev
{
    /// <summary>
    /// Standard deviation Common Base Listing
    /// </summary>
    internal static readonly IndicatorListing CommonListing =
        new CatalogListingBuilder()
            .WithName("Standard deviation")
            .WithId("STDEV")
            .WithCategory(Category.PriceCharacteristic)
            .AddParameter<int>("lookbackPeriods", "Lookback Periods", description: "Number of periods for the standard deviation calculation", isRequired: false, defaultValue: 14, minimum: 1, maximum: 250)
            .AddResult("StdDev", "Standard deviation", ResultType.Default, isReusable: true)
            .Build();

    /// <summary>
    /// Standard deviation Series Listing
    /// </summary>
    internal static readonly IndicatorListing SeriesListing =
        new CatalogListingBuilder(CommonListing)
            .WithStyle(Style.Series)
            .WithMethodName("ToStdDev")
            .Build();

    /// <summary>
    /// Standard deviation Stream Listing
    /// </summary>
    internal static readonly IndicatorListing StreamListing =
        new CatalogListingBuilder(CommonListing)
            .WithStyle(Style.Stream)
            .WithMethodName("ToStdDevHub")
            .Build();

    /// <summary>
    /// Standard deviation Buffer Listing
    /// </summary>
    internal static readonly IndicatorListing BufferListing =
        new CatalogListingBuilder(CommonListing)
            .WithStyle(Style.Buffer)
            .WithMethodName("ToStdDevList")
            .Build();
}
