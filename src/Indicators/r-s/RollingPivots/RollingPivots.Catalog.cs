namespace FacioQuo.Stock.Indicators;

public static partial class RollingPivots
{
    /// <summary>
    /// Rolling Pivots Common Base Listing
    /// </summary>
    internal static readonly IndicatorListing CommonListing =
        new CatalogListingBuilder()
            .WithName("Rolling Pivots")
            .WithId("ROLLING-PIVOTS")
            .WithCategory(Category.PriceTrend)
            .AddParameter<int>("windowPeriods", "Window Periods", description: "Number of periods for the rolling window", isRequired: false, defaultValue: 20, minimum: 1, maximum: 250)
            .AddParameter<int>("offsetPeriods", "Offset Periods", description: "Number of periods to offset the pivots", isRequired: false, defaultValue: 0, minimum: 0, maximum: 100)
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
    /// Rolling Pivots Series Listing
    /// </summary>
    internal static readonly IndicatorListing SeriesListing =
        new CatalogListingBuilder(CommonListing)
            .WithStyle(Style.Series)
            .WithMethodName("ToRollingPivots")
            .Build();

    /// <summary>
    /// Rolling Pivots Stream Listing
    /// </summary>
    internal static readonly IndicatorListing StreamListing =
        new CatalogListingBuilder(CommonListing)
            .WithStyle(Style.Stream)
            .WithMethodName("ToRollingPivotsHub")
            .Build();

    /// <summary>
    /// Rolling Pivots Buffer Listing
    /// </summary>
    internal static readonly IndicatorListing BufferListing =
        new CatalogListingBuilder(CommonListing)
            .WithStyle(Style.Buffer)
            .WithMethodName("ToRollingPivotsList")
            .Build();
}
