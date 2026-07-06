namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Streaming hub for Chandelier Exit.
/// </summary>
public class ChandelierHub
    : ChainHub<IBar, ChandelierResult>, IChandelier
{
    private CircularDoubleBuffer _highBuffer;
    private CircularDoubleBuffer _lowBuffer;
    private double _prevClose = double.NaN;
    private double _prevAtr = double.NaN;

    internal ChandelierHub(
        IBarProvider<IBar> provider,
        int lookbackPeriods,
        double multiplier,
        Direction type)
        : base(provider)
    {
        ArgumentNullException.ThrowIfNull(provider);
        Chandelier.Validate(lookbackPeriods, multiplier);

        LookbackPeriods = lookbackPeriods;
        Multiplier = multiplier;
        Type = type;

        string typeName = type.ToString().ToUpperInvariant();
        Name = FormattableString.Invariant(
            $"CHEXIT({lookbackPeriods},{multiplier},{typeName})");

        _highBuffer = new CircularDoubleBuffer(lookbackPeriods);
        _lowBuffer = new CircularDoubleBuffer(lookbackPeriods);

        // Validate cache size for warmup requirements
        ValidateCacheSize(lookbackPeriods + 1, Name);

        Reinitialize();
    }

    /// <inheritdoc/>
    public int LookbackPeriods { get; init; }

    /// <inheritdoc/>
    public double Multiplier { get; init; }

    /// <inheritdoc/>
    public Direction Type { get; init; }

    /// <inheritdoc/>
    protected override (ChandelierResult result, int index)
        ToIndicator(IBar item, int? indexHint)
    {
        ArgumentNullException.ThrowIfNull(item);
        int i = indexHint ?? ProviderCache.IndexOf(item, true);

        double high = (double)item.High;
        double low = (double)item.Low;
        double close = (double)item.Close;

        // Add current High/Low to circular buffers for O(capacity) max/min scan
        _highBuffer.Add(high);
        _lowBuffer.Add(low);

        // first period: store previous close for TR calculation; no result yet
        if (i == 0)
        {
            _prevClose = close;
            return (new ChandelierResult(item.Timestamp, null), i);
        }

        double prevClose = _prevClose;
        _prevClose = close;

        // handle warmup periods
        if (i < LookbackPeriods)
        {
            return (new ChandelierResult(item.Timestamp, null), i);
        }

        // compute ATR: re/initialize as SMA when no prior value, else use SMMA
        double atr;

        if (double.IsNaN(_prevAtr))
        {
            // Lazy initialization: compute SMA of the last LookbackPeriods true ranges.
            // This handles both the initial warmup completion and post-rollback re-init,
            // matching the approach used in AtrHub and VolatilityStopHub.
            double sumTr = 0;
            for (int p = i + 1 - LookbackPeriods; p <= i; p++)
            {
                sumTr += Tr.Increment(
                    (double)ProviderCache[p].High,
                    (double)ProviderCache[p].Low,
                    (double)ProviderCache[p - 1].Close);
            }

            atr = sumTr / LookbackPeriods;
        }
        else
        {
            atr = Atr.Increment(LookbackPeriods, high, low, prevClose, _prevAtr);
        }

        _prevAtr = atr;

        double? exit = Type switch {
            Direction.Long => _highBuffer.GetMax() - (atr * Multiplier),
            Direction.Short => _lowBuffer.GetMin() + (atr * Multiplier),
            _ => throw new InvalidOperationException($"Unknown direction type: {Type}")
        };

        return (new ChandelierResult(item.Timestamp, exit), i);
    }

    /// <summary>
    /// Restores rolling window and ATR state to the point just before the rollback timestamp.
    /// </summary>
    /// <inheritdoc/>
    protected override void RollbackState(int restoreIndex)
    {
        // Reset all state
        _highBuffer.Clear();
        _lowBuffer.Clear();
        _prevClose = double.NaN;
        _prevAtr = double.NaN;

        if (restoreIndex < 0)
        {
            return;
        }

        // Rebuild circular buffers from the last LookbackPeriods items
        int windowStart = Math.Max(0, restoreIndex + 1 - LookbackPeriods);
        for (int p = windowStart; p <= restoreIndex; p++)
        {
            IBar q = ProviderCache[p];
            _highBuffer.Add((double)q.High);
            _lowBuffer.Add((double)q.Low);
        }

        // Set prevClose to the last retained bar's close
        _prevClose = (double)ProviderCache[restoreIndex].Close;

        // Rebuild ATR state from LookbackPeriods to restoreIndex.
        // Lazy SMA init fires at the first eligible index; SMMA increments thereafter.
        if (restoreIndex < LookbackPeriods)
        {
            return;
        }

        for (int p = LookbackPeriods; p <= restoreIndex; p++)
        {
            double atr;

            if (double.IsNaN(_prevAtr))
            {
                double sumTr = 0;
                for (int q = p + 1 - LookbackPeriods; q <= p; q++)
                {
                    sumTr += Tr.Increment(
                        (double)ProviderCache[q].High,
                        (double)ProviderCache[q].Low,
                        (double)ProviderCache[q - 1].Close);
                }

                atr = sumTr / LookbackPeriods;
            }
            else
            {
                atr = Atr.Increment(
                    LookbackPeriods,
                    (double)ProviderCache[p].High,
                    (double)ProviderCache[p].Low,
                    (double)ProviderCache[p - 1].Close,
                    _prevAtr);
            }

            _prevAtr = atr;
        }
    }
}

/// <summary>
/// Streaming hub for Chandelier Exit using a stream hub.
/// </summary>
public static partial class Chandelier
{
    /// <summary>
    /// Creates a Chandelier Exit streaming hub from a bars provider.
    /// </summary>
    /// <param name="barProvider">Bar provider.</param>
    /// <param name="lookbackPeriods">Quantity of periods in lookback window.</param>
    /// <param name="multiplier">Multiplier to apply to the ATR.</param>
    /// <param name="type">Type of Chandelier Exit to calculate (Long or Short).</param>
    /// <returns>An instance of <see cref="ChandelierHub"/>.</returns>
    public static ChandelierHub ToChandelierHub(
        this IBarProvider<IBar> barProvider,
        int lookbackPeriods = 22,
        double multiplier = 3,
        Direction type = Direction.Long)
             => new(barProvider, lookbackPeriods, multiplier, type);
}
