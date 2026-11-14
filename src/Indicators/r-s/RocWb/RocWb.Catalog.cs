namespace FacioQuo.Stock.Indicators;

public static partial class RocWb
{
    /// <summary>
    /// Rate of Change with Bands Common Base Listing
    /// </summary>
    internal static readonly IndicatorListing CommonListing =
        new CatalogListingBuilder()
            .WithName("Rate of Change with Bands")
            .WithId("ROC-WB")
            .WithCategory(Category.PriceCharacteristic)
            .AddParameter<int>("lookbackPeriods", "Lookback Periods", description: "Number of periods for the ROC calculation", isRequired: false, defaultValue: 20, minimum: 1, maximum: 250)
            .AddParameter<int>("emaPeriods", "EMA Periods", description: "Number of periods for the EMA calculation", isRequired: false, defaultValue: 5, minimum: 1, maximum: 100)
            .AddParameter<int>("stdDevPeriods", "Standard Deviation Periods", description: "Number of periods for the standard deviation calculation", isRequired: false, defaultValue: 5, minimum: 1, maximum: 100)
            .AddResult(nameof(RocWbResult.Roc), "ROC", "RocWb", ResultType.Default, isReusable: true)
            .AddResult(nameof(RocWbResult.RocEma), "ROC EMA", "RocWb", ResultType.Default)
            .AddResult(nameof(RocWbResult.UpperBand), "Upper Band", "RocWb", ResultType.Default)
            .AddResult(nameof(RocWbResult.LowerBand), "Lower Band", "RocWb", ResultType.Default)
            .Build();

    /// <summary>
    /// Rate of Change with Bands Series Listing
    /// </summary>
    internal static readonly IndicatorListing SeriesListing =
        new CatalogListingBuilder(CommonListing)
            .WithStyle(Style.Series)
            .WithMethodName(typeof(RocWb), "ToRocWb")
            .Build();

    /// <summary>
    /// Rate of Change with Bands Stream Listing
    /// </summary>
    internal static readonly IndicatorListing StreamListing =
        new CatalogListingBuilder(CommonListing)
            .WithStyle(Style.Stream)
            .WithMethodName(typeof(RocWb), "ToRocWbHub")
            .Build();

    /// <summary>
    /// Rate of Change with Bands Buffer Listing
    /// </summary>
    internal static readonly IndicatorListing BufferListing =
        new CatalogListingBuilder(CommonListing)
            .WithStyle(Style.Buffer)
            .WithMethodName(typeof(RocWb), "ToRocWbList")
            .Build();
}
