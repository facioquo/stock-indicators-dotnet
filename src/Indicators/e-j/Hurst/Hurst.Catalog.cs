namespace FacioQuo.Stock.Indicators;

public static partial class Hurst
{
    /// <summary>
    /// HURST Common Base Listing
    /// </summary>
    internal static readonly IndicatorListing CommonListing =
        new CatalogListingBuilder()
            .WithName("Hurst Exponent")
            .WithId("HURST")
            .WithCategory(Category.PriceCharacteristic)
            .AddParameter<int>("lookbackPeriods", "Lookback Periods", defaultValue: 100, minimum: 20, maximum: 250)
            .AddResult("HurstExponent", "Hurst Exponent", ResultType.Default, isReusable: true)
            .AddResult("HurstExponentAL", "Hurst Exponent AL", ResultType.Default)
            .Build();

    /// <summary>
    /// HURST Series Listing
    /// </summary>
    internal static readonly IndicatorListing SeriesListing =
        new CatalogListingBuilder(CommonListing)
            .WithStyle(Style.Series)
            .WithMethodName("ToHurst")
            .Build();

    /// <summary>
    /// HURST Buffer Listing
    /// </summary>
    internal static readonly IndicatorListing BufferListing =
        new CatalogListingBuilder(CommonListing)
            .WithStyle(Style.Buffer)
            .WithMethodName("ToHurstList")
            .Build();

    /// <summary>
    /// HURST Stream Listing
    /// </summary>
    internal static readonly IndicatorListing StreamListing =
        new CatalogListingBuilder(CommonListing)
            .WithStyle(Style.Stream)
            .WithMethodName("ToHurstHub")
            .Build();
}
