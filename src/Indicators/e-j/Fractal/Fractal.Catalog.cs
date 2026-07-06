namespace FacioQuo.Stock.Indicators;

public static partial class Fractal
{
    /// <summary>
    /// Fractal Common Base Listing
    /// </summary>
    internal static readonly IndicatorListing CommonListing =
        new CatalogListingBuilder()
            .WithName("Williams Fractal (high/low)")
            .WithId("FRACTAL")
            .WithCategory(Category.PricePattern)
            .AddParameter<int>("windowSpan", "Window Span", description: "Number of periods to look back and forward for the calculation", isRequired: false, defaultValue: 2, minimum: 1, maximum: 100)
            .AddEnumParameter<EndType>("endType", "End Type", description: "Type of price to use for the calculation", isRequired: false, defaultValue: EndType.HighLow)
            .AddResult("FractalBear", "Bear Fractal", ResultType.Default)
            .AddResult("FractalBull", "Bull Fractal", ResultType.Default, isReusable: true)
            .Build();

    /// <summary>
    /// Fractal Series Listing
    /// </summary>
    internal static readonly IndicatorListing SeriesListing =
        new CatalogListingBuilder(CommonListing)
            .WithStyle(Style.Series)
            .WithMethodName("ToFractal")
            .Build();

    /// <summary>
    /// Fractal Stream Listing
    /// </summary>
    internal static readonly IndicatorListing StreamListing =
        new CatalogListingBuilder(CommonListing)
            .WithStyle(Style.Stream)
            .WithMethodName("ToFractalHub")
            .Build();

    /// <summary>
    /// Fractal Buffer Listing
    /// </summary>
    internal static readonly IndicatorListing BufferListing =
        new CatalogListingBuilder(CommonListing)
            .WithStyle(Style.Buffer)
            .WithMethodName("ToFractalList")
            .Build();
}
