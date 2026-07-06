namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Provides methods to convert a list of bars to a ZigZag series.
/// </summary>
public static partial class ZigZag
{
    /// <summary>
    /// Converts a list of bars to a ZigZag series.
    /// </summary>
    /// <param name="bars">Aggregate OHLCV price bars, time sorted.</param>
    /// <param name="endType">Type of end to use (Close or HighLow).</param>
    /// <param name="percentChange">Percentage change threshold for ZigZag points.</param>
    /// <returns>A list of ZigZag results.</returns>
    public static IReadOnlyList<ZigZagResult> ToZigZag(
        this IReadOnlyList<IBar> bars,
        EndType endType = EndType.Close,
        decimal percentChange = 5
    )
    {
        // check parameter arguments
        ArgumentNullException.ThrowIfNull(bars);
        Validate(percentChange);

        // initialize
        int length = bars.Count;
        List<ZigZagResult> results = new(length);

        if (length == 0)
        {
            return results;
        }

        IBar q0 = bars[0];

        ZigZagEval eval = GetZigZagEval(endType, 1, q0);
        decimal changeThreshold = percentChange / 100m;

        ZigZagPoint lastPoint = new() {
            Index = eval.Index,
            Value = q0.Close,
            PointType = "U"
        };

        ZigZagPoint lastHighPoint = new() {
            Index = eval.Index,
            Value = eval.High,
            PointType = "H"
        };

        ZigZagPoint lastLowPoint = new() {
            Index = eval.Index,
            Value = eval.Low,
            PointType = "L"
        };

        // roll through source values, to find initial trend
        for (int i = 0; i < length; i++)
        {
            IBar q = bars[i];
            int index = i + 1;

            eval = GetZigZagEval(endType, index, q);

            decimal? changeUp = lastLowPoint.Value == 0
                ? null
                : (eval.High - lastLowPoint.Value)
                / lastLowPoint.Value;

            decimal? changeDn = lastHighPoint.Value == 0
                ? null
                : (lastHighPoint.Value - eval.Low)
                / lastHighPoint.Value;

            if (changeUp >= changeThreshold && changeUp > changeDn)
            {
                lastPoint.Index = lastLowPoint.Index;
                lastPoint.Value = lastLowPoint.Value;
                lastPoint.PointType = lastLowPoint.PointType;
                break;
            }

            if (changeDn >= changeThreshold && changeDn > changeUp)
            {
                lastPoint.Index = lastHighPoint.Index;
                lastPoint.Value = lastHighPoint.Value;
                lastPoint.PointType = lastHighPoint.PointType;
                break;
            }
        }

        // add first point to results
        ZigZagResult firstResult = new(q0.Timestamp);
        results.Add(firstResult);

        // find and draw lines
        while (lastPoint.Index < length)
        {
            ZigZagPoint nextPoint = EvaluateNextPoint(
                bars, endType, changeThreshold, lastPoint);

            string lastDirection = lastPoint.PointType;

            // draw line (and reset last point)
            DrawZigZagLine(results, bars, lastPoint, nextPoint);

            // draw retrace line (and reset last high/low point)
            DrawRetraceLine(results, lastDirection, lastLowPoint,
                lastHighPoint, nextPoint);
        }

        return results;
    }

    /// <summary>
    /// Evaluates the next ZigZag point.
    /// </summary>
    /// <param name="barsList">List of bars.</param>
    /// <param name="endType">Type of end to use (Close or HighLow).</param>
    /// <param name="changeThreshold">Percentage change threshold for ZigZag points.</param>
    /// <param name="lastPoint">Last ZigZag point.</param>
    /// <returns>Next ZigZag point.</returns>
    internal static ZigZagPoint EvaluateNextPoint(
        IReadOnlyList<IBar> barsList,
        EndType endType,
        decimal changeThreshold,
        ZigZagPoint lastPoint)
    {
        // initialize
        bool trendUp = lastPoint.PointType == "L";

        // candidate
        ZigZagPoint extremePoint = new() {
            Index = lastPoint.Index,
            Value = lastPoint.Value,
            PointType = trendUp ? "H" : "L"
        };

        // find extreme point before reversal point
        for (int i = lastPoint.Index; i < barsList.Count; i++)
        {
            IBar q = barsList[i];
            int index = i + 1;
            decimal? change;

            ZigZagEval eval = GetZigZagEval(endType, index, q);

            // reset extreme point
            if (trendUp)
            {
                if (eval.High >= extremePoint.Value)
                {
                    extremePoint.Index = eval.Index;
                    extremePoint.Value = eval.High;
                    change = 0;
                }
                else
                {
                    change = extremePoint.Value == 0
                        ? null
                        : (extremePoint.Value - eval.Low)
                        / extremePoint.Value;
                }
            }
            else if (eval.Low <= extremePoint.Value)
            {
                extremePoint.Index = eval.Index;
                extremePoint.Value = eval.Low;
                change = 0;
            }
            else
            {
                change = extremePoint.Value == 0
                    ? null
                    : (eval.High - extremePoint.Value)
                    / extremePoint.Value;
            }

            // return extreme point when reversion threshold met
            if (change >= changeThreshold)
            {
                return extremePoint;
            }

            // add last unconfirmed end point
            if (index == barsList.Count)
            {
                extremePoint.Index = index;
                extremePoint.Value = trendUp ? eval.High : eval.Low;
                extremePoint.PointType = null;
            }
        }

        return extremePoint;
    }

    /// <summary>
    /// Draws a ZigZag line between two points.
    /// </summary>
    /// <param name="results">List of ZigZag results.</param>
    /// <param name="bars">Aggregate OHLCV price bars, time sorted.</param>
    /// <param name="lastPoint">Last ZigZag point.</param>
    /// <param name="nextPoint">Next ZigZag point.</param>
    private static void DrawZigZagLine(
        List<ZigZagResult> results, IReadOnlyList<IBar> bars,
        ZigZagPoint lastPoint, ZigZagPoint nextPoint)
    {
        if (nextPoint.Index != lastPoint.Index)
        {
            decimal? increment
                = (nextPoint.Value - lastPoint.Value)
                / (nextPoint.Index - lastPoint.Index);

            // add new line segment
            for (int i = lastPoint.Index; i < nextPoint.Index; i++)
            {
                IBar q = bars[i];
                int index = i + 1;

                ZigZagResult result = new(
                    Timestamp: q.Timestamp,
                    ZigZag: lastPoint.Index != 1 || index == nextPoint.Index
                        ? lastPoint.Value + (increment * (index - lastPoint.Index))
                        : null,
                    PointType: index == nextPoint.Index
                        ? nextPoint.PointType
                        : null);

                results.Add(result);
            }
        }

        // reset lastpoint
        lastPoint.Index = nextPoint.Index;
        lastPoint.Value = nextPoint.Value;
        lastPoint.PointType = nextPoint.PointType;
    }

    /// <summary>
    /// Draws a retrace line between two points.
    /// </summary>
    /// <param name="results">List of ZigZag results.</param>
    /// <param name="lastDirection">Direction of the last ZigZag point.</param>
    /// <param name="lastLowPoint">Last low ZigZag point.</param>
    /// <param name="lastHighPoint">Last high ZigZag point.</param>
    /// <param name="nextPoint">Next ZigZag point.</param>
    private static void DrawRetraceLine(
        List<ZigZagResult> results,
        string lastDirection,
        ZigZagPoint lastLowPoint,
        ZigZagPoint lastHighPoint,
        ZigZagPoint nextPoint)
    {
        ZigZagPoint priorPoint = new();

        switch (lastDirection)
        {
            // handle type and reset last point
            case "L":
                priorPoint.Index = lastHighPoint.Index;
                priorPoint.Value = lastHighPoint.Value;

                lastHighPoint.Index = nextPoint.Index;
                lastHighPoint.Value = nextPoint.Value;
                break;

            // low line
            case "H":
                priorPoint.Index = lastLowPoint.Index;
                priorPoint.Value = lastLowPoint.Value;

                lastLowPoint.Index = nextPoint.Index;
                lastLowPoint.Value = nextPoint.Value;
                break;

            default:
                // no action needed for other cases
                break;
        }

        // nothing to draw cases
        if (
            lastDirection == "U" // first line skipped, single line
         || priorPoint.Index == 1 // first line skipped, normal case
         || nextPoint.Index == priorPoint.Index) // no span
        {
            return;
        }

        // narrow to period
        decimal? increment
            = (nextPoint.Value - priorPoint.Value)
            / (nextPoint.Index - priorPoint.Index);

        // add new line segment
        for (int i = priorPoint.Index - 1; i < nextPoint.Index; i++)
        {
            ZigZagResult r = results[i];
            int index = i + 1;

            switch (lastDirection)
            {
                // high line
                case "L":

                    decimal? retraceHigh
                        = priorPoint.Value
                        + (increment * (index - priorPoint.Index));

                    results[i] = r with { RetraceHigh = retraceHigh };
                    break;

                // low line
                case "H":

                    decimal? retraceLow
                        = priorPoint.Value
                        + (increment * (index - priorPoint.Index));

                    results[i] = r with { RetraceLow = retraceLow };
                    break;

                default:
                    // no action needed for other cases
                    break;
            }
        }
    }

    /// <summary>
    /// Gets the ZigZag evaluation for a bar.
    /// </summary>
    /// <param name="endType">Type of end to use (Close or HighLow).</param>
    /// <param name="index">Index of the bar.</param>
    /// <param name="q">Bar.</param>
    /// <returns>ZigZag evaluation.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when a parameter is out of the valid range</exception>
    internal static ZigZagEval GetZigZagEval(
        EndType endType,
        int index,
        IBar q)
    {
        ZigZagEval eval = new() {
            Index = index
        };

        // consider `type`
        switch (endType)
        {
            case EndType.Close:

                eval.Low = q.Close;
                eval.High = q.Close;
                break;

            case EndType.HighLow:

                eval.Low = q.Low;
                eval.High = q.High;
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(endType));
        }

        return eval;
    }

    /// <summary>
    /// Calculates interpolated ZigZag value between two points.
    /// </summary>
    /// <param name="fromPoint">Starting ZigZag point.</param>
    /// <param name="toPoint">Ending ZigZag point.</param>
    /// <param name="currentIndex">Current index for interpolation.</param>
    /// <returns>Interpolated ZigZag value.</returns>
    internal static decimal? CalculateInterpolatedValue(
        ZigZagPoint fromPoint,
        ZigZagPoint toPoint,
        int currentIndex)
    {
        if (fromPoint.Index == 0 || currentIndex <= fromPoint.Index)
        {
            return null;
        }

        if (toPoint.Index == fromPoint.Index)
        {
            return fromPoint.Value;
        }

        decimal? increment = (toPoint.Value - fromPoint.Value)
                           / (toPoint.Index - fromPoint.Index);

        return fromPoint.Value + (increment * (currentIndex - fromPoint.Index));
    }
}
