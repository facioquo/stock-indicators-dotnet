namespace FacioQuo.Stock.Indicators;

/// <summary>
/// MESA Adaptive Moving Average (MAMA) for a series of bars indicator.
/// </summary>
public static partial class Mama
{
    /// <summary>
    /// Converts a list of source values to MESA Adaptive Moving Average (MAMA) results.
    /// </summary>
    /// <param name="source">List of source values to transform.</param>
    /// <param name="fastLimit">Fast limit for the MAMA calculation. Default is 0.5.</param>
    /// <param name="slowLimit">Slow limit for the MAMA calculation. Default is 0.05.</param>
    /// <returns>A list of MESA Adaptive Moving Average (MAMA) results.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the fast or slow limit is out of range.</exception>
    public static IReadOnlyList<MamaResult> ToMama(
        this IReadOnlyList<IReusable> source,
        double fastLimit = 0.5,
        double slowLimit = 0.05)
    {
        // check parameter arguments
        Validate(fastLimit, slowLimit);

        // prefer HL2 when IBar
        IReadOnlyList<IReusable> values
            = source.ToPreferredList(CandlePart.HL2);

        // initialize
        int length = values.Count;
        List<MamaResult> results = new(length);

        double prevMama = double.NaN;
        double prevFama = double.NaN;

        double[] pr = new double[length]; // price
        double[] sm = new double[length]; // smooth
        double[] dt = new double[length]; // detrender
        double[] pd = new double[length]; // period

        double[] q1 = new double[length]; // quadrature
        double[] i1 = new double[length]; // in-phase

        double[] q2 = new double[length]; // adj. quadrature
        double[] i2 = new double[length]; // adj. in-phase

        double[] re = new double[length]; // real part
        double[] im = new double[length]; // imaginary part

        double[] ph = new double[length]; // phase

        // roll through source values
        for (int i = 0; i < length; i++)
        {
            IReusable s = values[i];
            pr[i] = s.Value;

            // skip incalculable periods
            if (i < 5)
            {
                results.Add(new(s.Timestamp));
                continue;
            }

            double mama;
            double fama;

            // initialization
            if (double.IsNaN(prevMama))
            {
                double sum = 0;
                for (int p = i - 5; p <= i; p++)
                {
                    pd[p] = 0;
                    sm[p] = 0;
                    dt[p] = 0;

                    i1[p] = 0;
                    q1[p] = 0;
                    i2[p] = 0;
                    q2[p] = 0;

                    re[p] = 0;
                    im[p] = 0;

                    ph[p] = 0;

                    sum += pr[p];
                }

                mama = fama = sum / 6;
            }

            // normal MAMA
            else
            {
                double adj = (0.075 * pd[i - 1]) + 0.54;

                // smooth and detrender
                sm[i] = ((4 * pr[i]) + (3 * pr[i - 1]) + (2 * pr[i - 2]) + pr[i - 3]) / 10;
                dt[i] = ((0.0962 * sm[i]) + (0.5769 * sm[i - 2]) - (0.5769 * sm[i - 4]) - (0.0962 * sm[i - 6])) * adj;

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
                pd[i] = pd[i] < 6 ? 6 : pd[i];
                pd[i] = pd[i] > 50 ? 50 : pd[i];

                // smooth the period
                pd[i] = (0.2 * pd[i]) + (0.8 * pd[i - 1]);

                // determine phase position
                ph[i] = i1[i] != 0 ? DeMath.Atan(q1[i] / i1[i]) * 180d / Math.PI : 0d;

                // change in phase
                double delta = Math.Max(ph[i - 1] - ph[i], 1);

                // adaptive alpha value
                double alpha = Math.Max(fastLimit / delta, slowLimit);

                // final indicators
                mama = (alpha * pr[i]) + ((1d - alpha) * prevMama);
                fama = (0.5 * alpha * mama) + ((1d - (0.5 * alpha)) * prevFama);
            }

            results.Add(new MamaResult(
                Timestamp: s.Timestamp,
                Mama: mama.NaN2Null(),
                Fama: fama.NaN2Null()));

            prevMama = mama;
            prevFama = fama;
        }

        return results;
    }
}
