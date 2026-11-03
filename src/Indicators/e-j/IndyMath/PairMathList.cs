namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Arithmetic between two incremental series, matched by timestamp.
/// </summary>
/// <remarks>
/// See <see cref="PairBufferList{TResult}"/> for how the two series are added and matched.
/// </remarks>
public class PairMathList : PairBufferList<MathResult>, IPairMath
{
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
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(other);

        AddSource(source);
        AddOther(other);
    }

    /// <inheritdoc />
    public PairOperation Operation { get; }

    /// <inheritdoc />
    private protected override void AddPair(PairedValue pair)
        => AddInternal(new MathResult(
            pair.Timestamp,
            PairKernel.Apply(Operation, pair.Value, pair.Other).NaN2Null()));
}

public static partial class IndyMath
{
    /// <summary>
    /// Creates a buffer list that adds the values of two series at each matched timestamp.
    /// </summary>
    /// <remarks>A timestamp that only one series holds produces no result, where the Series method throws.</remarks>
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
    /// <remarks>A timestamp that only one series holds produces no result, where the Series method throws.</remarks>
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
    /// <remarks>A timestamp that only one series holds produces no result, where the Series method throws.</remarks>
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
    /// <remarks>A timestamp that only one series holds produces no result, where the Series method throws.</remarks>
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
    /// <remarks>A timestamp that only one series holds produces no result, where the Series method throws.</remarks>
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
    /// <remarks>A timestamp that only one series holds produces no result, where the Series method throws.</remarks>
    /// <param name="source">Chainable results, time sorted.</param>
    /// <param name="other">Chainable results to compare, time sorted.</param>
    /// <returns>A buffer list of the smaller value at each matched timestamp.</returns>
    /// <exception cref="ArgumentNullException">Thrown when either series is null.</exception>
    public static PairMathList ToMinWithList(
        this IReadOnlyList<IReusable> source,
        IReadOnlyList<IReusable> other)
        => new(PairOperation.MinWith, source, other);
}
