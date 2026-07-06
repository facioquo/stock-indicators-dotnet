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
            .AddParameter<int>("lookbackPeriods", "Lookback Periods", defaultValue: 20, minimum: 1, maximum: 250)
            .AddResult("VarianceA", "Variance A", ResultType.Default)
            .AddResult("VarianceB", "Variance B", ResultType.Default)
            .AddResult("Covariance", "Covariance", ResultType.Default)
            .AddResult("Correlation", "Correlation", ResultType.Default, isReusable: true)
            .AddResult("RSquared", "R-squared", ResultType.Default)
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
