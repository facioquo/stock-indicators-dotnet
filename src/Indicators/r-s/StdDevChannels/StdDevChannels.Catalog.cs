namespace FacioQuo.Stock.Indicators;

public static partial class StdDevChannels
{
    /// <summary>
    /// Standard Deviation Channels Common Base Listing
    /// </summary>
    internal static readonly IndicatorListing CommonListing =
        new CatalogListingBuilder()
            .WithName("Standard Deviation Channels")
            .WithId("STDEV-CHANNELS")
            .WithCategory(Category.PriceChannel)
            .AddParameter<int>("lookbackPeriods", "Lookback Periods", description: "Number of periods for the standard deviation calculation", isRequired: false, defaultValue: 20, minimum: 1, maximum: 250)
            .AddParameter<double>("stdDeviations", "Standard Deviations", description: "Number of standard deviations for the channels", isRequired: false, defaultValue: 2.0, minimum: 0.01, maximum: 10.0)
            .AddResult("UpperChannel", "Upper Channel", ResultType.Default)
            .AddResult("Centerline", "Centerline", ResultType.Default, isReusable: true)
            .AddResult("LowerChannel", "Lower Channel", ResultType.Default)
            .Build();

    /// <summary>
    /// Standard Deviation Channels Series Listing
    /// </summary>
    internal static readonly IndicatorListing SeriesListing =
        new CatalogListingBuilder(CommonListing)
            .WithStyle(Style.Series)
            .WithMethodName("ToStdDevChannels")
            .Build();
}
