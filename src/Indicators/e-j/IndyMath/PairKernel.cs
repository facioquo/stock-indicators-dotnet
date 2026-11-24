namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Arithmetic between the values of two series, shared by every style.
/// </summary>
internal static class PairKernel
{
    /// <summary>
    /// Validates a pair operation.
    /// </summary>
    /// <param name="operation">Operation to check.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the operation is undefined.</exception>
    internal static void Validate(PairOperation operation)
    {
        if (!Enum.IsDefined(operation))
        {
            throw new ArgumentOutOfRangeException(
                nameof(operation), operation, "Undefined pair operation.");
        }
    }

    /// <summary>
    /// Applies the operation to one matched pair of values.
    /// </summary>
    /// <param name="operation">Operation to apply.</param>
    /// <param name="source">Source value; NaN propagates.</param>
    /// <param name="other">Other value; NaN propagates.</param>
    /// <returns>The operation result; NaN when dividing by 0.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static double Apply(PairOperation operation, double source, double other)
        => operation switch {
            PairOperation.Plus => source + other,
            PairOperation.Minus => source - other,
            PairOperation.Times => source * other,
            PairOperation.DividedBy => other != 0 ? source / other : double.NaN,
            PairOperation.MaxWith => Math.Max(source, other),
            _ => Math.Min(source, other)
        };

    /// <summary>
    /// Formats the display name of a pair operation, such as <c>PLUS-SERIES</c> or <c>MAX-WITH</c>.
    /// </summary>
    /// <param name="operation">Operation applied.</param>
    /// <returns>The display name, matching the operation's catalog ID.</returns>
    internal static string FormatName(PairOperation operation)
        => operation switch {
            PairOperation.Plus => "PLUS-SERIES",
            PairOperation.Minus => "MINUS-SERIES",
            PairOperation.Times => "TIMES-SERIES",
            PairOperation.DividedBy => "DIVIDED-BY-SERIES",
            PairOperation.MaxWith => "MAX-WITH",
            _ => "MIN-WITH"
        };
}
