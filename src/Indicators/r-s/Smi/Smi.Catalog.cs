namespace FacioQuo.Stock.Indicators;

public static partial class Smi
{
    /// <summary>
    /// Stochastic Momentum Index Common Base Listing
    /// </summary>
    internal static readonly IndicatorListing CommonListing =
        new CatalogListingBuilder()
            .WithName("Stochastic Momentum Index")
            .WithId("SMI")
            .WithCategory(Category.Oscillator)
            .AddParameter<int>("lookbackPeriods", "Lookback Periods", description: "Number of periods for the SMI calculation", isRequired: false, defaultValue: 13, minimum: 1, maximum: 300)
            .AddParameter<int>("firstSmoothPeriods", "First Smooth Periods", description: "Number of periods for the first smoothing", isRequired: false, defaultValue: 25, minimum: 1, maximum: 300)
            .AddParameter<int>("secondSmoothPeriods", "Second Smooth Periods", description: "Number of periods for the second smoothing", isRequired: false, defaultValue: 2, minimum: 1, maximum: 50)
            .AddParameter<int>("signalPeriods", "Signal Periods", description: "Number of periods for the signal line", isRequired: false, defaultValue: 3, minimum: 1, maximum: 50)
            .AddResult("Smi", "SMI", ResultType.Default, isReusable: true)
            .AddResult("Signal", "Signal", ResultType.Default)
            .Build();

    /// <summary>
    /// Stochastic Momentum Index Series Listing
    /// </summary>
    internal static readonly IndicatorListing SeriesListing =
        new CatalogListingBuilder(CommonListing)
            .WithStyle(Style.Series)
            .WithMethodName("ToSmi")
            .Build();

    /// <summary>
    /// Stochastic Momentum Index StreamHub Listing
    /// </summary>
    internal static readonly IndicatorListing StreamListing =
        new CatalogListingBuilder(CommonListing)
            .WithStyle(Style.Stream)
            .WithMethodName("ToSmiHub")
            .Build();

    /// <summary>
    /// Stochastic Momentum Index BufferList Listing
    /// </summary>
    internal static readonly IndicatorListing BufferListing =
        new CatalogListingBuilder(CommonListing)
            .WithStyle(Style.Buffer)
            .WithMethodName("ToSmiList")
            .Build();
}
