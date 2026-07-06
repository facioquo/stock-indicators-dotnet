namespace FacioQuo.Stock.Indicators;

public static partial class Tema
{
    /// <summary>
    /// Triple Exponential Moving Average Common Base Listing
    /// </summary>
    internal static readonly IndicatorListing CommonListing =
        new CatalogListingBuilder()
            .WithName("Triple Exponential Moving Average")
            .WithId("TEMA")
            .WithCategory(Category.MovingAverage)
            .AddParameter<int>("lookbackPeriods", "Lookback Periods", description: "Number of periods for the TEMA calculation", isRequired: false, defaultValue: 20, minimum: 2, maximum: 250)
            .AddResult("Tema", "TEMA", ResultType.Default, isReusable: true)
            .Build();

    /// <summary>
    /// Triple Exponential Moving Average Series Listing
    /// </summary>
    internal static readonly IndicatorListing SeriesListing =
        new CatalogListingBuilder(CommonListing)
            .WithStyle(Style.Series)
            .WithMethodName("ToTema")
            .Build();

    /// <summary>
    /// Triple Exponential Moving Average Stream Listing
    /// </summary>
    internal static readonly IndicatorListing StreamListing =
        new CatalogListingBuilder(CommonListing)
            .WithStyle(Style.Stream)
            .WithMethodName("ToTemaHub")
            .Build();

    /// <summary>
    /// Triple Exponential Moving Average Buffer Listing
    /// </summary>
    internal static readonly IndicatorListing BufferListing =
        new CatalogListingBuilder(CommonListing)
            .WithStyle(Style.Buffer)
            .WithMethodName("ToTemaList")
            .Build();
}
