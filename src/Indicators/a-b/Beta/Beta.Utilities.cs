namespace FacioQuo.Stock.Indicators;

public static partial class Beta
{
    /// <summary>
    /// parameter validation
    /// </summary>
    /// <typeparam name="T">Type of record</typeparam>
    /// <param name="sourceEval">Eval bar series</param>
    /// <param name="sourceMrkt">Market bar series</param>
    /// <param name="lookbackPeriods">Quantity of periods in lookback window.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when a parameter is out of the valid range</exception>
    /// <exception cref="InvalidBarsException">Thrown when bars are invalid or insufficient</exception>
    internal static void Validate<T>(
        IReadOnlyList<T> sourceEval,
        IReadOnlyList<T> sourceMrkt,
        int lookbackPeriods)
        where T : ISeries
    {
        // check parameter arguments
        if (lookbackPeriods <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(lookbackPeriods), lookbackPeriods,
                "Lookback periods must be greater than 0 for Beta.");
        }

        // check bars
        if (sourceEval.Count != sourceMrkt.Count)
        {
            throw new InvalidBarsException(
                nameof(sourceEval),
                "Eval bars should have the same number of Market bars for Beta.");
        }
    }
}
