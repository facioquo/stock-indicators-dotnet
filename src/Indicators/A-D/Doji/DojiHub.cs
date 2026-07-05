namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Provides methods for identifying Doji candlestick patterns.
/// </summary>
public class DojiHub
    : StreamHub<IBar, CandleResult>, IDoji
{
    private readonly double _maxPriceChangePercentDecimal;

    internal DojiHub(
        IStreamObservable<IBar> provider,
        double maxPriceChangePercent) : base(provider)
    {
        Doji.Validate(maxPriceChangePercent);
        MaxPriceChangePercent = maxPriceChangePercent;
        _maxPriceChangePercentDecimal = maxPriceChangePercent / 100;
        Name = $"DOJI({maxPriceChangePercent:F1})";

        // Validate cache size for warmup requirements
        ValidateCacheSize(1, Name);  // Pattern detection requires at least 1 period

        Reinitialize();
    }

    /// <inheritdoc/>
    public double MaxPriceChangePercent { get; init; }
    /// <inheritdoc/>
    protected override (CandleResult result, int index)
        ToIndicator(IBar item, int? indexHint)
    {
        ArgumentNullException.ThrowIfNull(item);
        int i = indexHint ?? ProviderCache.IndexOf(item, true);

        decimal? matchPrice = null;
        Match matchType = Match.None;

        // Check for Doji pattern
        if (item.Open != 0
            && Math.Abs((double)(item.Close / item.Open) - 1d) <= _maxPriceChangePercentDecimal)
        {
            matchPrice = item.Close;
            matchType = Match.Neutral;
        }

        // Candidate result
        CandleResult r = new(
            timestamp: item.Timestamp,
            bar: item,
            match: matchType,
            price: matchPrice);

        return (r, i);
    }
}

public static partial class Doji
{
    /// <summary>
    /// Creates a Doji hub from a bar provider.
    /// </summary>
    /// <param name="provider">Bar provider.</param>
    /// <param name="maxPriceChangePercent">Maximum absolute percent difference in open and close price. Default is 0.1.</param>
    /// <returns>A Doji hub.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the provider is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the maxPriceChangePercent is invalid.</exception>
    public static DojiHub ToDojiHub(
        this IStreamObservable<IBar> provider,
        double maxPriceChangePercent = 0.1)
        => new(provider, maxPriceChangePercent);
}
