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
        if (lookbackPeriods is int periods and not int.MinValue)
        {
            Validate(periods);
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

    /// <summary>
    /// Validates the PRS% lookback periods.
    /// </summary>
    /// <param name="lookbackPeriods">Number of periods for the PRS% lookback calculation.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the lookback periods are less than or equal to 0.</exception>
    internal static void Validate(int lookbackPeriods)
    {
        if (lookbackPeriods <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(lookbackPeriods), lookbackPeriods,
                "Lookback periods must be greater than 0 for Price Relative Strength.");
        }
    }

    /// <summary>
    /// Calculates the relative strength ratio of an evaluated value to a base value.
    /// </summary>
    /// <param name="evalValue">Evaluated value.</param>
    /// <param name="baseValue">Base value.</param>
    /// <returns>The ratio; <see langword="null"/> when the base value is 0 or the ratio is NaN.</returns>
    internal static double? Ratio(double evalValue, double baseValue)
        => baseValue == 0
            ? null
            : (evalValue / baseValue).NaN2Null();

    /// <summary>
    /// Calculates the evaluated percent change minus the base percent change since earlier values.
    /// </summary>
    /// <param name="evalValue">Current evaluated value.</param>
    /// <param name="baseValue">Current base value.</param>
    /// <param name="evalBack">Evaluated value <c>N</c> periods earlier.</param>
    /// <param name="baseBack">Base value <c>N</c> periods earlier.</param>
    /// <returns>The PRS%; <see langword="null"/> when an earlier value is 0 or the result is NaN.</returns>
    internal static double? Percent(
        double evalValue, double baseValue, double evalBack, double baseBack)
    {
        if (baseBack == 0 || evalBack == 0)
        {
            return null;
        }

        double pctB = (baseValue - baseBack) / baseBack;
        double pctE = (evalValue - evalBack) / evalBack;

        return (pctE - pctB).NaN2Null();
    }
}
