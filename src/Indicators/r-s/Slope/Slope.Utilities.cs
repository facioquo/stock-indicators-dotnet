namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Defines the interface for Slope calculations.
/// </summary>
public interface ISlope
{
    /// <summary>
    /// Gets the number of periods to look back.
    /// </summary>
    int LookbackPeriods { get; }
}

/// <summary>
/// Provides utility methods for Slope and linear regression calculations.
/// </summary>
public static partial class Slope
{
    /// <summary>
    /// Validates the lookback periods parameter.
    /// </summary>
    /// <param name="lookbackPeriods">Number of lookback periods to validate.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the lookback periods are less than or equal to 1.</exception>
    internal static void Validate(
        int lookbackPeriods)
    {
        // check parameter arguments
        if (lookbackPeriods <= 1)
        {
            throw new ArgumentOutOfRangeException(nameof(lookbackPeriods), lookbackPeriods,
                "Lookback periods must be greater than 1 for Slope/Linear regression.");
        }
    }
}
