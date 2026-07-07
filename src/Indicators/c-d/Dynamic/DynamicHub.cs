namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Streaming hub for McGinley Dynamic.
/// </summary>
public class DynamicHub
    : ChainHub<IReusable, DynamicResult>, IDynamic
{
    internal DynamicHub(
        IChainProvider<IReusable> provider,
        int lookbackPeriods,
        double kFactor = 0.6) : base(provider)
    {
        MgDynamic.Validate(lookbackPeriods, kFactor);
        LookbackPeriods = lookbackPeriods;
        KFactor = kFactor;
        Name = $"DYNAMIC({lookbackPeriods},{kFactor})";

        // Validate cache size for warmup requirements
        ValidateCacheSize(lookbackPeriods, Name);

        Reinitialize();
    }

    /// <inheritdoc/>
    public int LookbackPeriods { get; init; }

    /// <inheritdoc/>
    public double KFactor { get; init; }
    /// <inheritdoc/>
    protected override (DynamicResult result, int index)
        ToIndicator(IReusable item, int? indexHint)
    {
        ArgumentNullException.ThrowIfNull(item);

        int i = indexHint ?? ProviderCache.IndexOf(item, true);

        // skip first period
        if (i == 0)
        {
            DynamicResult firstResult = new(
                Timestamp: item.Timestamp,
                Dynamic: null);

            return (firstResult, i);
        }

        // calculate dynamic
        double prevDyn = Cache[i - 1].Dynamic ?? ProviderCache[i - 1].Value;
        double dyn = MgDynamic.Increment(
            LookbackPeriods,
            KFactor,
            newVal: item.Value,
            prevDyn: prevDyn);

        // candidate result
        DynamicResult r = new(
            Timestamp: item.Timestamp,
            Dynamic: dyn.NaN2Null());

        return (r, i);
    }
}

public static partial class MgDynamic
{
    /// <summary>
    /// Creates a Dynamic streaming hub from a chain provider.
    /// </summary>
    /// <param name="chainProvider">Chain provider.</param>
    /// <param name="lookbackPeriods">Quantity of periods in lookback window.</param>
    /// <param name="kFactor">Smoothing factor for the calculation.</param>
    /// <returns>A Dynamic hub.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the chain provider is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the lookback periods or kFactor are invalid.</exception>
    public static DynamicHub ToDynamicHub(
        this IChainProvider<IReusable> chainProvider,
        int lookbackPeriods,
        double kFactor = 0.6)
             => new(chainProvider, lookbackPeriods, kFactor);
}
