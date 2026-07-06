namespace FacioQuo.Stock.Indicators;

public static partial class Roc
{
    /// <summary>
    /// Rate of Change Common Base Listing
    /// </summary>
    internal static readonly IndicatorListing CommonListing =
        new CatalogListingBuilder()
            .WithName("Rate of Change")
            .WithId("ROC")
            .WithCategory(Category.Oscillator)
            .AddParameter<int>("lookbackPeriods", "Lookback Periods", description: "Number of periods for the ROC calculation", isRequired: false, defaultValue: 14, minimum: 1, maximum: 250)
            .AddResult("Roc", "ROC", ResultType.Default, isReusable: true)
            .Build();

    /// <summary>
    /// Rate of Change Series Listing
    /// </summary>
    internal static readonly IndicatorListing SeriesListing =
        new CatalogListingBuilder(CommonListing)
            .WithStyle(Style.Series)
            .WithMethodName("ToRoc")
            .Build();

    /// <summary>
    /// Rate of Change Stream Listing
    /// </summary>
    internal static readonly IndicatorListing StreamListing =
        new CatalogListingBuilder(CommonListing)
            .WithStyle(Style.Stream)
            .WithMethodName("ToRocHub")
            .Build();

    /// <summary>
    /// Rate of Change Buffer Listing
    /// </summary>
    internal static readonly IndicatorListing BufferListing =
        new CatalogListingBuilder(CommonListing)
            .WithStyle(Style.Buffer)
            .WithMethodName("ToRocList")
            .Build();
}
