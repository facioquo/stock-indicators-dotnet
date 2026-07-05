namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Streaming hub for Choppiness Index (CHOP) on a series of bars.
/// </summary>
public class ChopHub
    : ChainHub<IBar, ChopResult>, IChop
{
    private CircularDoubleBuffer _trueHighBuffer;
    private CircularDoubleBuffer _trueLowBuffer;
    private readonly Queue<double> _trueRangeBuffer;
    private double _sumTrueRange;

    internal ChopHub(
        IBarProvider<IBar> provider,
        int lookbackPeriods) : base(provider)
    {
        Chop.Validate(lookbackPeriods);
        LookbackPeriods = lookbackPeriods;
        _trueHighBuffer = new CircularDoubleBuffer(lookbackPeriods);
        _trueLowBuffer = new CircularDoubleBuffer(lookbackPeriods);
        _trueRangeBuffer = new Queue<double>(lookbackPeriods);
        _sumTrueRange = 0;
        Name = $"CHOP({lookbackPeriods})";

        // Validate cache size for warmup requirements
        ValidateCacheSize(lookbackPeriods + 1, Name);

        Reinitialize();
    }

    /// <inheritdoc/>
    public int LookbackPeriods { get; init; }
    /// <inheritdoc/>
    protected override (ChopResult result, int index)
        ToIndicator(IBar item, int? indexHint)
    {
        ArgumentNullException.ThrowIfNull(item);
        int i = indexHint ?? ProviderCache.IndexOf(item, true);

        // skip first period (no previous close)
        if (i == 0)
        {
            return (new ChopResult(item.Timestamp, null), i);
        }

        double? chop = null;

        double prevClose = (double)ProviderCache[i - 1].Close;
        double trueHigh = Math.Max((double)item.High, prevClose);
        double trueLow = Math.Min((double)item.Low, prevClose);
        double trueRange = trueHigh - trueLow;

        _trueHighBuffer.Add(trueHigh);
        _trueLowBuffer.Add(trueLow);

        double? dequeuedTR = _trueRangeBuffer.UpdateWithDequeue(LookbackPeriods, trueRange);
        if (dequeuedTR.HasValue)
        {
            _sumTrueRange = _sumTrueRange - dequeuedTR.Value + trueRange;
        }
        else
        {
            _sumTrueRange += trueRange;
        }

        if (i >= LookbackPeriods)
        {
            double maxTrueHigh = _trueHighBuffer.GetMax();
            double minTrueLow = _trueLowBuffer.GetMin();
            double range = maxTrueHigh - minTrueLow;

            if (range != 0)
            {
                chop = 100 * (Math.Log(_sumTrueRange / range) / Math.Log(LookbackPeriods));
            }
        }

        ChopResult r = new(
            Timestamp: item.Timestamp,
            Chop: chop);

        return (r, i);
    }

    /// <summary>
    /// Restores the rolling window state up to the specified timestamp.
    /// </summary>
    /// <inheritdoc/>
    protected override void RollbackState(int restoreIndex)
    {
        _trueHighBuffer.Clear();
        _trueLowBuffer.Clear();
        _trueRangeBuffer.Clear();
        _sumTrueRange = 0;

        if (restoreIndex < 0)
        {
            return;
        }

        int startIdx = Math.Max(1, restoreIndex + 1 - LookbackPeriods);

        for (int p = startIdx; p <= restoreIndex; p++)
        {
            IBar current = ProviderCache[p];
            double prevClose = (double)ProviderCache[p - 1].Close;

            double trueHigh = Math.Max((double)current.High, prevClose);
            double trueLow = Math.Min((double)current.Low, prevClose);
            double trueRange = trueHigh - trueLow;

            _trueHighBuffer.Add(trueHigh);
            _trueLowBuffer.Add(trueLow);
            _trueRangeBuffer.Enqueue(trueRange);
            _sumTrueRange += trueRange;
        }
    }
}

public static partial class Chop
{
    /// <summary>
    /// Creates a Choppiness Index (CHOP) streaming hub from a bar provider.
    /// </summary>
    /// <param name="barProvider">Bar provider.</param>
    /// <param name="lookbackPeriods">Quantity of periods in lookback window.</param>
    /// <returns>A ChopHub instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the bar provider is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the lookback periods are invalid.</exception>
    public static ChopHub ToChopHub(
        this IBarProvider<IBar> barProvider,
        int lookbackPeriods = 14)
             => new(barProvider, lookbackPeriods);
}
