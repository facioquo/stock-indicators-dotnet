namespace FacioQuo.Stock.Indicators;

public static partial class Vortex
{
    /// <summary>
    /// Vortex Indicator Common Base Listing
    /// </summary>
    internal static readonly IndicatorListing CommonListing =
        new CatalogListingBuilder()
            .WithName("Vortex Indicator")
            .WithId("VORTEX")
            .WithCategory(Category.PriceTrend)
            .AddParameter<int>("lookbackPeriods", "Lookback Periods", description: "Number of periods for the Vortex calculation", isRequired: false, defaultValue: 14, minimum: 2, maximum: 100)
            .AddResult("Pvi", "VI+", ResultType.Default, isReusable: true)
            .AddResult("Nvi", "VI-", ResultType.Default)
            .Build();

    /// <summary>
    /// Vortex Indicator Series Listing
    /// </summary>
    internal static readonly IndicatorListing SeriesListing =
        new CatalogListingBuilder(CommonListing)
            .WithStyle(Style.Series)
            .WithMethodName("ToVortex")
            .Build();

    /// <summary>
    /// Vortex Indicator Buffer Listing
    /// </summary>
    internal static readonly IndicatorListing BufferListing =
        new CatalogListingBuilder(CommonListing)
            .WithStyle(Style.Buffer)
            .WithMethodName("ToVortexList")
            .Build();

    /// <summary>
    /// Vortex Indicator Stream Listing
    /// </summary>
    internal static readonly IndicatorListing StreamListing =
        new CatalogListingBuilder(CommonListing)
            .WithStyle(Style.Stream)
            .WithMethodName("ToVortexHub")
            .Build();
}
