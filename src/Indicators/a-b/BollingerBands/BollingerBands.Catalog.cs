namespace FacioQuo.Stock.Indicators;

public static partial class BollingerBands
{
    /// <summary>
    /// BB Common Base Listing
    /// </summary>
    internal static readonly IndicatorListing CommonListing =
        new CatalogListingBuilder()
            .WithName("Bollinger Bands®")
            .WithId("BB")
            .WithCategory(Category.PriceChannel)
            .AddParameter<int>("lookbackPeriods", "Lookback Periods", defaultValue: 20, minimum: 2, maximum: 250)
            .AddParameter<double>("standardDeviations", "Standard Deviations", defaultValue: 2.0, minimum: 0.01, maximum: 10.0)
            .AddResult(nameof(BollingerBandsResult.Sma), "Centerline (SMA)", ResultType.Default)
            .AddResult(nameof(BollingerBandsResult.UpperBand), "Upper Band", ResultType.Default)
            .AddResult(nameof(BollingerBandsResult.LowerBand), "Lower Band", ResultType.Default)
            .AddResult(nameof(BollingerBandsResult.PercentB), "%B", ResultType.Default, isReusable: true)
            .AddResult(nameof(BollingerBandsResult.ZScore), "Z-score", ResultType.Default)
            .AddResult(nameof(BollingerBandsResult.Width), "Width", ResultType.Default)
            .Build();

    /// <summary>
    /// BB Series Listing
    /// </summary>
    internal static readonly IndicatorListing SeriesListing =
        new CatalogListingBuilder(CommonListing)
            .WithStyle(Style.Series)
            .WithMethodName("ToBollingerBands")
            .Build();

    /// <summary>
    /// BB Stream Listing
    /// </summary>
    internal static readonly IndicatorListing StreamListing =
        new CatalogListingBuilder(CommonListing)
            .WithStyle(Style.Stream)
            .WithMethodName("ToBollingerBandsHub")
            .Build();

    /// <summary>
    /// BB Buffer Listing
    /// </summary>
    internal static readonly IndicatorListing BufferListing =
        new CatalogListingBuilder(CommonListing)
            .WithStyle(Style.Buffer)
            .WithMethodName("ToBollingerBandsList")
            .Build();
}
