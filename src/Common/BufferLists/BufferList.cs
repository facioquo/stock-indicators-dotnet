using System.Collections;

namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Standalone indicator lists with buffering cache.
/// </summary>
/// <remarks>
/// Based on <see cref="IReadOnlyList{TResult}"/>, this class
/// contains core buffering list features for safe operations.
/// <para>
/// <b>Input must be supplied in chronological order.</b> A buffer list is a
/// single-pass, append-only accumulator: each <c>Add</c> advances internal
/// state on the assumption that the incoming value is newer than everything
/// added so far. It does <b>not</b> detect or repair out-of-order, duplicate,
/// or revised timestamps — feeding such input produces silently incorrect
/// results (values computed over the wrong window, mismatched timestamps,
/// or duplicate rows). Sort your data by timestamp before adding, or use a
/// <c>StreamHub</c> — which supports late arrivals, same-timestamp
/// corrections, and rollback — when input can arrive out of order.
/// </para>
/// </remarks>
/// <inheritdoc/>
public abstract class BufferList<TResult> : IReadOnlyList<TResult>
    where TResult : ISeries
{
    private readonly PruningList<TResult> _internalList = [];

    /// <summary>
    /// Gets the result at the specified index.
    /// </summary>
    /// <param name="index">Zero-based index of the result to get.</param>
    /// <returns>Result at the specified index.</returns>
    public TResult this[int index] => _internalList[index];

    /// <inheritdoc/>
    public int Count => _internalList.Count;

    /// <summary>
    /// Gets the name of the buffer list.
    /// </summary>
    public string Name { get; protected init; } = null!;

    /// <summary>
    /// Gets or sets the maximum number of results to retain in the list.
    /// When the list exceeds this value, the oldest items are pruned.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when a parameter is out of the valid range
    /// </exception>
    public int MaxListSize
    {
        get;
        set {
            if (value <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(value),
                    "MaxListSize must be greater than 0.");
            }

            field = value;

            if (_internalList.Count > field)
            {
                PruneList();
            }
        }
    } = 100_000;

    /// <summary>
    /// Gets or sets whether adding a result that is not newer than the last one throws.
    /// Default is <c>false</c>, which leaves out-of-order and duplicate input undetected.
    /// </summary>
    /// <remarks>
    /// Enabling this turns silently incorrect results into a fast failure
    /// for debugging and test data. The list has already advanced its internal
    /// state when the exception is thrown, so discard it rather than continuing.
    /// Use a <c>StreamHub</c> for feeds that legitimately deliver late or
    /// revised input.
    /// </remarks>
    public bool ThrowOnOutOfOrder { get; set; }

    /// <summary>
    /// Gets whether consecutive results may share a timestamp, as when one input
    /// produces several results. Strict ordering then rejects only earlier timestamps.
    /// </summary>
    private protected virtual bool AllowsRepeatedTimestamps => false;

    /// <summary>
    /// Adds an item to the list using internal buffer logic.
    /// Automatically prunes the list if it exceeds MaxListSize.
    /// </summary>
    /// <param name="item">Item to add.</param>
    /// <exception cref="InvalidOperationException">
    /// Thrown when <see cref="ThrowOnOutOfOrder"/> is enabled and the item's
    /// timestamp is not after the last item's.
    /// </exception>
    protected void AddInternal(TResult item)
    {
        if (ThrowOnOutOfOrder
            && _internalList.Count > 0
            && (item.Timestamp < _internalList[^1].Timestamp
                || (item.Timestamp == _internalList[^1].Timestamp && !AllowsRepeatedTimestamps)))
        {
            string listName = string.IsNullOrEmpty(Name) ? GetType().Name : Name;

            throw new InvalidOperationException(
                $"{listName} received timestamp {item.Timestamp:O}, which is not after the last "
                + $"timestamp {_internalList[^1].Timestamp:O}. A BufferList requires chronological, "
                + "unique input; use a StreamHub for out-of-order or revised data.");
        }

        _internalList.Add(item);

        if (_internalList.Count > MaxListSize)
        {
            PruneList();
        }
    }

    /// <summary>
    /// Updates an existing item in the list at the specified index.
    /// Used for indicators that need to revise historical values (repaint).
    /// </summary>
    /// <param name="index">Zero-based index of the item to update.</param>
    /// <param name="item">Updated item.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when index is out of range.</exception>
    protected void UpdateInternal(int index, TResult item)
    {
        if (index < 0 || index >= _internalList.Count)
        {
            throw new ArgumentOutOfRangeException(
                nameof(index),
                "Index must be within the bounds of the list.");
        }

        _internalList[index] = item;
    }

    /// <summary>
    /// Gets the number of results pruned from the start of the list since it was created or cleared.
    /// </summary>
    private protected int PrunedCount { get; private set; }

    /// <summary>
    /// Gets the number of results added since the list was created or cleared, including pruned ones.
    /// Use it instead of <see cref="Count"/> for warmup checks and absolute positions,
    /// which must not depend on <see cref="MaxListSize"/>.
    /// </summary>
    private protected int TotalCount => _internalList.Count + PrunedCount;

    /// <summary>
    /// Removes the oldest results when the list exceeds <see cref="MaxListSize"/>.
    /// Can be overridden in derived classes for custom pruning logic.
    /// </summary>
    protected virtual void PruneList()
    {
        int overflow = _internalList.Count - MaxListSize;

        if (overflow <= 0)
        {
            return;
        }

        _internalList.RemoveRange(0, overflow);
        PrunedCount += overflow;
    }

    /* IList<TResult> features (limited set) */

    /// <inheritdoc cref="List{TResult}.Clear"/>
    public virtual void Clear()
    {
        _internalList.Clear();
        PrunedCount = 0;
    }

    /// <inheritdoc cref="List{TResult}.Contains(TResult)"/>
    public bool Contains(TResult item) => _internalList.Contains(item);

    /// <inheritdoc cref="List{TResult}.CopyTo(TResult[], int)"/>
    public void CopyTo(TResult[] array, int arrayIndex)
        => _internalList.CopyTo(array, arrayIndex);

    /// <inheritdoc/>
    public IEnumerator<TResult> GetEnumerator() => _internalList.GetEnumerator();

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /// <inheritdoc/>
    public override string ToString() => Name;
}
