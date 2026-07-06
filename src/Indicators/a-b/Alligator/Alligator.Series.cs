namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Williams Alligator indicator.
/// </summary>
public static partial class Alligator
{
    /// <summary>
    /// Calculates the Williams Alligator indicator for a series of data.
    /// </summary>
    /// <param name="source">Time-series values to transform.</param>
    /// <param name="jawPeriods">Lookback periods for the Jaw line.</param>
    /// <param name="jawOffset">Offset periods for the Jaw line.</param>
    /// <param name="teethPeriods">Lookback periods for the Teeth line.</param>
    /// <param name="teethOffset">Offset periods for the Teeth line.</param>
    /// <param name="lipsPeriods">Lookback periods for the Lips line.</param>
    /// <param name="lipsOffset">Offset periods for the Lips line.</param>
    /// <returns>Time series of Alligator values.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Invalid parameter value provided.
    /// </exception>
    public static IReadOnlyList<AlligatorResult> ToAlligator(
        this IReadOnlyList<IReusable> source,
        int jawPeriods = 13,
        int jawOffset = 8,
        int teethPeriods = 8,
        int teethOffset = 5,
        int lipsPeriods = 5,
        int lipsOffset = 3)
    {
        // check parameter arguments
        Validate(
            jawPeriods,
            jawOffset,
            teethPeriods,
            teethOffset,
            lipsPeriods,
            lipsOffset);

        // prefer HL2 when IBar
        IReadOnlyList<IReusable> values
            = source.ToPreferredList(CandlePart.HL2);

        // initialize
        int length = values.Count;
        List<AlligatorResult> results = new(length);

        // roll through source values
        for (int i = 0; i < length; i++)
        {
            double jaw = double.NaN;
            double lips = double.NaN;
            double teeth = double.NaN;

            // calculate alligator's jaw, when in range
            if (i >= jawPeriods + jawOffset - 1)
            {
                // first/reset value: calculate SMA
                if (results[i - 1].Jaw is null)
                {
                    double sum = 0;
                    for (int p = i - jawPeriods - jawOffset + 1; p <= i - jawOffset; p++)
                    {
                        sum += values[p].Value;
                    }

                    jaw = sum / jawPeriods;
                }

                // remaining values: SMMA
                else
                {
                    double prevJaw = results[i - 1].Jaw.Null2NaN();

                    jaw = ((prevJaw * (jawPeriods - 1)) + values[i - jawOffset].Value) / jawPeriods;
                }
            }

            // calculate alligator's teeth, when in range
            if (i >= teethPeriods + teethOffset - 1)
            {
                // first/reset value: calculate SMA
                if (results[i - 1].Teeth is null)
                {
                    double sum = 0;
                    for (int p = i - teethPeriods - teethOffset + 1; p <= i - teethOffset; p++)
                    {
                        sum += values[p].Value;
                    }

                    teeth = sum / teethPeriods;
                }

                // remaining values: SMMA
                else
                {
                    double prevTooth = results[i - 1].Teeth.Null2NaN();

                    teeth = ((prevTooth * (teethPeriods - 1)) + values[i - teethOffset].Value) / teethPeriods;
                }
            }

            // calculate alligator's lips, when in range
            if (i >= lipsPeriods + lipsOffset - 1)
            {
                // first/reset value: calculate SMA
                if (results[i - 1].Lips is null)
                {
                    double sum = 0;
                    for (int p = i - lipsPeriods - lipsOffset + 1; p <= i - lipsOffset; p++)
                    {
                        sum += values[p].Value;
                    }

                    lips = sum / lipsPeriods;
                }

                // remaining values: SMMA
                else
                {
                    double prevLips = results[i - 1].Lips.Null2NaN();

                    lips = ((prevLips * (lipsPeriods - 1)) + values[i - lipsOffset].Value) / lipsPeriods;
                }
            }

            // result
            results.Add(new AlligatorResult(
                values[i].Timestamp,
                jaw.NaN2Null(),
                teeth.NaN2Null(),
                lips.NaN2Null()));
        }

        return results;
    }
}
