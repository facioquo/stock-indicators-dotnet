namespace FacioQuo.Stock.Indicators;

public static partial class Pvo
{
    /// <summary>
    /// Price Volume Oscillator Common Base Listing
    /// </summary>
    internal static readonly IndicatorListing CommonListing =
        new CatalogListingBuilder()
            .WithName("Price Volume Oscillator")
            .WithId("PVO")
            .WithCategory(Category.VolumeBased)
            .AddParameter<int>("fastPeriods", "Fast Periods", description: "Number of periods for the fast EMA", isRequired: false, defaultValue: 12, minimum: 1, maximum: 100)
            .AddParameter<int>("slowPeriods", "Slow Periods", description: "Number of periods for the slow EMA", isRequired: false, defaultValue: 26, minimum: 1, maximum: 250)
            .AddParameter<int>("signalPeriods", "Signal Periods", description: "Number of periods for the signal line", isRequired: false, defaultValue: 9, minimum: 1, maximum: 50)
            .AddResult("Pvo", "PVO", ResultType.Default, isReusable: true)
            .AddResult("Signal", "Signal", ResultType.Default)
            .AddResult("Histogram", "Histogram", ResultType.Default)
            .Build();

    /// <summary>
    /// Price Volume Oscillator Series Listing
    /// </summary>
    internal static readonly IndicatorListing SeriesListing =
        new CatalogListingBuilder(CommonListing)
            .WithStyle(Style.Series)
            .WithMethodName("ToPvo")
            .Build();

    /// <summary>
    /// Price Volume Oscillator Buffer Listing
    /// </summary>
    internal static readonly IndicatorListing BufferListing =
        new CatalogListingBuilder(CommonListing)
            .WithStyle(Style.Buffer)
            .WithMethodName("ToPvoList")
            .Build();

    /// <summary>
    /// Price Volume Oscillator Stream Listing
    /// </summary>
    internal static readonly IndicatorListing StreamListing =
        new CatalogListingBuilder(CommonListing)
            .WithStyle(Style.Stream)
            .WithMethodName("ToPvoHub")
            .Build();
}
