namespace FacioQuo.Stock.Indicators;

public static partial class Awesome
{
    /// <summary>
    /// AWESOME Common Base Listing
    /// </summary>
    internal static readonly IndicatorListing CommonListing =
        new CatalogListingBuilder()
            .WithName("Awesome Oscillator")
            .WithId("AWESOME")
            .WithCategory(Category.Oscillator)
            .AddParameter<int>("fastPeriods", "Fast Periods", defaultValue: 5, minimum: 1, maximum: 100)
            .AddParameter<int>("slowPeriods", "Slow Periods", defaultValue: 34, minimum: 1, maximum: 250)
            .AddResult("Oscillator", "Oscillator", ResultType.Default, isReusable: true)
            .AddResult("Normalized", "Normalized", ResultType.Default)
            .Build();

    /// <summary>
    /// AWESOME Series Listing
    /// </summary>
    internal static readonly IndicatorListing SeriesListing =
        new CatalogListingBuilder(CommonListing)
            .WithStyle(Style.Series)
            .WithMethodName("ToAwesome")
            .Build();

    /// <summary>
    /// AWESOME Stream Listing
    /// </summary>
    internal static readonly IndicatorListing StreamListing =
        new CatalogListingBuilder(CommonListing)
            .WithStyle(Style.Stream)
            .WithMethodName("ToAwesomeHub")
            .Build();

    /// <summary>
    /// AWESOME Buffer Listing
    /// </summary>
    internal static readonly IndicatorListing BufferListing =
        new CatalogListingBuilder(CommonListing)
            .WithStyle(Style.Buffer)
            .WithMethodName("ToAwesomeList")
            .Build();
}
