namespace FacioQuo.Stock.Indicators;

public static partial class HeikinAshi
{
    /// <summary>
    /// HEIKINASHI Common Base Listing
    /// </summary>
    internal static readonly IndicatorListing CommonListing =
        new CatalogListingBuilder()
            .WithName("HeikinAshi")
            .WithId("HEIKINASHI")
            .WithCategory(Category.PriceTransform)
            .AddResult("Open", "Open", ResultType.Default)
            .AddResult("High", "High", ResultType.Default)
            .AddResult("Low", "Low", ResultType.Default)
            .AddResult("Close", "Close", ResultType.Default, isReusable: true)
            .AddResult("Volume", "Volume", ResultType.Default)
            .Build();

    /// <summary>
    /// HEIKINASHI Series Listing
    /// </summary>
    internal static readonly IndicatorListing SeriesListing =
        new CatalogListingBuilder(CommonListing)
            .WithStyle(Style.Series)
            .WithMethodName("ToHeikinAshi")
            .Build();

    /// <summary>
    /// HEIKINASHI Buffer Listing
    /// </summary>
    internal static readonly IndicatorListing BufferListing =
        new CatalogListingBuilder(CommonListing)
            .WithStyle(Style.Buffer)
            .WithMethodName("ToHeikinAshiList")
            .Build();

    /// <summary>
    /// HEIKINASHI Stream Listing
    /// </summary>
    internal static readonly IndicatorListing StreamListing =
        new CatalogListingBuilder(CommonListing)
            .WithStyle(Style.Stream)
            .WithMethodName("ToHeikinAshiHub")
            .Build();
}
