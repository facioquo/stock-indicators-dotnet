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
    /// <param name="basisSource">Initial basis source values, time sorted.</param>
    /// <param name="otherSource">Initial other source values, time sorted.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the operation is undefined.</exception>
    /// <exception cref="ArgumentNullException">Thrown when either series is null.</exception>
    public PairMathList(
        PairOperation operation,
        IReadOnlyList<IReusable> basisSource,
        IReadOnlyList<IReusable> otherSource)
        : this(operation)
    {
        ArgumentNullException.ThrowIfNull(basisSource);
        ArgumentNullException.ThrowIfNull(otherSource);

        StageBoth(basisSource, otherSource);
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
    /// <param name="basisSource">Chainable results, time sorted.</param>
    /// <param name="otherSource">Chainable results to add, time sorted.</param>
    /// <returns>A buffer list of <c>basisSource + otherSource</c> values.</returns>
    /// <exception cref="ArgumentNullException">Thrown when either series is null.</exception>
    public static PairMathList ToPlusList(
        this IReadOnlyList<IReusable> basisSource,
        IReadOnlyList<IReusable> otherSource)
        => new(PairOperation.Plus, basisSource, otherSource);

    /// <summary>
    /// Creates a buffer list that subtracts the values of another series at each matched timestamp.
    /// </summary>
    /// <remarks>A timestamp that only one series holds produces no result, where the Series method throws.</remarks>
    /// <param name="basisSource">Chainable results, time sorted.</param>
    /// <param name="otherSource">Chainable results to subtract, time sorted.</param>
    /// <returns>A buffer list of <c>basisSource − otherSource</c> values.</returns>
    /// <exception cref="ArgumentNullException">Thrown when either series is null.</exception>
    public static PairMathList ToMinusList(
        this IReadOnlyList<IReusable> basisSource,
        IReadOnlyList<IReusable> otherSource)
        => new(PairOperation.Minus, basisSource, otherSource);

    /// <summary>
    /// Creates a buffer list that multiplies the values of two series at each matched timestamp.
    /// </summary>
    /// <remarks>A timestamp that only one series holds produces no result, where the Series method throws.</remarks>
    /// <param name="basisSource">Chainable results, time sorted.</param>
    /// <param name="otherSource">Chainable results to multiply by, time sorted.</param>
    /// <returns>A buffer list of <c>basisSource × otherSource</c> values.</returns>
    /// <exception cref="ArgumentNullException">Thrown when either series is null.</exception>
    public static PairMathList ToTimesList(
        this IReadOnlyList<IReusable> basisSource,
        IReadOnlyList<IReusable> otherSource)
        => new(PairOperation.Times, basisSource, otherSource);

    /// <summary>
    /// Creates a buffer list that divides by the values of another series at each matched timestamp.
    /// </summary>
    /// <remarks>A timestamp that only one series holds produces no result, where the Series method throws.</remarks>
    /// <param name="basisSource">Chainable results, time sorted.</param>
    /// <param name="otherSource">Chainable results (divisors), time sorted.</param>
    /// <returns>A buffer list of <c>basisSource ÷ otherSource</c> values; <see langword="null"/> where the divisor is 0.</returns>
    /// <exception cref="ArgumentNullException">Thrown when either series is null.</exception>
    public static PairMathList ToDividedByList(
        this IReadOnlyList<IReusable> basisSource,
        IReadOnlyList<IReusable> otherSource)
        => new(PairOperation.DividedBy, basisSource, otherSource);

    /// <summary>
    /// Creates a buffer list that selects the larger value of two series at each matched timestamp.
    /// </summary>
    /// <remarks>A timestamp that only one series holds produces no result, where the Series method throws.</remarks>
    /// <param name="basisSource">Chainable results, time sorted.</param>
    /// <param name="otherSource">Chainable results to compare, time sorted.</param>
    /// <returns>A buffer list of the larger value at each matched timestamp.</returns>
    /// <exception cref="ArgumentNullException">Thrown when either series is null.</exception>
    public static PairMathList ToMaxWithList(
        this IReadOnlyList<IReusable> basisSource,
        IReadOnlyList<IReusable> otherSource)
        => new(PairOperation.MaxWith, basisSource, otherSource);

    /// <summary>
    /// Creates a buffer list that selects the smaller value of two series at each matched timestamp.
    /// </summary>
    /// <remarks>A timestamp that only one series holds produces no result, where the Series method throws.</remarks>
    /// <param name="basisSource">Chainable results, time sorted.</param>
    /// <param name="otherSource">Chainable results to compare, time sorted.</param>
    /// <returns>A buffer list of the smaller value at each matched timestamp.</returns>
    /// <exception cref="ArgumentNullException">Thrown when either series is null.</exception>
    public static PairMathList ToMinWithList(
        this IReadOnlyList<IReusable> basisSource,
        IReadOnlyList<IReusable> otherSource)
        => new(PairOperation.MinWith, basisSource, otherSource);
}
