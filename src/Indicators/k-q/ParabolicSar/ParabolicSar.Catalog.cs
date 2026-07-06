namespace FacioQuo.Stock.Indicators;

public static partial class ParabolicSar
{
    /// <summary>
    /// Parabolic SAR Common Base Listing
    /// </summary>
    internal static readonly IndicatorListing CommonListing =
        new CatalogListingBuilder()
            .WithName("Parabolic SAR")
            .WithId("PSAR")
            .WithCategory(Category.StopAndReverse)
            .AddParameter<double>("accelerationStep", "Acceleration Step", description: "Acceleration step for the Parabolic SAR calculation", isRequired: false, defaultValue: 0.02, minimum: 0.01, maximum: 0.1)
            .AddParameter<double>("maxAccelerationFactor", "Max Acceleration Factor", description: "Maximum acceleration factor for the Parabolic SAR calculation", isRequired: false, defaultValue: 0.2, minimum: 0.1, maximum: 1.0)
            .AddResult("Sar", "Parabolic SAR", ResultType.Default, isReusable: true)
            .AddResult("IsReversal", "Is Reversal", ResultType.Default)
            .Build();

    /// <summary>
    /// Parabolic SAR Series Listing
    /// </summary>
    internal static readonly IndicatorListing SeriesListing =
        new CatalogListingBuilder(CommonListing)
            .WithStyle(Style.Series)
            .WithMethodName("ToParabolicSar")
            .Build();

    /// <summary>
    /// Parabolic SAR Buffer Listing
    /// </summary>
    internal static readonly IndicatorListing BufferListing =
        new CatalogListingBuilder(CommonListing)
            .WithStyle(Style.Buffer)
            .WithMethodName("ToParabolicSarList")
            .Build();

    /// <summary>
    /// Parabolic SAR Stream Listing
    /// </summary>
    internal static readonly IndicatorListing StreamListing =
        new CatalogListingBuilder(CommonListing)
            .WithStyle(Style.Stream)
            .WithMethodName("ToParabolicSarHub")
            .Build();
}
