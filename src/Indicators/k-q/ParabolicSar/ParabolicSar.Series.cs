namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Parabolic SAR for a series of bars indicator.
/// </summary>
public static partial class ParabolicSar
{
    /// <summary>
    /// Converts a list of bars to Parabolic SAR results.
    /// </summary>
    /// <param name="bars">Aggregate OHLCV price bars, time sorted.</param>
    /// <param name="accelerationStep">Acceleration step for the SAR calculation. Default is 0.02.</param>
    /// <param name="maxAccelerationFactor">Maximum acceleration factor for the SAR calculation. Default is 0.2.</param>
    /// <returns>A list of <see cref="ParabolicSarResult"/> containing the SAR values.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the bars list is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the acceleration step or maximum acceleration factor are out of range.</exception>
    public static IReadOnlyList<ParabolicSarResult> ToParabolicSar(
        this IReadOnlyList<IBar> bars,
        double accelerationStep = 0.02,
        double maxAccelerationFactor = 0.2)

        => bars
            .ToBarDList()
            .CalcParabolicSar(
                accelerationStep,
                maxAccelerationFactor,
                accelerationStep);

    /// <summary>
    /// Gets the Parabolic SAR results for a list of bars.
    /// </summary>
    /// <param name="bars">Aggregate OHLCV price bars, time sorted.</param>
    /// <param name="accelerationStep">Acceleration step for the SAR calculation.</param>
    /// <param name="maxAccelerationFactor">Maximum acceleration factor for the SAR calculation.</param>
    /// <param name="initialFactor">Initial acceleration factor for the SAR calculation.</param>
    /// <returns>A list of <see cref="ParabolicSarResult"/> containing the SAR values.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the bars list is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the acceleration step, maximum acceleration factor, or initial factor are out of range.</exception>
    public static IReadOnlyList<ParabolicSarResult> ToParabolicSar(
        this IReadOnlyList<IBar> bars,
        double accelerationStep,
        double maxAccelerationFactor,
        double initialFactor)
        => bars
            .ToBarDList()
            .CalcParabolicSar(
                accelerationStep,
                maxAccelerationFactor,
                initialFactor);

    /// <summary>
    /// Calculates the Parabolic SAR for a list of bars.
    /// </summary>
    /// <param name="bars">Source list of bars.</param>
    /// <param name="accelerationStep">Acceleration step for the SAR calculation.</param>
    /// <param name="maxAccelerationFactor">Maximum acceleration factor for the SAR calculation.</param>
    /// <param name="initialFactor">Initial acceleration factor for the SAR calculation.</param>
    /// <returns>A list of <see cref="ParabolicSarResult"/> containing the SAR values.</returns>
    private static List<ParabolicSarResult> CalcParabolicSar(
        this List<BarD> bars,
        double accelerationStep,
        double maxAccelerationFactor,
        double initialFactor)
    {
        // check parameter arguments
        Validate(
            accelerationStep, maxAccelerationFactor, initialFactor);

        // initialize
        int length = bars.Count;
        List<ParabolicSarResult> results = new(length);

        if (length == 0)
        {
            return results;
        }

        BarD q0 = bars[0];

        double accelerationFactor = initialFactor;
        double extremePoint = q0.High;
        double priorSar = q0.Low;
        bool isRising = true;  // initial guess

        // roll through source values
        for (int i = 0; i < length; i++)
        {
            BarD q = bars[i];

            bool? isReversal;
            double psar;

            // skip first one
            if (i == 0)
            {
                results.Add(new(q.Timestamp));
                continue;
            }

            // was rising
            if (isRising)
            {
                double sar =
                    priorSar + (accelerationFactor * (extremePoint - priorSar));

                // SAR cannot be higher than last two lows
                if (i >= 2)
                {
                    double minLastTwo =
                        Math.Min(
                            bars[i - 1].Low,
                            bars[i - 2].Low);

                    sar = Math.Min(sar, minLastTwo);
                }

                // turn down
                if (q.Low < sar)
                {
                    isReversal = true;
                    psar = extremePoint;

                    isRising = false;
                    accelerationFactor = initialFactor;
                    extremePoint = q.Low;
                }

                // continue rising
                else
                {
                    isReversal = false;
                    psar = sar;

                    // new high extreme point
                    if (q.High > extremePoint)
                    {
                        extremePoint = q.High;
                        accelerationFactor =
                            Math.Min(
                                accelerationFactor + accelerationStep,
                                maxAccelerationFactor);
                    }
                }
            }

            // was falling
            else
            {
                double sar
                    = priorSar - (accelerationFactor * (priorSar - extremePoint));

                // SAR cannot be lower than last two highs
                if (i >= 2)
                {
                    double maxLastTwo = Math.Max(
                        bars[i - 1].High,
                        bars[i - 2].High);

                    sar = Math.Max(sar, maxLastTwo);
                }

                // turn up
                if (q.High > sar)
                {
                    isReversal = true;
                    psar = extremePoint;

                    isRising = true;
                    accelerationFactor = initialFactor;
                    extremePoint = q.High;
                }

                // continue falling
                else
                {
                    isReversal = false;
                    psar = sar;

                    // new low extreme point
                    if (q.Low < extremePoint)
                    {
                        extremePoint = q.Low;
                        accelerationFactor =
                            Math.Min(
                                accelerationFactor + accelerationStep,
                                maxAccelerationFactor);
                    }
                }
            }

            results.Add(new ParabolicSarResult(
                Timestamp: q.Timestamp,
                Sar: psar.NaN2Null(),
                IsReversal: isReversal));

            priorSar = psar;
        }

        // remove first trendline since it is an invalid guess
        int cutIndex = results.FindIndex(static x => x.IsReversal ?? false);

        cutIndex = cutIndex < 0 ? length - 1 : cutIndex;

        for (int d = 0; d <= cutIndex; d++)
        {
            results[d] = (results[d] with {
                Sar = null,
                IsReversal = null
            });
        }

        return results;
    }
}
