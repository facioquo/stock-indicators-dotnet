namespace FacioQuo.Stock.Indicators;

public static partial class Mama
{
    /// <summary>
    /// MAMA Common Base Listing
    /// </summary>
    internal static readonly IndicatorListing CommonListing =
        new CatalogListingBuilder()
            .WithName("MESA Adaptive Moving Average")
            .WithId("MAMA")
            .WithCategory(Category.MovingAverage)
            .AddParameter<double>("fastLimit", "Fast Limit", defaultValue: 0.5, minimum: 0.01, maximum: 0.99)
            .AddParameter<double>("slowLimit", "Slow Limit", defaultValue: 0.05, minimum: 0.01, maximum: 0.99)
            .AddResult("Mama", "MAMA", ResultType.Default, isReusable: true)
            .AddResult("Fama", "FAMA", ResultType.Default)
            .Build();

    /// <summary>
    /// MAMA Series Listing
    /// </summary>
    internal static readonly IndicatorListing SeriesListing =
        new CatalogListingBuilder(CommonListing)
            .WithStyle(Style.Series)
            .WithMethodName("ToMama")
            .Build();

    /// <summary>
    /// MAMA Buffer Listing
    /// </summary>
    internal static readonly IndicatorListing BufferListing =
        new CatalogListingBuilder(CommonListing)
            .WithStyle(Style.Buffer)
            .WithMethodName("ToMamaList")
            .Build();

    /// <summary>
    /// MAMA Stream Listing
    /// </summary>
    internal static readonly IndicatorListing StreamListing =
        new CatalogListingBuilder(CommonListing)
            .WithStyle(Style.Stream)
            .WithMethodName("ToMamaHub")
            .Build();
}
