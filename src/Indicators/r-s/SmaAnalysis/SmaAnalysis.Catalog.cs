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
            .AddResult(nameof(SmaAnalysisResult.Sma), "SMA", IndicatorResult.PricePane, ResultType.Default, isReusable: true)
            .AddResult(nameof(SmaAnalysisResult.Mad), "Mean Absolute Deviation", "Mad", ResultType.Default)
            .AddResult(nameof(SmaAnalysisResult.Mse), "Mean Square Error", "Mse", ResultType.Default)
            .AddResult(nameof(SmaAnalysisResult.Mape), "Mean Absolute Percentage Error", "Mape", ResultType.Default)
            .Build();

    /// <summary>
    /// SMA Analysis Series Listing
    /// </summary>
    internal static readonly IndicatorListing SeriesListing =
        new CatalogListingBuilder(CommonListing)
            .WithStyle(Style.Series)
            .WithMethodName(typeof(SmaAnalysis), "ToSmaAnalysis")
            .Build();

    /// <summary>
    /// SMA Analysis Stream Listing
    /// </summary>
    internal static readonly IndicatorListing StreamListing =
        new CatalogListingBuilder(CommonListing)
            .WithStyle(Style.Stream)
            .WithMethodName(typeof(SmaAnalysis), "ToSmaAnalysisHub")
            .Build();

    /// <summary>
    /// SMA Analysis Buffer Listing
    /// </summary>
    internal static readonly IndicatorListing BufferListing =
        new CatalogListingBuilder(CommonListing)
            .WithStyle(Style.Buffer)
            .WithMethodName(typeof(SmaAnalysis), "ToSmaAnalysisList")
            .Build();
}
