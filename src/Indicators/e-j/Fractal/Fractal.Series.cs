namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Williams Fractal indicator.
/// </summary>
public static partial class Fractal
{
    /// <summary>
    /// Converts a list of bars to Fractal results using the same span for both left and right wings.
    /// </summary>
    /// <param name="bars">Aggregate OHLCV price bars, time sorted.</param>
    /// <param name="windowSpan">Number of periods to look back and forward for the calculation. Default is 2.</param>
    /// <param name="endType">Type of price to use for the calculation. Default is HighLow.</param>
    /// <returns>A list of Fractal results.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the bars list is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the window span is invalid.</exception>
    public static IReadOnlyList<FractalResult> ToFractal(
        this IReadOnlyList<IBar> bars,
        int windowSpan = 2,
        EndType endType = EndType.HighLow)
        => bars
            .ToFractal(windowSpan, windowSpan, endType);

    /// <summary>
    /// Converts a list of bars to Fractal results using different spans for left and right wings.
    /// </summary>
    /// <param name="bars">Aggregate OHLCV price bars, time sorted.</param>
    /// <param name="leftSpan">Number of periods to look back for the calculation.</param>
    /// <param name="rightSpan">Number of periods to look forward for the calculation.</param>
    /// <param name="endType">Type of price to use for the calculation. Default is HighLow.</param>
    /// <returns>A list of Fractal results.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the bars list is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the left or right span is invalid.</exception>
    public static IReadOnlyList<FractalResult> ToFractal(
        this IReadOnlyList<IBar> bars,
        int leftSpan,
        int rightSpan,
        EndType endType = EndType.HighLow)
    {
        // check parameter arguments
        ArgumentNullException.ThrowIfNull(bars);
        Validate(Math.Min(leftSpan, rightSpan));

        // initialize
        int length = bars.Count;
        List<FractalResult> results = new(length);

        // roll through source values
        for (int i = 0; i < length; i++)
        {
            IBar q = bars[i];
            decimal? fractalBear = null;
            decimal? fractalBull = null;

            if (i + 1 > leftSpan && i + 1 <= length - rightSpan)
            {
                bool isHigh = true;
                bool isLow = true;

                decimal evalHigh = endType == EndType.Close ?
                   q.Close : q.High;

                decimal evalLow = endType == EndType.Close ?
                    q.Close : q.Low;

                // compare today with wings
                for (int p = i - leftSpan; p <= i + rightSpan; p++)
                {
                    // skip center eval period
                    if (p == i)
                    {
                        continue;
                    }

                    // evaluate wing periods
                    IBar wing = bars[p];

                    decimal wingHigh = endType == EndType.Close ?
                        wing.Close : wing.High;

                    decimal wingLow = endType == EndType.Close ?
                        wing.Close : wing.Low;

                    if (evalHigh <= wingHigh)
                    {
                        isHigh = false;
                    }

                    if (evalLow >= wingLow)
                    {
                        isLow = false;
                    }
                }

                // bearish signal
                if (isHigh)
                {
                    fractalBear = evalHigh;
                }

                // bullish signal
                if (isLow)
                {
                    fractalBull = evalLow;
                }
            }

            results.Add(new(
                Timestamp: q.Timestamp,
                FractalBear: fractalBear,
                FractalBull: fractalBull));
        }

        return results;
    }
}
