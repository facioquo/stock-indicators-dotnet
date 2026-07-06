namespace FacioQuo.Stock.Indicators;

public static partial class Chandelier
{
    /// <summary>
    /// CHEXIT Common Base Listing
    /// </summary>
    internal static readonly IndicatorListing CommonListing =
        new CatalogListingBuilder()
            .WithName("Chandelier Exit")
            .WithId("CHEXIT")
            .WithCategory(Category.StopAndReverse)
            .AddParameter<int>("lookbackPeriods", "Lookback Periods", defaultValue: 22, minimum: 1, maximum: 250)
            .AddParameter<double>("multiplier", "Multiplier", defaultValue: 3.0, minimum: 1.0, maximum: 10.0)
            .AddEnumParameter<Direction>("type", "Direction", defaultValue: Direction.Long)
            .AddResult("ChandelierExit", "Chandelier Exit", ResultType.Default, isReusable: true)
            .Build();

    /// <summary>
    /// CHEXIT Series Listing
    /// </summary>
    internal static readonly IndicatorListing SeriesListing =
        new CatalogListingBuilder(CommonListing)
            .WithStyle(Style.Series)
            .WithMethodName("ToChandelier")
            .Build();

    /// <summary>
    /// CHEXIT Stream Listing
    /// </summary>
    internal static readonly IndicatorListing StreamListing =
        new CatalogListingBuilder(CommonListing)
            .WithStyle(Style.Stream)
            .WithMethodName("ToChandelierHub")
            .Build();

    /// <summary>
    /// CHEXIT Buffer Listing
    /// </summary>
    internal static readonly IndicatorListing BufferListing =
        new CatalogListingBuilder(CommonListing)
            .WithStyle(Style.Buffer)
            .WithMethodName("ToChandelierList")
            .Build();
}
