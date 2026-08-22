namespace FacioQuo.Stock.Indicators;

public static partial class Correlation
{
    /// <summary>
    /// CORR Common Base Listing
    /// </summary>
    internal static readonly IndicatorListing CommonListing =
        new CatalogListingBuilder()
            .WithName("Correlation")
            .WithId("CORR")
            .WithCategory(Category.Oscillator)
            .AddSeriesParameter("sourceA", "Source A")
            .AddSeriesParameter("sourceB", "Source B")
            .AddParameter<int>("lookbackPeriods", "Lookback Periods", isRequired: true, defaultValue: 20, minimum: 1, maximum: 250)
            .AddResult(nameof(CorrResult.VarianceA), "Variance A", ResultType.Default)
            .AddResult(nameof(CorrResult.VarianceB), "Variance B", ResultType.Default)
            .AddResult(nameof(CorrResult.Covariance), "Covariance", ResultType.Default)
            .AddResult(nameof(CorrResult.Correlation), "Correlation", ResultType.Default, isReusable: true)
            .AddResult(nameof(CorrResult.RSquared), "R-squared", ResultType.Default)
            .Build();

    /// <summary>
    /// CORR Series Listing
    /// </summary>
    internal static readonly IndicatorListing SeriesListing =
        new CatalogListingBuilder(CommonListing)
            .WithStyle(Style.Series)
            .WithMethodName("ToCorrelation")
            .Build();
}
