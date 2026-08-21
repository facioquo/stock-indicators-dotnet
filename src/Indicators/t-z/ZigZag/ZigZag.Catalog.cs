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
            .AddParameter<decimal>("percentChange", "Percent Change", description: "Minimum percent change required for a new direction", isRequired: false, defaultValue: 5.0m, minimum: 1.0, maximum: 200.0)
            .AddResult(nameof(ZigZagResult.ZigZag), "Zig Zag", ResultType.Default, isReusable: true)
            .AddResult(nameof(ZigZagResult.PointType), "Point Type", ResultType.Default)
            .AddResult(nameof(ZigZagResult.RetraceHigh), "Retrace High", ResultType.Default)
            .AddResult(nameof(ZigZagResult.RetraceLow), "Retrace Low", ResultType.Default)
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
