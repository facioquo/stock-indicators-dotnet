namespace FacioQuo.Stock.Indicators;

public static partial class Fractal
{
    /// <summary>
    /// Fractal Common Base Listing
    /// </summary>
    internal static readonly IndicatorListing CommonListing =
        new CatalogListingBuilder()
            .WithName("Williams Fractal")
            .WithId("FRACTAL")
            .WithCategory(Category.PricePattern)
            .AddParameter<int>("windowSpan", "Window Span", description: "Number of periods to look back and forward for the calculation", isRequired: false, defaultValue: 2, minimum: 2, maximum: 100)
            .AddEnumParameter<EndType>("endType", "End Type", description: "Type of price to use for the calculation", isRequired: false, defaultValue: EndType.HighLow)
            .AddResult(nameof(FractalResult.FractalBear), "Bear Fractal", IndicatorResult.PricePane, ResultType.Default)
            .AddResult(nameof(FractalResult.FractalBull), "Bull Fractal", IndicatorResult.PricePane, ResultType.Default, isReusable: true)
            .Build();

    /// <summary>
    /// Fractal Series Listing
    /// </summary>
    internal static readonly IndicatorListing SeriesListing =
        new CatalogListingBuilder(CommonListing)
            .WithStyle(Style.Series)
            .WithMethodName(typeof(Fractal), "ToFractal")
            .Build();

    /// <summary>
    /// Fractal Stream Listing
    /// </summary>
    internal static readonly IndicatorListing StreamListing =
        new CatalogListingBuilder(CommonListing)
            .WithStyle(Style.Stream)
            .WithMethodName(typeof(Fractal), "ToFractalHub")
            .Build();

    /// <summary>
    /// Fractal Buffer Listing
    /// </summary>
    internal static readonly IndicatorListing BufferListing =
        new CatalogListingBuilder(CommonListing)
            .WithStyle(Style.Buffer)
            .WithMethodName(typeof(Fractal), "ToFractalList")
            .Build();
}
