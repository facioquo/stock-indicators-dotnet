namespace FacioQuo.Stock.Indicators;

public static partial class Dpo
{
    /// <summary>
    /// DPO Common Base Listing
    /// </summary>
    internal static readonly IndicatorListing CommonListing =
        new CatalogListingBuilder()
            .WithName("Detrended Price Oscillator")
            .WithId("DPO")
            .WithCategory(Category.Oscillator)
            .AddParameter<int>("lookbackPeriods", "Lookback Periods", defaultValue: 14, minimum: 2, maximum: 250)
            .AddResult("Dpo", "DPO", ResultType.Default, isReusable: true)
            .AddResult("Sma", "SMA", ResultType.Default)
            .Build();

    /// <summary>
    /// DPO Series Listing
    /// </summary>
    internal static readonly IndicatorListing SeriesListing =
        new CatalogListingBuilder(CommonListing)
            .WithStyle(Style.Series)
            .WithMethodName("ToDpo")
            .Build();

    /// <summary>
    /// DPO BufferList Listing
    /// </summary>
    internal static readonly IndicatorListing BufferListing =
        new CatalogListingBuilder(CommonListing)
            .WithStyle(Style.Buffer)
            .WithMethodName("ToDpoList")
            .Build();

    /// <summary>
    /// DPO StreamHub Listing
    /// </summary>
    internal static readonly IndicatorListing StreamListing =
        new CatalogListingBuilder(CommonListing)
            .WithStyle(Style.Stream)
            .WithMethodName("ToDpoHub")
            .Build();
}
