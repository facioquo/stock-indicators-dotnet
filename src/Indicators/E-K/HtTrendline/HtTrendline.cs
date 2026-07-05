namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Hilbert Transform Instantaneous Trendline (HTL) indicator.
/// </summary>
public static partial class HtTrendline
{
    /// <summary>
    /// Converts a list of time-series values to Hilbert Transform Instantaneous Trendline (HTL) results.
    /// </summary>
    /// <param name="source">List of time-series values to transform.</param>
    /// <returns>A list of HTL results and smoothed price.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="source"/> is null.</exception>
    public static IReadOnlyList<HtlResult> ToHtTrendline(
        this IReadOnlyList<IReusable> source)
    {
        // prefer HL2 when IBar
        IReadOnlyList<IReusable> values
            = source.ToPreferredList(CandlePart.HL2);

        // initialize
        int length = values.Count;
        List<HtlResult> results = new(length);

        double[] pr = new double[length]; // price
        double[] sp = new double[length]; // smooth price
        double[] dt = new double[length]; // detrender
        double[] pd = new double[length]; // period

        double[] q1 = new double[length]; // quadrature
        double[] i1 = new double[length]; // in-phase

        double[] q2 = new double[length]; // adj. quadrature
        double[] i2 = new double[length]; // adj. in-phase

        double[] re = new double[length]; // real part
        double[] im = new double[length]; // imaginary part

        double[] sd = new double[length]; // smooth period
        double[] it = new double[length]; // instantaneous trend (raw)

        // roll through source values
        for (int i = 0; i < length; i++)
        {
            IReusable s = values[i];
            pr[i] = s.Value;

            if (i > 5)
            {
                double adj = (0.075 * pd[i - 1]) + 0.54;

                // smooth and detrender
                sp[i] = ((4 * pr[i]) + (3 * pr[i - 1]) + (2 * pr[i - 2]) + pr[i - 3]) / 10;
                dt[i] = ((0.0962 * sp[i]) + (0.5769 * sp[i - 2]) - (0.5769 * sp[i - 4]) - (0.0962 * sp[i - 6])) * adj;

                // in-phase and quadrature
                q1[i] = ((0.0962 * dt[i]) + (0.5769 * dt[i - 2]) - (0.5769 * dt[i - 4]) - (0.0962 * dt[i - 6])) * adj;
                i1[i] = dt[i - 3];

                // advance the phases by 90 degrees
                double jI = ((0.0962 * i1[i]) + (0.5769 * i1[i - 2]) - (0.5769 * i1[i - 4]) - (0.0962 * i1[i - 6])) * adj;
                double jQ = ((0.0962 * q1[i]) + (0.5769 * q1[i - 2]) - (0.5769 * q1[i - 4]) - (0.0962 * q1[i - 6])) * adj;

                // phasor addition for 3-bar averaging
                i2[i] = i1[i] - jQ;
                q2[i] = q1[i] + jI;

                i2[i] = (0.2 * i2[i]) + (0.8 * i2[i - 1]);  // smoothing it
                q2[i] = (0.2 * q2[i]) + (0.8 * q2[i - 1]);

                // homodyne discriminator
                re[i] = (i2[i] * i2[i - 1]) + (q2[i] * q2[i - 1]);
                im[i] = (i2[i] * q2[i - 1]) - (q2[i] * i2[i - 1]);

                re[i] = (0.2 * re[i]) + (0.8 * re[i - 1]);  // smoothing it
                im[i] = (0.2 * im[i]) + (0.8 * im[i - 1]);

                // calculate period
                pd[i] = im[i] != 0 && re[i] != 0
                    ? 2 * Math.PI / DeMath.Atan(im[i] / re[i])
                    : 0d;

                // adjust period to thresholds
                pd[i] = pd[i] > 1.5 * pd[i - 1] ? 1.5 * pd[i - 1] : pd[i];
                pd[i] = pd[i] < 0.67 * pd[i - 1] ? 0.67 * pd[i - 1] : pd[i];
                pd[i] = pd[i] < 6d ? 6d : pd[i];
                pd[i] = pd[i] > 50d ? 50d : pd[i];

                // smooth the period
                pd[i] = (0.2 * pd[i]) + (0.8 * pd[i - 1]);
                sd[i] = (0.33 * pd[i]) + (0.67 * sd[i - 1]);

                // smooth dominant cycle period
                int dcPeriods = (int)(double.IsNaN(sd[i]) ? 0 : sd[i] + 0.5);
                double sumPr = 0;
                for (int d = i - dcPeriods + 1; d <= i; d++)
                {
                    if (d >= 0)
                    {
                        sumPr += pr[d];
                    }

                    // handle insufficient lookback bars (trim scope)
                    else
                    {
                        dcPeriods--;
                    }
                }

                it[i] = dcPeriods > 0 ? sumPr / dcPeriods : pr[i];

                // final indicators
                results.Add(new(

                    Timestamp: s.Timestamp,
                    DcPeriods: dcPeriods > 0 ? dcPeriods : null,

                    Trendline: i >= 11 // 12th bar
                      ? (((4 * it[i]) + (3 * it[i - 1]) + (2 * it[i - 2]) + it[i - 3]) / 10d).NaN2Null()
                      : pr[i].NaN2Null(),

                    SmoothPrice: sp[i].NaN2Null()));
            }

            // initialization period
            else
            {
                results.Add(new(
                    Timestamp: s.Timestamp,
                    DcPeriods: null,
                    Trendline: pr[i].NaN2Null(),
                    SmoothPrice: null));

                pd[i] = 0;
                sp[i] = 0;
                dt[i] = 0;

                i1[i] = 0;
                q1[i] = 0;
                i2[i] = 0;
                q2[i] = 0;

                re[i] = 0;
                im[i] = 0;

                sd[i] = 0;
            }
        }

        return results;
    }
}
