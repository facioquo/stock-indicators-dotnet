namespace FacioQuo.Stock.Indicators;

public static partial class TtmSqueeze
{
    /// <summary>
    /// TTM-SQUEEZE Common Base Listing
    /// </summary>
    internal static readonly IndicatorListing CommonListing =
        new CatalogListingBuilder()
            .WithName("TTM Squeeze")
            .WithId("TTM-SQUEEZE")
            .WithCategory(Category.Oscillator)
            .AddParameter<int>("lookbackPeriods", "Lookback Periods", defaultValue: 20, minimum: 2, maximum: 250)
            .AddParameter<double>("standardDeviations", "Standard Deviations", defaultValue: 2.0, minimum: 0.01, maximum: 10.0)
            .AddParameter<double>("multiplier", "ATR Multiplier", defaultValue: 1.5, minimum: 0.01, maximum: 10.0)
            .AddResult(nameof(TtmSqueezeResult.Momentum), "Momentum", "TtmSqueeze", ResultType.Bar, isReusable: true)
            .AddResult(nameof(TtmSqueezeResult.IsSqueezeOn), "Squeeze On", null, ResultType.Default)
            .Build();

    /// <summary>
    /// TTM-SQUEEZE Series Listing
    /// </summary>
    internal static readonly IndicatorListing SeriesListing =
        new CatalogListingBuilder(CommonListing)
            .WithStyle(Style.Series)
            .WithMethodName(typeof(TtmSqueeze), "ToTtmSqueeze")
            .Build();

    /// <summary>
    /// TTM-SQUEEZE Buffer Listing
    /// </summary>
    internal static readonly IndicatorListing BufferListing =
        new CatalogListingBuilder(CommonListing)
            .WithStyle(Style.Buffer)
            .WithMethodName(typeof(TtmSqueeze), "ToTtmSqueezeList")
            .Build();

    /// <summary>
    /// TTM-SQUEEZE Stream Listing
    /// </summary>
    internal static readonly IndicatorListing StreamListing =
        new CatalogListingBuilder(CommonListing)
            .WithStyle(Style.Stream)
            .WithMethodName(typeof(TtmSqueeze), "ToTtmSqueezeHub")
            .Build();
}
