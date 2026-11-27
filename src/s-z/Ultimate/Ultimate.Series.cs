namespace Skender.Stock.Indicators;

// ULTIMATE OSCILLATOR (SERIES)
public static partial class Indicator
{
    internal static List<UltimateResult> CalcUltimate(
        this List<QuoteD> qdList,
        int shortPeriods,
        int middlePeriods,
        int longPeriods)
    {
        // check parameter arguments
        ValidateUltimate(shortPeriods, middlePeriods, longPeriods);

        // initialize
        int length = qdList.Count;
        List<UltimateResult> results = new(length);
        double[] bp = new double[length]; // buying pressure
        double[] tr = new double[length]; // true range

        double priorClose = 0;

        // roll through quotes
        for (int i = 0; i < qdList.Count; i++)
        {
            QuoteD q = qdList[i];

            UltimateResult r = new(q.Date);
            results.Add(r);

            if (i > 0)
            {
                bp[i] = q.Close - Math.Min(q.Low, priorClose);
                tr[i] = Math.Max(q.High, priorClose) - Math.Min(q.Low, priorClose);
            }

            if (i >= longPeriods)
            {
                double sumBP1 = 0;
                double sumBP2 = 0;
                double sumBP3 = 0;

                double sumTR1 = 0;
                double sumTR2 = 0;
                double sumTR3 = 0;

                for (int p = i + 1 - longPeriods; p <= i; p++)
                {
                    int pIndex = p + 1;

                    // short aggregate
                    if (pIndex > i + 1 - shortPeriods)
                    {
                        sumBP1 += bp[p];
                        sumTR1 += tr[p];
                    }

                    // middle aggregate
                    if (pIndex > i + 1 - middlePeriods)
                    {
                        sumBP2 += bp[p];
                        sumTR2 += tr[p];
                    }

                    // long aggregate
                    sumBP3 += bp[p];
                    sumTR3 += tr[p];
                }

                double avg1 = (sumTR1 == 0) ? double.NaN : sumBP1 / sumTR1;
                double avg2 = (sumTR2 == 0) ? double.NaN : sumBP2 / sumTR2;
                double avg3 = (sumTR3 == 0) ? double.NaN : sumBP3 / sumTR3;

                r.Ultimate = (100d * ((4d * avg1) + (2d * avg2) + avg3) / 7d).NaN2Null();
            }

            priorClose = q.Close;
        }

        return results;
    }

    // parameter validation
    private static void ValidateUltimate(
        int shortPeriods,
        int middleAverage,
        int longPeriods)
    {
        // check parameter arguments
        if (shortPeriods <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(shortPeriods), shortPeriods,
                "Short periods must be greater than 0 for Ultimate Oscillator.");
        }

        if (middleAverage <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(middleAverage), middleAverage,
                "Middle periods must be greater than 0 for Ultimate Oscillator.");
        }

        if (longPeriods <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(longPeriods), longPeriods,
                "Long periods must be greater than 0 for Ultimate Oscillator.");
        }

        if (shortPeriods >= middleAverage || middleAverage >= longPeriods)
        {
            throw new ArgumentOutOfRangeException(nameof(middleAverage), middleAverage,
                "Average periods must be increasingly larger than each other for Ultimate Oscillator.");
        }
    }
}
