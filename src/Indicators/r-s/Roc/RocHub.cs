namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Streaming hub for Rate of Change (ROC).
/// </summary>
public class RocHub
    : ChainHub<IReusable, RocResult>, IRoc
{
    internal RocHub(
        IChainProvider<IReusable> provider,
        int lookbackPeriods) : base(provider)
    {
        Roc.Validate(lookbackPeriods);
        LookbackPeriods = lookbackPeriods;
        Name = $"ROC({lookbackPeriods})";

        // Validate cache size for warmup requirements
        ValidateCacheSize(lookbackPeriods + 1, Name);

        Reinitialize();
    }

    /// <inheritdoc/>
    public int LookbackPeriods { get; init; }
    /// <inheritdoc/>
    protected override (RocResult result, int index)
        ToIndicator(IReusable item, int? indexHint)
    {
        ArgumentNullException.ThrowIfNull(item);

        int i = indexHint ?? ProviderCache.IndexOf(item, true);

        double roc;
        double momentum;

        if (i + 1 > LookbackPeriods)
        {
            IReusable back = ProviderCache[i - LookbackPeriods];

            momentum = item.Value - back.Value;

            roc = back.Value == 0
                ? double.NaN
                : 100d * momentum / back.Value;
        }
        else
        {
            momentum = double.NaN;
            roc = double.NaN;
        }

        // candidate result
        RocResult r = new(
            Timestamp: item.Timestamp,
            Momentum: momentum.NaN2Null(),
            Roc: roc.NaN2Null());

        return (r, i);
    }
}

public static partial class Roc
{
    /// <summary>
    /// Creates an ROC streaming hub from a chain provider.
    /// </summary>
    /// <param name="chainProvider">Chain provider.</param>
    /// <param name="lookbackPeriods">Quantity of periods in lookback window.</param>
    /// <returns>An ROC hub.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the chain provider is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the lookback periods are invalid.</exception>
    public static RocHub ToRocHub(
        this IChainProvider<IReusable> chainProvider,
        int lookbackPeriods = 14)
             => new(chainProvider, lookbackPeriods);
}
