namespace FacioQuo.Stock.Indicators;

public static partial class SmaAnalysis
{
    /// <summary>
    /// SMA Analysis Common Base Listing
    /// </summary>
    internal static readonly IndicatorListing CommonListing =
        new CatalogListingBuilder()
            .WithName("Simple Moving Average Analysis")
            .WithId("SMA-ANALYSIS")
            .WithCategory(Category.PriceCharacteristic)
            .AddParameter<int>("lookbackPeriods", "Lookback Periods", description: "Number of periods for the SMA analysis", isRequired: true, defaultValue: 20, minimum: 1, maximum: 250)
            .AddResult("Sma", "SMA", ResultType.Default, isReusable: true)
            .AddResult("Mad", "Mean absolute deviation", ResultType.Default)
            .AddResult("Mse", "Mean square error", ResultType.Default)
            .AddResult("Mape", "Mean absolute percentage error", ResultType.Default)
            .Build();

    /// <summary>
    /// SMA Analysis Series Listing
    /// </summary>
    internal static readonly IndicatorListing SeriesListing =
        new CatalogListingBuilder(CommonListing)
            .WithStyle(Style.Series)
            .WithMethodName("ToSmaAnalysis")
            .Build();

    /// <summary>
    /// SMA Analysis Stream Listing
    /// </summary>
    internal static readonly IndicatorListing StreamListing =
        new CatalogListingBuilder(CommonListing)
            .WithStyle(Style.Stream)
            .WithMethodName("ToSmaAnalysisHub")
            .Build();

    /// <summary>
    /// SMA Analysis Buffer Listing
    /// </summary>
    internal static readonly IndicatorListing BufferListing =
        new CatalogListingBuilder(CommonListing)
            .WithStyle(Style.Buffer)
            .WithMethodName("ToSmaAnalysisList")
            .Build();
}
