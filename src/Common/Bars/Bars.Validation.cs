using System.Globalization;

namespace FacioQuo.Stock.Indicators;

// BAR UTILITIES: VALIDATION

public static partial class Bars
{
    private static readonly CultureInfo invariantCulture
        = CultureInfo.InvariantCulture;

    /// <summary>
    /// Check that bars are valid and in ascending order.
    /// </summary>
    /// <typeparam name="TBar">IBar type</typeparam>
    /// <param name="bars">Aggregate OHLCV price bars, time sorted.</param>
    /// <returns>Valid list of bars</returns>
    /// <exception cref="ArgumentNullException">
    /// List of bars cannot be a null reference.
    /// </exception>
    /// <exception cref="InvalidBarsException">
    /// Duplicate or out of sequence bars found.
    /// </exception>
    public static IReadOnlyList<TBar> Validate<TBar>(
        this IReadOnlyList<TBar> bars)
        where TBar : IBar
    {
        ArgumentNullException.ThrowIfNull(bars);

        if (bars.Count == 0)
        {
            return bars;
        }

        DateTime lastDate = bars[0].Timestamp;
        for (int i = 1; i < bars.Count; i++)
        {
            DateTime currentDate = bars[i].Timestamp;

            if (lastDate == currentDate)
            {
                string msg =
                    $"Duplicate date found on {currentDate.ToString("o", invariantCulture)}.";

                throw new InvalidBarsException(nameof(bars), msg);
            }

            if (lastDate > currentDate)
            {
                string msg =
                    $"Bars are out of sequence on {currentDate.ToString("o", invariantCulture)}.";

                throw new InvalidBarsException(nameof(bars), msg);
            }

            lastDate = currentDate;
        }

        return bars;
    }
}
