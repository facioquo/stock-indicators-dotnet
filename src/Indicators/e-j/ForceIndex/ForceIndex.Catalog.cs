namespace FacioQuo.Stock.Indicators;

public static partial class ForceIndex
{
    /// <summary>
    /// FORCE Common Base Listing
    /// </summary>
    internal static readonly IndicatorListing CommonListing =
        new CatalogListingBuilder()
            .WithName("Force Index")
            .WithId("FORCE")
            .WithCategory(Category.VolumeBased)
            .AddParameter<int>("lookbackPeriods", "Lookback Periods", defaultValue: 2, minimum: 1, maximum: 250)
            .AddResult("ForceIndex", "Force Index", ResultType.Default, isReusable: true)
            .Build();

    /// <summary>
    /// FORCE Series Listing
    /// </summary>
    internal static readonly IndicatorListing SeriesListing =
        new CatalogListingBuilder(CommonListing)
            .WithStyle(Style.Series)
            .WithMethodName("ToForceIndex")
            .Build();

    /// <summary>
    /// FORCE Buffer Listing
    /// </summary>
    internal static readonly IndicatorListing BufferListing =
        new CatalogListingBuilder(CommonListing)
            .WithStyle(Style.Buffer)
            .WithMethodName("ToForceIndexList")
            .Build();

    /// <summary>
    /// FORCE Stream Listing
    /// </summary>
    internal static readonly IndicatorListing StreamListing =
        new CatalogListingBuilder(CommonListing)
            .WithStyle(Style.Stream)
            .WithMethodName("ToForceIndexHub")
            .Build();
}
