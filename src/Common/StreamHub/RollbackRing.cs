namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Fixed-size ring of recent <c>(timestamp, state)</c> snapshots that makes
/// <see cref="StreamHub{TIn, TOut}.RollbackState(int)"/> O(1) for near-tail
/// rollbacks — the dominant case for live feeds, where corrections target
/// recent bars and aggregators re-send the forming bar on every tick.
/// </summary>
/// <remarks>
/// A hub records a snapshot of its internal state after processing each item.
/// On rollback, the state for the restore point is recovered from the ring
/// by timestamp — bit-exact, because it is the very value the live
/// computation produced — and only rollbacks deeper than the ring's capacity
/// fall back to the indicator's full state replay.
/// <para>
/// Adding with the same timestamp as the newest entry replaces that entry,
/// so repeated forming-bar updates (and same-timestamp duplicates from
/// providers like Renko) occupy a single slot and cannot flush the ring.
/// Lookups scan newest-first, so re-recorded states after a replay shadow
/// any stale older entries for the same timestamp.
/// </para>
/// </remarks>
/// <typeparam name="TState">Snapshot value, typically a small struct.</typeparam>
internal sealed class RollbackRing<TState>
{
    private const int defaultCapacity = 32;

    private readonly DateTime[] _timestamps;
    private readonly TState[] _states;
    private readonly int _capacity;

    private int _next;  // next write position
    private int _count; // filled entries

    /// <summary>
    /// Initializes a new instance of the <see cref="RollbackRing{TState}"/> class.
    /// </summary>
    /// <param name="capacity">Maximum retained snapshots.</param>
    internal RollbackRing(int capacity = defaultCapacity)
    {
        if (capacity <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(capacity), capacity,
                "Capacity must be greater than 0.");
        }

        _capacity = capacity;
        _timestamps = new DateTime[capacity];
        _states = new TState[capacity];
    }

    /// <summary>
    /// Records a snapshot, replacing the newest entry when the timestamp
    /// repeats (forming-bar updates, same-timestamp duplicates).
    /// </summary>
    /// <param name="timestamp">Timestamp of the processed item.</param>
    /// <param name="state">State after processing the item.</param>
    internal void Add(DateTime timestamp, in TState state)
    {
        if (_count > 0)
        {
            int newest = (_next - 1 + _capacity) % _capacity;

            if (_timestamps[newest] == timestamp)
            {
                _states[newest] = state;
                return;
            }
        }

        _timestamps[_next] = timestamp;
        _states[_next] = state;
        _next = (_next + 1) % _capacity;

        if (_count < _capacity)
        {
            _count++;
        }
    }

    /// <summary>
    /// Finds the newest snapshot recorded for <paramref name="timestamp"/>.
    /// </summary>
    /// <param name="timestamp">Restore-point timestamp to find.</param>
    /// <param name="state">Recovered state, when found.</param>
    /// <returns>True when a snapshot was found.</returns>
    internal bool TryGet(DateTime timestamp, out TState state)
    {
        for (int back = 1; back <= _count; back++)
        {
            int index = (_next - back + _capacity) % _capacity;

            if (_timestamps[index] == timestamp)
            {
                state = _states[index];
                return true;
            }
        }

        state = default!;
        return false;
    }

    /// <summary>
    /// Removes all snapshots.
    /// </summary>
    internal void Clear()
    {
        _next = 0;
        _count = 0;
    }
}
