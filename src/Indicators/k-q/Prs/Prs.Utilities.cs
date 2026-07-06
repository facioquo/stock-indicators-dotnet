using System.Globalization;

namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Provides utility methods for the PRS (Price Relative Strength) indicator.
/// </summary>
public static partial class Prs
{
    private static readonly CultureInfo invariantCulture = CultureInfo.InvariantCulture;

    /// <summary>
    /// Validates the parameters for PRS calculations.
    /// </summary>
    /// <typeparam name="T">Type of the source values.</typeparam>
    /// <param name="barsEval">List of evaluation bars.</param>
    /// <param name="barsBase">List of base bars.</param>
    /// <param name="lookbackPeriods">Number of periods for the lookback calculation.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the lookback periods are less than or equal to 0.</exception>
    /// <exception cref="InvalidBarsException">Thrown when there are insufficient bars or mismatched bar counts.</exception>
    internal static void Validate<T>(
        IReadOnlyList<T> barsEval,
        IReadOnlyList<T> barsBase,
        int? lookbackPeriods)
        where T : IReusable
    {
        // check parameter arguments
        if (lookbackPeriods is <= 0 and not int.MinValue)
        {
            throw new ArgumentOutOfRangeException(
                nameof(lookbackPeriods), lookbackPeriods,
                "Lookback periods must be greater than 0 for Price Relative Strength.");
        }

        // check bars
        int qtyHistoryEval = barsEval.Count;
        int qtyHistoryBase = barsBase.Count;

        if (qtyHistoryEval < lookbackPeriods)
        {
            string message = "Insufficient bars provided for Price Relative Strength.  " +
                string.Format(
                    invariantCulture,
                    "You provided {0} periods of bars when at least {1} are required.",
                    qtyHistoryEval, lookbackPeriods);

            throw new InvalidBarsException(nameof(barsEval), message);
        }

        if (qtyHistoryBase != qtyHistoryEval)
        {
            throw new InvalidBarsException(
                nameof(barsBase),
                "Base bars should have at least as many records as Eval bars for PRS.");
        }
    }

}
