namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Provides functionality to calculate the Average True Range (ATR) for a series of bars.
/// </summary>
public class AtrHub
    : ChainHub<IBar, AtrResult>, IAtr
{
    internal AtrHub(IBarProvider<IBar> provider,
        int lookbackPeriods)
        : base(provider)
    {
        Atr.Validate(lookbackPeriods);
        LookbackPeriods = lookbackPeriods;
        Name = $"ATR({lookbackPeriods})";

        // Validate cache size for warmup requirements
        // ATR re-init loop accesses ProviderCache[p] and ProviderCache[p-1] where p runs up to
        // i = LookbackPeriods, so index LookbackPeriods must exist: needs LookbackPeriods + 1 items.
        ValidateCacheSize(lookbackPeriods + 1, Name);

        Reinitialize();
    }

    /// <inheritdoc/>
    public int LookbackPeriods { get; init; }

    /// <inheritdoc/>
    protected override (AtrResult result, int index)
        ToIndicator(IBar item, int? indexHint)
    {
        ArgumentNullException.ThrowIfNull(item);

        int i = indexHint ?? ProviderCache.IndexOf(item, true);

        // skip incalculable periods
        if (i == 0)
        {
            return (new AtrResult(item.Timestamp), i);
        }

        AtrResult r;

        // re-initialize as average TR, if necessary
        if (Cache[i - 1].Atr is null && i >= LookbackPeriods)
        {
            double sumTr = 0;
            double tr = double.NaN;

            for (int p = i - LookbackPeriods + 1; p <= i; p++)
            {
                tr = Tr.Increment(
                    (double)ProviderCache[p].High,
                    (double)ProviderCache[p].Low,
                    (double)ProviderCache[p - 1].Close);

                sumTr += tr;
            }

            double atr = sumTr / LookbackPeriods;

            r = new AtrResult(
                item.Timestamp,
                tr,
                atr,
                atr / (double)item.Close * 100);
        }

        // calculate ATR (normally)
        else
        {
            r = Atr.Increment(
                LookbackPeriods,
                item,
                (double)ProviderCache[i - 1].Close,
                Cache[i - 1].Atr);
        }

        return (r, i);
    }
}

public static partial class Atr
{
    /// <summary>
    /// Converts the provided bar provider to an ATR hub with the specified lookback periods.
    /// </summary>
    /// <param name="barProvider">Bar provider to convert.</param>
    /// <param name="lookbackPeriods">Quantity of periods in lookback window.</param>
    /// <returns>An instance of <see cref="AtrHub"/>.</returns>
    public static AtrHub ToAtrHub(
        this IBarProvider<IBar> barProvider,
        int lookbackPeriods = 14)
             => new(barProvider, lookbackPeriods);
}
