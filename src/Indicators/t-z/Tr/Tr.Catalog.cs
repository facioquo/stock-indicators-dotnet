namespace FacioQuo.Stock.Indicators;

public static partial class Tr
{
    /// <summary>
    /// TR Common Base Listing
    /// </summary>
    internal static readonly IndicatorListing CommonListing =
        new CatalogListingBuilder()
            .WithName("True Range")
            .WithId("TR")
            .WithCategory(Category.PriceCharacteristic)
            .AddResult("Tr", "True Range", ResultType.Default, isReusable: true)
            .Build();

    /// <summary>
    /// TR Series Listing
    /// </summary>
    internal static readonly IndicatorListing SeriesListing =
        new CatalogListingBuilder(CommonListing)
            .WithStyle(Style.Series)
            .WithMethodName("ToTr")
            .Build();

    /// <summary>
    /// TR Stream Listing
    /// </summary>
    internal static readonly IndicatorListing StreamListing =
        new CatalogListingBuilder(CommonListing)
            .WithStyle(Style.Stream)
            .WithMethodName("ToTrHub")
            .Build();

    /// <summary>
    /// TR Buffer Listing
    /// </summary>
    internal static readonly IndicatorListing BufferListing =
        new CatalogListingBuilder(CommonListing)
            .WithStyle(Style.Buffer)
            .WithMethodName("ToTrList")
            .Build();
}
