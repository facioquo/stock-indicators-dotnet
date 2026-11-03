namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Provides utility methods for the TTM Squeeze indicator.
/// </summary>
public static partial class TtmSqueeze
{
    /// <summary>
    /// Calculates the squeeze state and the momentum before smoothing for the newest bar.
    /// </summary>
    /// <param name="highs">High prices of the last N bars, oldest first.</param>
    /// <param name="lows">Low prices of the last N bars, oldest first.</param>
    /// <param name="closes">Close prices of the last N bars, oldest first.</param>
    /// <param name="trueRanges">True ranges of the last N bars, oldest first, or empty until N are available.</param>
    /// <param name="standardDeviations">Width of the Bollinger Bands, in standard deviations.</param>
    /// <param name="multiplier">Width of the Keltner Channels, in average true ranges.</param>
    /// <returns>The momentum before smoothing, and whether the squeeze is on.</returns>
    internal static (double Delta, bool? IsSqueezeOn) Increment(
        ReadOnlySpan<double> highs,
        ReadOnlySpan<double> lows,
        ReadOnlySpan<double> closes,
        ReadOnlySpan<double> trueRanges,
        double standardDeviations,
        double multiplier)
    {
        int n = closes.Length;

        double sum = 0;
        double highestHigh = highs[0];
        double lowestLow = lows[0];

        for (int p = 0; p < n; p++)
        {
            sum += closes[p];
            highestHigh = Math.Max(highestHigh, highs[p]);
            lowestLow = Math.Min(lowestLow, lows[p]);
        }

        double sma = sum / n;

        // close less the average of the Donchian midline and the SMA
        double delta = closes[n - 1] - ((((highestHigh + lowestLow) / 2) + sma) / 2);

        if (trueRanges.Length < n)
        {
            return (delta, null);
        }

        double sumSqDev = 0;
        double sumTr = 0;

        for (int p = 0; p < n; p++)
        {
            double dev = closes[p] - sma;
            sumSqDev += dev * dev;
            sumTr += trueRanges[p];
        }

        double stdDev = Math.Sqrt(sumSqDev / n);
        double atr = sumTr / n;

        // both envelopes share the SMA centerline
        double upperBand = sma + (standardDeviations * stdDev);
        double lowerBand = sma - (standardDeviations * stdDev);
        double upperChannel = sma + (multiplier * atr);
        double lowerChannel = sma - (multiplier * atr);

        bool? isSqueezeOn = double.IsNaN(upperBand) || double.IsNaN(upperChannel)
            ? null
            : upperBand < upperChannel && lowerBand > lowerChannel;

        return (delta, isSqueezeOn);
    }

    /// <summary>
    /// Smooths momentum with a least squares line through the last N values.
    /// </summary>
    /// <param name="deltas">Momentum before smoothing for the last N bars, oldest first.</param>
    /// <returns>The value of the line at the newest bar.</returns>
    internal static double LinearRegression(ReadOnlySpan<double> deltas)
    {
        int n = deltas.Length;

        double sumY = 0;

        for (int p = 0; p < n; p++)
        {
            sumY += deltas[p];
        }

        // x runs from 1 to N
        double avgX = (n + 1) / 2d;
        double avgY = sumY / n;

        double sumSqX = 0;
        double sumSqXy = 0;

        for (int p = 0; p < n; p++)
        {
            double devX = p + 1 - avgX;
            sumSqX += devX * devX;
            sumSqXy += devX * (deltas[p] - avgY);
        }

        double slope = sumSqXy / sumSqX;

        return avgY + (slope * (n - avgX));
    }

    /// <summary>
    /// Validates the parameters for the TTM Squeeze calculation.
    /// </summary>
    /// <param name="lookbackPeriods">Number of periods for the bands, channels, and momentum.</param>
    /// <param name="standardDeviations">Width of the Bollinger Bands, in standard deviations.</param>
    /// <param name="multiplier">Width of the Keltner Channels, in average true ranges.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when any of the parameters are out of their valid range.
    /// </exception>
    internal static void Validate(
        int lookbackPeriods,
        double standardDeviations,
        double multiplier)
    {
        // check parameter arguments
        if (lookbackPeriods <= 1)
        {
            throw new ArgumentOutOfRangeException(nameof(lookbackPeriods), lookbackPeriods,
                "Lookback periods must be greater than 1 for TTM Squeeze.");
        }

        if (standardDeviations <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(standardDeviations), standardDeviations,
                "Standard Deviations must be greater than 0 for TTM Squeeze.");
        }

        if (multiplier <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(multiplier), multiplier,
                "Multiplier must be greater than 0 for TTM Squeeze.");
        }
    }
}
