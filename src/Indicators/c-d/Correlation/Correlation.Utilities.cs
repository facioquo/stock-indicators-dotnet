namespace FacioQuo.Stock.Indicators;

public static partial class Correlation
{
    /// <summary>
    /// parameter validation
    /// </summary>
    /// <typeparam name="T">Type of record</typeparam>
    /// <param name="sourceA">First series of values</param>
    /// <param name="sourceB">Second series of values</param>
    /// <param name="lookbackPeriods">Quantity of periods in lookback window.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when a parameter is out of the valid range</exception>
    /// <exception cref="InvalidBarsException">Thrown when bars are invalid or insufficient</exception>
    internal static void Validate<T>(
        IReadOnlyList<T> sourceA,
        IReadOnlyList<T> sourceB,
        int lookbackPeriods)
        where T : ISeries
    {
        // check parameter arguments
        Validate(lookbackPeriods);

        // check bars
        if (sourceA.Count != sourceB.Count)
        {
            throw new InvalidBarsException(
                nameof(sourceB),
                "B bars should have at least as many records as A bars for Correlation.");
        }
    }

    /// <summary>
    /// Validates the lookback periods for Correlation calculations.
    /// </summary>
    /// <param name="lookbackPeriods">The number of periods to look back for the calculation.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the lookback periods are less than or equal to 0.</exception>
    internal static void Validate(int lookbackPeriods)
    {
        if (lookbackPeriods <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(lookbackPeriods), lookbackPeriods,
                "Lookback periods must be greater than 0 for Correlation.");
        }
    }
}
