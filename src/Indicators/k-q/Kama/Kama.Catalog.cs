namespace FacioQuo.Stock.Indicators;

public static partial class Kama
{
    /// <summary>
    /// KAMA Common Base Listing
    /// </summary>
    internal static readonly IndicatorListing CommonListing =
        new CatalogListingBuilder()
            .WithName("Kaufman's Adaptive Moving Average")
            .WithId("KAMA")
            .WithCategory(Category.MovingAverage)
            .AddParameter<int>("erPeriods", "ER Periods", defaultValue: 10, minimum: 2, maximum: 250)
            .AddParameter<int>("fastPeriods", "Fast Periods", defaultValue: 2, minimum: 1, maximum: 50)
            .AddParameter<int>("slowPeriods", "Slow Periods", defaultValue: 30, minimum: 1, maximum: 250)
            .AddResult("Er", "ER", ResultType.Default)
            .AddResult("Kama", "KAMA", ResultType.Default, isReusable: true)
            .Build();

    /// <summary>
    /// KAMA Series Listing
    /// </summary>
    internal static readonly IndicatorListing SeriesListing =
        new CatalogListingBuilder(CommonListing)
            .WithStyle(Style.Series)
            .WithMethodName("ToKama")
            .Build();

    /// <summary>
    /// KAMA Stream Listing
    /// </summary>
    internal static readonly IndicatorListing StreamListing =
        new CatalogListingBuilder(CommonListing)
            .WithStyle(Style.Stream)
            .WithMethodName("ToKamaHub")
            .Build();

    /// <summary>
    /// KAMA Buffer Listing
    /// </summary>
    internal static readonly IndicatorListing BufferListing =
        new CatalogListingBuilder(CommonListing)
            .WithStyle(Style.Buffer)
            .WithMethodName("ToKamaList")
            .Build();
}
