namespace FacioQuo.Stock.Indicators;

#pragma warning disable IDE0072 // Missing cases in switch statement

/// <summary>
/// pivot points from a series of bars indicator.
/// </summary>
public static partial class PivotPoints
{
    /// <summary>
    /// Converts a series of bars to pivot points.
    /// </summary>
    /// <param name="bars">Series of bars.</param>
    /// <param name="windowSize">Size of the window for pivot point calculation.</param>
    /// <param name="pointType">Type of pivot point calculation to use.</param>
    /// <returns>A list of pivot point results.</returns>
    public static IReadOnlyList<PivotPointsResult> ToPivotPoints(
        this IReadOnlyList<IBar> bars,
        BarInterval windowSize = BarInterval.Month,
        PivotPointType pointType = PivotPointType.Standard)
    {
        ArgumentNullException.ThrowIfNull(bars);

        // initialize
        int length = bars.Count;
        List<PivotPointsResult> results = new(length);

        WindowPoint windowPoint = new();

        if (length == 0)
        {
            return results;
        }

        IBar h0 = bars[0];

        int windowId = GetWindowNumber(h0.Timestamp, windowSize);

        bool firstWindow = true;

        double windowHigh = (double)h0.High;
        double windowLow = (double)h0.Low;
        double windowOpen = (double)h0.Open;
        double windowClose = (double)h0.Close;

        // roll through source values
        for (int i = 0; i < length; i++)
        {
            IBar q = bars[i];

            // new window evaluation
            int windowEval = GetWindowNumber(q.Timestamp, windowSize);

            if (windowEval != windowId)
            {
                windowId = windowEval;
                firstWindow = false;

                // set new levels
                if (pointType == PivotPointType.Woodie)
                {
                    windowOpen = (double)q.Open;
                }

                windowPoint = GetPivotPoint(
                    pointType, windowOpen, windowHigh, windowLow, windowClose);

                // reset window min/max thresholds
                windowOpen = (double)q.Open;
                windowHigh = (double)q.High;
                windowLow = (double)q.Low;
            }

            // add levels
            PivotPointsResult r
                = !firstWindow
                ? new() {

                    Timestamp = q.Timestamp,

                    // pivot point
                    PP = windowPoint.PP,

                    // support
                    S1 = windowPoint.S1,
                    S2 = windowPoint.S2,
                    S3 = windowPoint.S3,
                    S4 = windowPoint.S4,

                    // resistance
                    R1 = windowPoint.R1,
                    R2 = windowPoint.R2,
                    R3 = windowPoint.R3,
                    R4 = windowPoint.R4
                }
                : new PivotPointsResult {
                    Timestamp = q.Timestamp
                };

            results.Add(r);

            // capture window threholds (for next iteration)
            windowHigh = (double)q.High > windowHigh ? (double)q.High : windowHigh;
            windowLow = (double)q.Low < windowLow ? (double)q.Low : windowLow;
            windowClose = (double)q.Close;
        }

        return results;
    }

    /// <summary>
    /// Gets the pivot point based on the specified type and price values.
    /// </summary>
    /// <param name="pointType">Type of pivot point calculation to use.</param>
    /// <param name="open">Opening price.</param>
    /// <param name="high">Highest price.</param>
    /// <param name="low">Lowest price.</param>
    /// <param name="close">Closing price.</param>
    /// <returns>A WindowPoint object containing the calculated pivot points.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when a parameter is out of the valid range</exception>
    internal static WindowPoint GetPivotPoint(
        PivotPointType pointType, double open, double high, double low, double close)
        => pointType switch {

            PivotPointType.Standard => GetPivotPointStandard(high, low, close),
            PivotPointType.Camarilla => GetPivotPointCamarilla(high, low, close),
            PivotPointType.Demark => GetPivotPointDemark(open, high, low, close),
            PivotPointType.Fibonacci => GetPivotPointFibonacci(high, low, close),
            PivotPointType.Woodie => GetPivotPointWoodie(open, high, low),

            _ => throw new ArgumentOutOfRangeException(
                nameof(pointType), pointType, "Invalid pointType provided.")
        };

    /// <summary>
    /// Gets the window number based on the date and window size.
    /// </summary>
    /// <param name="d">Date.</param>
    /// <param name="windowSize">Size of the window.</param>
    /// <returns>Window number.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when a parameter is out of the valid range</exception>
    internal static int GetWindowNumber(DateTime d, BarInterval windowSize)

        => windowSize switch {

            BarInterval.Month => d.Month,

            BarInterval.Week => calendar.GetWeekOfYear(
                d, calendarWeekRule, firstDayOfWeek),

            BarInterval.Day => d.Day,
            BarInterval.OneHour => d.Hour,

            _ => throw new ArgumentOutOfRangeException(
                nameof(windowSize), windowSize,
                string.Format(
                    invariantCulture,
                    "Pivot Points does not support BarInterval of {0}. "
                    + "See documentation for valid options.",
                    Enum.GetName(windowSize)))
        };

    /// <summary>
    /// Gets the standard pivot point based on the high, low, and close prices.
    /// </summary>
    /// <param name="high">Highest price.</param>
    /// <param name="low">Lowest price.</param>
    /// <param name="close">Closing price.</param>
    /// <returns>A WindowPoint object containing the calculated pivot points.</returns>
    private static WindowPoint GetPivotPointStandard(
        double high, double low, double close)
    {
        double pp = (high + low + close) / 3d;

        return new() {
            PP = pp,
            S1 = (pp * 2) - high,
            S2 = pp - (high - low),
            S3 = low - (2 * (high - pp)),
            R1 = (pp * 2) - low,
            R2 = pp + (high - low),
            R3 = high + (2 * (pp - low))
        };
    }

    /// <summary>
    /// Gets the Camarilla pivot point based on the high, low, and close prices.
    /// </summary>
    /// <param name="high">Highest price.</param>
    /// <param name="low">Lowest price.</param>
    /// <param name="close">Closing price.</param>
    /// <returns>A WindowPoint object containing the calculated pivot points.</returns>
    private static WindowPoint GetPivotPointCamarilla(
        double high, double low, double close)
        => new() {
            PP = close,
            S1 = close - (1.1d / 12 * (high - low)),
            S2 = close - (1.1d / 6 * (high - low)),
            S3 = close - (1.1d / 4 * (high - low)),
            S4 = close - (1.1d / 2 * (high - low)),
            R1 = close + (1.1d / 12 * (high - low)),
            R2 = close + (1.1d / 6 * (high - low)),
            R3 = close + (1.1d / 4 * (high - low)),
            R4 = close + (1.1d / 2 * (high - low))
        };

    /// <summary>
    /// Gets the Demark pivot point based on the open, high, low, and close prices.
    /// </summary>
    /// <param name="open">Opening price.</param>
    /// <param name="high">Highest price.</param>
    /// <param name="low">Lowest price.</param>
    /// <param name="close">Closing price.</param>
    /// <returns>A WindowPoint object containing the calculated pivot points.</returns>
    internal static WindowPoint GetPivotPointDemark(
        double open, double high, double low, double close)
    {
        double x = close < open
            ? high + (2 * low) + close
            : close > open
            ? (2 * high) + low + close
            : high + low + (2 * close);

        return new() {
            PP = x / 4d,
            S1 = (x / 2d) - high,
            R1 = (x / 2d) - low
        };
    }

    /// <summary>
    /// Gets the Fibonacci pivot point based on the high, low, and close prices.
    /// </summary>
    /// <param name="high">Highest price.</param>
    /// <param name="low">Lowest price.</param>
    /// <param name="close">Closing price.</param>
    /// <returns>A WindowPoint object containing the calculated pivot points.</returns>
    private static WindowPoint GetPivotPointFibonacci(
        double high, double low, double close)
    {
        double pp = (high + low + close) / 3d;

        return new() {
            PP = pp,
            S1 = pp - (0.382d * (high - low)),
            S2 = pp - (0.618d * (high - low)),
            S3 = pp - (1.000d * (high - low)),
            R1 = pp + (0.382d * (high - low)),
            R2 = pp + (0.618d * (high - low)),
            R3 = pp + (1.000d * (high - low))
        };
    }

    /// <summary>
    /// Gets the Woodie pivot point based on the current open, high, and low prices.
    /// </summary>
    /// <param name="currentOpen">Current opening price.</param>
    /// <param name="high">Highest price.</param>
    /// <param name="low">Lowest price.</param>
    /// <returns>A WindowPoint object containing the calculated pivot points.</returns>
    private static WindowPoint GetPivotPointWoodie(
        double currentOpen, double high, double low)
    {
        double pp = (high + low + (2 * currentOpen)) / 4d;

        return new() {
            PP = pp,
            S1 = (pp * 2) - high,
            S2 = pp - high + low,
            S3 = low - (2 * (high - pp)),
            R1 = (pp * 2) - low,
            R2 = pp + high - low,
            R3 = high + (2 * (pp - low))
        };
    }
}
