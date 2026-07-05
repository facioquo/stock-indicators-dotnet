namespace FacioQuo.Stock.Indicators;

public static partial class Dema
{
    /// <summary>
    /// DEMA Common Base Listing
    /// </summary>
    internal static readonly IndicatorListing CommonListing =
        new CatalogListingBuilder()
            .WithName("Double Exponential Moving Average")
            .WithId("DEMA")
            .WithCategory(Category.MovingAverage)
            .AddParameter<int>("lookbackPeriods", "Lookback Periods", description: "Number of periods for the DEMA calculation", isRequired: false, defaultValue: 14, minimum: 2, maximum: 250)
            .AddResult("Dema", "DEMA", ResultType.Default, isReusable: true)
            .Build();

    /// <summary>
    /// DEMA Series Listing
    /// </summary>
    internal static readonly IndicatorListing SeriesListing =
        new CatalogListingBuilder(CommonListing)
            .WithStyle(Style.Series)
            .WithMethodName("ToDema")
            .Build();

    /// <summary>
    /// DEMA Stream Listing
    /// </summary>
    internal static readonly IndicatorListing StreamListing =
        new CatalogListingBuilder(CommonListing)
            .WithStyle(Style.Stream)
            .WithMethodName("ToDemaHub")
            .Build();

    /// <summary>
    /// DEMA Buffer Listing
    /// </summary>
    internal static readonly IndicatorListing BufferListing =
        new CatalogListingBuilder(CommonListing)
            .WithStyle(Style.Buffer)
            .WithMethodName("ToDemaList")
            .Build();
}
