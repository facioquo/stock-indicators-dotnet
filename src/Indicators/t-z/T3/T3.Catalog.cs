namespace FacioQuo.Stock.Indicators;

public static partial class T3
{
    /// <summary>
    /// T3 Moving Average Common Base Listing
    /// </summary>
    internal static readonly IndicatorListing CommonListing =
        new CatalogListingBuilder()
            .WithName("T3 Moving Average")
            .WithId("T3")
            .WithCategory(Category.MovingAverage)
            .AddParameter<int>("lookbackPeriods", "Lookback Periods", description: "Number of periods for the T3 calculation", isRequired: false, defaultValue: 5, minimum: 1, maximum: 250)
            .AddParameter<double>("volumeFactor", "Volume Factor", description: "Volume factor for the T3 calculation", isRequired: false, defaultValue: 0.7, minimum: 0.0, maximum: 1.0)
            .AddResult("T3", "T3", ResultType.Default, isReusable: true)
            .Build();

    /// <summary>
    /// T3 Moving Average Series Listing
    /// </summary>
    internal static readonly IndicatorListing SeriesListing =
        new CatalogListingBuilder(CommonListing)
            .WithStyle(Style.Series)
            .WithMethodName("ToT3")
            .Build();

    /// <summary>
    /// T3 Moving Average Stream Listing
    /// </summary>
    internal static readonly IndicatorListing StreamListing =
        new CatalogListingBuilder(CommonListing)
            .WithStyle(Style.Stream)
            .WithMethodName("ToT3Hub")
            .Build();

    /// <summary>
    /// T3 Moving Average Buffer Listing
    /// </summary>
    internal static readonly IndicatorListing BufferListing =
        new CatalogListingBuilder(CommonListing)
            .WithStyle(Style.Buffer)
            .WithMethodName("ToT3List")
            .Build();
}
