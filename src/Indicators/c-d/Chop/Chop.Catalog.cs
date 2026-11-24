namespace FacioQuo.Stock.Indicators;

public static partial class Chop
{
    /// <summary>
    /// CHOP Common Base Listing
    /// </summary>
    internal static readonly IndicatorListing CommonListing =
        new CatalogListingBuilder()
            .WithName("Choppiness Index")
            .WithId("CHOP")
            .WithCategory(Category.PriceCharacteristic)
            .AddParameter<int>("lookbackPeriods", "Lookback Periods", defaultValue: 14, minimum: 2, maximum: 250)
            .AddResult(nameof(ChopResult.Chop), "CHOP", "Chop", ResultType.Default, isReusable: true)
            .Build();

    /// <summary>
    /// CHOP Series Listing
    /// </summary>
    internal static readonly IndicatorListing SeriesListing =
        new CatalogListingBuilder(CommonListing)
            .WithStyle(Style.Series)
            .WithMethodName(typeof(Chop), "ToChop")
            .Build();

    /// <summary>
    /// CHOP Stream Listing
    /// </summary>
    internal static readonly IndicatorListing StreamListing =
        new CatalogListingBuilder(CommonListing)
            .WithStyle(Style.Stream)
            .WithMethodName(typeof(Chop), "ToChopHub")
            .Build();

    /// <summary>
    /// CHOP Buffer Listing
    /// </summary>
    internal static readonly IndicatorListing BufferListing =
        new CatalogListingBuilder(CommonListing)
            .WithStyle(Style.Buffer)
            .WithMethodName(typeof(Chop), "ToChopList")
            .Build();
}
