namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Streaming hub for Exponential Moving Average (EMA).
/// </summary>
public class EmaHub
    : ChainHub<IReusable, EmaResult>, IEma
{
    internal EmaHub(
        IChainProvider<IReusable> provider,
        int lookbackPeriods) : base(provider)
    {
        Ema.Validate(lookbackPeriods);
        LookbackPeriods = lookbackPeriods;
        K = 2d / (lookbackPeriods + 1);
        Name = $"EMA({lookbackPeriods})";

        // Validate cache size for warmup requirements
        ValidateCacheSize(lookbackPeriods, Name);

        Reinitialize();
    }

    /// <inheritdoc/>
    public int LookbackPeriods { get; init; }

    /// <inheritdoc/>
    public double K { get; private init; }

    /// <inheritdoc/>
    protected override (EmaResult result, int index)
        ToIndicator(IReusable item, int? indexHint)
    {
        ArgumentNullException.ThrowIfNull(item);

        int i = indexHint ?? ProviderCache.IndexOf(item, true);

        double ema = i >= LookbackPeriods - 1
            ? Cache[i - 1].Ema is not null

                // normal
                ? Ema.Increment(K, Cache[i - 1].Value, item.Value)

                // re/initialize as SMA
                : Sma.Increment(ProviderCache, LookbackPeriods, i)

            // warmup periods are never calculable
            : double.NaN;

        // candidate result
        EmaResult r = new(
            Timestamp: item.Timestamp,
            Ema: ema.NaN2Null());

        return (r, i);
    }
}

public static partial class Ema
{
    /// <summary>
    /// Creates an EMA streaming hub with a chain provider source.
    /// </summary>
    /// <remarks>If providers contain historical data, this hub will fast-forward its cache.</remarks>
    /// <param name="chainProvider">Chain provider.</param>
    /// <param name="lookbackPeriods">Quantity of periods in lookback window.</param>
    /// <returns>A chain-sourced instance of <see cref="EmaHub"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="chainProvider"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="lookbackPeriods"/> is invalid.</exception>
    public static EmaHub ToEmaHub(
        this IChainProvider<IReusable> chainProvider,
        int lookbackPeriods)
        => new(chainProvider, lookbackPeriods);
}
