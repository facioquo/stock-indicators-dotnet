namespace FacioQuo.Stock.Indicators;

public static partial class Prs
{
    /// <summary>
    /// Price Relative Strength Common Base Listing
    /// </summary>
    internal static readonly IndicatorListing CommonListing =
        new CatalogListingBuilder()
            .WithName("Price Relative Strength")
            .WithId("PRS")
            .WithCategory(Category.PriceCharacteristic)
            .AddSeriesParameter("sourceEval", "Source Evaluated", description: "Source data to be evaluated")
            .AddSeriesParameter("sourceBase", "Source Base", description: "Base source data for comparison")
            .AddParameter<int>("lookbackPeriods", "Lookback Periods", description: "Number of periods for the PRS calculation", isRequired: false, defaultValue: 20, minimum: 1, maximum: 250)
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
