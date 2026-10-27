using FacioQuo.Stock.Indicators;

namespace Custom.Stock.Indicators
{

    /// <summary>
    /// Represents alternative Pivot Points with custom fourth- and fifth-level bars added.
    /// </summary>
    [Serializable]
    public record ExtendedPivotPointsResult : PivotPointsResult
    {
        // Reuse the library's pivot fields; R5 and S5 extend its result shape.
        public double? R5 { get; init; }
        public double? S5 { get; init; }
    }

    public static partial class CustomIndicators
    {
        /// <summary>
        /// Calculates alternative Pivot Points with custom fourth- and fifth-level bars added.
        /// </summary>
        /// <param name="bars">Historical price bars in ascending order.</param>
        /// <param name="windowSize">Calendar interval used to form each completed pivot window.</param>
        /// <returns>Pivot Points results with custom fourth and fifth levels, aligned to the input.</returns>
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
                    ? new ExtendedPivotPointsResult
                    {
                        Timestamp = p.Timestamp,
                        PP = pp,
                        R1 = r1,
                        R2 = p.R2,
                        R3 = p.R3,
                        R4 = (3 * r1) - s1 - pp,
                        S1 = s1,
                        S2 = p.S2,
                        S3 = p.S3,
                        S4 = (3 * s1) - r1 - pp,
                        R5 = (4 * r1) - s1 - (2 * pp),
                        S5 = (4 * s1) - r1 - (2 * pp)
                    }
                    : new ExtendedPivotPointsResult { Timestamp = p.Timestamp };
            }

            return results;
        }
    }

}
