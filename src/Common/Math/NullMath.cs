namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Nullable <c>System.<see cref="Math"/></c> functions.
/// </summary>
/// <remarks>
/// <c>System.Math</c> infamously does not allow
/// or handle nullable input values.
/// Instead of adding repetitive inline defensive code,
/// we're using these equivalents.  Most are simple wrappers.
/// </remarks>
public static class NullMath
{
    /// <summary>
    /// Returns the absolute value of a nullable double.
    /// </summary>
    /// <param name="value">Nullable double value.</param>
    /// <returns>Absolute value, or null if the input is null.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double? Abs(this double? value)
        => value.HasValue
        ? (value.GetValueOrDefault() < 0
            ? -value.GetValueOrDefault()
            : value)
        : null;

    /// <summary>
    /// Converts a nullable double value to NaN if it is null.
    /// </summary>
    /// <param name="value">Nullable double value.</param>
    /// <returns>Value, or NaN if the input is null.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double Null2NaN(this double? value)
        => value ?? double.NaN;

    /// <summary>
    /// Converts a nullable decimal value to NaN if it is null.
    /// </summary>
    /// <param name="value">Nullable decimal value.</param>
    /// <returns>Value as a double, or NaN if the input is null.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double Null2NaN(this decimal? value)
        => value.HasValue
        ? (double)value.GetValueOrDefault()
        : double.NaN;

    /// <summary>
    /// Converts a nullable double value to null if it is NaN.
    /// </summary>
    /// <param name="value">Nullable double value.</param>
    /// <returns>Value, or null if the input is NaN.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double? NaN2Null(this double? value)
        => value.HasValue && double.IsNaN(value.GetValueOrDefault())
        ? null
        : value;

    /// <summary>
    /// Converts a double value to null if it is NaN.
    /// </summary>
    /// <param name="value">Double value.</param>
    /// <returns>Value, or null if the input is NaN.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double? NaN2Null(this double value)
        => double.IsNaN(value)
        ? null
        : value;
}
