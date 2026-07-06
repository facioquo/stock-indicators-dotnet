namespace FacioQuo.Stock.Indicators;

public static partial class FisherTransform
{
    /// <summary>
    /// FISHER Common Base Listing
    /// </summary>
    internal static readonly IndicatorListing CommonListing =
        new CatalogListingBuilder()
            .WithName("Ehlers Fisher Transform")
            .WithId("FISHER")
            .WithCategory(Category.PriceTransform)
            .AddParameter<int>("lookbackPeriods", "Lookback Periods", defaultValue: 10, minimum: 1, maximum: 250)
            .AddResult("Fisher", "Fisher", ResultType.Default, isReusable: true)
            .AddResult("Trigger", "Trigger", ResultType.Default)
            .Build();

    /// <summary>
    /// FISHER Series Listing
    /// </summary>
    internal static readonly IndicatorListing SeriesListing =
        new CatalogListingBuilder(CommonListing)
            .WithStyle(Style.Series)
            .WithMethodName("ToFisherTransform")
            .Build();

    /// <summary>
    /// FISHER Stream Listing
    /// </summary>
    internal static readonly IndicatorListing StreamListing =
        new CatalogListingBuilder(CommonListing)
            .WithStyle(Style.Stream)
            .WithMethodName("ToFisherTransformHub")
            .Build();

    /// <summary>
    /// FISHER Buffer Listing
    /// </summary>
    internal static readonly IndicatorListing BufferListing =
        new CatalogListingBuilder(CommonListing)
            .WithStyle(Style.Buffer)
            .WithMethodName("ToFisherTransformList")
            .Build();
}
