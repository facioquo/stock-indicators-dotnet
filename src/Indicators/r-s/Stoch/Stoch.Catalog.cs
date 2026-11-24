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
            .AddParameter<int>("lookbackPeriods", "Lookback Periods (%K)", description: "Number of periods for the stochastic calculation", isRequired: false, defaultValue: 14, minimum: 1, maximum: 250)
            .AddParameter<int>("signalPeriods", "Signal Periods (%D)", description: "Number of periods for the signal line", isRequired: false, defaultValue: 3, minimum: 1, maximum: 250)
            .AddParameter<int>("smoothPeriods", "Smooth Periods", description: "Number of periods for smoothing", isRequired: false, defaultValue: 3, minimum: 1, maximum: 50)
            .AddParameter<double>("kFactor", "K Factor", description: "Weight of %K in the %J calculation", isRequired: true, defaultValue: 3.0, minimum: 0.1, maximum: 10.0)
            .AddParameter<double>("dFactor", "D Factor", description: "Weight of %D in the %J calculation", isRequired: true, defaultValue: 2.0, minimum: 0.1, maximum: 10.0)
            .AddEnumParameter<MaType>("movingAverageType", "Moving Average Type", description: "Moving average used to smooth %K and %D: SMA or SMMA", isRequired: true, defaultValue: MaType.SMA)
            .AddResult(nameof(StochResult.Oscillator), "%K", "Stoch", ResultType.Default, isReusable: true)
            .AddResult(nameof(StochResult.Signal), "%D", "Stoch", ResultType.Default)
            .AddResult(nameof(StochResult.PercentJ), "%J", "Stoch", ResultType.Default)
            .WithPane("Stoch", null, null, 80, 20)
            .Build();

    /// <summary>
    /// Stochastic Oscillator Series Listing
    /// </summary>
    internal static readonly IndicatorListing SeriesListing =
        new CatalogListingBuilder(CommonListing)
            .WithStyle(Style.Series)
            .WithMethodName(typeof(Stoch), "ToStoch")
            .Build();

    /// <summary>
    /// Stochastic Oscillator Stream Listing
    /// </summary>
    internal static readonly IndicatorListing StreamListing =
        new CatalogListingBuilder(CommonListing)
            .WithStyle(Style.Stream)
            .WithMethodName(typeof(Stoch), "ToStochHub")
            .Build();

    /// <summary>
    /// Stochastic Oscillator Buffer Listing
    /// </summary>
    internal static readonly IndicatorListing BufferListing =
        new CatalogListingBuilder(CommonListing)
            .WithStyle(Style.Buffer)
            .WithMethodName(typeof(Stoch), "ToStochList")
            .Build();
}
