namespace FacioQuo.Stock.Indicators;

public static partial class Prs
{
    /// <summary>
    /// Price Relative Strength Common Base Listing
    /// </summary>
    /// <remarks>
    /// <c>lookbackPeriods</c> is required even though <c>ToPrs(sourceEval, sourceBase)</c>
    /// exists, because that overload does not take the declared default — it computes no
    /// <c>PrsPercent</c> at all. Marking the parameter optional would advertise a default
    /// of 20 that omitting the argument does not produce, so a catalog-driven caller that
    /// left it out would silently get a different indicator than the listing describes.
    /// The no-<c>PrsPercent</c> form stays reachable through
    /// <see cref="ListingExecutionBuilder.WithoutParam(string)"/>.
    /// </remarks>
    internal static readonly IndicatorListing CommonListing =
        new CatalogListingBuilder()
            .WithName("Price Relative Strength")
            .WithId("PRS")
            .WithCategory(Category.PriceCharacteristic)
            .AddSeriesParameter("sourceEval", "Source Evaluated", description: "Source data to be evaluated")
            .AddSeriesParameter("sourceBase", "Source Base", description: "Base source data for comparison")
            .AddParameter<int>("lookbackPeriods", "Lookback Periods", description: "Number of periods for the PRS calculation", isRequired: true, defaultValue: 20, minimum: 1, maximum: 250)
            .AddResult(nameof(PrsResult.Prs), "PRS", ResultType.Default, isReusable: true)
            .AddResult(nameof(PrsResult.PrsPercent), "PRS %", ResultType.Default)
            .Build();

    /// <summary>
    /// Price Relative Strength Series Listing
    /// </summary>
    internal static readonly IndicatorListing SeriesListing =
        new CatalogListingBuilder(CommonListing)
            .WithStyle(Style.Series)
            .WithMethodName("ToPrs")
            .Build();
}
