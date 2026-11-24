namespace FacioQuo.Stock.Indicators;

public static partial class Aroon
{
    /// <summary>
    /// AROON Common Base Listing
    /// </summary>
    internal static readonly IndicatorListing CommonListing =
        new CatalogListingBuilder()
            .WithName("Aroon")
            .WithId("AROON")
            .WithCategory(Category.PriceTrend)
            .AddParameter<int>("lookbackPeriods", "Lookback Periods", defaultValue: 25, minimum: 1, maximum: 250)
            .AddResult(nameof(AroonResult.AroonUp), "Aroon Up", "Aroon", ResultType.Default)
            .AddResult(nameof(AroonResult.AroonDown), "Aroon Down", "Aroon", ResultType.Default)
            .AddResult(nameof(AroonResult.Oscillator), "Oscillator", "Oscillator", ResultType.Default, isReusable: true)
            .WithPane("Aroon", 0, 100, 70, 50, 30)
            .WithPane("Oscillator", -100, 100, 0)
            .Build();

    /// <summary>
    /// AROON Series Listing
    /// </summary>
    internal static readonly IndicatorListing SeriesListing =
        new CatalogListingBuilder(CommonListing)
            .WithStyle(Style.Series)
            .WithMethodName(typeof(Aroon), "ToAroon")
            .Build();

    /// <summary>
    /// AROON Stream Listing
    /// </summary>
    internal static readonly IndicatorListing StreamListing =
        new CatalogListingBuilder(CommonListing)
            .WithStyle(Style.Stream)
            .WithMethodName(typeof(Aroon), "ToAroonHub")
            .Build();

    /// <summary>
    /// AROON Buffer Listing
    /// </summary>
    internal static readonly IndicatorListing BufferListing =
        new CatalogListingBuilder(CommonListing)
            .WithStyle(Style.Buffer)
            .WithMethodName(typeof(Aroon), "ToAroonList")
            .Build();
}
