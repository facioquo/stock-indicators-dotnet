namespace FacioQuo.Stock.Indicators;

public static partial class Donchian
{
    /// <summary>
    /// DONCHIAN Common Base Listing
    /// </summary>
    internal static readonly IndicatorListing CommonListing =
        new CatalogListingBuilder()
            .WithName("Donchian Channels")
            .WithId("DONCHIAN")
            .WithCategory(Category.PriceChannel)
            .AddParameter<int>("lookbackPeriods", "Lookback Periods", defaultValue: 20, minimum: 1, maximum: 250)
            .AddResult("UpperBand", "Upper Band", ResultType.Default)
            .AddResult("Centerline", "Centerline", ResultType.Default, isReusable: true)
            .AddResult("LowerBand", "Lower Band", ResultType.Default)
            .AddResult("Width", "Width", ResultType.Default)
            .Build();

    /// <summary>
    /// DONCHIAN Series Listing
    /// </summary>
    internal static readonly IndicatorListing SeriesListing =
        new CatalogListingBuilder(CommonListing)
            .WithStyle(Style.Series)
            .WithMethodName("ToDonchian")
            .Build();

    /// <summary>
    /// DONCHIAN Stream Listing
    /// </summary>
    internal static readonly IndicatorListing StreamListing =
        new CatalogListingBuilder(CommonListing)
            .WithStyle(Style.Stream)
            .WithMethodName("ToDonchianHub")
            .Build();

    /// <summary>
    /// DONCHIAN Buffer Listing
    /// </summary>
    internal static readonly IndicatorListing BufferListing =
        new CatalogListingBuilder(CommonListing)
            .WithStyle(Style.Buffer)
            .WithMethodName("ToDonchianList")
            .Build();
}
