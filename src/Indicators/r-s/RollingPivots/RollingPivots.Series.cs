namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Rolling Pivot Points indicator.
/// </summary>
public static partial class RollingPivots
{
    /// <summary>
    /// Converts a list of bars to a list of Rolling Pivot Points results.
    /// </summary>
    /// <param name="bars">Aggregate OHLCV price bars, time sorted.</param>
    /// <param name="windowPeriods">Number of periods in the rolling window.</param>
    /// <param name="offsetPeriods">Number of periods to offset the window.</param>
    /// <param name="pointType">Type of pivot point calculation to use.</param>
    /// <returns>A list of Rolling Pivot Points results.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the bars are null.</exception>
    public static IReadOnlyList<RollingPivotsResult> ToRollingPivots(
        this IReadOnlyList<IBar> bars,
        int windowPeriods = 20,
        int offsetPeriods = 0,
        PivotPointType pointType = PivotPointType.Standard)
    {
        // check parameter arguments
        ArgumentNullException.ThrowIfNull(bars);
        Validate(windowPeriods, offsetPeriods);

        // initialize
        int length = bars.Count;
        List<RollingPivotsResult> results = new(length);

        // roll through source values
        for (int i = 0; i < length; i++)
        {
            IBar q = bars[i];

            RollingPivotsResult r;

            if (i >= windowPeriods + offsetPeriods)
            {
                // window values
                int s = i - windowPeriods - offsetPeriods;
                IBar hi = bars[s];

                double windowHigh = (double)hi.High;
                double windowLow = (double)hi.Low;
                double windowClose = (double)bars[i - offsetPeriods - 1].Close;

                for (int p = s; p <= i - offsetPeriods - 1; p++)
                {
                    IBar d = bars[p];
                    windowHigh = (double)d.High > windowHigh ? (double)d.High : windowHigh;
                    windowLow = (double)d.Low < windowLow ? (double)d.Low : windowLow;
                }

                // pivot points
                WindowPoint wp = PivotPoints.GetPivotPoint(
                        pointType, (double)q.Open, windowHigh, windowLow, windowClose);

                r = new() {

                    Timestamp = q.Timestamp,

                    // pivot point
                    PP = wp.PP,

                    // support
                    S1 = wp.S1,
                    S2 = wp.S2,
                    S3 = wp.S3,
                    S4 = wp.S4,

                    // resistance
                    R1 = wp.R1,
                    R2 = wp.R2,
                    R3 = wp.R3,
                    R4 = wp.R4
                };
            }
            else
            {
                r = new() { Timestamp = q.Timestamp };
            }

            results.Add(r);
        }

        return results;
    }
}
