namespace FacioQuo.Stock.Indicators;

public static partial class Macd
{
    /// <summary>
    /// MACD Common Base Listing
    /// </summary>
    internal static readonly IndicatorListing CommonListing =
        new CatalogListingBuilder()
            .WithName("Moving Average Convergence/Divergence")
            .WithId("MACD")
            .WithCategory(Category.PriceTrend)
            .AddParameter<int>("fastPeriods", "Fast Periods", description: "Number of periods for the fast EMA", isRequired: false, defaultValue: 12, minimum: 1, maximum: 200)
            .AddParameter<int>("slowPeriods", "Slow Periods", description: "Number of periods for the slow EMA", isRequired: false, defaultValue: 26, minimum: 1, maximum: 250)
            .AddParameter<int>("signalPeriods", "Signal Periods", description: "Number of periods for the signal line", isRequired: false, defaultValue: 9, minimum: 1, maximum: 50)
            .AddResult("Macd", "MACD", ResultType.Default, isReusable: true)
            .AddResult("Signal", "Signal", ResultType.Default)
            .AddResult("Histogram", "Histogram", ResultType.Bar)
            .Build();

    /// <summary>
    /// MACD Series Listing
    /// </summary>
    internal static readonly IndicatorListing SeriesListing =
        new CatalogListingBuilder(CommonListing)
            .WithStyle(Style.Series)
            .WithMethodName("ToMacd")
            .Build();

    /// <summary>
    /// MACD Stream Listing
    /// </summary>
    internal static readonly IndicatorListing StreamListing =
        new CatalogListingBuilder(CommonListing)
            .WithStyle(Style.Stream)
            .WithMethodName("ToMacdHub")
            .Build();

    /// <summary>
    /// MACD Buffer Listing
    /// </summary>
    internal static readonly IndicatorListing BufferListing =
        new CatalogListingBuilder(CommonListing)
            .WithStyle(Style.Buffer)
            .WithMethodName("ToMacdList")
            .Build();
}
