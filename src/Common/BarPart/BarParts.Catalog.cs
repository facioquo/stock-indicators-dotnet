namespace FacioQuo.Stock.Indicators;

public static partial class BarParts
{
    /// <summary>
    /// BARPART Common Base Listing
    /// </summary>
    internal static readonly IndicatorListing CommonListing =
        new CatalogListingBuilder()
            .WithName("Bar Part")
            .WithId("BARPART")
            .WithCategory(Category.PriceTransform)
            .AddEnumParameter<CandlePart>("candlePart", "Candle Part", defaultValue: CandlePart.Close)
            .AddResult("Value", "Value", ResultType.Default, isReusable: true)
            .Build();

    /// <summary>
    /// BARPART Series Listing
    /// </summary>
    internal static readonly IndicatorListing SeriesListing =
        new CatalogListingBuilder(CommonListing)
            .WithStyle(Style.Series)
            .WithMethodName("ToBarPart")
            .Build();

    /// <summary>
    /// BARPART Stream Listing
    /// </summary>
    internal static readonly IndicatorListing StreamListing =
        new CatalogListingBuilder(CommonListing)
            .WithStyle(Style.Stream)
            .WithMethodName("ToBarPartHub")
            .Build();

    /// <summary>
    /// BARPART Buffer Listing
    /// </summary>
    internal static readonly IndicatorListing BufferListing =
        new CatalogListingBuilder(CommonListing)
            .WithStyle(Style.Buffer)
            .WithMethodName("ToBarPartList")
            .Build();
}
