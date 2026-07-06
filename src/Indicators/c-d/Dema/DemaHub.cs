namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Streaming hub for Double Exponential Moving Average (DEMA).
/// </summary>
public class DemaHub
    : ChainHub<IReusable, DemaResult>, IDema
{
    private double lastEma1 = double.NaN;
    private double lastEma2 = double.NaN;

    internal DemaHub(
        IChainProvider<IReusable> provider,
        int lookbackPeriods) : base(provider)
    {
        Dema.Validate(lookbackPeriods);
        LookbackPeriods = lookbackPeriods;
        K = 2d / (lookbackPeriods + 1);
        Name = $"DEMA({lookbackPeriods})";

        // Validate cache size for warmup requirements
        ValidateCacheSize(lookbackPeriods * 2, Name);

        Reinitialize();
    }

    /// <inheritdoc/>
    public int LookbackPeriods { get; init; }

    /// <inheritdoc/>
    public double K { get; private init; }
    /// <inheritdoc/>
    protected override (DemaResult result, int index)
        ToIndicator(IReusable item, int? indexHint)
    {
        ArgumentNullException.ThrowIfNull(item);
        int i = indexHint ?? ProviderCache.IndexOf(item, true);

        // if out-of-order change (insertion/deletion before current index) occurred
        // invalidate state and backfill from previous cached EMA layers
        if (i > 0 && Cache.Count > i && Cache[i - 1].Dema is not null && (double.IsNaN(lastEma1) || Cache[i - 1].Ema1 != lastEma1))
        {
            lastEma1 = Cache[i - 1].Ema1;
            lastEma2 = Cache[i - 1].Ema2;
        }

        double dema = i >= LookbackPeriods - 1
            ? Cache[i - 1].Dema is not null

                // normal
                ? CalculateIncrement(item.Value)

                // re/initialize as SMA
                : InitializeDema(i)

            // warmup periods are never calculable
            : double.NaN;

        DemaResult r = new(
            Timestamp: item.Timestamp,
            Dema: dema.NaN2Null()) {
            Ema1 = lastEma1,
            Ema2 = lastEma2
        };

        return (r, i);
    }

    /// <inheritdoc/>
    protected override void RollbackState(int restoreIndex)
    {
        if (restoreIndex >= LookbackPeriods)
        {
            DemaResult prior = Cache[restoreIndex];
            lastEma1 = prior.Ema1;
            lastEma2 = prior.Ema2;
        }
        else
        {
            lastEma1 = lastEma2 = double.NaN;
        }
    }

    private double InitializeDema(int index)
    {
        double sum = 0;
        for (int p = index - LookbackPeriods + 1; p <= index; p++)
        {
            sum += ProviderCache[p].Value;
        }

        lastEma1 = lastEma2 = sum / LookbackPeriods;
        return (2 * lastEma1) - lastEma2;
    }

    private double CalculateIncrement(double value)
    {
        lastEma1 = Ema.Increment(K, lastEma1, value);
        lastEma2 = Ema.Increment(K, lastEma2, lastEma1);
        return (2 * lastEma1) - lastEma2;
    }
}

public static partial class Dema
{
    /// <summary>
    /// Creates a DEMA streaming hub from a chain provider.
    /// </summary>
    /// <param name="chainProvider">Chain provider.</param>
    /// <param name="lookbackPeriods">Quantity of periods in lookback window.</param>
    /// <returns>A DEMA hub.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the chain provider is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the lookback periods are invalid.</exception>
    public static DemaHub ToDemaHub(
        this IChainProvider<IReusable> chainProvider,
        int lookbackPeriods = 14)
        => new(chainProvider, lookbackPeriods);
}
