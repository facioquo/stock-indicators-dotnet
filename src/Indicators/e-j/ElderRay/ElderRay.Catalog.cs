namespace FacioQuo.Stock.Indicators;

public static partial class ElderRay
{
    /// <summary>
    /// ELDER-RAY Common Base Listing
    /// </summary>
    internal static readonly IndicatorListing CommonListing =
        new CatalogListingBuilder()
            .WithName("Elder-ray Index")
            .WithId("ELDER-RAY")
            .WithCategory(Category.PriceTrend)
            .AddParameter<int>("lookbackPeriods", "Lookback Periods", defaultValue: 13, minimum: 1, maximum: 250)
            .AddResult("Ema", "EMA", ResultType.Default)
            .AddResult("BullPower", "Bull Power", ResultType.Default)
            .AddResult("BearPower", "Bear Power", ResultType.Default)
            .AddResult("Value", "Elder Ray", ResultType.Default, isReusable: true) // Calculated value (BullPower + BearPower) for IReusable.Value
            .Build();

    /// <summary>
    /// ELDER-RAY Series Listing
    /// </summary>
    internal static readonly IndicatorListing SeriesListing =
        new CatalogListingBuilder(CommonListing)
            .WithStyle(Style.Series)
            .WithMethodName("ToElderRay")
            .Build();

    /// <summary>
    /// ELDER-RAY StreamHub Listing
    /// </summary>
    internal static readonly IndicatorListing StreamListing =
        new CatalogListingBuilder(CommonListing)
            .WithStyle(Style.Stream)
            .WithMethodName("ToElderRayHub")
            .Build();

    /// <summary>
    /// ELDER-RAY BufferList Listing
    /// </summary>
    internal static readonly IndicatorListing BufferListing =
        new CatalogListingBuilder(CommonListing)
            .WithStyle(Style.Buffer)
            .WithMethodName("ToElderRayList")
            .Build();
}
