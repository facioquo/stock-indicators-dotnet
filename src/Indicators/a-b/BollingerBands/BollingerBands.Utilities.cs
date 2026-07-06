namespace FacioQuo.Stock.Indicators;

public static partial class BollingerBands
{
    // remove recommended periods
    /// <inheritdoc cref="Reusable.RemoveWarmupPeriods{T}(IReadOnlyList{T})"/>
    public static IReadOnlyList<BollingerBandsResult> RemoveWarmupPeriods(
        this IReadOnlyList<BollingerBandsResult> results)
    {
        ArgumentNullException.ThrowIfNull(results);

        int removePeriods = results
            .FindIndex(static x => x.Width != null);

        return results.Remove(removePeriods);
    }

    /// <summary>
    /// Bollinger Bands calculation for streaming scenarios.
    /// </summary>
    /// <param name="source">List of chainable values.</param>
    /// <param name="lookbackPeriods">Quantity of periods in lookback window.</param>
    /// <param name="standardDeviations">Number of standard deviations for bands.</param>
    /// <param name="endIndex">Index position to evaluate.</param>
    /// <typeparam name="T">IReusable (chainable) type.</typeparam>
    /// <returns>Bollinger Bands result or null result when insufficient data.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when a parameter is out of the valid range</exception>
    internal static BollingerBandsResult Increment<T>(
        IReadOnlyList<T> source,
        int lookbackPeriods,
        double standardDeviations,
        int endIndex)
        where T : IReusable
    {
        ArgumentNullException.ThrowIfNull(source);

        if ((uint)endIndex >= (uint)source.Count)
        {
            throw new ArgumentOutOfRangeException(
                nameof(endIndex), endIndex,
                "End index must refer to an existing element in the source cache.");
        }

        DateTime timestamp = source[endIndex].Timestamp;

        if (endIndex < lookbackPeriods - 1)
        {
            return new BollingerBandsResult(timestamp);
        }

        // Calculate SMA
        double sum = 0;
        for (int i = endIndex - lookbackPeriods + 1; i <= endIndex; i++)
        {
            sum += source[i].Value;
        }

        double sma = sum / lookbackPeriods;

        // Calculate standard deviation
        double sumSquaredDiff = 0;
        for (int i = endIndex - lookbackPeriods + 1; i <= endIndex; i++)
        {
            double diff = source[i].Value - sma;
            sumSquaredDiff += diff * diff;
        }

        double stdDev = Math.Sqrt(sumSquaredDiff / lookbackPeriods);

        // Calculate bands
        double upperBand = sma + (standardDeviations * stdDev);
        double lowerBand = sma - (standardDeviations * stdDev);

        // Get current value for derived calculations
        double currentValue = source[endIndex].Value;

        // Calculate derived values
        double? percentB = upperBand == lowerBand ? null
            : (currentValue - lowerBand) / (upperBand - lowerBand);

        double? zScore = stdDev == 0 ? null : (currentValue - sma) / stdDev;
        double? width = sma == 0 ? null : (upperBand - lowerBand) / sma;

        return new BollingerBandsResult(
            Timestamp: timestamp,
            Sma: sma,
            UpperBand: upperBand,
            LowerBand: lowerBand,
            PercentB: percentB,
            ZScore: zScore,
            Width: width
        );
    }

    /// <summary>
    /// parameter validation
    /// </summary>
    /// <param name="lookbackPeriods">Quantity of periods in lookback window.</param>
    /// <param name="standardDeviations">Number of standard deviations</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when a parameter is out of the valid range</exception>
    internal static void Validate(
        int lookbackPeriods,
        double standardDeviations)
    {
        // check parameter arguments
        if (lookbackPeriods <= 1)
        {
            throw new ArgumentOutOfRangeException(nameof(lookbackPeriods), lookbackPeriods,
                "Lookback periods must be greater than 1 for Bollinger Bands.");
        }

        if (standardDeviations <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(standardDeviations), standardDeviations,
                "Standard Deviations must be greater than 0 for Bollinger Bands.");
        }
    }
}
