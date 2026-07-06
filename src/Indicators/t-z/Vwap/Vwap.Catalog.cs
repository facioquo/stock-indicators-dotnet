namespace FacioQuo.Stock.Indicators;

public static partial class Vwap
{
    /// <summary>
    /// VWAP Common Base Listing
    /// </summary>
    internal static readonly IndicatorListing CommonListing =
        new CatalogListingBuilder()
            .WithName("Volume Weighted Average Price")
            .WithId("VWAP")
            .WithCategory(Category.PriceChannel)
            .AddDateParameter("startDate", "Start Date", description: "Starting date for VWAP calculation", isRequired: false)
            .AddResult("Vwap", "VWAP", ResultType.Default, isReusable: true)
            .AddResult("UpperBand", "Upper Band", ResultType.Default)
            .AddResult("LowerBand", "Lower Band", ResultType.Default)
            .Build();

    /// <summary>
    /// VWAP Series Listing
    /// </summary>
    internal static readonly IndicatorListing SeriesListing =
        new CatalogListingBuilder(CommonListing)
            .WithStyle(Style.Series)
            .WithMethodName("ToVwap")
            .Build();

    /// <summary>
    /// VWAP Buffer Listing
    /// </summary>
    internal static readonly IndicatorListing BufferListing =
        new CatalogListingBuilder(CommonListing)
            .WithStyle(Style.Buffer)
            .WithMethodName("ToVwapList")
            .Build();

    /// <summary>
    /// VWAP Stream Listing
    /// </summary>
    internal static readonly IndicatorListing StreamListing =
        new CatalogListingBuilder(CommonListing)
            .WithStyle(Style.Stream)
            .WithMethodName("ToVwapHub")
            .Build();
}
