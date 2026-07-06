namespace FacioQuo.Stock.Indicators;

public static partial class Alligator
{
    /// <summary>
    /// Alligator Common Base Listing
    /// </summary>
    internal static readonly IndicatorListing CommonListing =
        new CatalogListingBuilder()
            .WithName("Williams Alligator")
            .WithId("ALLIGATOR")
            .WithCategory(Category.PriceTrend)
            .AddParameter<int>("jawPeriods", "Jaw Periods", description: "Lookback periods for the Jaw line", isRequired: true, defaultValue: 13, minimum: 2, maximum: 250)
            .AddParameter<int>("jawOffset", "Jaw Offset", description: "Offset periods for the Jaw line", isRequired: true, defaultValue: 8, minimum: 1, maximum: 50)
            .AddParameter<int>("teethPeriods", "Teeth Periods", description: "Lookback periods for the Teeth line", isRequired: true, defaultValue: 8, minimum: 2, maximum: 250)
            .AddParameter<int>("teethOffset", "Teeth Offset", description: "Offset periods for the Teeth line", isRequired: true, defaultValue: 5, minimum: 1, maximum: 50)
            .AddParameter<int>("lipsPeriods", "Lips Periods", description: "Lookback periods for the Lips line", isRequired: true, defaultValue: 5, minimum: 2, maximum: 250)
            .AddParameter<int>("lipsOffset", "Lips Offset", description: "Offset periods for the Lips line", isRequired: true, defaultValue: 3, minimum: 1, maximum: 50)
            .AddResult("Jaw", "Jaw", ResultType.Default)
            .AddResult("Teeth", "Teeth", ResultType.Default)
            .AddResult("Lips", "Lips", ResultType.Default, isReusable: true)
            .Build();

    /// <summary>
    /// Alligator Series Listing
    /// </summary>
    internal static readonly IndicatorListing SeriesListing =
        new CatalogListingBuilder(CommonListing)
            .WithStyle(Style.Series)
            .WithMethodName("ToAlligator")
            .Build();

    /// <summary>
    /// Alligator Stream Listing
    /// </summary>
    internal static readonly IndicatorListing StreamListing =
        new CatalogListingBuilder(CommonListing)
            .WithStyle(Style.Stream)
            .WithMethodName("ToAlligatorHub")
            .Build();

    /// <summary>
    /// Alligator Buffer Listing
    /// </summary>
    internal static readonly IndicatorListing BufferListing =
        new CatalogListingBuilder(CommonListing)
            .WithStyle(Style.Buffer)
            .WithMethodName("ToAlligatorList")
            .Build();
}
