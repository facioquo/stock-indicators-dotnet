namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Average Directional Index (ADX) indicator.
/// </summary>
public static partial class Adx
{
    /// <summary>
    /// Calculates the Average Directional Index (ADX) from a series of bars.
    /// </summary>
    /// <param name="bars">Aggregate OHLCV price bars, time sorted.</param>
    /// <param name="lookbackPeriods">Quantity of periods in lookback window.</param>
    /// <returns>A list of ADX results.</returns>
    public static IReadOnlyList<AdxResult> ToAdx(
        this IReadOnlyList<IBar> bars,
        int lookbackPeriods = 14)
        => bars
            .ToBarDList()
            .CalcAdx(lookbackPeriods);

    /// <summary>
    /// Calculates the ADX from a list of bars.
    /// </summary>
    /// <param name="bars">List of bars.</param>
    /// <param name="lookbackPeriods">Quantity of periods in lookback window.</param>
    /// <returns>A list of ADX results.</returns>
    private static List<AdxResult> CalcAdx(
        this List<BarD> bars,
        int lookbackPeriods = 14)
    {
        // check parameter arguments
        Validate(lookbackPeriods);

        // initialize
        int length = bars.Count;
        List<AdxResult> results = new(length);

        double prevHigh = 0;
        double prevLow = 0;
        double prevClose = 0;
        double prevTrs = double.NaN; // smoothed
        double prevPdm = double.NaN;
        double prevMdm = double.NaN;
        double prevAdx = double.NaN;

        double sumTr = 0;
        double sumPdm = 0;
        double sumMdm = 0;
        double sumDx = 0;

        // roll through source values
        for (int i = 0; i < length; i++)
        {
            BarD q = bars[i];

            // skip first period
            if (i == 0)
            {
                prevHigh = q.High;
                prevLow = q.Low;
                prevClose = q.Close;

                results.Add(new(Timestamp: q.Timestamp));
                continue;
            }

            double hmpc = Math.Abs(q.High - prevClose);
            double lmpc = Math.Abs(q.Low - prevClose);
            double hmph = q.High - prevHigh;
            double plml = prevLow - q.Low;

            double tr = Math.Max(q.High - q.Low, Math.Max(hmpc, lmpc));

            double pdm1 = hmph > plml ? Math.Max(hmph, 0) : 0;
            double mdm1 = plml > hmph ? Math.Max(plml, 0) : 0;

            prevHigh = q.High;
            prevLow = q.Low;
            prevClose = q.Close;

            // accumulate DM initialization period values
            if (i <= lookbackPeriods)
            {
                sumTr += tr;
                sumPdm += pdm1;
                sumMdm += mdm1;
            }

            // skip DM initialization period
            if (i < lookbackPeriods)
            {
                results.Add(new(Timestamp: q.Timestamp));
                continue;
            }

            // smoothed true range and directional movement
            double trs;
            double pdm;
            double mdm;

            if (double.IsNaN(prevTrs))
            {
                // initialize smoothed values at first calculation after lookback
                trs = sumTr;
                pdm = sumPdm;
                mdm = sumMdm;
            }
            else
            {
                trs = prevTrs - (prevTrs / lookbackPeriods) + tr;
                pdm = prevPdm - (prevPdm / lookbackPeriods) + pdm1;
                mdm = prevMdm - (prevMdm / lookbackPeriods) + mdm1;
            }

            prevTrs = trs;
            prevPdm = pdm;
            prevMdm = mdm;

            if (trs == 0)
            {
                results.Add(new(Timestamp: q.Timestamp));
                continue;
            }

            // directional increments
            double pdi = 100 * pdm / trs;
            double mdi = 100 * mdm / trs;

            // calculate ADX
            double dx = pdi == mdi
                ? 0
                : pdi + mdi != 0
                ? 100 * Math.Abs(pdi - mdi) / (pdi + mdi)
                : double.NaN;

            double adx = double.NaN;
            double adxr = double.NaN;

            // ADX initialization period - accumulate DX values
            if (i < (2 * lookbackPeriods))
            {
                sumDx += dx;

                // calculate initial ADX after accumulating lookbackPeriods DX values
                if (double.IsNaN(prevAdx) && i == (2 * lookbackPeriods) - 1)
                {
                    adx = sumDx / lookbackPeriods;
                    prevAdx = adx;
                }
            }
            // ongoing ADX smoothing
            else
            {
                adx = ((prevAdx * (lookbackPeriods - 1)) + dx) / lookbackPeriods;

                // Calculate ADXR only if we have a valid prior ADX value
                int priorAdxIndex = i - lookbackPeriods;
                if (priorAdxIndex >= (2 * lookbackPeriods) - 1)
                {
                    double priorAdx = results[priorAdxIndex].Adx.Null2NaN();

                    // Only compute ADXR if prior ADX actually exists
                    if (!double.IsNaN(priorAdx))
                    {
                        adxr = (adx + priorAdx) / 2;
                    }
                }

                prevAdx = adx;
            }

            AdxResult r = new(
                Timestamp: q.Timestamp,
                Pdi: pdi,
                Mdi: mdi,
                Dx: dx.NaN2Null(),
                Adx: adx.NaN2Null(),
                Adxr: adxr.NaN2Null());

            results.Add(r);
        }

        return results;
    }
}
