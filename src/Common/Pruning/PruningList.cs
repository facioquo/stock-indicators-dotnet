using System.Collections;

namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Append-only list with O(1) amortized removal from the front, used as the
/// backing store for sliding-window caches that prune their oldest items.
/// </summary>
/// <remarks>
/// A plain <see cref="List{T}"/> shifts every remaining element on each
/// front removal, so pruning one item per add (the steady state once a
/// cache reaches its cap) degrades to O(n²) over a long stream. This list
/// keeps a logical <c>head</c> offset into an underlying <see cref="List{T}"/>:
/// front removals just advance the offset in O(1), and the dead prefix is
/// physically reclaimed only when it grows to the size of the live window —
/// keeping amortized removal O(1) and storage bounded to ~2x the live count.
/// <para>
/// Index, count, enumeration, and mutation semantics match
/// <see cref="List{T}"/> over the live window, so this type is a drop-in
/// replacement wherever only the retained (post-prune) elements are observed.
/// </para>
/// </remarks>
/// <typeparam name="T">Type of elements stored in the list.</typeparam>
internal sealed class PruningList<T> : IList<T>, IReadOnlyList<T>
{
    private const string ModifiedDuringEnumeration
        = "Collection was modified; enumeration operation may not execute.";

    private readonly List<T> _items;

    /// <summary>
    /// Number of logically-removed items at the front of <see cref="_items"/>.
    /// </summary>
    private int _head;

    /// <summary>
    /// Mutation counter used to detect modification during enumeration.
    /// </summary>
    private int _version;

    /// <summary>
    /// Initializes a new instance of the <see cref="PruningList{T}"/> class.
    /// </summary>
    public PruningList() => _items = [];

    /// <inheritdoc/>
    public int Count => _items.Count - _head;

    /// <inheritdoc/>
    public bool IsReadOnly => false;

    /// <inheritdoc/>
    public T this[int index]
    {
        get {
            if ((uint)index >= (uint)Count)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            return _items[_head + index];
        }

        set {
            if ((uint)index >= (uint)Count)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            _items[_head + index] = value;
            _version++;
        }
    }

    /// <inheritdoc/>
    public void Add(T item)
    {
        _items.Add(item);
        _version++;
    }

    /// <inheritdoc/>
    public void Insert(int index, T item)
    {
        if ((uint)index > (uint)Count)
        {
            throw new ArgumentOutOfRangeException(nameof(index));
        }

        _items.Insert(_head + index, item);
        _version++;
    }

    /// <inheritdoc/>
    public void RemoveAt(int index)
    {
        if ((uint)index >= (uint)Count)
        {
            throw new ArgumentOutOfRangeException(nameof(index));
        }

        if (index == 0)
        {
            AdvanceHead(1);
        }
        else
        {
            _items.RemoveAt(_head + index);
            _version++;
        }
    }

    /// <summary>
    /// Removes a range of elements from the live window.
    /// Front removals (<paramref name="index"/> of 0) are O(1) amortized.
    /// </summary>
    /// <param name="index">Zero-based starting index of the range to remove.</param>
    /// <param name="count">Number of elements to remove.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when index or count is negative.</exception>
    /// <exception cref="ArgumentException">Thrown when the range exceeds the live window.</exception>
    public void RemoveRange(int index, int count)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(index);
        ArgumentOutOfRangeException.ThrowIfNegative(count);

        if (Count - index < count)
        {
            throw new ArgumentException(
                "Offset and length were out of bounds for the list.");
        }

        if (count == 0)
        {
            return;
        }

        if (index == 0)
        {
            AdvanceHead(count);
        }
        else
        {
            _items.RemoveRange(_head + index, count);
            _version++;
        }
    }

    /// <summary>
    /// Removes all elements in the live window that match the predicate.
    /// </summary>
    /// <param name="match">Predicate that defines the elements to remove.</param>
    /// <returns>Number of elements removed.</returns>
    public int RemoveAll(Predicate<T> match)
    {
        // physically reclaim the dead prefix so List<T>.RemoveAll only
        // scans the live window (this path is infrequent)
        Reclaim();

        int removed = _items.RemoveAll(match);

        if (removed > 0)
        {
            _version++;
        }

        return removed;
    }

    /// <inheritdoc/>
    public void Clear()
    {
        _items.Clear();
        _head = 0;
        _version++;
    }

    /// <inheritdoc/>
    public bool Contains(T item) => IndexOf(item) >= 0;

    /// <inheritdoc/>
    public int IndexOf(T item)
    {
        int found = _items.IndexOf(item, _head);
        return found < 0 ? -1 : found - _head;
    }

    /// <inheritdoc/>
    public bool Remove(T item)
    {
        int index = IndexOf(item);

        if (index < 0)
        {
            return false;
        }

        RemoveAt(index);
        return true;
    }

    /// <inheritdoc/>
    public void CopyTo(T[] array, int arrayIndex)
        => _items.CopyTo(_head, array, arrayIndex, Count);

    /// <summary>
    /// Copies the live window to a new array.
    /// </summary>
    /// <returns>An array containing the live (retained) elements.</returns>
    public T[] ToArray()
    {
        int count = Count;

        if (count == 0)
        {
            return [];
        }

        T[] array = new T[count];
        _items.CopyTo(_head, array, 0, count);
        return array;
    }

    /// <inheritdoc/>
    public IEnumerator<T> GetEnumerator()
    {
        int version = _version;
        int end = _items.Count;

        for (int i = _head; i < end; i++)
        {
            if (version != _version)
            {
                throw new InvalidOperationException(ModifiedDuringEnumeration);
            }

            yield return _items[i];
        }

        if (version != _version)
        {
            throw new InvalidOperationException(ModifiedDuringEnumeration);
        }
    }

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /// <summary>
    /// Advances the logical head, reclaiming storage once the dead prefix
    /// reaches the size of the live window to keep amortized cost O(1).
    /// </summary>
    /// <param name="count">Number of front items to logically remove.</param>
    private void AdvanceHead(int count)
    {
        _head += count;
        _version++;

        if (_head >= _items.Count - _head)
        {
            Reclaim();
        }
    }

    /// <summary>
    /// Physically removes the dead prefix from the underlying list.
    /// </summary>
    private void Reclaim()
    {
        if (_head == 0)
        {
            return;
        }

        _items.RemoveRange(0, _head);
        _head = 0;
    }
}
