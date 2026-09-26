namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Streaming hub for aggregating raw tick data into OHLCV price bars.
/// </summary>
/// <remarks>
/// Gap behavior: by default (<see cref="GapFillMode.None"/>, or
/// the equivalent <c>fillGaps: false</c>) the hub emits only the bars that
/// incoming ticks actually populate — buckets with no upstream activity are
/// not synthesized. <see cref="GapFillMode.ForwardFill"/>
/// (equivalent to <c>fillGaps: true</c>) synthesizes zero-volume bars
/// carrying the prior bar's close as O/H/L/C through the silent period.
/// <see cref="GapFillMode.Interpolate"/> instead synthesizes
/// zero-volume bars whose O/H/L/C are linearly interpolated between the
/// prior bar's close and the arriving tick's price, evenly spaced across the
/// missing buckets.
/// </remarks>
public class TradeTickAggregatorHub
    : BarProvider<ITradeTick, IBar>
{
    private const int maxExecutionIdCacheSize = 10000;

    private readonly Dictionary<string, DateTime> _processedExecutionIds = [];
    private readonly TimeSpan _executionIdRetentionPeriod;
    private Bar? _currentBar;
    private DateTime _currentBarTimestamp;

    /// <summary>
    /// Initializes a new instance of the <see cref="TradeTickAggregatorHub"/> class.
    /// </summary>
    /// <param name="provider">The tick data provider.</param>
    /// <param name="barInterval">The period size to aggregate to.</param>
    /// <param name="fillGaps">Whether to fill gaps by carrying forward the last known price.</param>
    public TradeTickAggregatorHub(
        IStreamObservable<ITradeTick> provider,
        BarInterval barInterval,
        bool fillGaps = false)
        : this(provider, barInterval, fillGaps ? GapFillMode.ForwardFill : GapFillMode.None)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TradeTickAggregatorHub"/> class.
    /// </summary>
    /// <param name="provider">The tick data provider.</param>
    /// <param name="barInterval">The period size to aggregate to.</param>
    /// <param name="gapFillMode">How silent buckets are handled.</param>
    public TradeTickAggregatorHub(
        IStreamObservable<ITradeTick> provider,
        BarInterval barInterval,
        GapFillMode gapFillMode)
        : base(provider)
    {
        if (barInterval == BarInterval.Month)
        {
            throw new ArgumentException(
                $"Month aggregation is not supported in streaming mode. barInterval={barInterval}. Use TimeSpan overload for custom periods.",
                nameof(barInterval));
        }

        TimeSpan agg = barInterval.ToTimeSpan();

        if (agg == TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(barInterval), barInterval, "Unsupported BarInterval");
        }

        AggregationPeriod = agg;
        GapFillMode = gapFillMode;
        Name = $"TRADE-TICK-AGG({barInterval})";

        // Keep execution IDs for 100x the aggregation period or at least 1 hour
        _executionIdRetentionPeriod = TimeSpan.FromTicks(Math.Max(
            AggregationPeriod.Ticks * 100,
            TimeSpan.FromHours(1).Ticks));

        Reinitialize();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TradeTickAggregatorHub"/> class.
    /// </summary>
    /// <param name="provider">The tick data provider.</param>
    /// <param name="timeSpan">The time span to aggregate to.</param>
    /// <param name="fillGaps">Whether to fill gaps by carrying forward the last known price.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the time span is less than or equal to zero.</exception>
    public TradeTickAggregatorHub(
        IStreamObservable<ITradeTick> provider,
        TimeSpan timeSpan,
        bool fillGaps = false)
        : this(provider, timeSpan, fillGaps ? GapFillMode.ForwardFill : GapFillMode.None)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TradeTickAggregatorHub"/> class.
    /// </summary>
    /// <param name="provider">The tick data provider.</param>
    /// <param name="timeSpan">The time span to aggregate to.</param>
    /// <param name="gapFillMode">How silent buckets are handled.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the time span is less than or equal to zero.</exception>
    public TradeTickAggregatorHub(
        IStreamObservable<ITradeTick> provider,
        TimeSpan timeSpan,
        GapFillMode gapFillMode)
        : base(provider)
    {
        if (timeSpan <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(timeSpan), timeSpan,
                "Aggregation period must be greater than zero.");
        }

        AggregationPeriod = timeSpan;
        GapFillMode = gapFillMode;
        Name = $"TRADE-TICK-AGG({timeSpan})";

        // Keep execution IDs for 100x the aggregation period or at least 1 hour
        _executionIdRetentionPeriod = TimeSpan.FromTicks(Math.Max(
            timeSpan.Ticks * 100,
            TimeSpan.FromHours(1).Ticks));

        Reinitialize();
    }

    /// <summary>
    /// Gets a value indicating whether gap filling is enabled.
    /// </summary>
    /// <remarks>
    /// Equivalent to <c>GapFillMode != GapFillMode.None</c>. When <c>true</c>,
    /// buckets that have no upstream activity between the last emitted bar
    /// and the next active bucket are filled with zero-volume synthetic
    /// bars. See <see cref="GapFillMode"/> for the specific fill policy
    /// (forward carry or interpolation). When <c>false</c> (default),
    /// silent buckets are simply omitted from the output stream.
    /// </remarks>
    public bool FillGaps => GapFillMode != GapFillMode.None;

    /// <summary>
    /// Gets the gap-fill policy applied to silent buckets.
    /// </summary>
    /// <remarks>
    /// <see cref="GapFillMode.None"/> (default) omits silent
    /// buckets. <see cref="GapFillMode.ForwardFill"/> carries the
    /// prior bar's close forward as O/H/L/C. <see cref="GapFillMode.Interpolate"/>
    /// linearly interpolates O/H/L/C between the prior bar's close and the
    /// arriving tick's price across the missing buckets. Volume is always
    /// zero for a synthesized bar, regardless of mode.
    /// </remarks>
    public GapFillMode GapFillMode { get; }

    /// <summary>
    /// Gets the aggregation period.
    /// </summary>
    public TimeSpan AggregationPeriod { get; }

    /// <inheritdoc/>
    public override void OnAdd(ITradeTick item, bool notify, int? indexHint)
    {
        ArgumentNullException.ThrowIfNull(item);

        lock (CacheLock)
        {
            // Check for duplicate execution IDs with time-based pruning
            if (!string.IsNullOrEmpty(item.ExecutionId))
            {
                if (_processedExecutionIds.ContainsKey(item.ExecutionId))
                {
                    // Skip duplicate tick
                    return;
                }

                // Add execution ID with timestamp
                _processedExecutionIds[item.ExecutionId] = item.Timestamp;

                // Prune old execution IDs (by time or size)
                if (_processedExecutionIds.Count > (maxExecutionIdCacheSize / 2))
                {
                    DateTime pruneThreshold = item.Timestamp.Add(-_executionIdRetentionPeriod);

                    // Dictionary<TKey,TValue> supports Remove during enumeration,
                    // avoiding a per-tick list allocation on this hot path
                    foreach (KeyValuePair<string, DateTime> entry in _processedExecutionIds)
                    {
                        if (entry.Value < pruneThreshold)
                        {
                            _processedExecutionIds.Remove(entry.Key);
                        }
                    }
                }

                // Hard limit: if still too large after pruning, remove oldest entries.
                // A sorted-timestamp threshold replaces the former
                // OrderBy/Take/Select/ToList chain: one array allocation
                // instead of four intermediate collections on this
                // emergency overflow path.
                if (_processedExecutionIds.Count > maxExecutionIdCacheSize)
                {
                    int removeCount = _processedExecutionIds.Count - (maxExecutionIdCacheSize / 2);

                    DateTime[] timestamps = new DateTime[_processedExecutionIds.Count];
                    _processedExecutionIds.Values.CopyTo(timestamps, 0);
                    Array.Sort(timestamps);
                    DateTime threshold = timestamps[removeCount - 1];

                    // remove all entries older than the threshold, then trim
                    // threshold-equal entries until the quota is met
                    int removed = 0;
                    foreach (KeyValuePair<string, DateTime> entry in _processedExecutionIds)
                    {
                        if (entry.Value < threshold && _processedExecutionIds.Remove(entry.Key))
                        {
                            removed++;
                        }
                    }

                    foreach (KeyValuePair<string, DateTime> entry in _processedExecutionIds)
                    {
                        if (removed >= removeCount)
                        {
                            break;
                        }

                        if (entry.Value == threshold && _processedExecutionIds.Remove(entry.Key))
                        {
                            removed++;
                        }
                    }
                }
            }

            DateTime barTimestamp = item.Timestamp.RoundDown(AggregationPeriod);

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
            if (GapFillMode != GapFillMode.None && isFutureBar && _currentBar != null)
            {
                DateTime lastBarTimestamp = _currentBarTimestamp;
                DateTime nextExpectedBarTimestamp = lastBarTimestamp.Add(AggregationPeriod);

                // Interpolation anchors: the last real close and the
                // arriving tick's price, spread evenly across the gap steps.
                decimal startPrice = _currentBar.Close;
                decimal endPrice = item.Price;
                long gapSteps = ((barTimestamp - lastBarTimestamp).Ticks / AggregationPeriod.Ticks) - 1;
                long step = 0;

                // Fill gaps between last bar and current bar
                while (nextExpectedBarTimestamp < barTimestamp)
                {
                    step++;

                    decimal gapPrice = GapFillMode == GapFillMode.Interpolate
                        ? startPrice + ((endPrice - startPrice) * step / (gapSteps + 1))
                        : startPrice;

                    // Create a gap-fill bar with carried-forward or interpolated prices
                    Bar gapBar = new(
                        Timestamp: nextExpectedBarTimestamp,
                        Open: gapPrice,
                        High: gapPrice,
                        Low: gapPrice,
                        Close: gapPrice,
                        Volume: 0m);

                    // Add gap bar directly to cache
                    AppendCache(gapBar, notify);

                    // Update current bar to the gap bar
                    _currentBar = gapBar;
                    _currentBarTimestamp = nextExpectedBarTimestamp;

                    nextExpectedBarTimestamp = nextExpectedBarTimestamp.Add(AggregationPeriod);
                }
            }

            // Handle new bar or update to current bar
            if (isFutureBar)
            {
                // Start a new bar from the tick
                _currentBar = CreateOrUpdateBar(null, barTimestamp, item);
                _currentBarTimestamp = barTimestamp;

                // Add the new bar directly to cache
                AppendCache(_currentBar, notify);
            }
            else // isCurrentBar
            {
                // Update existing bar with new tick data
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
    /// Creates a new bar or updates an existing bar with tick data.
    /// </summary>
    /// <param name="existingBar">Existing bar to update, or null to create new.</param>
    /// <param name="barTimestamp">Timestamp for the bar.</param>
    /// <param name="tick">TradeTick data to incorporate.</param>
    /// <returns>Updated or new Bar bar.</returns>
    private static Bar CreateOrUpdateBar(Bar? existingBar, DateTime barTimestamp, ITradeTick tick)
    {
        if (existingBar == null)
        {
            // Create new bar from tick
            return new Bar(
                Timestamp: barTimestamp,
                Open: tick.Price,
                High: tick.Price,
                Low: tick.Price,
                Close: tick.Price,
                Volume: tick.Volume);
        }
        // Update existing bar
        return new Bar(
            Timestamp: barTimestamp,
            Open: existingBar.Open,
            High: Math.Max(existingBar.High, tick.Price),
            Low: Math.Min(existingBar.Low, tick.Price),
            Close: tick.Price,
            Volume: existingBar.Volume + tick.Volume);
    }

    /// <inheritdoc/>
    protected override (IBar result, int index)
        ToIndicator(ITradeTick item, int? indexHint)
    {
        ArgumentNullException.ThrowIfNull(item);

        DateTime barTimestamp = item.Timestamp.RoundDown(AggregationPeriod);

        // bar fast path: ticks nearly always land on the forming (last) bar
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

        // Convert tick to a single-price bar
        Bar bar = new(
            Timestamp: barTimestamp,
            Open: item.Price,
            High: item.Price,
            Low: item.Price,
            Close: item.Price,
            Volume: item.Volume);

        return (bar, index);
    }

    /// <inheritdoc/>
    public override string ToString()
        => $"TRADE-TICK-AGG<{AggregationPeriod}>: {Cache.Count} items";

    /// <inheritdoc/>
    /// <remarks>
    /// Aligns the rebuild timestamp to the bucket boundary so that an
    /// upstream-triggered rebuild (whose timestamp is the late input tick's
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
            _processedExecutionIds.Clear();
            return;
        }

        // Clear execution IDs for rolled back period
        DateTime preserveTimestamp = ProviderCache[restoreIndex].Timestamp;

        List<string> toRemove = _processedExecutionIds
            .Where(kvp => kvp.Value > preserveTimestamp)
            .Select(kvp => kvp.Key)
            .ToList();

        foreach (string key in toRemove)
        {
            _processedExecutionIds.Remove(key);
        }
    }
}
