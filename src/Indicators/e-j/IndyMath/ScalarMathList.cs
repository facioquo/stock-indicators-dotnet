namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Arithmetic between incremental reusable values and a fixed operand.
/// </summary>
public class ScalarMathList : BufferList<MathResult>, IIncrementFromChain, IScalarMath
{
    private readonly ScalarKernel _kernel;

    /// <summary>
    /// Initializes a new instance of the <see cref="ScalarMathList"/> class.
    /// </summary>
    /// <param name="operation">Arithmetic to apply to each value.</param>
    /// <param name="operand">Finite operand; non-zero for <see cref="ScalarOperation.DividedBy"/>.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when the operation is undefined, the operand is not finite, or a divisor is 0.
    /// </exception>
    public ScalarMathList(ScalarOperation operation, double operand)
        : this(operation, operand, nameof(operand)) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="ScalarMathList"/> class with initial reusable values.
    /// </summary>
    /// <param name="operation">Arithmetic to apply to each value.</param>
    /// <param name="operand">Finite operand; non-zero for <see cref="ScalarOperation.DividedBy"/>.</param>
    /// <param name="values">Initial reusable values to populate the list.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when the operation is undefined, the operand is not finite, or a divisor is 0.
    /// </exception>
    public ScalarMathList(ScalarOperation operation, double operand, IReadOnlyList<IReusable> values)
        : this(operation, operand, nameof(operand)) => Add(values);

    /// <summary>
    /// Initializes a new instance of the <see cref="ScalarMathList"/> class,
    /// reporting an invalid operand under the caller's parameter name.
    /// </summary>
    /// <param name="operation">Arithmetic to apply to each value.</param>
    /// <param name="operand">Finite operand; non-zero for <see cref="ScalarOperation.DividedBy"/>.</param>
    /// <param name="paramName">Caller's parameter name for the operand.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when the operation is undefined, the operand is not finite, or a divisor is 0.
    /// </exception>
    internal ScalarMathList(ScalarOperation operation, double operand, string paramName)
    {
        _kernel = new ScalarKernel(operation, operand, paramName);
        Operation = operation;
        Operand = operand;
        Name = ScalarKernel.FormatName(operation, operand);
    }

    /// <summary>
    /// Gets the arithmetic applied to each value.
    /// </summary>
    public ScalarOperation Operation { get; }

    /// <summary>
    /// Gets the fixed operand, as given.
    /// </summary>
    public double Operand { get; }

    /// <inheritdoc />
    public void Add(DateTime timestamp, double value)
        => AddInternal(new MathResult(timestamp, _kernel.Apply(value).NaN2Null()));

    /// <summary>
    /// Adds a new reusable value to the list.
    /// </summary>
    /// <param name="value">Reusable value to add.</param>
    /// <exception cref="ArgumentNullException">Thrown when the value is null.</exception>
    public void Add(IReusable value)
    {
        ArgumentNullException.ThrowIfNull(value);
        Add(value.Timestamp, value.Value);
    }

    /// <summary>
    /// Adds a list of reusable values to the list.
    /// </summary>
    /// <param name="values">List of reusable values to add.</param>
    /// <exception cref="ArgumentNullException">Thrown when the values list is null.</exception>
    public void Add(IReadOnlyList<IReusable> values)
    {
        ArgumentNullException.ThrowIfNull(values);

        for (int i = 0; i < values.Count; i++)
        {
            Add(values[i].Timestamp, values[i].Value);
        }
    }
}

public static partial class IndyMath
{
    /// <summary>
    /// Creates a buffer list that adds a fixed amount to every value.
    /// </summary>
    /// <param name="source">Chainable results, time sorted.</param>
    /// <param name="amount">Finite amount to add.</param>
    /// <returns>A buffer list of <c>source + amount</c> values.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the amount is not finite.</exception>
    public static ScalarMathList ToPlusList(
        this IReadOnlyList<IReusable> source,
        double amount)
        => new(ScalarOperation.Plus, amount, nameof(amount)) { source };

    /// <summary>
    /// Creates a buffer list that offsets every value by a percentage of itself.
    /// </summary>
    /// <param name="source">Chainable results, time sorted.</param>
    /// <param name="percentOffset">Finite percent offset, where 2.5 adds 2.5% and -2.5 subtracts 2.5%.</param>
    /// <returns>A buffer list of <c>source × (1 + percentOffset / 100)</c> values.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the percent offset is not finite.</exception>
    public static ScalarMathList ToPlusPercentList(
        this IReadOnlyList<IReusable> source,
        double percentOffset)
        => new(ScalarOperation.PlusPercent, percentOffset, nameof(percentOffset)) { source };

    /// <summary>
    /// Creates a buffer list that subtracts a fixed amount from every value.
    /// </summary>
    /// <param name="source">Chainable results, time sorted.</param>
    /// <param name="amount">Finite amount to subtract.</param>
    /// <returns>A buffer list of <c>source − amount</c> values.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the amount is not finite.</exception>
    public static ScalarMathList ToMinusList(
        this IReadOnlyList<IReusable> source,
        double amount)
        => new(ScalarOperation.Minus, amount, nameof(amount)) { source };

    /// <summary>
    /// Creates a buffer list that multiplies every value by a fixed factor.
    /// </summary>
    /// <param name="source">Chainable results, time sorted.</param>
    /// <param name="factor">Finite multiplier.</param>
    /// <returns>A buffer list of <c>source × factor</c> values.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the factor is not finite.</exception>
    public static ScalarMathList ToTimesList(
        this IReadOnlyList<IReusable> source,
        double factor)
        => new(ScalarOperation.Times, factor, nameof(factor)) { source };

    /// <summary>
    /// Creates a buffer list that divides every value by a fixed divisor.
    /// </summary>
    /// <param name="source">Chainable results, time sorted.</param>
    /// <param name="divisor">Finite, non-zero divisor.</param>
    /// <returns>A buffer list of <c>source ÷ divisor</c> values.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the divisor is 0 or not finite.</exception>
    public static ScalarMathList ToDividedByList(
        this IReadOnlyList<IReusable> source,
        double divisor)
        => new(ScalarOperation.DividedBy, divisor, nameof(divisor)) { source };
}
