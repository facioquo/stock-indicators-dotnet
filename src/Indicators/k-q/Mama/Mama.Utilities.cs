namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Provides utility methods for the MESA Adaptive Moving Average (MAMA).
/// </summary>
public static partial class Mama
{
    /// <summary>
    /// Removes the recommended warmup periods from the MAMA results.
    /// </summary>
    /// <inheritdoc cref="Reusable.RemoveWarmupPeriods{T}(IReadOnlyList{T})"/>
    public static IReadOnlyList<MamaResult> RemoveWarmupPeriods(
        this IReadOnlyList<MamaResult> results) => results.Remove(50);

    /// <summary>
    /// Validates the parameters for the MAMA calculation.
    /// </summary>
    /// <param name="fastLimit">Fast limit for the MAMA calculation.</param>
    /// <param name="slowLimit">Slow limit for the MAMA calculation.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when the fast limit is less than or equal to the slow limit, or greater than or equal to 1,
    /// or when the slow limit is less than or equal to 0.
    /// </exception>
    internal static void Validate(
        double fastLimit,
        double slowLimit)
    {
        // check parameter arguments
        if (fastLimit <= slowLimit || fastLimit >= 1)
        {
            throw new ArgumentOutOfRangeException(nameof(fastLimit), fastLimit,
                "Fast Limit must be greater than Slow Limit and less than 1 for MAMA.");
        }

        if (slowLimit <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(slowLimit), slowLimit,
                "Slow Limit must be greater than 0 for MAMA.");
        }
    }
}
