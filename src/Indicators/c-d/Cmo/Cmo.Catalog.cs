namespace FacioQuo.Stock.Indicators;

public static partial class Cmo
{
    /// <summary>
    /// CMO Common Base Listing
    /// </summary>
    internal static readonly IndicatorListing CommonListing =
        new CatalogListingBuilder()
            .WithName("Chande Momentum Oscillator")
            .WithId("CMO")
            .WithCategory(Category.Oscillator)
            .AddParameter<int>("lookbackPeriods", "Lookback Periods", defaultValue: 14, minimum: 1, maximum: 250)
            .AddResult("Cmo", "CMO", ResultType.Default, isReusable: true)
            .Build();

    /// <summary>
    /// CMO Series Listing
    /// </summary>
    internal static readonly IndicatorListing SeriesListing =
        new CatalogListingBuilder(CommonListing)
            .WithStyle(Style.Series)
            .WithMethodName("ToCmo")
            .Build();

    /// <summary>
    /// CMO Stream Listing
    /// </summary>
    internal static readonly IndicatorListing StreamListing =
        new CatalogListingBuilder(CommonListing)
            .WithStyle(Style.Stream)
            .WithMethodName("ToCmoHub")
            .Build();

    /// <summary>
    /// CMO Buffer Listing
    /// </summary>
    internal static readonly IndicatorListing BufferListing =
        new CatalogListingBuilder(CommonListing)
            .WithStyle(Style.Buffer)
            .WithMethodName("ToCmoList")
            .Build();
}
