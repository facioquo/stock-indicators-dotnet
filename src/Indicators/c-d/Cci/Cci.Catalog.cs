namespace FacioQuo.Stock.Indicators;

public static partial class Cci
{
    /// <summary>
    /// CCI Common Base Listing
    /// </summary>
    internal static readonly IndicatorListing CommonListing =
        new CatalogListingBuilder()
            .WithName("Commodity Channel Index (CCI)")
            .WithId("CCI")
            .WithCategory(Category.Oscillator)
            .AddParameter<int>("lookbackPeriods", "Lookback Periods", defaultValue: 20, minimum: 1, maximum: 250)
            .AddResult("Cci", "CCI", ResultType.Default, isReusable: true)
            .Build();

    /// <summary>
    /// CCI Series Listing
    /// </summary>
    internal static readonly IndicatorListing SeriesListing =
        new CatalogListingBuilder(CommonListing)
            .WithStyle(Style.Series)
            .WithMethodName("ToCci")
            .Build();

    /// <summary>
    /// CCI Stream Listing
    /// </summary>
    internal static readonly IndicatorListing StreamListing =
        new CatalogListingBuilder(CommonListing)
            .WithStyle(Style.Stream)
            .WithMethodName("ToCciHub")
            .Build();

    /// <summary>
    /// CCI Buffer Listing
    /// </summary>
    internal static readonly IndicatorListing BufferListing =
        new CatalogListingBuilder(CommonListing)
            .WithStyle(Style.Buffer)
            .WithMethodName("ToCciList")
            .Build();
}
