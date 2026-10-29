namespace FacioQuo.Stock.Indicators;

/// <summary>
/// TTM Squeeze on a series of bars indicator.
/// </summary>
public static partial class TtmSqueeze
{
    /// <summary>
    /// Converts a list of bars to TTM Squeeze results.
    /// </summary>
    /// <param name="bars">Aggregate OHLCV price bars, time sorted.</param>
    /// <param name="lookbackPeriods">Number of periods for the bands, channels, and momentum.</param>
    /// <param name="standardDeviations">Width of the Bollinger Bands, in standard deviations.</param>
    /// <param name="multiplier">Width of the Keltner Channels, in average true ranges.</param>
    /// <returns>A list of TTM Squeeze results.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when any of the parameters are out of their valid range.</exception>
    public static IReadOnlyList<TtmSqueezeResult> ToTtmSqueeze(
        this IReadOnlyList<IBar> bars,
        int lookbackPeriods = 20,
        double standardDeviations = 2,
        double multiplier = 1.5)
        => bars
            .ToBarDList()
            .CalcTtmSqueeze(lookbackPeriods, standardDeviations, multiplier);

    /// <summary>
    /// Calculates the TTM Squeeze for a list of bars.
    /// </summary>
    /// <param name="bars">Source list of bars.</param>
    /// <param name="lookbackPeriods">Number of periods for the bands, channels, and momentum.</param>
    /// <param name="standardDeviations">Width of the Bollinger Bands, in standard deviations.</param>
    /// <param name="multiplier">Width of the Keltner Channels, in average true ranges.</param>
    /// <returns>A list of TTM Squeeze results.</returns>
    private static List<TtmSqueezeResult> CalcTtmSqueeze(
        this List<BarD> bars,
        int lookbackPeriods,
        double standardDeviations,
        double multiplier)
    {
        // check parameter arguments
        Validate(lookbackPeriods, standardDeviations, multiplier);

        // initialize
        int length = bars.Count;
        TtmSqueezeResult[] results = new TtmSqueezeResult[length];

        double[] highs = new double[length];
        double[] lows = new double[length];
        double[] closes = new double[length];
        double[] trueRanges = new double[length];
        double[] deltas = new double[length];

        // roll through source values
        for (int i = 0; i < length; i++)
        {
            BarD b = bars[i];

            highs[i] = b.High;
            lows[i] = b.Low;
            closes[i] = b.Close;
            trueRanges[i] = i > 0
                ? Tr.Increment(b.High, b.Low, bars[i - 1].Close)
                : double.NaN;

            if (i < lookbackPeriods - 1)
            {
                deltas[i] = double.NaN;
                results[i] = new(b.Timestamp);
                continue;
            }

            int start = i + 1 - lookbackPeriods;

            // the first bar has no true range, so the channels start one bar later
            (double delta, bool? isSqueezeOn) = Increment(
                highs.AsSpan(start, lookbackPeriods),
                lows.AsSpan(start, lookbackPeriods),
                closes.AsSpan(start, lookbackPeriods),
                i >= lookbackPeriods
                    ? trueRanges.AsSpan(start, lookbackPeriods)
                    : [],
                standardDeviations,
                multiplier);

            deltas[i] = delta;

            double? momentum = i >= (2 * lookbackPeriods) - 2
                ? LinearRegression(deltas.AsSpan(start, lookbackPeriods)).NaN2Null()
                : null;

            results[i] = new TtmSqueezeResult(
                Timestamp: b.Timestamp,
                Momentum: momentum,
                IsSqueezeOn: isSqueezeOn) {
                Delta = delta.NaN2Null()
            };
        }

        return new List<TtmSqueezeResult>(results);
    }
}
