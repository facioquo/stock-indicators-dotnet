namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Provides methods for creating WMA hubs.
/// </summary>
public class WmaHub
    : ChainHub<IReusable, WmaResult>, IWma
{
    internal WmaHub(
        IChainProvider<IReusable> provider,
        int lookbackPeriods) : base(provider)
    {
        Wma.Validate(lookbackPeriods);
        LookbackPeriods = lookbackPeriods;
        Name = $"WMA({lookbackPeriods})";

        // Validate cache size for warmup requirements
        ValidateCacheSize(lookbackPeriods, Name);

        Reinitialize();
    }

    /// <inheritdoc/>
    public int LookbackPeriods { get; init; }


    /// <inheritdoc />
    public override string ToString() => Name;

    /// <inheritdoc />
    protected override (WmaResult result, int index)
        ToIndicator(IReusable item, int? indexHint)
    {
        ArgumentNullException.ThrowIfNull(item);
        int index = indexHint ?? ProviderCache.IndexOf(item, true);

        // Calculate WMA efficiently using a rolling window over ProviderCache
        // This is O(lookbackPeriods) which is constant for a given configuration
        // and maintains exact precision with Series implementation
        double wma = double.NaN;

        if (index >= LookbackPeriods - 1)
        {
            double divisor = (double)LookbackPeriods * (LookbackPeriods + 1) / 2d;
            double weightedSum = 0d;
            int weight = 1;

            for (int i = index - LookbackPeriods + 1; i <= index; i++)
            {
                double value = ProviderCache[i].Value;
                if (double.IsNaN(value))
                {
                    wma = double.NaN;
                    break;
                }

                weightedSum += value * weight / divisor;
                weight++;
            }

            if (!double.IsNaN(weightedSum))
            {
                wma = weightedSum;
            }
        }

        WmaResult result = new(
            Timestamp: item.Timestamp,
            Wma: wma.NaN2Null());

        return (result, index);
    }
}

public static partial class Wma
{
    /// <summary>
    /// Converts the chain provider to a WMA hub.
    /// </summary>
    /// <param name="chainProvider">Chain provider.</param>
    /// <param name="lookbackPeriods">Quantity of periods in lookback window.</param>
    /// <returns>A WMA hub.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the chain provider is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the lookback periods are invalid.</exception>
    public static WmaHub ToWmaHub(
        this IChainProvider<IReusable> chainProvider,
        int lookbackPeriods)
    {
        ArgumentNullException.ThrowIfNull(chainProvider);
        return new(chainProvider, lookbackPeriods);
    }
}
