namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Streaming hub for aggregating bars into larger time periods.
/// </summary>
/// <remarks>
/// Gap behavior: by default (<c>fillGaps: false</c>) the hub emits only
/// the bars that incoming bars actually populate — buckets with no
/// upstream input are not synthesized. Set <c>fillGaps: true</c> to
/// synthesize zero-volume bars carrying the prior bar's close as
/// O/H/L/C through the silent period. Consumers that need a different
/// fill policy (e.g. interpolation) should pre-process the input stream
/// before subscribing this hub.
/// </remarks>
public class BarAggregatorHub
    : BarProvider<IBar, IBar>
{
    private const int maxInputTrackerSize = 1000;

    private readonly Dictionary<DateTime, IBar> _inputBarTracker = [];
    private Bar? _currentBar;
    private DateTime _currentBarTimestamp;

    /// <summary>
    /// Initializes a new instance of the <see cref="BarAggregatorHub"/> class.
    /// </summary>
    /// <param name="provider">The bar provider.</param>
    /// <param name="barInterval">The period size to aggregate to.</param>
    /// <param name="fillGaps">Whether to fill gaps by carrying forward the last known price.</param>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="barInterval"/> is <see cref="BarInterval.Month"/>,
    /// which is not supported in streaming mode.
    /// </exception>
    public BarAggregatorHub(
        IBarProvider<IBar> provider,
        BarInterval barInterval,
        bool fillGaps = false)
        : base(provider)
    {
        if (barInterval == BarInterval.Month)
        {
            throw new ArgumentException(
                $"Month aggregation is not supported in streaming mode. barInterval={barInterval}. Use TimeSpan overload for custom periods.",
                nameof(barInterval));
        }

        AggregationPeriod = barInterval.ToTimeSpan();

        if (AggregationPeriod == TimeSpan.Zero)
        {
            throw new ArgumentException(
                $"BarInterval '{barInterval}' maps to TimeSpan.Zero, which is not a valid aggregation period.",
                nameof(barInterval));
        }

        FillGaps = fillGaps;
        Name = $"BAR-AGG({barInterval})";

        Reinitialize();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="BarAggregatorHub"/> class.
    /// </summary>
    /// <param name="provider">The bar provider.</param>
    /// <param name="timeSpan">The time span to aggregate to.</param>
    /// <param name="fillGaps">Whether to fill gaps by carrying forward the last known price.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the time span is less than or equal to zero.</exception>
    public BarAggregatorHub(
        IBarProvider<IBar> provider,
        TimeSpan timeSpan,
        bool fillGaps = false)
        : base(provider)
    {
        if (timeSpan <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(timeSpan), timeSpan,
                "Aggregation period must be greater than zero.");
        }

        AggregationPeriod = timeSpan;
        FillGaps = fillGaps;
        Name = $"BAR-AGG({timeSpan})";

        Reinitialize();
    }

    /// <summary>
    /// Gets a value indicating whether gap filling is enabled.
    /// </summary>
    /// <remarks>
    /// When <c>true</c>, buckets that have no upstream input between
    /// the last emitted bar and the next active bucket are filled with
    /// zero-volume synthetic bars whose O/H/L/C all carry forward the
    /// prior bar's close. When <c>false</c> (default), silent buckets
    /// are simply omitted from the output stream.
    /// </remarks>
    public bool FillGaps { get; }

    /// <summary>
    /// Gets the aggregation period.
    /// </summary>
    public TimeSpan AggregationPeriod { get; }

    /// <inheritdoc/>
    public override void OnAdd(IBar item, bool notify, int? indexHint)
    {
        ArgumentNullException.ThrowIfNull(item);

        lock (CacheLock)
        {
            DateTime barTimestamp = item.Timestamp.RoundDown(AggregationPeriod);

            // Check if this exact input bar was already processed (duplicate detection)
            if (_inputBarTracker.ContainsKey(item.Timestamp))
            {
                // Update tracker with new bar
                _inputBarTracker[item.Timestamp] = item;

                // Rebuild from this bar to recalculate correctly
                if (_currentBar != null && barTimestamp == _currentBarTimestamp)
                {
                    Rebuild(barTimestamp);
                    return;
                }
            }
            else
            {
                // Track this input bar
                _inputBarTracker[item.Timestamp] = item;

                // Prune old tracker entries
                if (_inputBarTracker.Count > maxInputTrackerSize)
                {
                    DateTime pruneThreshold = item.Timestamp.Add(-10 * AggregationPeriod);

                    // Remove entries older than threshold (no LINQ)
                    List<DateTime> keysToRemove = [];
                    foreach (DateTime key in _inputBarTracker.Keys)
                    {
                        if (key < pruneThreshold)
                        {
                            keysToRemove.Add(key);
                        }
                    }

                    foreach (DateTime key in keysToRemove)
                    {
                        _inputBarTracker.Remove(key);
                    }

                    // Hard cap: if threshold pruning wasn't enough, remove oldest until within limit
                    while (_inputBarTracker.Count > maxInputTrackerSize)
                    {
                        DateTime oldest = DateTime.MaxValue;
                        foreach (DateTime key in _inputBarTracker.Keys)
                        {
                            if (key < oldest)
                            {
                                oldest = key;
                            }
                        }

                        _inputBarTracker.Remove(oldest);
                    }
                }
            }

            // Determine if this is for current bar, future bar, or past bar
            bool isFutureBar = _currentBar == null || barTimestamp > _currentBarTimestamp;
            bool isPastBar = _currentBar != null && barTimestamp < _currentBarTimestamp;

            // Handle late arrival for past bar
            if (isPastBar)
            {
                Rebuild(barTimestamp);
                return;
            }

            // Handle gap filling if enabled and moving to future bar
            if (FillGaps && isFutureBar && _currentBar != null)
            {
                DateTime lastBarTimestamp = _currentBarTimestamp;
                DateTime nextExpectedBarTimestamp = lastBarTimestamp.Add(AggregationPeriod);

                // Fill gaps between last bar and current bar
                while (AggregationPeriod > TimeSpan.Zero && nextExpectedBarTimestamp < barTimestamp)
                {
                    // Create a gap-fill bar with carried-forward prices
                    Bar gapBar = new(
                        Timestamp: nextExpectedBarTimestamp,
                        Open: _currentBar.Close,
                        High: _currentBar.Close,
                        Low: _currentBar.Close,
                        Close: _currentBar.Close,
                        Volume: 0m);

                    // Add gap bar using base class logic
                    (IBar gapResult, _) = ToIndicator(gapBar, null);
                    AppendCache(gapResult, notify);

                    // Update current bar to the gap bar
                    _currentBar = gapBar;
                    _currentBarTimestamp = nextExpectedBarTimestamp;

                    nextExpectedBarTimestamp = nextExpectedBarTimestamp.Add(AggregationPeriod);
                }
            }

            // Handle new bar or update to current bar
            if (isFutureBar)
            {
                // Start a new bar
                _currentBar = CreateOrUpdateBar(null, barTimestamp, item);
                _currentBarTimestamp = barTimestamp;

                // Use base class to add the new bar
                (IBar result, _) = ToIndicator(_currentBar, indexHint);
                AppendCache(result, notify);
            }
            else // isCurrentBar
            {
                // Update existing bar - for bars with same timestamp, replace
                _currentBar = CreateOrUpdateBar(_currentBar, barTimestamp, item);

                // Replace the last item in cache with updated bar
                int index = Cache.Count - 1;
                if (index >= 0)
                {
                    Cache[index] = _currentBar;

                    // Notify observers of the update
                    if (notify)
                    {
                        NotifyObserversOnRebuild(_currentBar.Timestamp);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Creates a new bar or updates an existing bar with bar data.
    /// </summary>
    /// <param name="existingBar">Existing bar to update, or null to create new.</param>
    /// <param name="barTimestamp">Timestamp for the bar.</param>
    /// <param name="bar">Bar data to incorporate.</param>
    /// <returns>Updated or new Bar bar.</returns>
    private static Bar CreateOrUpdateBar(Bar? existingBar, DateTime barTimestamp, IBar bar)
    {
        if (existingBar == null)
        {
            // Create new bar from bar
            return new Bar(
                Timestamp: barTimestamp,
                Open: bar.Open,
                High: bar.High,
                Low: bar.Low,
                Close: bar.Close,
                Volume: bar.Volume);
        }
        // Update existing bar
        return new Bar(
            Timestamp: barTimestamp,
            Open: existingBar.Open,
            High: Math.Max(existingBar.High, bar.High),
            Low: Math.Min(existingBar.Low, bar.Low),
            Close: bar.Close,
            Volume: existingBar.Volume + bar.Volume);
    }

    /// <inheritdoc/>
    protected override (IBar result, int index)
        ToIndicator(IBar item, int? indexHint)
    {
        ArgumentNullException.ThrowIfNull(item);

        DateTime barTimestamp = item.Timestamp.RoundDown(AggregationPeriod);

        // bar fast path: bars nearly always land on the forming (last) bar
        // or open a new one; skip the binary search for both cases
        int index;
        if (indexHint.HasValue)
        {
            index = indexHint.Value;
        }
        else if (Cache.Count == 0 || barTimestamp > Cache[^1].Timestamp)
        {
            index = Cache.Count;
        }
        else if (barTimestamp == Cache[^1].Timestamp)
        {
            index = Cache.Count - 1;
        }
        else
        {
            index = Cache.IndexGte(barTimestamp);

            if (index == -1)
            {
                index = Cache.Count;
            }
        }

        return (item, index);
    }

    /// <inheritdoc/>
    public override string ToString()
        => $"BAR-AGG<{AggregationPeriod}>: {Cache.Count} items";

    /// <inheritdoc/>
    /// <remarks>
    /// Aligns the rebuild timestamp to the bucket boundary so that an
    /// upstream-triggered rebuild (whose timestamp is the late input bar's
    /// timestamp, not a bucket start) clears the partial aggregated bar and
    /// re-aggregates the bucket from scratch. Without this alignment the
    /// existing in-cache bar at the bucket start is kept and the replay
    /// appends a duplicate bar at the same timestamp.
    /// </remarks>
    public override void Rebuild(DateTime fromTimestamp)
    {
        DateTime alignedTimestamp = fromTimestamp == DateTime.MinValue
            ? fromTimestamp
            : fromTimestamp.RoundDown(AggregationPeriod);

        base.Rebuild(alignedTimestamp);
    }

    /// <inheritdoc/>
    protected override void RollbackState(int restoreIndex)
    {
        _currentBar = null;
        _currentBarTimestamp = default;

        if (restoreIndex < 0)
        {
            _inputBarTracker.Clear();
            return;
        }

        // Clear input tracker for rolled back period
        DateTime preserveTimestamp = ProviderCache[restoreIndex].Timestamp;

        List<DateTime> toRemove = _inputBarTracker
            .Where(kvp => kvp.Key > preserveTimestamp)
            .Select(kvp => kvp.Key)
            .ToList();

        foreach (DateTime key in toRemove)
        {
            _inputBarTracker.Remove(key);
        }
    }
}

public static partial class Bars
{
    /// <summary>
    /// Creates a BarAggregatorHub that aggregates bars from the provider into larger time periods.
    /// </summary>
    /// <param name="barProvider">The bar provider to aggregate.</param>
    /// <param name="barInterval">The period size to aggregate to.</param>
    /// <param name="fillGaps">Whether to fill gaps by carrying forward the last known price.</param>
    /// <returns>A new instance of BarAggregatorHub.</returns>
    public static BarAggregatorHub ToBarAggregatorHub(
        this IBarProvider<IBar> barProvider,
        BarInterval barInterval,
        bool fillGaps = false)
        => new(barProvider, barInterval, fillGaps);

    /// <summary>
    /// Creates a BarAggregatorHub that aggregates bars from the provider into larger time periods.
    /// </summary>
    /// <param name="barProvider">The bar provider to aggregate.</param>
    /// <param name="timeSpan">The time span to aggregate to.</param>
    /// <param name="fillGaps">Whether to fill gaps by carrying forward the last known price.</param>
    /// <returns>A new instance of BarAggregatorHub.</returns>
    public static BarAggregatorHub ToBarAggregatorHub(
        this IBarProvider<IBar> barProvider,
        TimeSpan timeSpan,
        bool fillGaps = false)
        => new(barProvider, timeSpan, fillGaps);
}
