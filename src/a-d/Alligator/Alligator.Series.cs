namespace Skender.Stock.Indicators;

// WILLIAMS ALLIGATOR (SERIES)
public static partial class Indicator
{
    internal static List<AlligatorResult> CalcAlligator(
        this List<(DateTime Date, double Value)> tpList,
        int jawPeriods,
        int jawOffset,
        int teethPeriods,
        int teethOffset,
        int lipsPeriods,
        int lipsOffset)
    {
        // check parameter arguments
        ValidateAlligator(
            jawPeriods,
            jawOffset,
            teethPeriods,
            teethOffset,
            lipsPeriods,
            lipsOffset);

        // initialize
        int length = tpList.Count;
        double[] pr = new double[length]; // median price

        List<AlligatorResult> results =
            tpList
            .Select(static x => new AlligatorResult(x.Date))
            .ToList();

        // roll through quotes
        for (int i = 0; i < length; i++)
        {
            (DateTime _, double value) = tpList[i];
            pr[i] = value;

            // only calculate jaw if the array offset is still in valid range
            if (i + jawOffset < length)
            {
                AlligatorResult jawResult = results[i + jawOffset];

                // calculate alligator's jaw
                // first value: calculate SMA
                if (i + 1 == jawPeriods)
                {
                    double sumMedianPrice = 0;
                    for (int p = i + 1 - jawPeriods; p <= i; p++)
                    {
                        sumMedianPrice += pr[p];
                    }

                    jawResult.Jaw = sumMedianPrice / jawPeriods;
                }

                // remaining values: SMMA
                else if (i + 1 > jawPeriods)
                {
                    double? prevValue = results[i + jawOffset - 1].Jaw;
                    jawResult.Jaw = ((prevValue * (jawPeriods - 1)) + pr[i]) / jawPeriods;
                }

                jawResult.Jaw = jawResult.Jaw.NaN2Null();
            }

            // only calculate teeth if the array offset is still in valid range
            if (i + teethOffset < length)
            {
                AlligatorResult teethResult = results[i + teethOffset];

                // calculate alligator's teeth
                // first value: calculate SMA
                if (i + 1 == teethPeriods)
                {
                    double sumMedianPrice = 0;
                    for (int p = i + 1 - teethPeriods; p <= i; p++)
                    {
                        sumMedianPrice += pr[p];
                    }

                    teethResult.Teeth = sumMedianPrice / teethPeriods;
                }

                // remaining values: SMMA
                else if (i + 1 > teethPeriods)
                {
                    double? prevValue = results[i + teethOffset - 1].Teeth;
                    teethResult.Teeth = ((prevValue * (teethPeriods - 1)) + pr[i]) / teethPeriods;
                }

                teethResult.Teeth = teethResult.Teeth.NaN2Null();
            }

            // only calculate lips if the array offset is still in valid range
            if (i + lipsOffset < length)
            {
                AlligatorResult lipsResult = results[i + lipsOffset];

                // calculate alligator's lips
                // first value: calculate SMA
                if (i + 1 == lipsPeriods)
                {
                    double sumMedianPrice = 0;
                    for (int p = i + 1 - lipsPeriods; p <= i; p++)
                    {
                        sumMedianPrice += pr[p];
                    }

                    lipsResult.Lips = sumMedianPrice / lipsPeriods;
                }

                // remaining values: SMMA
                else if (i + 1 > lipsPeriods)
                {
                    double? prevValue = results[i + lipsOffset - 1].Lips;
                    lipsResult.Lips = ((prevValue * (lipsPeriods - 1)) + pr[i]) / lipsPeriods;
                }

                lipsResult.Lips = lipsResult.Lips.NaN2Null();
            }
        }

        return results;
    }

    // parameter validation
    private static void ValidateAlligator(
        int jawPeriods,
        int jawOffset,
        int teethPeriods,
        int teethOffset,
        int lipsPeriods,
        int lipsOffset)
    {
        // check parameter arguments
        if (jawPeriods <= teethPeriods)
        {
            throw new ArgumentOutOfRangeException(nameof(jawPeriods), jawPeriods,
                "Jaw lookback periods must be greater than Teeth lookback periods for Alligator.");
        }

        if (teethPeriods <= lipsPeriods)
        {
            throw new ArgumentOutOfRangeException(nameof(teethPeriods), teethPeriods,
                "Teeth lookback periods must be greater than Lips lookback periods for Alligator.");
        }

        if (lipsPeriods <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(lipsPeriods), lipsPeriods,
                "Lips lookback periods must be greater than 0 for Alligator.");
        }

        if (jawOffset <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(jawOffset), jawOffset,
                "Jaw offset periods must be greater than 0 for Alligator.");
        }

        if (teethOffset <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(teethOffset), teethOffset,
                "Teeth offset periods must be greater than 0 for Alligator.");
        }

        if (lipsOffset <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(lipsOffset), lipsOffset,
                "Lips offset periods must be greater than 0 for Alligator.");
        }

        if (jawPeriods + jawOffset <= teethPeriods + teethOffset)
        {
            throw new ArgumentOutOfRangeException(nameof(jawPeriods), jawPeriods,
                "Jaw lookback + offset are too small for Alligator.");
        }

        if (teethPeriods + teethOffset <= lipsPeriods + lipsOffset)
        {
            throw new ArgumentOutOfRangeException(nameof(teethPeriods), teethPeriods,
                "Teeth lookback + offset are too small for Alligator.");
        }
    }
}
