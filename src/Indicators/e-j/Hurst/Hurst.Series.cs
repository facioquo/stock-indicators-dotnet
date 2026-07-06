namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Hurst Exponent indicator.
/// </summary>
public static partial class Hurst
{
    /// <summary>
    /// Converts a list of time-series values to Hurst Exponent results.
    /// </summary>
    /// <param name="source">List of time-series values to transform.</param>
    /// <param name="lookbackPeriods">Number of periods to look back for the calculation.</param>
    /// <returns>A list of Hurst Exponent results.</returns>
    public static IReadOnlyList<HurstResult> ToHurst(
        this IReadOnlyList<IReusable> source,
        int lookbackPeriods = 100)
    {
        // check parameter arguments
        ArgumentNullException.ThrowIfNull(source);
        Validate(lookbackPeriods);

        // initialize
        int length = source.Count;
        List<HurstResult> results = new(length);

        // depends only on lookbackPeriods, not on bar values
        double[] alCorrections = PrecomputeAlCorrections(lookbackPeriods);

        // roll through source values
        for (int i = 0; i < length; i++)
        {
            IReusable s = source[i];
            double? h = null;
            double? hAl = null;

            if (i + 1 > lookbackPeriods)
            {
                // get evaluation batch
                double[] values = new double[lookbackPeriods];

                int x = 0;
                double l = source[i - lookbackPeriods].Value;

                for (int p = i + 1 - lookbackPeriods; p <= i; p++)
                {
                    IReusable ps = source[p];

                    // log returns require strictly positive prices on both ends
                    values[x] = (l > 0 && ps.Value > 0) ? DeMath.Log(ps.Value / l) : double.NaN;

                    l = ps.Value;
                    x++;
                }

                // calculate hurst exponent
                (double rawH, double correctedH) = CalcHurstWindow(values, alCorrections);
                h = rawH.NaN2Null();
                hAl = correctedH.NaN2Null();
            }

            results.Add(new(
                Timestamp: s.Timestamp,
                HurstExponent: h,
                HurstExponentAL: hAl));
        }

        return results;
    }

    internal static (double H, double HurstExponentAL) CalcHurstWindow(
        double[] values,
        double[] alCorrections)
    {
        int totalSize = values.Length;
        int maxChunks = 0;
        int setQty = 0;

        // determine max chunk quantity so chunks are
        // not smaller than 8 observations
        // and not to exceed 32 total chunks
        for (int chunkQty = 1; chunkQty <= 32; chunkQty *= 2)
        {
            if (totalSize / chunkQty >= 8)
            {
                maxChunks = chunkQty;
                setQty++;
            }
            else
            {
                break;
            }
        }

        // initialize result sets
        double[] logRs = new double[setQty];
        double[] logRsAL = new double[setQty];
        double[] logSize = new double[setQty];
        int setNum = 0;

        // roll through sets
        for (int chunkQty = 1; chunkQty <= maxChunks; chunkQty *= 2)
        {
            // initialize set and chunks
            int chunkSize = totalSize / chunkQty;
            double sumChunkRs = 0;

            // starting index position used to skip
            // observations to enforce same-sized chunks
            int startIndex = totalSize - (chunkSize * chunkQty);

            // analyze chunks in set
            for (int chunkNum = 1; chunkNum <= chunkQty; chunkNum++)
            {
                // chunk mean
                double sum = 0;
                for (int i = startIndex; i < startIndex + chunkSize; i++)
                {
                    sum += values[i];
                }

                double chunkMean = sum / chunkSize;

                // chunk mean diff
                double sumY = 0;
                double sumSq = 0;
                double maxY = double.MinValue;
                double minY = double.MaxValue;

                for (int i = startIndex; i < startIndex + chunkSize; i++)
                {
                    double y = values[i] - chunkMean;
                    sumY += y;
                    minY = sumY < minY ? sumY : minY;
                    maxY = sumY > maxY ? sumY : maxY;

                    sumSq += y * y;
                }

                // chunk rescaled range
                double r = maxY - minY;
                double s = Math.Sqrt(sumSq / chunkSize);
                double rs = s != 0 ? r / s : double.NaN;

                sumChunkRs += rs;

                // increment starting index
                startIndex += chunkSize;
            }

            // average R/S for this chunk size
            double avgRs = sumChunkRs / chunkQty;

            // Anis-Lloyd corrected R/S:
            // RS_corrected = avgRs - E[R/S]_AL + √(π·n/2)
            // The bias-correction term (√(π·n/2) - E[R/S]_AL) is precomputed
            // per chunk size since it depends only on n.
            double rsAL = avgRs + alCorrections[setNum];

            // set results
            logSize[setNum] = DeMath.Log10(chunkSize);
            logRs[setNum] = avgRs > 0
                ? DeMath.Log10(avgRs)
                : double.NaN;

            // NaN-guards: rsAL is non-negative for any finite avgRs (the
            // bias-correction term is positive across all chunk sizes used),
            // so this branch fires only when avgRs is NaN.
            logRsAL[setNum] = rsAL > 0
                ? DeMath.Log10(rsAL)
                : double.NaN;

            // increment set
            setNum++;
        }

        // hurst exponents: raw and anis-lloyd corrected
        return (
            Numerical.Slope(logSize, logRs),
            Numerical.Slope(logSize, logRsAL));
    }

    internal static double[] PrecomputeAlCorrections(int totalSize)
    {
        int setQty = 0;
        int maxChunks = 0;
        for (int chunkQty = 1; chunkQty <= 32; chunkQty *= 2)
        {
            if (totalSize / chunkQty >= 8)
            {
                maxChunks = chunkQty;
                setQty++;
            }
            else
            {
                break;
            }
        }

        double[] corrections = new double[setQty];
        int setIdx = 0;
        for (int chunkQty = 1; chunkQty <= maxChunks; chunkQty *= 2)
        {
            int chunkSize = totalSize / chunkQty;
            double eRsAsymptotic = Math.Sqrt(Math.PI * chunkSize / 2.0);
            corrections[setIdx++] = eRsAsymptotic - HurstExpectedRs(chunkSize);
        }

        return corrections;
    }

    // Anis-Lloyd expected R/S for a random series of length n
    // Reference: Anis and Lloyd (1976), Peters (1994)
    private static double HurstExpectedRs(int n)
    {
        // inner sum: sum(j=1..n-1, sqrt((n-j)/j))
        double innerSum = 0;
        for (int j = 1; j < n; j++)
        {
            innerSum += Math.Sqrt((double)(n - j) / j);
        }

        double gammaFactor;

        // Branch threshold n=340: below this, LogGamma is fast and exact;
        // above, the Stirling approximation introduces <0.05% error and
        // avoids LogGamma overflow risk for very large n.
        if (n <= 340)
        {
            // exact: Gamma((n-1)/2) / (sqrt(pi) * Gamma(n/2))
            gammaFactor = DeMath.Exp(
                Numerical.LogGamma((n - 1.0) / 2.0)
                - (0.5 * DeMath.Log(Math.PI))
                - Numerical.LogGamma(n / 2.0));
        }
        else
        {
            // Stirling Variant A (Peters 1994): sqrt(2 / (pi * (n-1)))
            gammaFactor = Math.Sqrt(2.0 / (Math.PI * (n - 1.0)));
        }

        return gammaFactor * innerSum;
    }
}
