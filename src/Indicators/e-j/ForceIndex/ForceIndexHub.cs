namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Provides streaming hub for Force Index calculations.
/// </summary>
public class ForceIndexHub
    : ChainHub<IBar, ForceIndexResult>, IForceIndex
{
    private readonly double _k;
    private double _sumRawFi;

    internal ForceIndexHub(
        IBarProvider<IBar> provider,
        int lookbackPeriods) : base(provider)
    {
        ForceIndex.Validate(lookbackPeriods);
        LookbackPeriods = lookbackPeriods;
        _k = 2d / (lookbackPeriods + 1);
        Name = $"FORCE({lookbackPeriods})";

        // Validate cache size for warmup requirements
        ValidateCacheSize(lookbackPeriods + 1, Name);

        Reinitialize();
    }

    /// <inheritdoc/>
    public int LookbackPeriods { get; init; }

    /// <inheritdoc />
    public override string ToString() => Name;

    /// <inheritdoc />
    protected override (ForceIndexResult result, int index)
        ToIndicator(IBar item, int? indexHint)
    {
        ArgumentNullException.ThrowIfNull(item);
        int i = indexHint ?? ProviderCache.IndexOf(item, true);

        double fi = double.NaN;

        // skip first period (need prior bar for delta)
        if (i > 0)
        {
            // get current and previous bars
            IBar currentBar = ProviderCache[i];
            IBar previousBar = ProviderCache[i - 1];

            // calculate raw Force Index
            double rawFi = (double)currentBar.Volume
                * ((double)currentBar.Close - (double)previousBar.Close);

            if (i >= LookbackPeriods)
            {
                // Check if previous result has a valid ForceIndex for incremental update
                if (Cache[i - 1].ForceIndex is not null)
                {
                    // Incremental O(1) EMA update
                    fi = Ema.Increment(_k, Cache[i - 1].Value, rawFi);
                }
                else
                {
                    // First EMA value - use accumulated sum for O(1) SMA calculation
                    _sumRawFi += rawFi;
                    fi = _sumRawFi / LookbackPeriods;
                }
            }
            else
            {
                // Warmup period - accumulate raw Force Index values
                _sumRawFi += rawFi;
            }
        }

        ForceIndexResult result = new(
            Timestamp: item.Timestamp,
            ForceIndex: fi.NaN2Null());

        return (result, i);
    }

    /// <inheritdoc/>
    protected override void RollbackState(int restoreIndex)
    {
        // Reset sum for recalculation during rebuild
        _sumRawFi = 0;

        // Derive mutation index (first entry being removed)
        int mutationIndex = restoreIndex + 1;

        // If rolling back to a point still in warmup period, rebuild the sum
        // The sum is only used to compute the first EMA value at index == LookbackPeriods
        if (mutationIndex >= 0 && mutationIndex < LookbackPeriods && ProviderCache.Count > 1)
        {
            // Rebuild sum for warmup period up to rollback point
            int endIndex = Math.Min(mutationIndex, LookbackPeriods - 1);

            for (int i = 1; i <= endIndex && i < ProviderCache.Count; i++)
            {
                IBar curr = ProviderCache[i];
                IBar prev = ProviderCache[i - 1];
                _sumRawFi += (double)curr.Volume * ((double)curr.Close - (double)prev.Close);
            }
        }

        // If past warmup period, sum is not needed - EMA continues incrementally from cache
    }
}

public static partial class ForceIndex
{
    /// <summary>
    /// Converts the bar provider to a Force Index hub.
    /// </summary>
    /// <param name="barProvider">Bar provider.</param>
    /// <param name="lookbackPeriods">Quantity of periods in lookback window.</param>
    /// <returns>A Force Index hub.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the bar provider is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the lookback periods are invalid.</exception>
    public static ForceIndexHub ToForceIndexHub(
        this IBarProvider<IBar> barProvider,
        int lookbackPeriods = 2)
    {
        ArgumentNullException.ThrowIfNull(barProvider);
        return new(barProvider, lookbackPeriods);
    }
}
