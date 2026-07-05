namespace FacioQuo.Stock.Indicators;

public static partial class Alma
{
    /// <summary>
    /// ALMA Common Base Listing
    /// </summary>
    internal static readonly IndicatorListing CommonListing =
        new CatalogListingBuilder()
            .WithName("Arnaud Legoux Moving Average (ALMA)")
            .WithId("ALMA")
            .WithCategory(Category.MovingAverage)
            .AddParameter<int>("lookbackPeriods", "Lookback Periods", defaultValue: 9, minimum: 2, maximum: 250)
            .AddParameter<double>("offset", "Offset", defaultValue: 0.85, minimum: 0.0, maximum: 1.0)
            .AddParameter<double>("sigma", "Sigma", defaultValue: 6.0, minimum: 0.1, maximum: 10.0)
            .AddResult("Alma", "Arnaud Legoux Moving Average (ALMA)", ResultType.Default, isReusable: true)
            .Build();

    /// <summary>
    /// ALMA Series Listing
    /// </summary>
    internal static readonly IndicatorListing SeriesListing =
        new CatalogListingBuilder(CommonListing)
            .WithStyle(Style.Series)
            .WithMethodName("ToAlma")
            .Build();

    /// <summary>
    /// ALMA Stream Listing
    /// </summary>
    internal static readonly IndicatorListing StreamListing =
        new CatalogListingBuilder(CommonListing)
            .WithStyle(Style.Stream)
            .WithMethodName("ToAlmaHub")
            .Build();

    /// <summary>
    /// ALMA Buffer Listing
    /// </summary>
    internal static readonly IndicatorListing BufferListing =
        new CatalogListingBuilder(CommonListing)
            .WithStyle(Style.Buffer)
            .WithMethodName("ToAlmaList")
            .Build();
}
