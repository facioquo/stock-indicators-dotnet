namespace FacioQuo.Stock.Indicators;

public static partial class IndyMath
{
    /// <summary>
    /// Validates the rolling window size.
    /// </summary>
    /// <param name="lookbackPeriods">Number of periods in the window.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the lookback periods are less than 1.</exception>
    internal static void Validate(int lookbackPeriods)
    {
        if (lookbackPeriods < 1)
        {
            throw new ArgumentOutOfRangeException(
                nameof(lookbackPeriods), lookbackPeriods,
                "Lookback periods must be greater than 0.");
        }
    }
}
