namespace FacioQuo.Stock.Indicators;

public static partial class Beta
{
    /// <summary>
    /// BETA Common Base Listing
    /// </summary>
    internal static readonly IndicatorListing CommonListing =
        new CatalogListingBuilder()
            .WithName("Beta")
            .WithId("BETA")
            .WithCategory(Category.PriceCharacteristic)
            .AddSeriesParameter("sourceEval", "Evaluated Prices")
            .AddSeriesParameter("sourceMrkt", "Market Prices")
            .AddParameter<int>("lookbackPeriods", "Lookback Periods", isRequired: true, defaultValue: 50, minimum: 1, maximum: 250)
            .AddEnumParameter<BetaType>("type", "Beta Type", defaultValue: BetaType.Standard)
            .AddResult(nameof(BetaResult.Beta), "Beta", "Beta", ResultType.Default, isReusable: true)
            .AddResult(nameof(BetaResult.BetaUp), "Beta Up", "Beta", ResultType.Default)
            .AddResult(nameof(BetaResult.BetaDown), "Beta Down", "Beta", ResultType.Default)
            .AddResult(nameof(BetaResult.Ratio), "Ratio", "Ratio", ResultType.Default)
            .AddResult(nameof(BetaResult.Convexity), "Convexity", "Convexity", ResultType.Default)
            .AddResult(nameof(BetaResult.ReturnsEval), "Returns Eval", "Returns", ResultType.Default)
            .AddResult(nameof(BetaResult.ReturnsMrkt), "Returns Mrkt", "Returns", ResultType.Default)
            .Build();

    /// <summary>
    /// BETA Series Listing
    /// </summary>
    internal static readonly IndicatorListing SeriesListing =
        new CatalogListingBuilder(CommonListing)
            .WithStyle(Style.Series)
            .WithMethodName("ToBeta")
            .Build();
}
