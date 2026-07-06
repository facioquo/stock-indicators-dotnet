namespace FacioQuo.Stock.Indicators;

public static partial class Renko
{
    /// <summary>
    /// Renko Common Base Listing
    /// </summary>
    internal static readonly IndicatorListing CommonListing =
        new CatalogListingBuilder()
            .WithName("Renko Chart")
            .WithId("RENKO")
            .WithCategory(Category.PriceTransform)
            .AddParameter<double>("brickSize", "Brick Size", description: "The size of each Renko brick", isRequired: true, defaultValue: 1.0, minimum: 0.001, maximum: 1000000.0)
            .AddEnumParameter<EndType>("endType", "End Type", description: "The price candle end type to use as the brick threshold", isRequired: false, defaultValue: EndType.Close)
            .AddResult("Open", "Open", ResultType.Default)
            .AddResult("High", "High", ResultType.Default)
            .AddResult("Low", "Low", ResultType.Default)
            .AddResult("Close", "Close", ResultType.Default, isReusable: true)
            .AddResult("Volume", "Volume", ResultType.Default)
            .AddResult("IsUp", "Is Up", ResultType.Default)
            .Build();

    /// <summary>
    /// Renko Series Listing
    /// </summary>
    internal static readonly IndicatorListing SeriesListing =
        new CatalogListingBuilder(CommonListing)
            .WithStyle(Style.Series)
            .WithMethodName("ToRenko")
            .Build();

    /// <summary>
    /// Renko Stream Listing
    /// </summary>
    internal static readonly IndicatorListing StreamListing =
        new CatalogListingBuilder(CommonListing)
            .WithStyle(Style.Stream)
            .WithMethodName("ToRenkoHub")
            .Build();

    /// <summary>
    /// Renko Buffer Listing
    /// </summary>
    internal static readonly IndicatorListing BufferListing =
        new CatalogListingBuilder(CommonListing)
            .WithStyle(Style.Buffer)
            .WithMethodName("ToRenkoList")
            .Build();
}
