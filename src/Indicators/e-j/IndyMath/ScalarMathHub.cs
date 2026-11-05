namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Streaming hub for arithmetic between chained values and a fixed operand.
/// </summary>
public class ScalarMathHub
    : ChainHub<IReusable, MathResult>, IScalarMath
{
    private readonly ScalarKernel _kernel;

    internal ScalarMathHub(
        IChainProvider<IReusable> provider,
        ScalarOperation operation,
        double operand,
        string paramName) : base(provider)
    {
        _kernel = new ScalarKernel(operation, operand, paramName);
        Operation = operation;
        Operand = operand;
        Name = ScalarKernel.FormatName(operation, operand);

        Reinitialize();
    }

    /// <summary>
    /// Gets the arithmetic applied to each value.
    /// </summary>
    public ScalarOperation Operation { get; }

    /// <summary>
    /// Gets the fixed operand, as given.
    /// </summary>
    public double Operand { get; }

    /// <inheritdoc/>
    protected override (MathResult result, int index)
        ToIndicator(IReusable item, int? indexHint)
    {
        ArgumentNullException.ThrowIfNull(item);

        int i = indexHint ?? ProviderCache.IndexOf(item, true);

        return (new MathResult(item.Timestamp, _kernel.Apply(item.Value).NaN2Null()), i);
    }
}

public static partial class IndyMath
{
    /// <summary>
    /// Creates a streaming hub that adds a fixed amount to every value.
    /// </summary>
    /// <param name="chainProvider">Chain provider.</param>
    /// <param name="amount">Finite amount to add.</param>
    /// <returns>A hub of <c>source + amount</c> values.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="chainProvider"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the amount is not finite.</exception>
    public static ScalarMathHub ToPlusHub(
        this IChainProvider<IReusable> chainProvider,
        double amount)
    {
        ArgumentNullException.ThrowIfNull(chainProvider);
        return new(chainProvider, ScalarOperation.Plus, amount, nameof(amount));
    }

    /// <summary>
    /// Creates a streaming hub that offsets every value by a percentage of itself.
    /// </summary>
    /// <param name="chainProvider">Chain provider.</param>
    /// <param name="percentOffset">Finite percent offset, where 2.5 adds 2.5% and -2.5 subtracts 2.5%.</param>
    /// <returns>A hub of <c>source × (1 + percentOffset / 100)</c> values.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="chainProvider"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the percent offset is not finite.</exception>
    public static ScalarMathHub ToPlusPercentHub(
        this IChainProvider<IReusable> chainProvider,
        double percentOffset)
    {
        ArgumentNullException.ThrowIfNull(chainProvider);
        return new(chainProvider, ScalarOperation.PlusPercent, percentOffset, nameof(percentOffset));
    }

    /// <summary>
    /// Creates a streaming hub that subtracts a fixed amount from every value.
    /// </summary>
    /// <param name="chainProvider">Chain provider.</param>
    /// <param name="amount">Finite amount to subtract.</param>
    /// <returns>A hub of <c>source − amount</c> values.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="chainProvider"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the amount is not finite.</exception>
    public static ScalarMathHub ToMinusHub(
        this IChainProvider<IReusable> chainProvider,
        double amount)
    {
        ArgumentNullException.ThrowIfNull(chainProvider);
        return new(chainProvider, ScalarOperation.Minus, amount, nameof(amount));
    }

    /// <summary>
    /// Creates a streaming hub that multiplies every value by a fixed factor.
    /// </summary>
    /// <param name="chainProvider">Chain provider.</param>
    /// <param name="factor">Finite multiplier.</param>
    /// <returns>A hub of <c>source × factor</c> values.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="chainProvider"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the factor is not finite.</exception>
    public static ScalarMathHub ToTimesHub(
        this IChainProvider<IReusable> chainProvider,
        double factor)
    {
        ArgumentNullException.ThrowIfNull(chainProvider);
        return new(chainProvider, ScalarOperation.Times, factor, nameof(factor));
    }

    /// <summary>
    /// Creates a streaming hub that divides every value by a fixed divisor.
    /// </summary>
    /// <param name="chainProvider">Chain provider.</param>
    /// <param name="divisor">Finite, non-zero divisor.</param>
    /// <returns>A hub of <c>source ÷ divisor</c> values.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="chainProvider"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the divisor is 0 or not finite.</exception>
    public static ScalarMathHub ToDividedByHub(
        this IChainProvider<IReusable> chainProvider,
        double divisor)
    {
        ArgumentNullException.ThrowIfNull(chainProvider);
        return new(chainProvider, ScalarOperation.DividedBy, divisor, nameof(divisor));
    }
}
