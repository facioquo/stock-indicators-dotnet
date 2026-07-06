namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Provides utility methods for the ALMA (Arnaud Legoux Moving Average) indicator.
/// </summary>
public static partial class Alma
{
    /// <summary>
    /// ALMA calculation for streaming scenarios.
    /// </summary>
    /// <param name="source">List of chainable values.</param>
    /// <param name="lookbackPeriods">Quantity of periods in lookback window.</param>
    /// <param name="offset">Offset parameter for the ALMA calculation.</param>
    /// <param name="sigma">Sigma parameter for the ALMA calculation.</param>
    /// <param name="endIndex">Index position to evaluate.</param>
    /// <typeparam name="T">IReusable (chainable) type.</typeparam>
    /// <returns>ALMA value or <see langword="double.NaN"/> when incalculable.</returns>
    internal static double Increment<T>(
        IReadOnlyList<T> source,
        int lookbackPeriods,
        double offset,
        double sigma,
        int endIndex)
        where T : IReusable
    {
        if (endIndex < lookbackPeriods - 1 || endIndex >= source.Count)
        {
            return double.NaN;
        }

        // Pre-calculate weights and normalization factor for efficiency
        double m = offset * (lookbackPeriods - 1);
        double s = lookbackPeriods / sigma;

        double[] weight = new double[lookbackPeriods];
        double norm = 0;

        for (int i = 0; i < lookbackPeriods; i++)
        {
            double wt = Math.Exp(-((i - m) * (i - m)) / (2 * s * s));
            weight[i] = wt;
            norm += wt;
        }

        // Apply weights to values in the lookback window
        double weightedSum = 0;
        for (int i = 0; i < lookbackPeriods; i++)
        {
            int sourceIndex = endIndex - lookbackPeriods + 1 + i;
            weightedSum += weight[i] * source[sourceIndex].Value;
        }

        return weightedSum / norm;
    }

    /// <summary>
    /// Validates the parameters for the ALMA calculation.
    /// </summary>
    /// <param name="lookbackPeriods">Quantity of periods in lookback window.</param>
    /// <param name="offset">Offset parameter for the ALMA calculation, must be between 0 and 1.</param>
    /// <param name="sigma">Sigma parameter for the ALMA calculation, must be greater than 0.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when the lookback periods are less than or equal to 1,
    /// the offset is not between 0 and 1, or the sigma is less than or equal to 0.
    /// </exception>
    internal static void Validate(
        int lookbackPeriods,
        double offset,
        double sigma)
    {
        // check parameter arguments
        if (lookbackPeriods <= 1)
        {
            throw new ArgumentOutOfRangeException(nameof(lookbackPeriods), lookbackPeriods,
                "Lookback periods must be greater than 1 for ALMA.");
        }

        if (offset is < 0 or > 1)
        {
            throw new ArgumentOutOfRangeException(nameof(offset), offset,
                "Offset must be between 0 and 1 for ALMA.");
        }

        if (sigma <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(sigma), sigma,
                "Sigma must be greater than 0 for ALMA.");
        }
    }
}
