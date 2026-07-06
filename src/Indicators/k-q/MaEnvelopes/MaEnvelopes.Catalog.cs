namespace FacioQuo.Stock.Indicators;

public static partial class MaEnvelopes
{
    /// <summary>
    /// MA-ENV Common Base Listing
    /// </summary>
    internal static readonly IndicatorListing CommonListing =
        new CatalogListingBuilder()
            .WithName("Moving Average Envelopes")
            .WithId("MA-ENV")
            .WithCategory(Category.PriceChannel)
            .AddParameter<int>("lookbackPeriods", "Lookback Periods", defaultValue: 20, minimum: 1, maximum: 250)
            .AddParameter<double>("percentOffset", "Percent Offset", defaultValue: 2.5, minimum: 0.1, maximum: 10.0)
            .AddEnumParameter<MaType>("movingAverageType", "Moving Average Type", defaultValue: MaType.SMA) // MaType.SMA corresponds to 7 from JSON
            .AddResult("Centerline", "Centerline", ResultType.Default, isReusable: true)
            .AddResult("UpperEnvelope", "Upper Envelope", ResultType.Default)
            .AddResult("LowerEnvelope", "Lower Envelope", ResultType.Default)
            .Build();

    /// <summary>
    /// MA-ENV Series Listing
    /// </summary>
    internal static readonly IndicatorListing SeriesListing =
        new CatalogListingBuilder(CommonListing)
            .WithStyle(Style.Series)
            .WithMethodName("ToMaEnvelopes")
            .Build();

    /// <summary>
    /// MA-ENV Buffer Listing
    /// </summary>
    internal static readonly IndicatorListing BufferListing =
        new CatalogListingBuilder(CommonListing)
            .WithStyle(Style.Buffer)
            .WithMethodName("ToMaEnvelopesList")
            .Build();

    /// <summary>
    /// MA-ENV Stream Listing
    /// </summary>
    internal static readonly IndicatorListing StreamListing =
        new CatalogListingBuilder(CommonListing)
            .WithStyle(Style.Stream)
            .WithMethodName("ToMaEnvelopesHub")
            .Build();
}
