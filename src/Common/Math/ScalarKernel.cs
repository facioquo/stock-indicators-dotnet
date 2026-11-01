using System.Globalization;

namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Validated arithmetic between a value and a fixed operand, shared by every style.
/// </summary>
internal sealed class ScalarKernel
{
    private readonly ScalarOperation _operation;
    private readonly double _operand;

    /// <summary>
    /// Initializes a new instance of the <see cref="ScalarKernel"/> class.
    /// </summary>
    /// <param name="operation">Arithmetic to apply.</param>
    /// <param name="operand">Finite operand; non-zero for <see cref="ScalarOperation.DividedBy"/>.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when the operation is undefined, the operand is not finite, or a divisor is 0.
    /// </exception>
    internal ScalarKernel(ScalarOperation operation, double operand)
    {
        string paramName = operation switch {
            ScalarOperation.Plus or ScalarOperation.Minus => "amount",
            ScalarOperation.Times => "factor",
            ScalarOperation.DividedBy => "divisor",
            ScalarOperation.PlusPercent => "percentOffset",
            _ => throw new ArgumentOutOfRangeException(
                nameof(operation), operation, "Undefined scalar operation.")
        };

        if (!double.IsFinite(operand))
        {
            throw new ArgumentOutOfRangeException(
                paramName, operand, $"The {paramName} value must be a finite number.");
        }

        if (operation == ScalarOperation.DividedBy && operand == 0)
        {
            throw new ArgumentOutOfRangeException(
                paramName, operand, "The divisor value must not be 0.");
        }

        _operation = operation;
        _operand = operation == ScalarOperation.PlusPercent
            ? 1d + (operand / 100d)
            : operand;

        Name = string.Create(
            CultureInfo.InvariantCulture,
            $"{operation.ToString().ToUpperInvariant()}({operand})");
    }

    /// <summary>
    /// Gets the display name, such as <c>PLUS(2.5)</c>.
    /// </summary>
    internal string Name { get; }

    /// <summary>
    /// Applies the operation to one value.
    /// </summary>
    /// <param name="value">Source value; NaN propagates.</param>
    /// <returns>The operation result.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal double Apply(double value)
        => _operation switch {
            ScalarOperation.Plus => value + _operand,
            ScalarOperation.Minus => value - _operand,
            ScalarOperation.Times or ScalarOperation.PlusPercent => value * _operand,
            _ => value / _operand
        };
}
