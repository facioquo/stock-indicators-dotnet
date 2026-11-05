namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Streaming hub for arithmetic between two chain providers, matched by timestamp.
/// </summary>
/// <remarks>
/// A result is emitted once both providers hold a timestamp. A timestamp that only one
/// provider holds produces no result; when the missing value arrives late, the usual
/// rebuild adds it. Both providers must be driven from one thread at a time, as they
/// are when both descend from one <see cref="BarHub"/>.
/// </remarks>
public class PairMathHub
    : ChainHub<PairedValue, MathResult>, IPairMath
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PairMathHub"/> class.
    /// </summary>
    /// <param name="provider">Hub that pairs the two series.</param>
    /// <param name="operation">Operation to apply at each matched timestamp.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the operation is undefined.</exception>
    internal PairMathHub(
        PairHub provider,
        PairOperation operation) : base(provider)
    {
        PairKernel.Validate(operation);
        Operation = operation;
        Name = PairKernel.FormatName(operation);

        // the pairing hub was built for this hub alone
        OwnProvider();

        Reinitialize();
    }

    /// <inheritdoc />
    public PairOperation Operation { get; }

    /// <inheritdoc/>
    protected override (MathResult result, int index)
        ToIndicator(PairedValue item, int? indexHint)
    {
        ArgumentNullException.ThrowIfNull(item);

        int i = indexHint ?? ProviderCache.IndexOf(item, true);

        return (new MathResult(
            item.Timestamp,
            PairKernel.Apply(Operation, item.Value, item.Other).NaN2Null()), i);
    }
}

public static partial class IndyMath
{
    /// <summary>
    /// Creates a stream hub that adds the values of two chain providers at each matched timestamp.
    /// </summary>
    /// <param name="source">Source chain provider.</param>
    /// <param name="other">Chain provider to add.</param>
    /// <returns>A stream hub of <c>source + other</c> values.</returns>
    /// <exception cref="ArgumentNullException">Thrown when either provider is null.</exception>
    public static PairMathHub Plus(
        this IChainProvider<IReusable> source,
        IChainProvider<IReusable> other)
        => source.ToPairMathHub(other, PairOperation.Plus);

    /// <summary>
    /// Creates a stream hub that subtracts the values of another chain provider at each matched timestamp.
    /// </summary>
    /// <param name="source">Source chain provider.</param>
    /// <param name="other">Chain provider to subtract.</param>
    /// <returns>A stream hub of <c>source − other</c> values.</returns>
    /// <exception cref="ArgumentNullException">Thrown when either provider is null.</exception>
    public static PairMathHub Minus(
        this IChainProvider<IReusable> source,
        IChainProvider<IReusable> other)
        => source.ToPairMathHub(other, PairOperation.Minus);

    /// <summary>
    /// Creates a stream hub that multiplies the values of two chain providers at each matched timestamp.
    /// </summary>
    /// <param name="source">Source chain provider.</param>
    /// <param name="other">Chain provider to multiply by.</param>
    /// <returns>A stream hub of <c>source × other</c> values.</returns>
    /// <exception cref="ArgumentNullException">Thrown when either provider is null.</exception>
    public static PairMathHub Times(
        this IChainProvider<IReusable> source,
        IChainProvider<IReusable> other)
        => source.ToPairMathHub(other, PairOperation.Times);

    /// <summary>
    /// Creates a stream hub that divides by the values of another chain provider at each matched timestamp.
    /// </summary>
    /// <param name="source">Source chain provider.</param>
    /// <param name="other">Chain provider of divisors.</param>
    /// <returns>A stream hub of <c>source ÷ other</c> values; <see langword="null"/> where the divisor is 0.</returns>
    /// <exception cref="ArgumentNullException">Thrown when either provider is null.</exception>
    public static PairMathHub DividedBy(
        this IChainProvider<IReusable> source,
        IChainProvider<IReusable> other)
        => source.ToPairMathHub(other, PairOperation.DividedBy);

    /// <summary>
    /// Creates a stream hub that selects the larger value of two chain providers at each matched timestamp.
    /// </summary>
    /// <param name="source">Source chain provider.</param>
    /// <param name="other">Chain provider to compare.</param>
    /// <returns>A stream hub of the larger value at each matched timestamp.</returns>
    /// <exception cref="ArgumentNullException">Thrown when either provider is null.</exception>
    public static PairMathHub MaxWith(
        this IChainProvider<IReusable> source,
        IChainProvider<IReusable> other)
        => source.ToPairMathHub(other, PairOperation.MaxWith);

    /// <summary>
    /// Creates a stream hub that selects the smaller value of two chain providers at each matched timestamp.
    /// </summary>
    /// <param name="source">Source chain provider.</param>
    /// <param name="other">Chain provider to compare.</param>
    /// <returns>A stream hub of the smaller value at each matched timestamp.</returns>
    /// <exception cref="ArgumentNullException">Thrown when either provider is null.</exception>
    public static PairMathHub MinWith(
        this IChainProvider<IReusable> source,
        IChainProvider<IReusable> other)
        => source.ToPairMathHub(other, PairOperation.MinWith);

    private static PairMathHub ToPairMathHub(
        this IChainProvider<IReusable> source,
        IChainProvider<IReusable> other,
        PairOperation operation)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(other);
        return new(new PairHub(source, other), operation);
    }
}
