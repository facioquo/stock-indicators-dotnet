namespace FacioQuo.Stock.Indicators;

public static partial class Pivots
{
    /// <summary>
    /// Pivots Common Base Listing
    /// </summary>
    internal static readonly IndicatorListing CommonListing =
        new CatalogListingBuilder()
            .WithName("Pivots")
            .WithId("PIVOTS")
            .WithCategory(Category.PriceTrend)
            .AddParameter<int>("leftSpan", "Left Span", description: "Number of periods to the left for pivot identification", isRequired: false, defaultValue: 2, minimum: 2, maximum: 100)
            .AddParameter<int>("rightSpan", "Right Span", description: "Number of periods to the right for pivot identification", isRequired: false, defaultValue: 2, minimum: 2, maximum: 100)
            .AddParameter<int>("maxTrendPeriods", "Max Trend Periods", description: "Maximum number of periods to track trend", isRequired: false, defaultValue: 20, minimum: 2, maximum: 1000)
            .AddEnumParameter<EndType>("endType", "End Type", description: "Type of price to use for pivot calculation", isRequired: false, defaultValue: EndType.HighLow)
            .AddResult("HighPoint", "High Point", ResultType.Default, isReusable: false)
            .AddResult("LowPoint", "Low Point", ResultType.Default, isReusable: false)
            .AddResult("HighLine", "High Line", ResultType.Default, isReusable: false)
            .AddResult("LowLine", "Low Line", ResultType.Default, isReusable: false)
            .AddResult("HighTrend", "High Trend", ResultType.Default, isReusable: false)
            .AddResult("LowTrend", "Low Trend", ResultType.Default, isReusable: false)
            .Build();

    /// <summary>
    /// Pivots Series Listing
    /// </summary>
    internal static readonly IndicatorListing SeriesListing =
        new CatalogListingBuilder(CommonListing)
            .WithStyle(Style.Series)
            .WithMethodName("ToPivots")
            .Build();

    /// <summary>
    /// Pivots Buffer Listing
    /// </summary>
    internal static readonly IndicatorListing BufferListing =
        new CatalogListingBuilder(CommonListing)
            .WithStyle(Style.Buffer)
            .WithMethodName("ToPivotsList")
            .Build();

    /// <summary>
    /// Pivots Stream Listing
    /// </summary>
    internal static readonly IndicatorListing StreamListing =
        new CatalogListingBuilder(CommonListing)
            .WithStyle(Style.Stream)
            .WithMethodName("ToPivotsHub")
            .Build();
}
