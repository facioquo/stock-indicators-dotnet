namespace FacioQuo.Stock.Indicators;

public static partial class Mfi
{
    /// <summary>
    /// MFI Common Base Listing
    /// </summary>
    internal static readonly IndicatorListing CommonListing =
        new CatalogListingBuilder()
            .WithName("Money Flow Index (MFI)")
            .WithId("MFI")
            .WithCategory(Category.VolumeBased)
            .AddParameter<int>("lookbackPeriods", "Lookback Periods", description: "Number of periods for the MFI calculation", defaultValue: 14, minimum: 1, maximum: 250)
            .AddResult("Mfi", "MFI", ResultType.Default, isReusable: true)
            .Build();

    /// <summary>
    /// MFI Series Listing
    /// </summary>
    internal static readonly IndicatorListing SeriesListing =
        new CatalogListingBuilder(CommonListing)
            .WithStyle(Style.Series)
            .WithMethodName("ToMfi")
            .Build();

    /// <summary>
    /// MFI Stream Listing
    /// </summary>
    internal static readonly IndicatorListing StreamListing =
        new CatalogListingBuilder(CommonListing)
            .WithStyle(Style.Stream)
            .WithMethodName("ToMfiHub")
            .Build();

    /// <summary>
    /// MFI Buffer Listing
    /// </summary>
    internal static readonly IndicatorListing BufferListing =
        new CatalogListingBuilder(CommonListing)
            .WithStyle(Style.Buffer)
            .WithMethodName("ToMfiList")
            .Build();
}
