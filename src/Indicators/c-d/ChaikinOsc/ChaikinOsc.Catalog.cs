namespace FacioQuo.Stock.Indicators;

public static partial class ChaikinOsc
{
    /// <summary>
    /// CHAIKIN-OSC Common Base Listing
    /// </summary>
    internal static readonly IndicatorListing CommonListing =
        new CatalogListingBuilder()
            .WithName("Chaikin Oscillator")
            .WithId("CHAIKIN-OSC")
            .WithCategory(Category.VolumeBased)
            .AddParameter<int>("fastPeriods", "Fast Periods", defaultValue: 3, minimum: 1, maximum: 100)
            .AddParameter<int>("slowPeriods", "Slow Periods", defaultValue: 10, minimum: 1, maximum: 100)
            .AddResult(nameof(ChaikinOscResult.MoneyFlowMultiplier), "Money Flow Multiplier", "MoneyFlowMultiplier", ResultType.Default)
            .AddResult(nameof(ChaikinOscResult.MoneyFlowVolume), "Money Flow Volume", "MoneyFlowVolume", ResultType.Default)
            .AddResult(nameof(ChaikinOscResult.Adl), "ADL", "Adl", ResultType.Default)
            .AddResult(nameof(ChaikinOscResult.Oscillator), "Oscillator", "Oscillator", ResultType.Default, isReusable: true)
            .Build();

    /// <summary>
    /// CHAIKIN-OSC Series Listing
    /// </summary>
    internal static readonly IndicatorListing SeriesListing =
        new CatalogListingBuilder(CommonListing)
            .WithStyle(Style.Series)
            .WithMethodName(typeof(ChaikinOsc), "ToChaikinOsc")
            .Build();

    /// <summary>
    /// CHAIKIN-OSC Stream Listing
    /// </summary>
    internal static readonly IndicatorListing StreamListing =
        new CatalogListingBuilder(CommonListing)
            .WithStyle(Style.Stream)
            .WithMethodName(typeof(ChaikinOsc), "ToChaikinOscHub")
            .Build();

    /// <summary>
    /// CHAIKIN-OSC Buffer Listing
    /// </summary>
    internal static readonly IndicatorListing BufferListing =
        new CatalogListingBuilder(CommonListing)
            .WithStyle(Style.Buffer)
            .WithMethodName(typeof(ChaikinOsc), "ToChaikinOscList")
            .Build();
}
