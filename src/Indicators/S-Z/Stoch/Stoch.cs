namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Stochastic Oscillator indicator.
/// </summary>
public static partial class Stoch
{
    /// <summary>
    /// Calculates the Stochastic Oscillator for a series of bars.
    /// </summary>
    /// <param name="bars">Aggregate OHLCV price bars, time sorted.</param>
    /// <param name="lookbackPeriods">Lookback period for the oscillator.</param>
    /// <param name="signalPeriods">Signal period for the oscillator.</param>
    /// <param name="smoothPeriods">Smoothing period for the oscillator.</param>
    /// <returns>A list of StochResult containing the oscillator values.</returns>
    public static IReadOnlyList<StochResult> ToStoch(
        this IReadOnlyList<IBar> bars,
        int lookbackPeriods = 14,
        int signalPeriods = 3,
        int smoothPeriods = 3)
        => bars
            .ToBarDList()
            .CalcStoch(
                lookbackPeriods,
                signalPeriods,
                smoothPeriods, 3, 2, MaType.SMA);

    /// <summary>
    /// Calculates the Stochastic Oscillator for a series of bars with specified factors and moving average type.
    /// </summary>
    /// <param name="bars">Aggregate OHLCV price bars, time sorted.</param>
    /// <param name="lookbackPeriods">Lookback period for the oscillator.</param>
    /// <param name="signalPeriods">Signal period for the oscillator.</param>
    /// <param name="smoothPeriods">Smoothing period for the oscillator.</param>
    /// <param name="kFactor">Factor for the %K line.</param>
    /// <param name="dFactor">Factor for the %D line.</param>
    /// <param name="movingAverageType">Type of moving average to use.</param>
    /// <returns>A list of StochResult containing the oscillator values.</returns>
    public static IReadOnlyList<StochResult> ToStoch(
        this IReadOnlyList<IBar> bars,
        int lookbackPeriods,
        int signalPeriods,
        int smoothPeriods,
        double kFactor,
        double dFactor,
        MaType movingAverageType)
        => bars
            .ToBarDList()
            .CalcStoch(
                lookbackPeriods,
                signalPeriods,
                smoothPeriods,
                kFactor,
                dFactor,
                movingAverageType);

    /// <summary>
    /// Creates a buffer list for Stochastic Oscillator calculations.
    /// </summary>
    /// <param name="bars">List of bars to process.</param>
    /// <param name="lookbackPeriods">Lookback period for the oscillator.</param>
    /// <param name="signalPeriods">Signal period for the oscillator.</param>
    /// <param name="smoothPeriods">Smoothing period for the oscillator.</param>
    /// <returns>A StochList instance initialized with the provided bars.</returns>
    public static StochList ToStochList(
        this IReadOnlyList<IBar> bars,
        int lookbackPeriods = 14,
        int signalPeriods = 3,
        int smoothPeriods = 3)
        => new(lookbackPeriods, signalPeriods, smoothPeriods) { bars };

    /// <summary>
    /// Creates a buffer list for Stochastic Oscillator calculations with extended parameters.
    /// </summary>
    /// <param name="bars">List of bars to process.</param>
    /// <param name="lookbackPeriods">Lookback period for the oscillator.</param>
    /// <param name="signalPeriods">Signal period for the oscillator.</param>
    /// <param name="smoothPeriods">Smoothing period for the oscillator.</param>
    /// <param name="kFactor">Factor for the %K line.</param>
    /// <param name="dFactor">Factor for the %D line.</param>
    /// <param name="movingAverageType">Type of moving average to use.</param>
    /// <returns>A StochList instance initialized with the provided bars and extended parameters.</returns>
    public static StochList ToStochList(
        this IReadOnlyList<IBar> bars,
        int lookbackPeriods,
        int signalPeriods,
        int smoothPeriods,
        double kFactor,
        double dFactor,
        MaType movingAverageType)
        => new(lookbackPeriods, signalPeriods, smoothPeriods, kFactor, dFactor, movingAverageType) { bars };

    /// <summary>
    /// Calculates the Stochastic Oscillator for a series of bars.
    /// </summary>
    /// <param name="bars">Source list of bars.</param>
    /// <param name="lookbackPeriods">Lookback period for the oscillator.</param>
    /// <param name="signalPeriods">Signal period for the oscillator.</param>
    /// <param name="smoothPeriods">Smoothing period for the oscillator.</param>
    /// <param name="kFactor">Factor for the %K line.</param>
    /// <param name="dFactor">Factor for the %D line.</param>
    /// <param name="movingAverageType">Type of moving average to use.</param>
    /// <returns>A list of StochResult containing the oscillator values.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the operation is invalid for the current state</exception>
    internal static List<StochResult> CalcStoch(
        this List<BarD> bars,
        int lookbackPeriods,
        int signalPeriods,
        int smoothPeriods,
        double kFactor,
        double dFactor,
        MaType movingAverageType)
    {
        // check parameter arguments
        Validate(
            lookbackPeriods, signalPeriods, smoothPeriods,
            kFactor, dFactor, movingAverageType);

        // initialize
        int length = bars.Count;
        List<StochResult> results = new(length);

        double[] o = new double[length]; // %K oscillator (initial)
        double[] k = new double[length]; // %K oscillator (final)

        double prevK = double.NaN;
        double prevD = double.NaN;

        // roll through source values
        for (int i = 0; i < length; i++)
        {
            BarD q = bars[i];

            // initial %K oscillator
            if (i >= lookbackPeriods - 1)
            {
                double highHigh = double.MinValue;
                double lowLow = double.MaxValue;
                bool isViable = true;

                for (int p = i - lookbackPeriods + 1; p <= i; p++)
                {
                    BarD x = bars[p];

                    if (double.IsNaN(x.High)
                     || double.IsNaN(x.Low)
                     || double.IsNaN(x.Close))
                    {
                        isViable = false;
                        break;
                    }

                    if (x.High > highHigh)
                    {
                        highHigh = x.High;
                    }

                    if (x.Low < lowLow)
                    {
                        lowLow = x.Low;
                    }
                }

                // Calculate raw %K oscillator with boundary detection
                // to avoid floating-point precision errors at 0 and 100
                if (!isViable)
                {
                    o[i] = double.NaN;
                }
                else if (highHigh == lowLow)
                {
                    o[i] = 0d;
                }
                else if (q.Close >= highHigh)
                {
                    // Boundary detection: exact 100 when close equals or exceeds highHigh
                    o[i] = 100d;
                }
                else if (q.Close <= lowLow)
                {
                    // Boundary detection: exact 0 when close equals or falls below lowLow
                    o[i] = 0d;
                }
                else
                {
                    o[i] = 100d * (q.Close - lowLow) / (highHigh - lowLow);
                }
            }
            else
            {
                o[i] = double.NaN;
            }

            // final %K oscillator, keep original
            if (smoothPeriods <= 1)
            {
                k[i] = o[i];
            }

            // final %K oscillator, if smoothed
            else if (i >= smoothPeriods)
            {
                k[i] = double.NaN;

                switch (movingAverageType)
                {
                    // SMA case
                    case MaType.SMA:
                        double sum = 0;
                        for (int p = i - smoothPeriods + 1; p <= i; p++)
                        {
                            sum += o[p];
                        }

                        k[i] = sum / smoothPeriods;
                        break;

                    // SMMA case
                    case MaType.SMMA:
                        // re/initialize with SMA when prevK is NaN
                        // (happens at start or after NaN input values)
                        // This matches standard SMMA pattern (see Alligator, SMMA indicators)
                        if (double.IsNaN(prevK))
                        {
                            double initSum = 0;
                            for (int p = i - smoothPeriods + 1; p <= i; p++)
                            {
                                initSum += o[p];
                            }

                            prevK = initSum / smoothPeriods;
                        }

                        k[i] = ((prevK * (smoothPeriods - 1)) + o[i]) / smoothPeriods;
                        prevK = k[i];
                        break;

                    default:
                        throw new InvalidOperationException(
                            "Invalid Stochastic moving average type.");
                }
            }
            else
            {
                k[i] = double.NaN;
            }

            double oscillator = k[i];
            double signal;

            // %D signal line
            if (signalPeriods <= 1)
            {
                signal = oscillator;
            }
            else if (i >= signalPeriods)
            {
                switch (movingAverageType)
                {
                    // SMA case
                    case MaType.SMA:
                        double sum = 0;
                        for (int p = i - signalPeriods + 1; p <= i; p++)
                        {
                            sum += k[p];
                        }

                        signal = sum / signalPeriods;
                        break;

                    // SMMA case
                    case MaType.SMMA:
                        // re/initialize with SMA when prevD is NaN
                        // (happens at start or after NaN input values)
                        // This matches standard SMMA pattern (see Alligator, SMMA indicators)
                        if (double.IsNaN(prevD))
                        {
                            double initSum = 0;
                            for (int p = i - signalPeriods + 1; p <= i; p++)
                            {
                                initSum += k[p];
                            }

                            prevD = initSum / signalPeriods;
                        }

                        double d = ((prevD * (signalPeriods - 1)) + k[i]) / signalPeriods;
                        signal = d;
                        prevD = d;
                        break;

                    default:
                        throw new InvalidOperationException("Invalid Stochastic moving average type.");
                }
            }
            else
            {
                signal = double.NaN;
            }

            results.Add(new(
                Timestamp: q.Timestamp,
                Oscillator: oscillator.NaN2Null(),
                Signal: signal.NaN2Null(),
                PercentJ: ((kFactor * oscillator) - (dFactor * signal)).NaN2Null()));
        }

        return results;
    }
}
