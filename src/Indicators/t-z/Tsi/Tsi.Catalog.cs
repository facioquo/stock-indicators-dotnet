namespace FacioQuo.Stock.Indicators;

public static partial class Tsi
{
    /// <summary>
    /// True Strength Index Common Base Listing
    /// </summary>
    internal static readonly IndicatorListing CommonListing =
        new CatalogListingBuilder()
            .WithName("True Strength Index")
            .WithId("TSI")
            .WithCategory(Category.Oscillator)
            .AddParameter<int>("lookbackPeriods", "Lookback Periods", description: "Number of periods for the TSI calculation", isRequired: false, defaultValue: 25, minimum: 1, maximum: 250)
            .AddParameter<int>("smoothPeriods", "Smooth Periods", description: "Number of periods for smoothing", isRequired: false, defaultValue: 13, minimum: 1, maximum: 250)
            .AddParameter<int>("signalPeriods", "Signal Periods", description: "Number of periods for the signal line", isRequired: false, defaultValue: 7, minimum: 1, maximum: 50)
            .AddResult("Tsi", "TSI", ResultType.Default, isReusable: true)
            .AddResult("Signal", "Signal", ResultType.Default)
            .Build();

    /// <summary>
    /// True Strength Index Series Listing
    /// </summary>
    internal static readonly IndicatorListing SeriesListing =
        new CatalogListingBuilder(CommonListing)
            .WithStyle(Style.Series)
            .WithMethodName("ToTsi")
            .Build();

    /// <summary>
    /// True Strength Index Buffer Listing
    /// </summary>
    internal static readonly IndicatorListing BufferListing =
        new CatalogListingBuilder(CommonListing)
            .WithStyle(Style.Buffer)
            .WithMethodName("ToTsiList")
            .Build();

    /// <summary>
    /// True Strength Index Stream Listing
    /// </summary>
    internal static readonly IndicatorListing StreamListing =
        new CatalogListingBuilder(CommonListing)
            .WithStyle(Style.Stream)
            .WithMethodName("ToTsiHub")
            .Build();
}
