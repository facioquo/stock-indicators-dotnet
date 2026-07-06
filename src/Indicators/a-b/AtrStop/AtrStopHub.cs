namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Streaming hub for ATR Trailing Stop using a stream hub.
/// </summary>
public class AtrStopHub
    : StreamHub<IBar, AtrStopResult>, IAtrStop
{
    internal AtrStopHub(
        IBarProvider<IBar> provider,
        int lookbackPeriods,
        double multiplier,
        EndType endType) : base(provider)
    {
        AtrStop.Validate(lookbackPeriods, multiplier);

        LookbackPeriods = lookbackPeriods;
        Multiplier = multiplier;
        EndType = endType;
        Name = $"ATR-STOP({lookbackPeriods},{multiplier},{endType.ToString().ToUpperInvariant()})";

        // Validate cache size for warmup requirements
        ValidateCacheSize(lookbackPeriods + 1, Name);

        Reinitialize();
    }

    /// <inheritdoc/>
    public int LookbackPeriods { get; init; }

    /// <inheritdoc/>
    public double Multiplier { get; init; }

    /// <inheritdoc/>
    public EndType EndType { get; init; }

    /// <summary>
    /// prevailing direction and band thresholds
    /// </summary>
    private bool IsBullish { get; set; } = true;
    private double UpperBand { get; set; } = double.MaxValue;
    private double LowerBand { get; set; } = double.MinValue;

    /// <inheritdoc/>
    protected override (AtrStopResult result, int index)
        ToIndicator(IBar item, int? indexHint)
    {
        ArgumentNullException.ThrowIfNull(item);

        // reminder: should only process "new" instructions

        int i = indexHint ?? ProviderCache.IndexOf(item, true);

        // handle warmup periods
        if (i < LookbackPeriods)
        {
            return (new AtrStopResult(item.Timestamp), i);
        }

        BarD newBar = item.ToBarD();
        double prevClose = (double)ProviderCache[i - 1].Close;

        // initialize direction on first evaluation (when no prior ATR exists)
        if (Cache[i - 1].Atr is null)
        {
            IsBullish = newBar.Close >= prevClose;
        }

        // calculate ATR
        double atr;

        if (Cache[i - 1].Atr is not null)
        {
            atr = Atr.Increment(
                LookbackPeriods,
                newBar.High,
                newBar.Low,
                prevClose,
                Cache[i - 1].Atr ?? double.NaN);
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

        // evaluate bands
        double upperEval;
        double lowerEval;

        // potential bands for CLOSE
        if (EndType == EndType.Close)
        {
            upperEval = newBar.Close + (Multiplier * atr);
            lowerEval = newBar.Close - (Multiplier * atr);
        }

        // potential bands for HIGH/LOW
        else
        {
            upperEval = newBar.High + (Multiplier * atr);
            lowerEval = newBar.Low - (Multiplier * atr);
        }

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

        // trailing stop: based on direction

        AtrStopResult r;

        // the upper band (short / buy-to-stop)
        if (newBar.Close <= (IsBullish ? LowerBand : UpperBand))
        {
            IsBullish = false;

            r = new AtrStopResult(
                Timestamp: newBar.Timestamp,
                AtrStop: UpperBand,
                BuyStop: UpperBand,
                SellStop: null,
                Atr: atr);
        }

        // the lower band (long / sell-to-stop)
        else
        {
            IsBullish = true;

            r = new AtrStopResult(
                Timestamp: newBar.Timestamp,
                AtrStop: LowerBand,
                BuyStop: null,
                SellStop: LowerBand,
                Atr: atr);
        }

        return (r, i);
    }

    /// <summary>
    /// Restores the prior ATR Stop state.
    /// </summary>
    /// <inheritdoc/>
    protected override void RollbackState(int restoreIndex)
    {
        // restore prior stop point
        if (restoreIndex >= LookbackPeriods)
        {
            AtrStopResult resetStop = Cache[restoreIndex];

            // prevailing direction and bands
            IsBullish = resetStop.AtrStop >= resetStop.SellStop;
            UpperBand = resetStop.BuyStop ?? default;
            LowerBand = resetStop.SellStop ?? default;
        }

        // or reset if no prior stop found
        else
        {
            IsBullish = default;
            UpperBand = default;
            LowerBand = default;
        }
    }
}

public static partial class AtrStop
{
    /// <summary>
    /// Creates an ATR Stop hub.
    /// </summary>
    /// <param name="barProvider">Bar provider.</param>
    /// <param name="lookbackPeriods">Number of lookback periods.</param>
    /// <param name="multiplier">ATR multiplier.</param>
    /// <param name="endType">Price end type to use.</param>
    /// <returns>An instance of <see cref="AtrStopHub"/>.</returns>
    public static AtrStopHub ToAtrStopHub(
       this IBarProvider<IBar> barProvider,
       int lookbackPeriods = 21,
       double multiplier = 3,
       EndType endType = EndType.Close)
           => new(barProvider, lookbackPeriods, multiplier, endType);
}
