namespace FacioQuo.Stock.Indicators;

public static partial class StochRsi
{
    /// <summary>
    /// Stochastic RSI Common Base Listing
    /// </summary>
    internal static readonly IndicatorListing CommonListing =
        new CatalogListingBuilder()
            .WithName("Stochastic RSI")
            .WithId("STOCH-RSI")
            .WithCategory(Category.Oscillator)
            .AddParameter<int>("rsiPeriods", "RSI Periods", description: "Number of periods for the RSI calculation", isRequired: false, defaultValue: 14, minimum: 1, maximum: 250)
            .AddParameter<int>("stochPeriods", "Stochastic Periods", description: "Number of periods for the Stochastic calculation", isRequired: false, defaultValue: 14, minimum: 1, maximum: 250)
            .AddParameter<int>("signalPeriods", "Signal Periods", description: "Number of periods for the signal line", isRequired: false, defaultValue: 3, minimum: 1, maximum: 50)
            .AddParameter<int>("smoothPeriods", "Smooth Periods", description: "Number of periods for smoothing", isRequired: false, defaultValue: 1, minimum: 1, maximum: 50)
            .AddResult("StochRsi", "%K", ResultType.Default, isReusable: true)
            .AddResult("Signal", "%D", ResultType.Default)
            .Build();

    /// <summary>
    /// Stochastic RSI Series Listing
    /// </summary>
    internal static readonly IndicatorListing SeriesListing =
        new CatalogListingBuilder(CommonListing)
            .WithStyle(Style.Series)
            .WithMethodName("ToStochRsi")
            .Build();

    /// <summary>
    /// Stochastic RSI Stream Listing
    /// </summary>
    internal static readonly IndicatorListing StreamListing =
        new CatalogListingBuilder(CommonListing)
            .WithStyle(Style.Stream)
            .WithMethodName("ToStochRsiHub")
            .Build();

    /// <summary>
    /// Stochastic RSI Buffer Listing
    /// </summary>
    internal static readonly IndicatorListing BufferListing =
        new CatalogListingBuilder(CommonListing)
            .WithStyle(Style.Buffer)
            .WithMethodName("ToStochRsiList")
            .Build();
}
