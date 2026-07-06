namespace FacioQuo.Stock.Indicators;

/// <inheritdoc />
public class UltimateHub
    : ChainHub<IBar, UltimateResult>, IUltimate
{
    // Rolling-sum queues for O(1) per-update calculation.
    // Each queue holds (BuyingPressure, TrueRange) pairs for its window.
    private readonly Queue<(double Bp, double Tr)> _shortQueue;
    private readonly Queue<(double Bp, double Tr)> _middleQueue;
    private readonly Queue<(double Bp, double Tr)> _longQueue;

    private double _sumBp1, _sumTr1;   // running sums for short window
    private double _sumBp2, _sumTr2;   // running sums for middle window
    private double _sumBp3, _sumTr3;   // running sums for long window

    internal UltimateHub(
        IBarProvider<IBar> provider,
        int shortPeriods,
        int middlePeriods,
        int longPeriods)
        : base(provider)
    {
        Ultimate.Validate(shortPeriods, middlePeriods, longPeriods);
        ShortPeriods = shortPeriods;
        MiddlePeriods = middlePeriods;
        LongPeriods = longPeriods;
        Name = $"UO({shortPeriods},{middlePeriods},{longPeriods})";

        _shortQueue = new Queue<(double, double)>(shortPeriods);
        _middleQueue = new Queue<(double, double)>(middlePeriods);
        _longQueue = new Queue<(double, double)>(longPeriods);

        // Validate cache size for warmup requirements
        // requiredWarmup = Math.Max(Math.Max(shortPeriods, middlePeriods), longPeriods) + 1
        // ToIndicator accesses ProviderCache[i-1], which requires one extra period beyond the largest window.
        // Ultimate.Validate() guarantees longPeriods >= middlePeriods >= shortPeriods,
        // so in practice longPeriods + 1 is the effective minimum.
        int requiredWarmup = Math.Max(Math.Max(shortPeriods, middlePeriods), longPeriods) + 1;
        ValidateCacheSize(requiredWarmup, Name);

        Reinitialize();
    }

    /// <inheritdoc/>
    public int ShortPeriods { get; init; }

    /// <inheritdoc/>
    public int MiddlePeriods { get; init; }

    /// <inheritdoc/>
    public int LongPeriods { get; init; }

    /// <inheritdoc/>
    protected override (UltimateResult result, int index)
        ToIndicator(IBar item, int? indexHint)
    {
        ArgumentNullException.ThrowIfNull(item);
        int i = indexHint ?? ProviderCache.IndexOf(item, true);

        // skip first period (need previous close)
        if (i == 0)
        {
            return (new UltimateResult(item.Timestamp, null), i);
        }

        // compute buying pressure and true range from provider cache (O(1))
        IBar prev = ProviderCache[i - 1];
        double high = (double)item.High;
        double low = (double)item.Low;
        double close = (double)item.Close;
        double prevClose = (double)prev.Close;

        double bp = close - Math.Min(low, prevClose);
        double tr = Math.Max(high, prevClose) - Math.Min(low, prevClose);

        // update each rolling-sum queue, subtracting the evicted value and adding the new one
        UpdateRollingSum(_longQueue, LongPeriods, bp, tr, ref _sumBp3, ref _sumTr3);
        UpdateRollingSum(_middleQueue, MiddlePeriods, bp, tr, ref _sumBp2, ref _sumTr2);
        UpdateRollingSum(_shortQueue, ShortPeriods, bp, tr, ref _sumBp1, ref _sumTr1);

        // not enough data for calculation yet
        if (_longQueue.Count < LongPeriods)
        {
            return (new UltimateResult(item.Timestamp, null), i);
        }

        // calculate averages (avoid division by zero)
        double avg1 = _sumTr1 == 0 ? double.NaN : _sumBp1 / _sumTr1;
        double avg2 = _sumTr2 == 0 ? double.NaN : _sumBp2 / _sumTr2;
        double avg3 = _sumTr3 == 0 ? double.NaN : _sumBp3 / _sumTr3;

        // calculate Ultimate Oscillator with weighted average
        double ultimate = (100d * ((4d * avg1) + (2d * avg2) + avg3) / 7d).NaN2Null() ?? double.NaN;

        return (new UltimateResult(item.Timestamp, ultimate.NaN2Null()), i);
    }

    /// <inheritdoc/>
    protected override void RollbackState(int restoreIndex)
    {
        // clear all rolling state
        _shortQueue.Clear();
        _middleQueue.Clear();
        _longQueue.Clear();
        _sumBp1 = _sumTr1 = 0;
        _sumBp2 = _sumTr2 = 0;
        _sumBp3 = _sumTr3 = 0;

        // restoreIndex < 0 means reset to initial state (no data to preserve)
        if (restoreIndex < 0)
        {
            return;
        }

        // rebuild queues: only need the last LongPeriods items up to restoreIndex
        int startIdx = Math.Max(1, restoreIndex + 1 - LongPeriods);

        for (int p = startIdx; p <= restoreIndex; p++)
        {
            IBar current = ProviderCache[p];
            IBar previous = ProviderCache[p - 1];

            double bp = (double)current.Close - Math.Min((double)current.Low, (double)previous.Close);
            double tr = Math.Max((double)current.High, (double)previous.Close) - Math.Min((double)current.Low, (double)previous.Close);

            UpdateRollingSum(_longQueue, LongPeriods, bp, tr, ref _sumBp3, ref _sumTr3);
            UpdateRollingSum(_middleQueue, MiddlePeriods, bp, tr, ref _sumBp2, ref _sumTr2);
            UpdateRollingSum(_shortQueue, ShortPeriods, bp, tr, ref _sumBp1, ref _sumTr1);
        }
    }

    /// <summary>
    /// Enqueues a new (bp, tr) pair into a bounded rolling-sum queue, adjusting
    /// the running sums by subtracting the evicted entry (if at capacity) and
    /// adding the new entry.
    /// </summary>
    /// <param name="queue">The bounded queue of (Bp, Tr) pairs.</param>
    /// <param name="capacity">Maximum number of entries to retain.</param>
    /// <param name="bp">Buying pressure value to enqueue.</param>
    /// <param name="tr">True range value to enqueue.</param>
    /// <param name="sumBp">Running sum of buying pressure, updated in place.</param>
    /// <param name="sumTr">Running sum of true range, updated in place.</param>
    private static void UpdateRollingSum(
        Queue<(double Bp, double Tr)> queue, int capacity,
        double bp, double tr,
        ref double sumBp, ref double sumTr)
    {
        (double Bp, double Tr)? evicted = queue.UpdateWithDequeue(capacity, (bp, tr));
        sumBp += bp - (evicted.HasValue ? evicted.Value.Bp : 0);
        sumTr += tr - (evicted.HasValue ? evicted.Value.Tr : 0);
    }
}

public static partial class Ultimate
{
    /// <summary>
    /// Converts the provided bar provider to an Ultimate Oscillator hub with the specified periods.
    /// </summary>
    /// <param name="barProvider">Bar provider to convert.</param>
    /// <param name="shortPeriods">Number of short lookback periods.</param>
    /// <param name="middlePeriods">Number of middle lookback periods.</param>
    /// <param name="longPeriods">Number of long lookback periods.</param>
    /// <returns>An instance of <see cref="UltimateHub"/>.</returns>
    public static UltimateHub ToUltimateHub(
        this IBarProvider<IBar> barProvider,
        int shortPeriods = 7,
        int middlePeriods = 14,
        int longPeriods = 28)
             => new(barProvider, shortPeriods, middlePeriods, longPeriods);
}
