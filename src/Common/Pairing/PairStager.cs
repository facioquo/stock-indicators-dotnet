using System.Diagnostics.CodeAnalysis;

namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Side of a two-series pairing.
/// </summary>
internal enum PairSide
{
    /// <summary>
    /// The source series; its value is the pair's chainable value.
    /// </summary>
    Source = 0,

    /// <summary>
    /// The other series, matched to the source by timestamp.
    /// </summary>
    Other = 1
}

/// <summary>
/// Holds the values of two series, each in timestamp order, and matches them by timestamp.
/// </summary>
/// <remarks>
/// Values from different timestamps are never paired. A timestamp that only one side
/// holds stays staged and produces no pair until the other side supplies it.
/// Each side keeps at most <c>maxCount</c> values, dropping the oldest first.
/// Not thread-safe; the owner serializes access.
/// </remarks>
internal sealed class PairStager
{
    private readonly PruningList<TimeValue> _source = new();
    private readonly PruningList<TimeValue> _other = new();
    private readonly int _maxCount;

    /// <summary>
    /// Initializes a new instance of the <see cref="PairStager"/> class.
    /// </summary>
    /// <param name="maxCount">Most values each side keeps.</param>
    internal PairStager(int maxCount)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(maxCount, 1);
        _maxCount = maxCount;
    }

    /// <summary>
    /// Gets the number of values staged on a side.
    /// </summary>
    /// <param name="side">Side to count.</param>
    /// <returns>The number of staged values.</returns>
    internal int Count(PairSide side) => List(side).Count;

    /// <summary>
    /// Gets the newest timestamp staged on a side.
    /// </summary>
    /// <param name="side">Side to read.</param>
    /// <returns>The newest timestamp, or <see langword="null"/> when the side is empty.</returns>
    internal DateTime? Last(PairSide side)
    {
        PruningList<TimeValue> list = List(side);
        return list.Count == 0 ? null : list[^1].Timestamp;
    }

    /// <summary>
    /// Stages a value, replacing any value the side already holds at that timestamp.
    /// </summary>
    /// <param name="side">Side the value belongs to.</param>
    /// <param name="timestamp">Timestamp of the value.</param>
    /// <param name="value">The value; NaN is kept as is.</param>
    internal void Set(PairSide side, DateTime timestamp, double value)
    {
        PruningList<TimeValue> list = List(side);
        TimeValue item = new(timestamp, value);

        // the usual case: a newer value appends
        if (list.Count == 0 || timestamp > list[^1].Timestamp)
        {
            list.Add(item);

            if (list.Count > _maxCount)
            {
                list.RemoveRange(0, list.Count - _maxCount);
            }

            return;
        }

        int index = list.IndexGte(timestamp);

        if (list[index].Timestamp == timestamp)
        {
            list[index] = item;
        }
        else
        {
            list.Insert(index, item);

            if (list.Count > _maxCount)
            {
                list.RemoveRange(0, list.Count - _maxCount);
            }
        }
    }

    /// <summary>
    /// Matches both sides at a timestamp.
    /// </summary>
    /// <param name="timestamp">Timestamp to match.</param>
    /// <param name="pair">The matched values, when both sides hold the timestamp.</param>
    /// <returns><see langword="true"/> when both sides hold the timestamp.</returns>
    internal bool TryGetPair(DateTime timestamp, [NotNullWhen(true)] out PairedValue? pair)
    {
        int s = _source.IndexOf(timestamp, false);
        int o = s == -1 ? -1 : _other.IndexOf(timestamp, false);

        if (o == -1)
        {
            pair = null;
            return false;
        }

        pair = new PairedValue(timestamp, _source[s].Value, _other[o].Value);
        return true;
    }

    /// <summary>
    /// Removes a side's values at or after a timestamp.
    /// </summary>
    /// <param name="side">Side to truncate.</param>
    /// <param name="timestamp">First timestamp to remove.</param>
    internal void RemoveFrom(PairSide side, DateTime timestamp)
    {
        PruningList<TimeValue> list = List(side);
        int index = list.IndexGte(timestamp);

        if (index >= 0)
        {
            list.RemoveRange(index, list.Count - index);
        }
    }

    /// <summary>
    /// Removes a side's values after a timestamp.
    /// </summary>
    /// <param name="side">Side to truncate.</param>
    /// <param name="timestamp">Newest timestamp to keep.</param>
    internal void RemoveAfter(PairSide side, DateTime timestamp)
    {
        PruningList<TimeValue> list = List(side);
        int index = list.IndexGte(timestamp);

        if (index >= 0 && list[index].Timestamp == timestamp)
        {
            index++;
        }

        if (index >= 0 && index < list.Count)
        {
            list.RemoveRange(index, list.Count - index);
        }
    }

    /// <summary>
    /// Removes a side's values at or before a timestamp.
    /// </summary>
    /// <param name="side">Side to trim.</param>
    /// <param name="timestamp">Newest timestamp to remove.</param>
    internal void RemoveThrough(PairSide side, DateTime timestamp)
    {
        PruningList<TimeValue> list = List(side);
        int index = list.IndexGte(timestamp);
        int count = index == -1
            ? list.Count
            : list[index].Timestamp == timestamp ? index + 1 : index;

        if (count > 0)
        {
            list.RemoveRange(0, count);
        }
    }

    /// <summary>
    /// Removes every staged value on both sides.
    /// </summary>
    internal void Clear()
    {
        _source.Clear();
        _other.Clear();
    }

    private PruningList<TimeValue> List(PairSide side)
        => side == PairSide.Source ? _source : _other;
}
