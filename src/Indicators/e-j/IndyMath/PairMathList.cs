namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Arithmetic between two incremental series, matched by timestamp.
/// </summary>
/// <remarks>
/// Add each series with <see cref="AddSource(IReusable)"/> and <see cref="AddOther(IReusable)"/>,
/// or both at once with <see cref="Add(IReusable, IReusable)"/>. Either side may run ahead:
/// its values wait until the other side supplies the same timestamp. A timestamp that only
/// one side holds produces no result. Each side must be added in chronological order.
/// </remarks>
public class PairMathList : BufferList<MathResult>, IIncrementFromPair, IPairMath
{
    private readonly PairStager _stager;
    private DateTime? _lastSource;
    private DateTime? _lastOther;

    /// <summary>
    /// Initializes a new instance of the <see cref="PairMathList"/> class.
    /// </summary>
    /// <param name="operation">Operation to apply at each matched timestamp.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the operation is undefined.</exception>
    public PairMathList(PairOperation operation)
    {
        PairKernel.Validate(operation);
        Operation = operation;
        Name = PairKernel.FormatName(operation);
        _stager = new PairStager(MaxListSize);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PairMathList"/> class with initial values of both series.
    /// </summary>
    /// <param name="operation">Operation to apply at each matched timestamp.</param>
    /// <param name="source">Initial source values, time sorted.</param>
    /// <param name="other">Initial other values, time sorted.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the operation is undefined.</exception>
    /// <exception cref="ArgumentNullException">Thrown when either series is null.</exception>
    public PairMathList(
        PairOperation operation,
        IReadOnlyList<IReusable> source,
        IReadOnlyList<IReusable> other)
        : this(operation)
    {
        AddSource(source);
        AddOther(other);
    }

    /// <inheritdoc />
    public PairOperation Operation { get; }

    /// <inheritdoc />
    public void AddSource(DateTime timestamp, double value)
        => Stage(PairSide.Source, timestamp, value);

    /// <inheritdoc />
    /// <exception cref="ArgumentNullException">Thrown when the value is null.</exception>
    public void AddSource(IReusable value)
    {
        ArgumentNullException.ThrowIfNull(value);
        Stage(PairSide.Source, value.Timestamp, value.Value);
    }

    /// <inheritdoc />
    /// <exception cref="ArgumentNullException">Thrown when the values list is null.</exception>
    public void AddSource(IReadOnlyList<IReusable> values)
        => StageAll(PairSide.Source, values);

    /// <inheritdoc />
    public void AddOther(DateTime timestamp, double value)
        => Stage(PairSide.Other, timestamp, value);

    /// <inheritdoc />
    /// <exception cref="ArgumentNullException">Thrown when the value is null.</exception>
    public void AddOther(IReusable value)
    {
        ArgumentNullException.ThrowIfNull(value);
        Stage(PairSide.Other, value.Timestamp, value.Value);
    }

    /// <inheritdoc />
    /// <exception cref="ArgumentNullException">Thrown when the values list is null.</exception>
    public void AddOther(IReadOnlyList<IReusable> values)
        => StageAll(PairSide.Other, values);

    /// <inheritdoc />
    /// <exception cref="ArgumentNullException">Thrown when either value is null.</exception>
    public void Add(IReusable source, IReusable other)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(other);

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

    private void StageAll(PairSide side, IReadOnlyList<IReusable> values)
    {
        ArgumentNullException.ThrowIfNull(values);

        for (int i = 0; i < values.Count; i++)
        {
            Stage(side, values[i].Timestamp, values[i].Value);
        }
    }

    private void Stage(PairSide side, DateTime timestamp, double value)
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
            AddInternal(new MathResult(
                timestamp,
                PairKernel.Apply(Operation, pair.Value, pair.Other).NaN2Null()));
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

public static partial class IndyMath
{
    /// <summary>
    /// Creates a buffer list that adds the values of two series at each matched timestamp.
    /// </summary>
    /// <param name="source">Chainable results, time sorted.</param>
    /// <param name="other">Chainable results to add, time sorted.</param>
    /// <returns>A buffer list of <c>source + other</c> values.</returns>
    /// <exception cref="ArgumentNullException">Thrown when either series is null.</exception>
    public static PairMathList ToPlusList(
        this IReadOnlyList<IReusable> source,
        IReadOnlyList<IReusable> other)
        => new(PairOperation.Plus, source, other);

    /// <summary>
    /// Creates a buffer list that subtracts the values of another series at each matched timestamp.
    /// </summary>
    /// <param name="source">Chainable results, time sorted.</param>
    /// <param name="other">Chainable results to subtract, time sorted.</param>
    /// <returns>A buffer list of <c>source − other</c> values.</returns>
    /// <exception cref="ArgumentNullException">Thrown when either series is null.</exception>
    public static PairMathList ToMinusList(
        this IReadOnlyList<IReusable> source,
        IReadOnlyList<IReusable> other)
        => new(PairOperation.Minus, source, other);

    /// <summary>
    /// Creates a buffer list that multiplies the values of two series at each matched timestamp.
    /// </summary>
    /// <param name="source">Chainable results, time sorted.</param>
    /// <param name="other">Chainable results to multiply by, time sorted.</param>
    /// <returns>A buffer list of <c>source × other</c> values.</returns>
    /// <exception cref="ArgumentNullException">Thrown when either series is null.</exception>
    public static PairMathList ToTimesList(
        this IReadOnlyList<IReusable> source,
        IReadOnlyList<IReusable> other)
        => new(PairOperation.Times, source, other);

    /// <summary>
    /// Creates a buffer list that divides by the values of another series at each matched timestamp.
    /// </summary>
    /// <param name="source">Chainable results, time sorted.</param>
    /// <param name="other">Chainable results (divisors), time sorted.</param>
    /// <returns>A buffer list of <c>source ÷ other</c> values; <see langword="null"/> where the divisor is 0.</returns>
    /// <exception cref="ArgumentNullException">Thrown when either series is null.</exception>
    public static PairMathList ToDividedByList(
        this IReadOnlyList<IReusable> source,
        IReadOnlyList<IReusable> other)
        => new(PairOperation.DividedBy, source, other);

    /// <summary>
    /// Creates a buffer list that selects the larger value of two series at each matched timestamp.
    /// </summary>
    /// <param name="source">Chainable results, time sorted.</param>
    /// <param name="other">Chainable results to compare, time sorted.</param>
    /// <returns>A buffer list of the larger value at each matched timestamp.</returns>
    /// <exception cref="ArgumentNullException">Thrown when either series is null.</exception>
    public static PairMathList ToMaxWithList(
        this IReadOnlyList<IReusable> source,
        IReadOnlyList<IReusable> other)
        => new(PairOperation.MaxWith, source, other);

    /// <summary>
    /// Creates a buffer list that selects the smaller value of two series at each matched timestamp.
    /// </summary>
    /// <param name="source">Chainable results, time sorted.</param>
    /// <param name="other">Chainable results to compare, time sorted.</param>
    /// <returns>A buffer list of the smaller value at each matched timestamp.</returns>
    /// <exception cref="ArgumentNullException">Thrown when either series is null.</exception>
    public static PairMathList ToMinWithList(
        this IReadOnlyList<IReusable> source,
        IReadOnlyList<IReusable> other)
        => new(PairOperation.MinWith, source, other);
}
