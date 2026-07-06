namespace FacioQuo.Stock.Indicators;

public static partial class ZigZag
{
    /// <summary>
    /// Zig Zag Common Base Listing
    /// </summary>
    internal static readonly IndicatorListing CommonListing =
        new CatalogListingBuilder()
            .WithName("Zig Zag")
            .WithId("ZIGZAG")
            .WithCategory(Category.PriceTransform)
            .AddEnumParameter<EndType>("endType", "End Type", description: "Type of price to use for the calculation", isRequired: false, defaultValue: EndType.Close)
            .AddParameter<double>("percentChange", "Percent Change", description: "Minimum percent change required for a new direction", isRequired: false, defaultValue: 5.0, minimum: 1.0, maximum: 200.0)
            .AddResult("ZigZag", "Zig Zag", ResultType.Default, isReusable: true)
            .AddResult("PointType", "Point Type", ResultType.Default)
            .AddResult("RetraceHigh", "Retrace High", ResultType.Default)
            .AddResult("RetraceLow", "Retrace Low", ResultType.Default)
            .Build();

    /// <summary>
    /// Zig Zag Series Listing
    /// </summary>
    internal static readonly IndicatorListing SeriesListing =
        new CatalogListingBuilder(CommonListing)
            .WithStyle(Style.Series)
            .WithMethodName("ToZigZag")
            .Build();
}
