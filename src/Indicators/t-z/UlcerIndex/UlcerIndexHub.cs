namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Provides streaming hub for calculating the Ulcer Index indicator.
/// </summary>
public class UlcerIndexHub
    : ChainHub<IReusable, UlcerIndexResult>, IUlcerIndex
{
    internal UlcerIndexHub(
        IChainProvider<IReusable> provider,
        int lookbackPeriods) : base(provider)
    {
        UlcerIndex.Validate(lookbackPeriods);
        LookbackPeriods = lookbackPeriods;
        Name = $"ULCER({lookbackPeriods})";

        // Validate cache size for warmup requirements
        ValidateCacheSize(lookbackPeriods, Name);

        Reinitialize();
    }

    /// <inheritdoc/>
    public int LookbackPeriods { get; init; }
    /// <inheritdoc/>
    protected override (UlcerIndexResult result, int index)
        ToIndicator(IReusable item, int? indexHint)
    {
        ArgumentNullException.ThrowIfNull(item);

        int i = indexHint ?? ProviderCache.IndexOf(item, true);

        double? ui = null;

        // Only calculate if we have enough data
        if (i >= LookbackPeriods - 1)
        {
            double sumSquared = 0;
            int startIdx = i + 1 - LookbackPeriods;

            // For each period in the lookback window
            // NOTE: O(n²) complexity is inherent to Ulcer Index algorithm.
            // The calculation requires finding progressive maxima (max from window start
            // to each period p) for accurate drawdown measurement. This matches the Series
            // implementation exactly and maintains mathematical correctness per Constitution §1.
            for (int p = startIdx; p <= i; p++)
            {
                IReusable ps = ProviderCache[p];
                double pValue = ps.Value;

                // Find maximum from start of window up to current period p
                double maxClose = 0;
                for (int z = startIdx; z <= p; z++)
                {
                    IReusable zs = ProviderCache[z];
                    double zValue = zs.Value;
                    if (zValue > maxClose)
                    {
                        maxClose = zValue;
                    }
                }

                // Calculate percent drawdown
                double percentDrawdown = maxClose == 0 ? double.NaN
                    : 100 * ((pValue - maxClose) / maxClose);

                sumSquared += percentDrawdown * percentDrawdown;
            }

            ui = Math.Sqrt(sumSquared / LookbackPeriods).NaN2Null();
        }

        // Candidate result
        UlcerIndexResult r = new(
            Timestamp: item.Timestamp,
            UlcerIndex: ui);

        return (r, i);
    }

    /// <summary>
    /// Rollback state is not required for this indicator.
    /// The calculation uses only ProviderCache lookups without maintaining
    /// internal state fields. Each ToIndicator call recalculates from the cache,
    /// so provider history mutations (Add/Remove) are handled automatically.
    /// </summary>
    /// <inheritdoc/>
    protected override void RollbackState(int restoreIndex)
    {
        // No stateful fields to rollback.
        // Calculation relies entirely on ProviderCache which is managed by the base class.
    }
}

public static partial class UlcerIndex
{
    /// <summary>
    /// Creates an Ulcer Index streaming hub from a chain provider.
    /// </summary>
    /// <param name="chainProvider">Chain provider.</param>
    /// <param name="lookbackPeriods">Quantity of periods in lookback window.</param>
    /// <returns>An Ulcer Index hub.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the chain provider is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the lookback periods are invalid.</exception>
    public static UlcerIndexHub ToUlcerIndexHub(
        this IChainProvider<IReusable> chainProvider,
        int lookbackPeriods = 14)
             => new(chainProvider, lookbackPeriods);
}
