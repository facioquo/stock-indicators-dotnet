namespace FacioQuo.Stock.Indicators;

public static partial class StarcBands
{
    /// <summary>
    /// STARC Bands Common Base Listing
    /// </summary>
    internal static readonly IndicatorListing CommonListing =
        new CatalogListingBuilder()
            .WithName("STARC Bands")
            .WithId("STARC")
            .WithCategory(Category.PriceChannel)
            .AddParameter<int>("smaPeriods", "SMA Periods", description: "Number of periods for the SMA calculation", isRequired: false, defaultValue: 5, minimum: 1, maximum: 50)
            .AddParameter<double>("multiplier", "Multiplier", description: "Multiplier for the ATR calculation", isRequired: false, defaultValue: 2.0, minimum: 1.0, maximum: 10.0)
            .AddParameter<int>("atrPeriods", "ATR Periods", description: "Number of periods for the ATR calculation", isRequired: false, defaultValue: 10, minimum: 1, maximum: 50)
            .AddResult("UpperBand", "Upper Band", ResultType.Default)
            .AddResult("Centerline", "Centerline", ResultType.Default, isReusable: true)
            .AddResult("LowerBand", "Lower Band", ResultType.Default)
            .Build();

    /// <summary>
    /// STARC Bands Series Listing
    /// </summary>
    internal static readonly IndicatorListing SeriesListing =
        new CatalogListingBuilder(CommonListing)
            .WithStyle(Style.Series)
            .WithMethodName("ToStarcBands")
            .Build();

    /// <summary>
    /// STARC Bands Stream Listing
    /// </summary>
    internal static readonly IndicatorListing StreamListing =
        new CatalogListingBuilder(CommonListing)
            .WithStyle(Style.Stream)
            .WithMethodName("ToStarcBandsHub")
            .Build();

    /// <summary>
    /// STARC Bands BufferList Listing
    /// </summary>
    internal static readonly IndicatorListing BufferListing =
        new CatalogListingBuilder(CommonListing)
            .WithStyle(Style.Buffer)
            .WithMethodName("ToStarcBandsList")
            .Build();
}
