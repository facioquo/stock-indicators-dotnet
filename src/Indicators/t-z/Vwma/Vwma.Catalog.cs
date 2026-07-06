namespace FacioQuo.Stock.Indicators;

public static partial class Vwma
{
    /// <summary>
    /// Volume Weighted Moving Average Common Base Listing
    /// </summary>
    internal static readonly IndicatorListing CommonListing =
        new CatalogListingBuilder()
            .WithName("Volume Weighted Moving Average")
            .WithId("VWMA")
            .WithCategory(Category.MovingAverage)
            .AddParameter<int>("lookbackPeriods", "Lookback Periods", description: "Number of periods for the VWMA calculation", isRequired: false, defaultValue: 14, minimum: 1, maximum: 250)
            .AddResult("Vwma", "VWMA", ResultType.Default, isReusable: true)
            .Build();

    /// <summary>
    /// Volume Weighted Moving Average Series Listing
    /// </summary>
    internal static readonly IndicatorListing SeriesListing =
        new CatalogListingBuilder(CommonListing)
            .WithStyle(Style.Series)
            .WithMethodName("ToVwma")
            .Build();

    /// <summary>
    /// Volume Weighted Moving Average Stream Listing
    /// </summary>
    internal static readonly IndicatorListing StreamListing =
        new CatalogListingBuilder(CommonListing)
            .WithStyle(Style.Stream)
            .WithMethodName("ToVwmaHub")
            .Build();

    /// <summary>
    /// Volume Weighted Moving Average Buffer Listing
    /// </summary>
    internal static readonly IndicatorListing BufferListing =
        new CatalogListingBuilder(CommonListing)
            .WithStyle(Style.Buffer)
            .WithMethodName("ToVwmaList")
            .Build();
}
