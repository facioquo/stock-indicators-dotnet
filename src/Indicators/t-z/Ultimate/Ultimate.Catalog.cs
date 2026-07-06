namespace FacioQuo.Stock.Indicators;

public static partial class Ultimate
{
    /// <summary>
    /// Ultimate Oscillator Common Base Listing
    /// </summary>
    internal static readonly IndicatorListing CommonListing =
        new CatalogListingBuilder()
            .WithName("Ultimate Oscillator")
            .WithId("UO")
            .WithCategory(Category.Oscillator)
            .AddParameter<int>("shortPeriods", "Short Periods", description: "Number of short periods for the Ultimate Oscillator calculation", isRequired: false, defaultValue: 7, minimum: 1, maximum: 50)
            .AddParameter<int>("middlePeriods", "Middle Periods", description: "Number of middle periods for the Ultimate Oscillator calculation", isRequired: false, defaultValue: 14, minimum: 1, maximum: 50)
            .AddParameter<int>("longPeriods", "Long Periods", description: "Number of long periods for the Ultimate Oscillator calculation", isRequired: false, defaultValue: 28, minimum: 1, maximum: 250)
            .AddResult("Ultimate", "Ultimate Oscillator", ResultType.Default, isReusable: true)
            .Build();

    /// <summary>
    /// Ultimate Oscillator Series Listing
    /// </summary>
    internal static readonly IndicatorListing SeriesListing =
        new CatalogListingBuilder(CommonListing)
            .WithStyle(Style.Series)
            .WithMethodName("ToUltimate")
            .Build();

    /// <summary>
    /// Ultimate Oscillator Stream Listing
    /// </summary>
    internal static readonly IndicatorListing StreamListing =
        new CatalogListingBuilder(CommonListing)
            .WithStyle(Style.Stream)
            .WithMethodName("ToUltimateHub")
            .Build();

    /// <summary>
    /// Ultimate Oscillator Buffer Listing
    /// </summary>
    internal static readonly IndicatorListing BufferListing =
        new CatalogListingBuilder(CommonListing)
            .WithStyle(Style.Buffer)
            .WithMethodName("ToUltimateList")
            .Build();
}
