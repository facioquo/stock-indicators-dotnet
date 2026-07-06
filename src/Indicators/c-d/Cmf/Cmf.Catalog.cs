namespace FacioQuo.Stock.Indicators;

public static partial class Cmf
{
    /// <summary>
    /// CMF Common Base Listing
    /// </summary>
    internal static readonly IndicatorListing CommonListing =
        new CatalogListingBuilder()
            .WithName("Chaikin Money Flow (CMF)")
            .WithId("CMF")
            .WithCategory(Category.VolumeBased)
            .AddParameter<int>("lookbackPeriods", "Lookback Periods", defaultValue: 20, minimum: 1, maximum: 250)
            .AddResult("MoneyFlowMultiplier", "Money Flow Multiplier", ResultType.Default)
            .AddResult("MoneyFlowVolume", "Money Flow Volume", ResultType.Default)
            .AddResult("Cmf", "CMF", ResultType.Default, isReusable: true)
            .Build();

    /// <summary>
    /// CMF Series Listing
    /// </summary>
    internal static readonly IndicatorListing SeriesListing =
        new CatalogListingBuilder(CommonListing)
            .WithStyle(Style.Series)
            .WithMethodName("ToCmf")
            .Build();

    /// <summary>
    /// CMF Stream Listing
    /// </summary>
    internal static readonly IndicatorListing StreamListing =
        new CatalogListingBuilder(CommonListing)
            .WithStyle(Style.Stream)
            .WithMethodName("ToCmfHub")
            .Build();

    /// <summary>
    /// CMF Buffer Listing
    /// </summary>
    internal static readonly IndicatorListing BufferListing =
        new CatalogListingBuilder(CommonListing)
            .WithStyle(Style.Buffer)
            .WithMethodName("ToCmfList")
            .Build();
}
