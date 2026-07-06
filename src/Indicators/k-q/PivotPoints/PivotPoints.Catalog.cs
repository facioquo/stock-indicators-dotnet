namespace FacioQuo.Stock.Indicators;

public static partial class PivotPoints
{
    /// <summary>
    /// Pivot Points Common Base Listing
    /// </summary>
    internal static readonly IndicatorListing CommonListing =
        new CatalogListingBuilder()
            .WithName("Pivot Points")
            .WithId("PIVOT-POINTS")
            .WithCategory(Category.PriceTrend)
            .AddEnumParameter<BarInterval>("windowSize", "Window Size", description: "Size of the window for pivot calculation", isRequired: false, defaultValue: BarInterval.Month)
            .AddEnumParameter<PivotPointType>("pointType", "Point Type", description: "Type of pivot points to calculate", isRequired: false, defaultValue: PivotPointType.Standard)
            .AddResult("R3", "Resistance 3", ResultType.Default)
            .AddResult("R2", "Resistance 2", ResultType.Default)
            .AddResult("R1", "Resistance 1", ResultType.Default)
            .AddResult("PP", "Pivot Point", ResultType.Default, isReusable: true)
            .AddResult("S1", "Support 1", ResultType.Default)
            .AddResult("S2", "Support 2", ResultType.Default)
            .AddResult("S3", "Support 3", ResultType.Default)
            .Build();

    /// <summary>
    /// Pivot Points Series Listing
    /// </summary>
    internal static readonly IndicatorListing SeriesListing =
        new CatalogListingBuilder(CommonListing)
            .WithStyle(Style.Series)
            .WithMethodName("ToPivotPoints")
            .Build();

    /// <summary>
    /// Pivot Points Buffer Listing
    /// </summary>
    internal static readonly IndicatorListing BufferListing =
        new CatalogListingBuilder(CommonListing)
            .WithStyle(Style.Buffer)
            .WithMethodName("ToPivotPointsList")
            .Build();

    /// <summary>
    /// Pivot Points Stream Listing
    /// </summary>
    internal static readonly IndicatorListing StreamListing =
        new CatalogListingBuilder(CommonListing)
            .WithStyle(Style.Stream)
            .WithMethodName("ToPivotPointsHub")
            .Build();
}
