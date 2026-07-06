namespace FacioQuo.Stock.Indicators;

public static partial class RocWb
{
    /// <summary>
    /// ROC with Bands Common Base Listing
    /// </summary>
    internal static readonly IndicatorListing CommonListing =
        new CatalogListingBuilder()
            .WithName("ROC with Bands")
            .WithId("ROC-WB")
            .WithCategory(Category.PriceTrend)
            .AddParameter<int>("lookbackPeriods", "Lookback Periods", description: "Number of periods for the ROC calculation", isRequired: false, defaultValue: 20, minimum: 1, maximum: 250)
            .AddParameter<int>("emaPeriods", "EMA Periods", description: "Number of periods for the EMA calculation", isRequired: false, defaultValue: 5, minimum: 1, maximum: 100)
            .AddParameter<int>("stdDevPeriods", "Standard deviation Periods", description: "Number of periods for the standard deviation calculation", isRequired: false, defaultValue: 5, minimum: 1, maximum: 100)
            .AddResult("Roc", "ROC", ResultType.Default, isReusable: true)
            .AddResult("RocEma", "ROC EMA", ResultType.Default)
            .AddResult("UpperBand", "Upper Band", ResultType.Default)
            .AddResult("LowerBand", "Lower Band", ResultType.Default)
            .Build();

    /// <summary>
    /// ROC with Bands Series Listing
    /// </summary>
    internal static readonly IndicatorListing SeriesListing =
        new CatalogListingBuilder(CommonListing)
            .WithStyle(Style.Series)
            .WithMethodName("ToRocWb")
            .Build();

    /// <summary>
    /// ROC with Bands Stream Listing
    /// </summary>
    internal static readonly IndicatorListing StreamListing =
        new CatalogListingBuilder(CommonListing)
            .WithStyle(Style.Stream)
            .WithMethodName("ToRocWbHub")
            .Build();

    /// <summary>
    /// ROC with Bands Buffer Listing
    /// </summary>
    internal static readonly IndicatorListing BufferListing =
        new CatalogListingBuilder(CommonListing)
            .WithStyle(Style.Buffer)
            .WithMethodName("ToRocWbList")
            .Build();
}
