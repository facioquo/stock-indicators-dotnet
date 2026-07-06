namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Provides methods for calculating the SuperTrend using a stream hub.
/// </summary>
public class SuperTrendHub
    : StreamHub<IBar, SuperTrendResult>, ISuperTrend
{
    internal SuperTrendHub(
        IBarProvider<IBar> provider,
        int lookbackPeriods,
        double multiplier) : base(provider)
    {
        SuperTrend.Validate(lookbackPeriods, multiplier);

        LookbackPeriods = lookbackPeriods;
        Multiplier = multiplier;
        Name = $"SUPERTREND({lookbackPeriods},{multiplier})";

        // Validate cache size for warmup requirements
        ValidateCacheSize(lookbackPeriods + 1, Name);

        Reinitialize();
    }

    /// <inheritdoc/>
    public int LookbackPeriods { get; init; }

    /// <inheritdoc/>
    public double Multiplier { get; init; }

    /// <summary>
    /// prevailing direction and band thresholds
    /// </summary>
    private bool IsBullish { get; set; } = true;
    private double UpperBand { get; set; } = double.MaxValue;
    private double LowerBand { get; set; } = double.MinValue;
    private double PrevAtr { get; set; } = double.NaN;

    // recent state snapshots for O(1) near-tail rollback
    private readonly RollbackRing<(bool IsBullish, double UpperBand, double LowerBand, double PrevAtr)> _rollback = new();

    /// <inheritdoc/>
    protected override (SuperTrendResult result, int index)
        ToIndicator(IBar item, int? indexHint)
    {
        ArgumentNullException.ThrowIfNull(item);

        // reminder: should only process "new" instructions

        int i = indexHint ?? ProviderCache.IndexOf(item, true);

        // handle warmup periods
        if (i < LookbackPeriods)
        {
            return (new SuperTrendResult(item.Timestamp, null, null, null), i);
        }

        BarD newBar = item.ToBarD();
        double prevClose = (double)ProviderCache[i - 1].Close;

        // initialize direction on first evaluation
        if (i == LookbackPeriods)
        {
            double mid = (newBar.High + newBar.Low) / 2;
            IsBullish = newBar.Close >= mid;
        }

        // calculate ATR
        double atr;

        if (!double.IsNaN(PrevAtr))
        {
            atr = Atr.Increment(
                LookbackPeriods,
                newBar.High,
                newBar.Low,
                prevClose,
                PrevAtr);
        }

        // initialize ATR
        else
        {
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

        // store ATR for next iteration
        PrevAtr = atr;

        // calculate mid point
        double mid2 = (newBar.High + newBar.Low) / 2;

        // potential bands
        double upperEval = mid2 + (Multiplier * atr);
        double lowerEval = mid2 - (Multiplier * atr);

        // new upper band: can only go down, or reverse
        if (upperEval < UpperBand || prevClose > UpperBand)
        {
            UpperBand = upperEval;
        }

        // new lower band: can only go up, or reverse
        if (lowerEval > LowerBand || prevClose < LowerBand)
        {
            LowerBand = lowerEval;
        }

        // supertrend: based on direction
        SuperTrendResult r;

        // the upper band (bearish / short)
        if (newBar.Close <= (IsBullish ? LowerBand : UpperBand))
        {
            IsBullish = false;

            r = new SuperTrendResult(
                Timestamp: newBar.Timestamp,
                SuperTrend: (decimal?)UpperBand,
                UpperBand: (decimal?)UpperBand,
                LowerBand: null);
        }

        // the lower band (bullish / long)
        else
        {
            IsBullish = true;

            r = new SuperTrendResult(
                Timestamp: newBar.Timestamp,
                SuperTrend: (decimal?)LowerBand,
                UpperBand: null,
                LowerBand: (decimal?)LowerBand);
        }

        // snapshot state for O(1) near-tail rollback
        _rollback.Add(item.Timestamp, (IsBullish, UpperBand, LowerBand, PrevAtr));

        return (r, i);
    }

    /// <summary>
    /// Restores the prior SuperTrend state.
    /// </summary>
    /// <inheritdoc/>
    protected override void RollbackState(int restoreIndex)
    {
        // Reset all state
        IsBullish = true;
        UpperBand = double.MaxValue;
        LowerBand = double.MinValue;
        PrevAtr = double.NaN;

        if (restoreIndex < 0)
        {
            return;
        }

        // O(1) fast path: restore the exact state snapshot recorded when the
        // restore-point item was processed (near-tail rollbacks, the common
        // case for live corrections and forming-bar updates)
        if (_rollback.TryGet(
            ProviderCache[restoreIndex].Timestamp,
            out (bool IsBullish, double UpperBand, double LowerBand, double PrevAtr) snapshot))
        {
            IsBullish = snapshot.IsBullish;
            UpperBand = snapshot.UpperBand;
            LowerBand = snapshot.LowerBand;
            PrevAtr = snapshot.PrevAtr;
            return;
        }

        // Replay up to restoreIndex to rebuild state
        for (int i = 0; i <= restoreIndex; i++)
        {
            IBar item = ProviderCache[i];

            // Skip warmup periods
            if (i < LookbackPeriods)
            {
                continue;
            }

            double high = (double)item.High;
            double low = (double)item.Low;
            double close = (double)item.Close;
            double prevClose = (double)ProviderCache[i - 1].Close;

            // Initialize direction on first evaluation
            if (i == LookbackPeriods)
            {
                double mid = (high + low) / 2;
                IsBullish = close >= mid;
            }

            // Calculate ATR
            double atr;
            if (!double.IsNaN(PrevAtr))
            {
                atr = Atr.Increment(
                    LookbackPeriods,
                    high,
                    low,
                    prevClose,
                    PrevAtr);
            }
            else
            {
                // Initialize ATR
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

            PrevAtr = atr;

            // Calculate mid point
            double mid2 = (high + low) / 2;

            // Potential bands
            double upperEval = mid2 + (Multiplier * atr);
            double lowerEval = mid2 - (Multiplier * atr);

            // New upper band
            if (upperEval < UpperBand || prevClose > UpperBand)
            {
                UpperBand = upperEval;
            }

            // New lower band
            if (lowerEval > LowerBand || prevClose < LowerBand)
            {
                LowerBand = lowerEval;
            }

            // Update direction
            IsBullish = !(close <= (IsBullish ? LowerBand : UpperBand));
        }
    }
}

public static partial class SuperTrend
{
    /// <summary>
    /// Creates a SuperTrend hub.
    /// </summary>
    /// <param name="barProvider">Bar provider.</param>
    /// <param name="lookbackPeriods">Number of lookback periods.</param>
    /// <param name="multiplier">ATR multiplier used for band calculation.</param>
    /// <returns>An instance of <see cref="SuperTrendHub"/>.</returns>
    public static SuperTrendHub ToSuperTrendHub(
       this IBarProvider<IBar> barProvider,
       int lookbackPeriods = 10,
       double multiplier = 3)
           => new(barProvider, lookbackPeriods, multiplier);
}
