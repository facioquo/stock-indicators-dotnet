namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Provides methods for calculating the Volatility Stop using a stream hub.
/// </summary>
public class VolatilityStopHub
    : StreamHub<IBar, VolatilityStopResult>, IVolatilityStop
{
    internal VolatilityStopHub(
        IBarProvider<IBar> provider,
        int lookbackPeriods,
        double multiplier) : base(provider)
    {
        VolatilityStop.Validate(lookbackPeriods, multiplier);

        LookbackPeriods = lookbackPeriods;
        Multiplier = multiplier;
        Name = $"VOLATILITY-STOP({lookbackPeriods},{multiplier})";

        // Validate cache size for warmup requirements
        ValidateCacheSize(lookbackPeriods + 1, Name);

        Reinitialize();
    }

    /// <inheritdoc/>
    public int LookbackPeriods { get; init; }

    /// <inheritdoc/>
    public double Multiplier { get; init; }

    /// <summary>
    /// Significant close value
    /// </summary>
    private double Sic { get; set; }

    /// <summary>
    /// Current trend direction (true = long, false = short)
    /// </summary>
    private bool IsLong { get; set; }

    /// <summary>
    /// Track if first stop has been found
    /// </summary>
    private bool FirstStopFound { get; set; }

    /// <summary>
    /// Track if nullification has been performed
    /// </summary>
    private bool NullificationDone { get; set; }

    /// <summary>
    /// Previous ATR value for incremental calculation
    /// </summary>
    private double? PrevAtr { get; set; }

    /// <inheritdoc/>
    protected override (VolatilityStopResult result, int index)
        ToIndicator(IBar item, int? indexHint)
    {
        ArgumentNullException.ThrowIfNull(item);

        int i = indexHint ?? ProviderCache.IndexOf(item, true);

        // During initialization period (first lookbackPeriods bars)
        if (i < LookbackPeriods)
        {
            // On the last initialization period, determine trend direction
            if (i == LookbackPeriods - 1)
            {
                Sic = (double)ProviderCache[0].Close;
                double currentClose = (double)item.Close;
                IsLong = currentClose > Sic;

                // Update sic for all initialization periods based on determined trend
                for (int j = 0; j <= i; j++)
                {
                    double closePriceJ = (double)ProviderCache[j].Close;
                    Sic = IsLong ? Math.Max(Sic, closePriceJ) : Math.Min(Sic, closePriceJ);
                }
            }

            return (new VolatilityStopResult(item.Timestamp), i);
        }

        // Calculate ATR - use previous period's ATR like Series does
        double close = (double)item.Close;
        double prevClose = (double)ProviderCache[i - 1].Close;
        double atr;

        if (PrevAtr.HasValue)
        {
            // Incremental ATR calculation for current period
            atr = Atr.Increment(
                LookbackPeriods,
                (double)item.High,
                (double)item.Low,
                prevClose,
                PrevAtr.Value);
        }
        else
        {
            // Initialize ATR - calculate through previous period
            double sumTr = 0;

            for (int p = i - LookbackPeriods + 1; p <= i; p++)
            {
                sumTr += Tr.Increment(
                    (double)ProviderCache[p].High,
                    (double)ProviderCache[p].Low,
                    (double)ProviderCache[p - 1].Close);
            }

            atr = sumTr / LookbackPeriods;
        }

        // Use previous period's ATR for SAR calculation (like Series implementation)
        double atrForSar = PrevAtr ?? atr;

        // Store current ATR for next iteration
        PrevAtr = atr;

        // Calculate SAR using previous period's ATR
        double arc = atrForSar * Multiplier;
        double sar = IsLong ? Sic - arc : Sic + arc;

        // Determine bands
        double? lowerBand = null;
        double? upperBand = null;

        if (IsLong)
        {
            lowerBand = sar;
        }
        else
        {
            upperBand = sar;
        }

        // Evaluate stop and reverse
        bool? isStop;

        if ((IsLong && close < sar) || (!IsLong && close > sar))
        {
            isStop = true;
            Sic = close;
            IsLong = !IsLong;
        }
        else
        {
            isStop = false;
            // Update significant close
            Sic = IsLong ? Math.Max(Sic, close) : Math.Min(Sic, close);
        }

        VolatilityStopResult result = new(
            Timestamp: item.Timestamp,
            Sar: sar,
            IsStop: isStop,
            UpperBand: upperBand,
            LowerBand: lowerBand);

        // If this is the first stop, nullify all previous results
        if (isStop == true && !FirstStopFound)
        {
            FirstStopFound = true;

            // Always nullify on first stop detection (unless explicitly marked as done)
            if (!NullificationDone)
            {
                NullificationDone = true;

                // Nullify all existing cache results from 0 to i-1
                for (int idx = 0; idx < i && idx < Cache.Count; idx++)
                {
                    VolatilityStopResult existing = Cache[idx];
                    Cache[idx] = existing with {
                        Sar = null,
                        UpperBand = null,
                        LowerBand = null,
                        IsStop = null
                    };
                }

                // Return nullified result for current index (the first stop itself)
                result = result with {
                    Sar = null,
                    UpperBand = null,
                    LowerBand = null,
                    IsStop = null
                };
            }
        }

        return (result, i);
    }

    /// <summary>
    /// Restores the Volatility Stop state up to the specified index.
    /// </summary>
    /// <inheritdoc/>
    protected override void RollbackState(int restoreIndex)
    {
        // Reset all state
        Sic = 0;
        IsLong = false;
        FirstStopFound = false;
        NullificationDone = false;
        PrevAtr = null;

        if (restoreIndex < LookbackPeriods)
        {
            return;
        }

        // Initialize trend (same as in ToIndicator)
        Sic = (double)ProviderCache[0].Close;
        double lastInitClose = (double)ProviderCache[LookbackPeriods - 1].Close;
        IsLong = lastInitClose > Sic;

        for (int j = 0; j < LookbackPeriods; j++)
        {
            double close = (double)ProviderCache[j].Close;
            Sic = IsLong ? Math.Max(Sic, close) : Math.Min(Sic, close);
        }

        // Replay calculations from LookbackPeriods to restoreIndex
        for (int j = LookbackPeriods; j <= restoreIndex; j++)
        {
            double close = (double)ProviderCache[j].Close;
            double prevClose = (double)ProviderCache[j - 1].Close;

            // Calculate ATR
            double atr;
            if (PrevAtr.HasValue)
            {
                atr = Atr.Increment(
                    LookbackPeriods,
                    (double)ProviderCache[j].High,
                    (double)ProviderCache[j].Low,
                    prevClose,
                    PrevAtr.Value);
            }
            else
            {
                double sumTr = 0;
                for (int p = j - LookbackPeriods + 1; p <= j; p++)
                {
                    sumTr += Tr.Increment(
                        (double)ProviderCache[p].High,
                        (double)ProviderCache[p].Low,
                        (double)ProviderCache[p - 1].Close);
                }

                atr = sumTr / LookbackPeriods;
            }

            // Use previous period's ATR for SAR calculation (mirrors ToIndicator)
            double atrForSar = PrevAtr ?? atr;
            PrevAtr = atr;

            // Calculate SAR (for stop detection, not needed to store)
            double arc = atrForSar * Multiplier;
            double sar = IsLong ? Sic - arc : Sic + arc;

            // Check for stop
            if ((IsLong && close < sar) || (!IsLong && close > sar))
            {
                // Stop detected
                if (!FirstStopFound)
                {
                    FirstStopFound = true;
                    // Mark nullification as done since cache already has nullified results
                    NullificationDone = true;
                }

                Sic = close;
                IsLong = !IsLong;
            }
            else
            {
                // Update significant close
                Sic = IsLong ? Math.Max(Sic, close) : Math.Min(Sic, close);
            }
        }
    }

}

public static partial class VolatilityStop
{
    /// <summary>
    /// Creates a Volatility Stop hub.
    /// </summary>
    /// <param name="barProvider">Bar provider.</param>
    /// <param name="lookbackPeriods">Number of lookback periods.</param>
    /// <param name="multiplier">Multiplier used to scale ATR for SAR.</param>
    /// <returns>An instance of <see cref="VolatilityStopHub"/>.</returns>
    public static VolatilityStopHub ToVolatilityStopHub(
       this IBarProvider<IBar> barProvider,
       int lookbackPeriods = 7,
       double multiplier = 3)
           => new(barProvider, lookbackPeriods, multiplier);
}
