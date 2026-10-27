using FacioQuo.Stock.Indicators;

namespace Custom.Stock.Indicators
{

    /// <summary>
    /// Represents alternative Pivot Points with custom fourth- and fifth-level bars added.
    /// </summary>
    [Serializable]
    public record ExtendedPivotPointsResult(
        DateTime Timestamp,
        double? R5 = null,
        double? R4 = null,
        double? R3 = null,
        double? R2 = null,
        double? R1 = null,
        double? PP = null,
        double? S1 = null,
        double? S2 = null,
        double? S3 = null,
        double? S4 = null,
        double? S5 = null) : IReusable
    {
        double IReusable.Value => PP.Null2NaN();
    }

    public static partial class CustomIndicators
    {
        /// <summary>
        /// Calculates alternative Pivot Points with custom fourth- and fifth-level bars added.
        /// </summary>
        /// <param name="bars">Historical price bars in ascending order.</param>
        /// <param name="windowSize">Calendar interval used to form each completed pivot window.</param>
        /// <returns>Extended Pivot Points results aligned to the input.</returns>
        public static IReadOnlyList<ExtendedPivotPointsResult> ToExtendedPivotPoints(
            this IReadOnlyList<IBar> bars,
            BarInterval windowSize = BarInterval.Month)
        {
            ArgumentNullException.ThrowIfNull(bars);

            IReadOnlyList<PivotPointsResult> pivots = bars.ToPivotPoints(
                windowSize,
                PivotPointType.Standard);

            ExtendedPivotPointsResult[] results = new ExtendedPivotPointsResult[pivots.Count];

            for (int i = 0; i < pivots.Count; i++)
            {
                PivotPointsResult p = pivots[i];

                results[i] = p is { PP: double pp, R1: double r1, S1: double s1 }
                    ? new ExtendedPivotPointsResult(
                        p.Timestamp,
                        R5: (4 * r1) - s1 - (2 * pp),
                        R4: (3 * r1) - s1 - pp,
                        R3: p.R3,
                        R2: p.R2,
                        R1: r1,
                        PP: pp,
                        S1: s1,
                        S2: p.S2,
                        S3: p.S3,
                        S4: (3 * s1) - r1 - pp,
                        S5: (4 * s1) - r1 - (2 * pp))
                    : new ExtendedPivotPointsResult(p.Timestamp);
            }

            return results;
        }
    }

}
