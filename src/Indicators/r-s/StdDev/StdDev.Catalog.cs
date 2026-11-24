namespace FacioQuo.Stock.Indicators;

public static partial class StdDev
{
    /// <summary>
    /// Standard deviation Common Base Listing
    /// </summary>
    internal static readonly IndicatorListing CommonListing =
        new CatalogListingBuilder()
            .WithName("Standard Deviation")
            .WithId("STDEV")
            .WithCategory(Category.PriceCharacteristic)
            .AddParameter<int>("lookbackPeriods", "Lookback Periods", description: "Number of periods for the standard deviation calculation", isRequired: false, defaultValue: 14, minimum: 2, maximum: 250)
            .AddResult(nameof(StdDevResult.StdDev), "Standard Deviation", "StdDev", ResultType.Default, isReusable: true)
            .AddResult(nameof(StdDevResult.Mean), "Mean", IndicatorResult.PricePane, ResultType.Default)
            .AddResult(nameof(StdDevResult.ZScore), "Z-Score", "ZScore", ResultType.Default)
            .Build();

    /// <summary>
    /// Standard deviation Series Listing
    /// </summary>
    internal static readonly IndicatorListing SeriesListing =
        new CatalogListingBuilder(CommonListing)
            .WithStyle(Style.Series)
            .WithMethodName(typeof(StdDev), "ToStdDev")
            .Build();

    /// <summary>
    /// Standard deviation Stream Listing
    /// </summary>
    internal static readonly IndicatorListing StreamListing =
        new CatalogListingBuilder(CommonListing)
            .WithStyle(Style.Stream)
            .WithMethodName(typeof(StdDev), "ToStdDevHub")
            .Build();

    /// <summary>
    /// Standard deviation Buffer Listing
    /// </summary>
    internal static readonly IndicatorListing BufferListing =
        new CatalogListingBuilder(CommonListing)
            .WithStyle(Style.Buffer)
            .WithMethodName(typeof(StdDev), "ToStdDevList")
            .Build();
}
