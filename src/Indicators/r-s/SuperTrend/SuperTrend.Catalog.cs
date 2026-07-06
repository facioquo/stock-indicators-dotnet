namespace FacioQuo.Stock.Indicators;

public static partial class SuperTrend
{
    /// <summary>
    /// SuperTrend Common Base Listing
    /// </summary>
    internal static readonly IndicatorListing CommonListing =
        new CatalogListingBuilder()
            .WithName("SuperTrend")
            .WithId("SUPERTREND")
            .WithCategory(Category.PriceTrend)
            .AddParameter<int>("lookbackPeriods", "Lookback Periods", description: "Number of periods for the SuperTrend calculation", isRequired: false, defaultValue: 10, minimum: 1, maximum: 50)
            .AddParameter<double>("multiplier", "Multiplier", description: "Multiplier for the ATR calculation", isRequired: false, defaultValue: 3.0, minimum: 0.1, maximum: 10.0)
            .AddResult("SuperTrend", "SuperTrend", ResultType.Default, isReusable: false)
            .AddResult("UpperBand", "Upper Band", ResultType.Default)
            .AddResult("LowerBand", "Lower Band", ResultType.Default)
            .Build();

    /// <summary>
    /// SuperTrend Series Listing
    /// </summary>
    internal static readonly IndicatorListing SeriesListing =
        new CatalogListingBuilder(CommonListing)
            .WithStyle(Style.Series)
            .WithMethodName("ToSuperTrend")
            .Build();

    /// <summary>
    /// SuperTrend Stream Listing
    /// </summary>
    internal static readonly IndicatorListing StreamListing =
        new CatalogListingBuilder(CommonListing)
            .WithStyle(Style.Stream)
            .WithMethodName("ToSuperTrendHub")
            .Build();

    /// <summary>
    /// SuperTrend Buffer Listing
    /// </summary>
    internal static readonly IndicatorListing BufferListing =
        new CatalogListingBuilder(CommonListing)
            .WithStyle(Style.Buffer)
            .WithMethodName("ToSuperTrendList")
            .Build();
}
