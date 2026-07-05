namespace FacioQuo.Stock.Indicators;

public static partial class ConnorsRsi
{
    /// <summary>
    /// CRSI Common Base Listing
    /// </summary>
    internal static readonly IndicatorListing CommonListing =
        new CatalogListingBuilder()
            .WithName("ConnorsRSI (CRSI)")
            .WithId("CRSI")
            .WithCategory(Category.Oscillator)
            .AddParameter<int>("rsiPeriods", "RSI Periods", defaultValue: 3, minimum: 2, maximum: 250)
            .AddParameter<int>("streakPeriods", "Streak Periods", defaultValue: 2, minimum: 2, maximum: 50)
            .AddParameter<int>("rankPeriods", "Rank Periods", defaultValue: 100, minimum: 2, maximum: 250)
            .AddResult("Streak", "Streak", ResultType.Default)
            .AddResult("Rsi", "RSI", ResultType.Default)
            .AddResult("RsiStreak", "RSI of Streak", ResultType.Default)
            .AddResult("PercentRank", "Percent Rank", ResultType.Default)
            .AddResult("ConnorsRsi", "ConnorsRSI", ResultType.Default, isReusable: true)
            .Build();

    /// <summary>
    /// CRSI Series Listing
    /// </summary>
    internal static readonly IndicatorListing SeriesListing =
        new CatalogListingBuilder(CommonListing)
            .WithStyle(Style.Series)
            .WithMethodName("ToConnorsRsi")
            .Build();

    /// <summary>
    /// CRSI Buffer Listing
    /// </summary>
    internal static readonly IndicatorListing BufferListing =
        new CatalogListingBuilder(CommonListing)
            .WithStyle(Style.Buffer)
            .WithMethodName("ToConnorsRsiList")
            .Build();

    /// <summary>
    /// CRSI Stream Listing
    /// </summary>
    internal static readonly IndicatorListing StreamListing =
        new CatalogListingBuilder(CommonListing)
            .WithStyle(Style.Stream)
            .WithMethodName("ToConnorsRsiHub")
            .Build();
}
