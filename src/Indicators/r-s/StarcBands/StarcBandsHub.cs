namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Represents a stream hub for calculating STARC Bands.
/// </summary>
public class StarcBandsHub
    : StreamHub<IBar, StarcBandsResult>, IStarcBands
{

    private double _prevAtr = double.NaN;

    internal StarcBandsHub(
        IBarProvider<IBar> provider,
        int smaPeriods,
        double multiplier,
        int atrPeriods) : base(provider)
    {
        StarcBands.Validate(smaPeriods, multiplier, atrPeriods);

        SmaPeriods = smaPeriods;
        Multiplier = multiplier;
        AtrPeriods = atrPeriods;
        Name = $"STARCBANDS({smaPeriods},{multiplier},{atrPeriods})";

        // Validate cache size for warmup requirements
        // ATR initialization accesses ProviderCache[p-1], requiring atrPeriods + 1 provider items.
        int requiredWarmup = Math.Max(smaPeriods, atrPeriods + 1);
        ValidateCacheSize(requiredWarmup, Name);

        Reinitialize();
    }

    /// <inheritdoc/>
    public int SmaPeriods { get; init; }

    /// <inheritdoc/>
    public double Multiplier { get; init; }

    /// <inheritdoc/>
    public int AtrPeriods { get; init; }
    /// <inheritdoc/>
    protected override void RollbackState(int restoreIndex)
    {
        // Reset ATR state
        _prevAtr = double.NaN;

        if (restoreIndex < 0)
        {
            return;
        }

        // Rebuild ATR state from cache
        for (int i = 0; i <= restoreIndex; i++)
        {
            if (i == 0)
            {
                continue; // ATR starts at index 1
            }

            IBar item = ProviderCache[i];

            if (i <= AtrPeriods)
            {
                // Initialize ATR: Sum TR from index 1 to AtrPeriods
                if (i == AtrPeriods)
                {
                    double sumTr = 0;
                    for (int p = 1; p <= AtrPeriods; p++)
                    {
                        sumTr += Tr.Increment(
                            (double)ProviderCache[p].High,
                            (double)ProviderCache[p].Low,
                            (double)ProviderCache[p - 1].Close);
                    }

                    _prevAtr = sumTr / AtrPeriods;
                }
            }
            else
            {
                // Incrementally update ATR using Wilder's smoothing
                double tr = Tr.Increment(
                    (double)item.High,
                    (double)item.Low,
                    (double)ProviderCache[i - 1].Close);

                _prevAtr = ((_prevAtr * (AtrPeriods - 1)) + tr) / AtrPeriods;
            }
        }
    }

    /// <summary>
    /// Calculates the simple moving average of Close prices.
    /// </summary>
    /// <param name="endIndex">Ending index for calculation</param>
    /// <param name="periods">Number of periods</param>
    private double CalculateSmaOfClose(int endIndex, int periods)
    {
        if (endIndex < periods - 1 || endIndex + 1 > ProviderCache.Count)
        {
            return double.NaN;
        }

        double sum = 0;
        for (int i = endIndex - periods + 1; i <= endIndex; i++)
        {
            sum += (double)ProviderCache[i].Close;
        }

        return sum / periods;
    }

    /// <inheritdoc/>
    protected override (StarcBandsResult result, int index)
        ToIndicator(IBar item, int? indexHint)
    {
        ArgumentNullException.ThrowIfNull(item);

        int i = indexHint ?? ProviderCache.IndexOf(item, true);

        // Calculate SMA of Close
        double sma = (i >= SmaPeriods - 1)
            ? CalculateSmaOfClose(i, SmaPeriods)
            : double.NaN;

        // Calculate ATR
        double atr;

        if (i == 0)
        {
            atr = double.NaN;
        }
        else if (!double.IsNaN(_prevAtr))
        {
            // Calculate ATR normally using previous ATR
            AtrResult atrResult = Atr.Increment(AtrPeriods, item, (double)ProviderCache[i - 1].Close, _prevAtr);
            atr = atrResult.Atr ?? double.NaN;
        }
        else if (i >= AtrPeriods)
        {
            // Initialize ATR using same method as Series:
            // Sum TR from index 1 to AtrPeriods, then incrementally update to current index
            double sumTr = 0;

            // Initial sum from index 1 to AtrPeriods (matching Series behavior)
            for (int p = 1; p <= AtrPeriods; p++)
            {
                sumTr += Tr.Increment(
                    (double)ProviderCache[p].High,
                    (double)ProviderCache[p].Low,
                    (double)ProviderCache[p - 1].Close);
            }

            double prevAtr = sumTr / AtrPeriods;

            // Incrementally update ATR from AtrPeriods+1 to i
            for (int p = AtrPeriods + 1; p <= i; p++)
            {
                double tr = Tr.Increment(
                    (double)ProviderCache[p].High,
                    (double)ProviderCache[p].Low,
                    (double)ProviderCache[p - 1].Close);

                prevAtr = ((prevAtr * (AtrPeriods - 1)) + tr) / AtrPeriods;
            }

            atr = prevAtr;
        }
        else
        {
            atr = double.NaN;
        }

        // Store current ATR for next iteration
        _prevAtr = atr;

        // Calculate bands - these will be null if SMA or ATR are NaN
        double? atrSpan = atr.NaN2Null() * Multiplier;

        StarcBandsResult r = new(
            Timestamp: item.Timestamp,
            UpperBand: sma.NaN2Null() + atrSpan,
            Centerline: sma.NaN2Null(),
            LowerBand: sma.NaN2Null() - atrSpan);

        return (r, i);
    }

}

public static partial class StarcBands
{
    /// <summary>
    /// Creates a STARC Bands streaming hub from a bar provider.
    /// </summary>
    /// <param name="barProvider">Bar provider.</param>
    /// <param name="smaPeriods">Number of periods for the SMA.</param>
    /// <param name="multiplier">Multiplier for the ATR.</param>
    /// <param name="atrPeriods">Number of periods for the ATR.</param>
    /// <returns>An instance of <see cref="StarcBandsHub"/>.</returns>
    public static StarcBandsHub ToStarcBandsHub(
        this IBarProvider<IBar> barProvider,
        int smaPeriods = 5,
        double multiplier = 2,
        int atrPeriods = 10)
        => new(barProvider, smaPeriods, multiplier, atrPeriods);
}
