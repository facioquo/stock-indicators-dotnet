namespace FacioQuo.Stock.Indicators;

public static partial class Aroon
{
    /// <summary>
    /// AROON Common Base Listing
    /// </summary>
    internal static readonly IndicatorListing CommonListing =
        new CatalogListingBuilder()
            .WithName("Aroon Up/Down")
            .WithId("AROON")
            .WithCategory(Category.PriceTrend)
            .AddParameter<int>("lookbackPeriods", "Lookback Periods", defaultValue: 25, minimum: 1, maximum: 250)
            .AddResult("AroonUp", "Aroon Up", ResultType.Default)
            .AddResult("AroonDown", "Aroon Down", ResultType.Default)
            .AddResult("Oscillator", "Oscillator", ResultType.Default, isReusable: true)
            .Build();

    /// <summary>
    /// AROON Series Listing
    /// </summary>
    internal static readonly IndicatorListing SeriesListing =
        new CatalogListingBuilder(CommonListing)
            .WithStyle(Style.Series)
            .WithMethodName("ToAroon")
            .Build();

    /// <summary>
    /// AROON Stream Listing
    /// </summary>
    internal static readonly IndicatorListing StreamListing =
        new CatalogListingBuilder(CommonListing)
            .WithStyle(Style.Stream)
            .WithMethodName("ToAroonHub")
            .Build();

    /// <summary>
    /// AROON Buffer Listing
    /// </summary>
    internal static readonly IndicatorListing BufferListing =
        new CatalogListingBuilder(CommonListing)
            .WithStyle(Style.Buffer)
            .WithMethodName("ToAroonList")
            .Build();
}
