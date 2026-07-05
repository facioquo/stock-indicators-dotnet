namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Provides methods for calculating the Smoothed Moving Average (SMMA) indicator.
/// </summary>
public class SmmaHub
    : ChainHub<IReusable, SmmaResult>, ISmma
{
    internal SmmaHub(
        IChainProvider<IReusable> provider,
        int lookbackPeriods) : base(provider)
    {
        Smma.Validate(lookbackPeriods);
        LookbackPeriods = lookbackPeriods;
        Name = $"SMMA({lookbackPeriods})";

        // Validate cache size for warmup requirements
        ValidateCacheSize(lookbackPeriods, Name);

        Reinitialize();
    }

    /// <inheritdoc/>
    public int LookbackPeriods { get; init; }
    /// <inheritdoc/>
    protected override (SmmaResult result, int index)
        ToIndicator(IReusable item, int? indexHint)
    {
        ArgumentNullException.ThrowIfNull(item);
        int i = indexHint ?? ProviderCache.IndexOf(item, true);

        double smma = i >= LookbackPeriods - 1

            // normal SMMA calculation when we have previous value
            ? Cache[i - 1].Smma is not null
                ? ((Cache[i - 1].Value * (LookbackPeriods - 1)) + item.Value) / LookbackPeriods

                // re/initialize as SMA when no previous SMMA
                : Sma.Increment(ProviderCache, LookbackPeriods, i)

            // warmup periods are never calculable
            : double.NaN;

        // candidate result
        SmmaResult r = new(
            Timestamp: item.Timestamp,
            Smma: smma.NaN2Null());

        return (r, i);
    }
}

public static partial class Smma
{
    /// <summary>
    /// Creates an SMMA streaming hub from a chain provider.
    /// </summary>
    /// <param name="chainProvider">Chain provider.</param>
    /// <param name="lookbackPeriods">Quantity of periods in lookback window.</param>
    /// <returns>An SMMA hub.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the chain provider is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the lookback periods are invalid.</exception>
    public static SmmaHub ToSmmaHub(
        this IChainProvider<IReusable> chainProvider,
        int lookbackPeriods)
        => new(chainProvider, lookbackPeriods);
}
