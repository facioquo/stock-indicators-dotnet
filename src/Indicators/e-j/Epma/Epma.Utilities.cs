namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Provides utility methods for Endpoint Moving Average (EPMA) calculations.
/// </summary>
public static partial class Epma
{
    /// <summary>
    /// Validates the lookback periods for EPMA calculations.
    /// </summary>
    /// <param name="lookbackPeriods">Quantity of periods in lookback window.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when the lookback periods are less than or equal to 0.
    /// </exception>
    internal static void Validate(
        int lookbackPeriods)
    {
        // check parameter arguments
        if (lookbackPeriods <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(lookbackPeriods), lookbackPeriods,
                "Lookback periods must be greater than 0 for Epma.");
        }
    }

    /// <summary>
    /// Calculates EPMA increment for the current position using linear regression.
    /// </summary>
    /// <typeparam name="T">Type of the source items, must implement IReusable.</typeparam>
    /// <param name="source">Source data provider cache.</param>
    /// <param name="lookbackPeriods">Quantity of periods in lookback window.</param>
    /// <param name="endIndex">Current cache index position to evaluate.</param>
    /// <param name="cacheOffset">Offset to calculate global positions (totalCount - cacheSize).</param>
    /// <returns>EPMA value or double.NaN if incalculable.</returns>
    internal static double Increment<T>(
        IReadOnlyList<T> source,
        int lookbackPeriods,
        int endIndex,
        int cacheOffset)
        where T : IReusable
    {
        if (endIndex < lookbackPeriods - 1 || endIndex >= source.Count)
        {
            return double.NaN;
        }

        // Calculate linear regression using global position indices (p + 1)
        // to match Series implementation, where p is the absolute position
        // For cache index i, global position = cacheOffset + i
        int startIndex = endIndex - lookbackPeriods + 1;

        // Calculate averages
        double sumX = 0;
        double sumY = 0;

        for (int i = 0; i < lookbackPeriods; i++)
        {
            int cacheIdx = startIndex + i;
            int globalPos = cacheOffset + cacheIdx;
            sumX += globalPos + 1; // X values: (p + 1) where p is global position
            sumY += source[cacheIdx].Value;
        }

        double avgX = sumX / lookbackPeriods;
        double avgY = sumY / lookbackPeriods;

        // Least squares method
        double sumSqX = 0;
        double sumSqXy = 0;

        for (int i = 0; i < lookbackPeriods; i++)
        {
            int cacheIdx = startIndex + i;
            int globalPos = cacheOffset + cacheIdx;
            double devX = (globalPos + 1) - avgX;
            double devY = source[cacheIdx].Value - avgY;

            sumSqX += devX * devX;
            sumSqXy += devX * devY;
        }

        if (sumSqX == 0)
        {
            return double.NaN;
        }

        double slope = sumSqXy / sumSqX;
        double intercept = avgY - (slope * avgX);

        // EPMA calculation: slope * (endpoint_X) + intercept
        // Endpoint is at cache index endIndex, global position = cacheOffset + endIndex
        int endpointX = cacheOffset + endIndex + 1;
        return (slope * endpointX) + intercept;
    }
}
