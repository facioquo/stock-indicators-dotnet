namespace FacioQuo.Stock.Indicators;

public static partial class Stoch
{
    /// <summary>
    /// Stochastic Oscillator Common Base Listing
    /// </summary>
    internal static readonly IndicatorListing CommonListing =
        new CatalogListingBuilder()
            .WithName("Stochastic Oscillator")
            .WithId("STOCH")
            .WithCategory(Category.Oscillator)
            .AddParameter<int>("lookbackPeriods", "Lookback Periods", description: "Number of periods for the stochastic calculation", isRequired: false, defaultValue: 14, minimum: 1, maximum: 250)
            .AddParameter<int>("signalPeriods", "Signal Periods", description: "Number of periods for the signal line", isRequired: false, defaultValue: 3, minimum: 1, maximum: 250)
            .AddParameter<int>("smoothPeriods", "Smooth Periods", description: "Number of periods for smoothing", isRequired: false, defaultValue: 3, minimum: 1, maximum: 50)
            .AddResult("Oscillator", "%K", ResultType.Default, isReusable: true)
            .AddResult("Signal", "%D", ResultType.Default)
            .Build();

    /// <summary>
    /// Stochastic Oscillator Series Listing
    /// </summary>
    internal static readonly IndicatorListing SeriesListing =
        new CatalogListingBuilder(CommonListing)
            .WithStyle(Style.Series)
            .WithMethodName("ToStoch")
            .Build();

    /// <summary>
    /// Stochastic Oscillator Stream Listing
    /// </summary>
    internal static readonly IndicatorListing StreamListing =
        new CatalogListingBuilder(CommonListing)
            .WithStyle(Style.Stream)
            .WithMethodName("ToStochHub")
            .Build();

    /// <summary>
    /// Stochastic Oscillator Buffer Listing
    /// </summary>
    internal static readonly IndicatorListing BufferListing =
        new CatalogListingBuilder(CommonListing)
            .WithStyle(Style.Buffer)
            .WithMethodName("ToStochList")
            .Build();
}
