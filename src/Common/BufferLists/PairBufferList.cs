namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Buffer list of results from two incremental series, matched by timestamp.
/// </summary>
/// <remarks>
/// <para>
/// Add each series with <see cref="AddSource(IReusable)"/> and <see cref="AddOther(IReusable)"/>,
/// or both at once with <see cref="Add(IReusable, IReusable)"/>. Either side may run ahead:
/// its values wait until the other side supplies the same timestamp. A timestamp that only
/// one side holds produces no result, where the Series method throws
/// <see cref="InvalidBarsException"/>.
/// </para>
/// <para>
/// Each side must arrive in strictly increasing timestamp order. An older or equal timestamp
/// throws <see cref="InvalidOperationException"/> whatever <see cref="BufferList{TResult}.ThrowOnOutOfOrder"/>
/// is set to, because an emitted result cannot be revised and the staged values are trimmed
/// on that assumption.
/// </para>
/// <para>
/// Each side stages at most <see cref="BufferList{TResult}.MaxListSize"/> values, oldest dropped
/// first. A side that falls more than that many values behind loses the pairs the leading side
/// has already dropped, which the Series method would still return.
/// </para>
/// </remarks>
/// <typeparam name="TResult">Type of the results.</typeparam>
public abstract class PairBufferList<TResult> : BufferList<TResult>, IIncrementFromPair
    where TResult : ISeries
{
    private readonly PairStager _stager;
    private DateTime? _lastSource;
    private DateTime? _lastOther;

    /// <summary>
    /// Initializes a new instance of the <see cref="PairBufferList{TResult}"/> class.
    /// </summary>
    private protected PairBufferList() => _stager = new PairStager(MaxListSize);

    /// <inheritdoc />
    /// <exception cref="InvalidOperationException">Thrown when the timestamp is not after the last source timestamp.</exception>
    public void AddSource(DateTime timestamp, double value)
        => Stage(PairSide.Source, timestamp, value);

    /// <inheritdoc />
    /// <exception cref="ArgumentNullException">Thrown when the value is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the timestamp is not after the last source timestamp.</exception>
    public void AddSource(IReusable value)
    {
        ArgumentNullException.ThrowIfNull(value);
        Stage(PairSide.Source, value.Timestamp, value.Value);
    }

    /// <inheritdoc />
    /// <exception cref="ArgumentNullException">Thrown when the list or a value in it is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when a timestamp is not after the last source timestamp; earlier values stay added.</exception>
    public void AddSource(IReadOnlyList<IReusable> values)
        => StageAll(PairSide.Source, values);

    /// <inheritdoc />
    /// <exception cref="InvalidOperationException">Thrown when the timestamp is not after the last other timestamp.</exception>
    public void AddOther(DateTime timestamp, double value)
        => Stage(PairSide.Other, timestamp, value);

    /// <inheritdoc />
    /// <exception cref="ArgumentNullException">Thrown when the value is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the timestamp is not after the last other timestamp.</exception>
    public void AddOther(IReusable value)
    {
        ArgumentNullException.ThrowIfNull(value);
        Stage(PairSide.Other, value.Timestamp, value.Value);
    }

    /// <inheritdoc />
    /// <exception cref="ArgumentNullException">Thrown when the list or a value in it is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when a timestamp is not after the last other timestamp; earlier values stay added.</exception>
    public void AddOther(IReadOnlyList<IReusable> values)
        => StageAll(PairSide.Other, values);

    /// <inheritdoc />
    /// <exception cref="ArgumentNullException">Thrown when either value is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when either timestamp is not after that side's last; the list is then unchanged.</exception>
    public void Add(IReusable source, IReusable other)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(other);

        // check both sides before staging either, so a rejected pair leaves nothing behind
        CheckOrder(PairSide.Source, source.Timestamp);
        CheckOrder(PairSide.Other, other.Timestamp);

        Stage(PairSide.Source, source.Timestamp, source.Value);
        Stage(PairSide.Other, other.Timestamp, other.Value);
    }

    /// <inheritdoc />
    public override void Clear()
    {
        base.Clear();
        _stager.Clear();
        _lastSource = null;
        _lastOther = null;
    }

    /// <summary>
    /// Adds the result for a newly matched pair.
    /// </summary>
    /// <param name="pair">Values of both series at a timestamp, in chronological order.</param>
    private protected abstract void AddPair(PairedValue pair);

    /// <summary>
    /// Stages two whole series in timestamp order.
    /// </summary>
    /// <remarks>
    /// Aligned input pairs as it goes. Staging one side in full first would make each
    /// trim shift that side's staged values, which is quadratic in the series length.
    /// </remarks>
    /// <param name="source">The source series, time sorted.</param>
    /// <param name="other">The other series, time sorted.</param>
    /// <exception cref="ArgumentNullException">Thrown when either series holds a null value.</exception>
    /// <exception cref="InvalidOperationException">Thrown when either series is out of timestamp order.</exception>
    private protected void StageBoth(IReadOnlyList<IReusable> source, IReadOnlyList<IReusable> other)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(other);

        int s = 0;
        int o = 0;

        while (s < source.Count || o < other.Count)
        {
            IReusable? a = s < source.Count
                ? source[s] ?? throw new ArgumentNullException(nameof(source))
                : null;
            IReusable? b = o < other.Count
                ? other[o] ?? throw new ArgumentNullException(nameof(other))
                : null;

            if (b is null || (a is not null && a.Timestamp <= b.Timestamp))
            {
                Stage(PairSide.Source, a!.Timestamp, a.Value);
                s++;
            }
            else
            {
                Stage(PairSide.Other, b.Timestamp, b.Value);
                o++;
            }
        }
    }

    private void StageAll(PairSide side, IReadOnlyList<IReusable> values)
    {
        ArgumentNullException.ThrowIfNull(values);

        for (int i = 0; i < values.Count; i++)
        {
            IReusable value = values[i];
            ArgumentNullException.ThrowIfNull(value, nameof(values));
            Stage(side, value.Timestamp, value.Value);
        }
    }

    private void CheckOrder(PairSide side, DateTime timestamp)
    {
        DateTime? last = side == PairSide.Source ? _lastSource : _lastOther;

        if (last is DateTime previous && timestamp <= previous)
        {
            string name = side == PairSide.Source ? "source" : "other";

            throw new InvalidOperationException(
                $"{Name} received {name} timestamp {timestamp:O}, which is not after the last "
                + $"{name} timestamp {previous:O}. A BufferList requires chronological, unique input "
                + "on each side; use a StreamHub for out-of-order or revised data.");
        }
    }

    private void Stage(PairSide side, DateTime timestamp, double value)
    {
        CheckOrder(side, timestamp);

        if (side == PairSide.Source)
        {
            _lastSource = timestamp;
        }
        else
        {
            _lastOther = timestamp;
        }

        _stager.MaxCount = MaxListSize;
        _stager.Set(side, timestamp, value);

        if (_stager.TryGetPair(timestamp, out PairedValue? pair))
        {
            AddPair(pair);
        }

        // values at or before the older of the two newest timestamps can never pair:
        // both sides only move forward
        if (_lastSource is DateTime s && _lastOther is DateTime o)
        {
            DateTime passed = s < o ? s : o;
            _stager.RemoveThrough(PairSide.Source, passed);
            _stager.RemoveThrough(PairSide.Other, passed);
        }
    }
}
