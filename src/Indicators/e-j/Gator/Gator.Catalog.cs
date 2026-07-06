namespace FacioQuo.Stock.Indicators;

public static partial class Gator
{
    /// <summary>
    /// GATOR Common Base Listing
    /// </summary>
    internal static readonly IndicatorListing CommonListing =
        new CatalogListingBuilder()
            .WithName("Gator Oscillator")
            .WithId("GATOR")
            .WithCategory(Category.Oscillator)
            .AddResult("Upper", "Upper", ResultType.Default, false)
            .AddResult("Lower", "Lower", ResultType.Default, false)
            .AddResult("UpperIsExpanding", "Upper Is Expanding", ResultType.Default, false)
            .AddResult("LowerIsExpanding", "Lower Is Expanding", ResultType.Default, false)
            .Build();

    /// <summary>
    /// GATOR Series Listing
    /// </summary>
    internal static readonly IndicatorListing SeriesListing =
        new CatalogListingBuilder(CommonListing)
            .WithStyle(Style.Series)
            .WithMethodName("ToGator")
            .Build();

    /// <summary>
    /// GATOR Buffer Listing
    /// </summary>
    internal static readonly IndicatorListing BufferListing =
        new CatalogListingBuilder(CommonListing)
            .WithStyle(Style.Buffer)
            .WithMethodName("ToGatorList")
            .Build();

    /// <summary>
    /// GATOR Stream Listing
    /// </summary>
    internal static readonly IndicatorListing StreamListing =
        new CatalogListingBuilder(CommonListing)
            .WithStyle(Style.Stream)
            .WithMethodName("ToGatorHub")
            .Build();
}
