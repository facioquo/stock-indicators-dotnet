namespace FacioQuo.Stock.Indicators;

public static partial class WilliamsR
{
    /// <summary>
    /// Williams %R Common Base Listing
    /// </summary>
    internal static readonly IndicatorListing CommonListing =
        new CatalogListingBuilder()
            .WithName("Williams %R")
            .WithId("WILLR")
            .WithCategory(Category.Oscillator)
            .AddParameter<int>("lookbackPeriods", "Lookback Periods", description: "Number of periods for the Williams %R calculation", isRequired: false, defaultValue: 14, minimum: 1, maximum: 250)
            .AddResult("WilliamsR", "Williams %R", ResultType.Default, isReusable: true)
            .Build();

    /// <summary>
    /// Williams %R Series Listing
    /// </summary>
    internal static readonly IndicatorListing SeriesListing =
        new CatalogListingBuilder(CommonListing)
            .WithStyle(Style.Series)
            .WithMethodName("ToWilliamsR")
            .Build();

    /// <summary>
    /// Williams %R Stream Listing
    /// </summary>
    internal static readonly IndicatorListing StreamListing =
        new CatalogListingBuilder(CommonListing)
            .WithStyle(Style.Stream)
            .WithMethodName("ToWilliamsRHub")
            .Build();

    /// <summary>
    /// Williams %R Buffer Listing
    /// </summary>
    internal static readonly IndicatorListing BufferListing =
        new CatalogListingBuilder(CommonListing)
            .WithStyle(Style.Buffer)
            .WithMethodName("ToWilliamsRList")
            .Build();
}
