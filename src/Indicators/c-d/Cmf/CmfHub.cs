namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Provides methods for creating CMF stream hubs.
/// </summary>
public class CmfHub : ChainHub<IBar, CmfResult>, ICmf
{
    internal CmfHub(
        IBarProvider<IBar> provider,
        int lookbackPeriods)
        : base(provider)
    {
        Cmf.Validate(lookbackPeriods);

        LookbackPeriods = lookbackPeriods;
        Name = $"CMF({lookbackPeriods})";

        // Validate cache size for warmup requirements
        ValidateCacheSize(lookbackPeriods, Name);

        Reinitialize();
    }

    /// <inheritdoc/>
    public int LookbackPeriods { get; init; }

    /// <inheritdoc/>
    public override string ToString() => Cache.Count == 0 ? Name : $"{Name}({Cache[0].Timestamp:d})";

    /// <inheritdoc/>
    protected override (CmfResult result, int index)
        ToIndicator(IBar item, int? indexHint)
    {
        ArgumentNullException.ThrowIfNull(item);
        int i = indexHint ?? ProviderCache.IndexOf(item, true);

        // Calculate Money Flow Multiplier and Money Flow Volume using ADL formula
        double high = (double)item.High;
        double low = (double)item.Low;
        double close = (double)item.Close;
        double volume = (double)item.Volume;

        double mfm = high == low
            ? 0
            : (close - low - (high - close)) / (high - low);

        double mfv = mfm * volume;

        // Calculate CMF if we have enough data
        double? cmf = null;
        if (i >= LookbackPeriods - 1)
        {
            double sumMfv = 0;
            double sumVol = 0;

            // Sum over the lookback period
            for (int p = i + 1 - LookbackPeriods; p <= i; p++)
            {
                IBar q = ProviderCache[p];
                double h = (double)q.High;
                double l = (double)q.Low;
                double c = (double)q.Close;
                double v = (double)q.Volume;

                double m = h == l ? 0 : (c - l - (h - c)) / (h - l);
                double mv = m * v;

                sumMfv += mv;
                sumVol += v;
            }

            double avgMfv = sumMfv / LookbackPeriods;
            double avgVol = sumVol / LookbackPeriods;

            if (avgVol != 0)
            {
                cmf = avgMfv / avgVol;
            }
        }

        // Create result
        CmfResult r = new(
            Timestamp: item.Timestamp,
            MoneyFlowMultiplier: mfm,
            MoneyFlowVolume: mfv,
            Cmf: cmf);

        return (r, i);
    }
}

public static partial class Cmf
{
    /// <summary>
    /// Converts the bar provider to a CMF hub.
    /// </summary>
    /// <param name="barProvider">Bar provider.</param>
    /// <param name="lookbackPeriods">Quantity of periods in lookback window.</param>
    /// <returns>A CMF hub.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the bar provider is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the lookback periods are invalid.</exception>
    public static CmfHub ToCmfHub(
        this IBarProvider<IBar> barProvider,
        int lookbackPeriods = 20)
    {
        ArgumentNullException.ThrowIfNull(barProvider);
        return new(barProvider, lookbackPeriods);
    }
}
