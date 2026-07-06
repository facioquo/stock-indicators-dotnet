namespace FacioQuo.Stock.Indicators;

public static partial class Trix
{
    /// <summary>
    /// TRIX Common Base Listing
    /// </summary>
    internal static readonly IndicatorListing CommonListing =
        new CatalogListingBuilder()
            .WithName("Triple Exponential Moving Average Oscillator")
            .WithId("TRIX")
            .WithCategory(Category.Oscillator)
            .AddParameter<int>("lookbackPeriods", "Lookback Periods", description: "Number of periods for the TRIX calculation", isRequired: false, defaultValue: 14, minimum: 1, maximum: 250)
            .AddResult("Trix", "TRIX", ResultType.Default, isReusable: true)
            .Build();

    /// <summary>
    /// TRIX Series Listing
    /// </summary>
    internal static readonly IndicatorListing SeriesListing =
        new CatalogListingBuilder(CommonListing)
            .WithStyle(Style.Series)
            .WithMethodName("ToTrix")
            .Build();

    /// <summary>
    /// TRIX Stream Listing
    /// </summary>
    internal static readonly IndicatorListing StreamListing =
        new CatalogListingBuilder(CommonListing)
            .WithStyle(Style.Stream)
            .WithMethodName("ToTrixHub")
            .Build();

    /// <summary>
    /// TRIX Buffer Listing
    /// </summary>
    internal static readonly IndicatorListing BufferListing =
        new CatalogListingBuilder(CommonListing)
            .WithStyle(Style.Buffer)
            .WithMethodName("ToTrixList")
            .Build();
}
