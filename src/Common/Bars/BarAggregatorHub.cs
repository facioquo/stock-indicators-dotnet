namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Streaming hub for aggregating bars into larger time periods.
/// </summary>
/// <remarks>
/// Gap behavior: by default (<see cref="GapFillMode.None"/>, or
/// the equivalent <c>fillGaps: false</c>) the hub emits only the bars that
/// incoming bars actually populate — buckets with no upstream input are not
/// synthesized. <see cref="GapFillMode.ForwardFill"/> (equivalent
/// to <c>fillGaps: true</c>) synthesizes zero-volume bars carrying the prior
/// bar's close as O/H/L/C through the silent period.
/// <see cref="GapFillMode.Interpolate"/> instead synthesizes
/// zero-volume bars whose O/H/L/C are linearly interpolated between the
/// prior bar's close and the next bar's open, evenly spaced across the
/// missing buckets.
/// </remarks>
public class BarAggregatorHub
    : BarProvider<IBar, IBar>
{
    private const int maxInputTrackerSize = 1000;

    private readonly Dictionary<DateTime, IBar> _inputBarTracker = [];
    private readonly PruningList<GapRun> _gapRuns = [];
    private Bar? _currentBar;
    private DateTime _currentBarTimestamp;
    private DateTime? _rebuildFrom;

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
        : this(provider, barInterval, fillGaps ? GapFillMode.ForwardFill : GapFillMode.None)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="BarAggregatorHub"/> class.
    /// </summary>
    /// <param name="provider">The bar provider.</param>
    /// <param name="barInterval">The period size to aggregate to.</param>
    /// <param name="gapFillMode">How silent buckets are handled.</param>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="barInterval"/> is <see cref="BarInterval.Month"/>,
    /// which is not supported in streaming mode.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="gapFillMode"/> is not a defined value.</exception>
    public BarAggregatorHub(
        IBarProvider<IBar> provider,
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

        AggregationPeriod = barInterval.ToTimeSpan();

        if (AggregationPeriod == TimeSpan.Zero)
        {
            throw new ArgumentException(
                $"BarInterval '{barInterval}' maps to TimeSpan.Zero, which is not a valid aggregation period.",
                nameof(barInterval));
        }

        GapFillMode = ValidateGapFillMode(gapFillMode);
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
        : this(provider, timeSpan, fillGaps ? GapFillMode.ForwardFill : GapFillMode.None)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="BarAggregatorHub"/> class.
    /// </summary>
    /// <param name="provider">The bar provider.</param>
    /// <param name="timeSpan">The time span to aggregate to.</param>
    /// <param name="gapFillMode">How silent buckets are handled.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the time span is less than or equal to zero, or <paramref name="gapFillMode"/> is not a defined value.</exception>
    public BarAggregatorHub(
        IBarProvider<IBar> provider,
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
        GapFillMode = ValidateGapFillMode(gapFillMode);
        Name = $"BAR-AGG({timeSpan})";

        Reinitialize();
    }

    /// <summary>
    /// Gets a value indicating whether gap filling is enabled.
    /// </summary>
    /// <remarks>
    /// Equivalent to <c>GapFillMode != GapFillMode.None</c>. When <c>true</c>,
    /// buckets that have no upstream input between the last emitted bar and
    /// the next active bucket are filled with zero-volume synthetic bars.
    /// See <see cref="GapFillMode"/> for the specific fill policy (forward
    /// carry or interpolation). When <c>false</c> (default), silent buckets
    /// are simply omitted from the output stream.
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
    /// next bar's open across the missing buckets. Volume is always zero
    /// for a synthesized bar, regardless of mode.
    /// </remarks>
    public GapFillMode GapFillMode { get; }

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
            if (GapFillMode != GapFillMode.None && isFutureBar && _currentBar != null)
            {
                DateTime lastBarTimestamp = _currentBarTimestamp;
                DateTime nextExpectedBarTimestamp = lastBarTimestamp.Add(AggregationPeriod);

                // Interpolation anchors: the last real close and the
                // arriving bar's open, spread evenly across the gap steps.
                decimal startPrice = _currentBar.Close;
                decimal endPrice = item.Open;
                long gapSteps = ((barTimestamp - lastBarTimestamp).Ticks / AggregationPeriod.Ticks) - 1;
                long step = 0;
                DateTime firstGapTimestamp = nextExpectedBarTimestamp;

                // Fill gaps between last bar and current bar
                while (AggregationPeriod > TimeSpan.Zero && nextExpectedBarTimestamp < barTimestamp)
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

                    // Add gap bar using base class logic
                    (IBar gapResult, _) = ToIndicator(gapBar, null);
                    AppendCache(gapResult, notify);

                    // Update current bar to the gap bar
                    _currentBar = gapBar;
                    _currentBarTimestamp = nextExpectedBarTimestamp;

                    nextExpectedBarTimestamp = nextExpectedBarTimestamp.Add(AggregationPeriod);
                }

                if (step > 0)
                {
                    TrackGapRun(new GapRun(firstGapTimestamp, _currentBarTimestamp, endPrice));
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

        lock (CacheLock)
        {
            // A gap run that loses the input it ends on, or an Interpolate
            // run whose anchor moved, rewinds to the last real bar before it
            // and replays the gap.
            if (GapFillMode != GapFillMode.None && alignedTimestamp != DateTime.MinValue)
            {
                alignedTimestamp = RewindTarget(alignedTimestamp);
            }

            if (GapFillMode != GapFillMode.None)
            {
                TruncateGapRuns(alignedTimestamp);
            }

            _rebuildFrom = alignedTimestamp;

            try
            {
                base.Rebuild(alignedTimestamp);
            }
            finally
            {
                _rebuildFrom = null;
            }
        }
    }

    private static GapFillMode ValidateGapFillMode(GapFillMode gapFillMode)
        => Enum.IsDefined(gapFillMode)
            ? gapFillMode
            : throw new ArgumentOutOfRangeException(
                nameof(gapFillMode), gapFillMode, "Invalid gapFillMode provided.");

    internal int GapRunCount => _gapRuns.Count;

    private void TrackGapRun(GapRun run)
    {
        _gapRuns.Add(run);

        // drop runs the cache has already pruned
        while (_gapRuns.Count > 0 && Cache.Count > 0 && _gapRuns[0].Last < Cache[0].Timestamp)
        {
            _gapRuns.RemoveAt(0);
        }
    }

    /// <summary>
    /// Picks where a gap-fill rebuild must restart. A gap that has lost the
    /// input it ends on rewinds to the last real bar before it, so no trailing
    /// gap bars remain. Under <see cref="GapFillMode.Interpolate"/> a change
    /// inside a gap, or one that moves the anchor the gap ends on, rewinds too;
    /// any other change in the bucket after a gap leaves the gap bars alone.
    /// </summary>
    /// <param name="bucket">Bucket start the rebuild begins at.</param>
    /// <returns>The bucket start to rewind to.</returns>
    private DateTime RewindTarget(DateTime bucket)
    {
        DateTime previous = bucket.Subtract(AggregationPeriod);

        for (int i = _gapRuns.Count - 1; i >= 0 && _gapRuns[i].Last >= previous; i--)
        {
            GapRun run = _gapRuns[i];

            if (run.Last == previous)
            {
                decimal? anchor = FirstAnchorIn(bucket);
                bool keep = anchor.HasValue
                    && (GapFillMode != GapFillMode.Interpolate || anchor.Value == run.EndAnchor);

                return keep ? bucket : run.First.Subtract(AggregationPeriod);
            }

            if (GapFillMode == GapFillMode.Interpolate && run.First <= bucket)
            {
                return run.First.Subtract(AggregationPeriod);
            }
        }

        return bucket;
    }

    private void TruncateGapRuns(DateTime from)
    {
        while (_gapRuns.Count > 0 && _gapRuns[^1].First >= from)
        {
            _gapRuns.RemoveAt(_gapRuns.Count - 1);
        }

        // a run that straddles the rebuild point keeps only the part before it
        if (_gapRuns.Count > 0 && _gapRuns[^1].Last >= from)
        {
            GapRun run = _gapRuns[^1];
            _gapRuns[^1] = run with { Last = from.Subtract(AggregationPeriod) };
        }
    }

    /// <summary>
    /// Gets the anchor of the first input in the bucket, or null when the
    /// bucket holds none.
    /// </summary>
    /// <param name="bucket">Bucket start to look in.</param>
    /// <returns>The first input's open, or null.</returns>
    private decimal? FirstAnchorIn(DateTime bucket)
    {
        int index = ProviderCache.IndexGte(bucket);

        if (index < 0 || ProviderCache[index].Timestamp >= bucket.Add(AggregationPeriod))
        {
            return null;
        }

        IBar item = ProviderCache[index];
        return item.Open;
    }

    private readonly record struct GapRun(DateTime First, DateTime Last, decimal EndAnchor);

    /// <inheritdoc/>
    protected override void RollbackState(int restoreIndex)
    {
        _currentBar = null;
        _currentBarTimestamp = default;

        // A gap-fill rebuild resumes from the last kept bar, so the replay
        // fills the silent buckets between it and the first replayed input.
        if (GapFillMode != GapFillMode.None && _rebuildFrom.HasValue)
        {
            int keepIndex = Cache.IndexGte(_rebuildFrom.Value);
            int lastKept = (keepIndex < 0 ? Cache.Count : keepIndex) - 1;

            if (lastKept >= 0)
            {
                IBar kept = Cache[lastKept];
                _currentBar = new Bar(kept.Timestamp, kept.Open, kept.High, kept.Low, kept.Close, kept.Volume);
                _currentBarTimestamp = kept.Timestamp;
            }
        }

        if (restoreIndex < 0)
        {
            _inputBarTracker.Clear();
            _gapRuns.Clear();
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
    /// <param name="barInterval">The period size to aggregate to.</param>
    /// <param name="gapFillMode">How silent buckets are handled.</param>
    /// <returns>A new instance of BarAggregatorHub.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="gapFillMode"/> is not a defined value.</exception>
    public static BarAggregatorHub ToBarAggregatorHub(
        this IBarProvider<IBar> barProvider,
        BarInterval barInterval,
        GapFillMode gapFillMode)
        => new(barProvider, barInterval, gapFillMode);

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

    /// <summary>
    /// Creates a BarAggregatorHub that aggregates bars from the provider into larger time periods.
    /// </summary>
    /// <param name="barProvider">The bar provider to aggregate.</param>
    /// <param name="timeSpan">The time span to aggregate to.</param>
    /// <param name="gapFillMode">How silent buckets are handled.</param>
    /// <returns>A new instance of BarAggregatorHub.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="gapFillMode"/> is not a defined value.</exception>
    public static BarAggregatorHub ToBarAggregatorHub(
        this IBarProvider<IBar> barProvider,
        TimeSpan timeSpan,
        GapFillMode gapFillMode)
        => new(barProvider, timeSpan, gapFillMode);
}
