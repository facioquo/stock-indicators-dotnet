namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Provides utility methods for the BOP (Balance of Power) indicator.
/// </summary>
public static partial class Bop
{
    /// <summary>
    /// Validates the parameters for the BOP calculation.
    /// </summary>
    /// <param name="smoothPeriods">Number of smoothing periods for the BOP calculation.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the smoothing periods are less than or equal to 0.</exception>
    internal static void Validate(
        int smoothPeriods)
    {
        // check parameter arguments
        if (smoothPeriods <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(smoothPeriods), smoothPeriods,
                "Smoothing periods must be greater than 0 for BOP.");
        }
    }
}
