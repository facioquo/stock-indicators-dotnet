namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Provides methods for creating VWMA hubs.
/// </summary>
public class VwmaHub
    : ChainHub<IBar, VwmaResult>, IVwma
{
    internal VwmaHub(
        IBarProvider<IBar> provider,
        int lookbackPeriods) : base(provider)
    {
        Vwma.Validate(lookbackPeriods);
        LookbackPeriods = lookbackPeriods;
        Name = $"VWMA({lookbackPeriods})";

        // Validate cache size for warmup requirements
        ValidateCacheSize(lookbackPeriods, Name);

        Reinitialize();
    }

    /// <inheritdoc/>
    public int LookbackPeriods { get; init; }


    /// <inheritdoc />
    public override string ToString() => Name;

    /// <inheritdoc />
    protected override (VwmaResult result, int index)
        ToIndicator(IBar item, int? indexHint)
    {
        ArgumentNullException.ThrowIfNull(item);
        int index = indexHint ?? ProviderCache.IndexOf(item, true);

        // Calculate VWMA efficiently using a rolling window over ProviderCache
        // This is O(lookbackPeriods) which is constant for a given configuration
        // and maintains exact precision with Series implementation
        double? vwma = null;

        if (index >= LookbackPeriods - 1)
        {
            double priceVolumeSum = 0;
            double volumeSum = 0;

            for (int p = index - LookbackPeriods + 1; p <= index; p++)
            {
                IBar bar = ProviderCache[p];
                double price = (double)bar.Close;
                double volume = (double)bar.Volume;

                priceVolumeSum += price * volume;
                volumeSum += volume;
            }

            vwma = volumeSum != 0 ? priceVolumeSum / volumeSum : null;
        }

        VwmaResult result = new(
            Timestamp: item.Timestamp,
            Vwma: vwma);

        return (result, index);
    }
}

public static partial class Vwma
{
    /// <summary>
    /// Converts the bar provider to a VWMA hub.
    /// </summary>
    /// <param name="barProvider">Bar provider.</param>
    /// <param name="lookbackPeriods">Quantity of periods in lookback window.</param>
    /// <returns>A VWMA hub.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the bar provider is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the lookback periods are invalid.</exception>
    public static VwmaHub ToVwmaHub(
        this IBarProvider<IBar> barProvider,
        int lookbackPeriods)
    {
        ArgumentNullException.ThrowIfNull(barProvider);
        return new(barProvider, lookbackPeriods);
    }
}
