namespace FacioQuo.Stock.Indicators;

public static partial class Wma
{
    /// <summary>
    /// Weighted Moving Average Common Base Listing
    /// </summary>
    internal static readonly IndicatorListing CommonListing =
        new CatalogListingBuilder()
            .WithName("Weighted Moving Average")
            .WithId("WMA")
            .WithCategory(Category.MovingAverage)
            .AddParameter<int>("lookbackPeriods", "Lookback Periods", description: "Number of periods for the WMA calculation", isRequired: true, defaultValue: 14, minimum: 1, maximum: 250)
            .AddResult("Wma", "WMA", ResultType.Default, isReusable: true)
            .Build();

    /// <summary>
    /// Weighted Moving Average Series Listing
    /// </summary>
    internal static readonly IndicatorListing SeriesListing =
        new CatalogListingBuilder(CommonListing)
            .WithStyle(Style.Series)
            .WithMethodName("ToWma")
            .Build();

    /// <summary>
    /// Weighted Moving Average Stream Listing
    /// </summary>
    internal static readonly IndicatorListing StreamListing =
        new CatalogListingBuilder(CommonListing)
            .WithStyle(Style.Stream)
            .WithMethodName("ToWmaHub")
            .Build();

    /// <summary>
    /// Weighted Moving Average Buffer Listing
    /// </summary>
    internal static readonly IndicatorListing BufferListing =
        new CatalogListingBuilder(CommonListing)
            .WithStyle(Style.Buffer)
            .WithMethodName("ToWmaList")
            .Build();
}
