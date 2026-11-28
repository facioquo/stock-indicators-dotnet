namespace Skender.Stock.Indicators;

// PRICE VOLUME OSCILLATOR (SERIES)
public static partial class Indicator
{
    internal static List<PvoResult> CalcPvo(
        this List<(DateTime, double)> tpList,
        int fastPeriods,
        int slowPeriods,
        int signalPeriods)
    {
        // check parameter arguments
        ValidatePvo(fastPeriods, slowPeriods, signalPeriods);

        // initialize
        List<EmaResult> emaFast = tpList.CalcEma(fastPeriods);
        List<EmaResult> emaSlow = tpList.CalcEma(slowPeriods);

        int length = tpList.Count;
        List<(DateTime, double)> emaDiff = [];
        List<PvoResult> results = new(length);

        // roll through quotes
        for (int i = 0; i < length; i++)
        {
            (DateTime date, double _) = tpList[i];
            EmaResult df = emaFast[i];
            EmaResult ds = emaSlow[i];

            PvoResult r = new(date);
            results.Add(r);

            if (i >= slowPeriods - 1)
            {
                double? pvo = (ds.Ema != 0) ?
                    100 * ((df.Ema - ds.Ema) / ds.Ema) : null;

                r.Pvo = pvo;

                // temp data for interim EMA of PVO
                (DateTime, double) diff = (date, (pvo == null) ? 0 : (double)pvo);

                emaDiff.Add(diff);
            }
        }

        // add signal and histogram to result
        List<EmaResult> emaSignal = CalcEma(emaDiff, signalPeriods);

        for (int d = slowPeriods - 1; d < length; d++)
        {
            PvoResult r = results[d];
            EmaResult ds = emaSignal[d + 1 - slowPeriods];

            r.Signal = ds.Ema;
            r.Histogram = r.Pvo - r.Signal;
        }

        return results;
    }

    // parameter validation
    private static void ValidatePvo(
        int fastPeriods,
        int slowPeriods,
        int signalPeriods)
    {
        // check parameter arguments
        if (fastPeriods <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(fastPeriods), fastPeriods,
                "Fast periods must be greater than 0 for PVO.");
        }

        if (signalPeriods <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(signalPeriods), signalPeriods,
                "Signal periods must be greater than 0 for PVO.");
        }

        if (slowPeriods <= fastPeriods)
        {
            throw new ArgumentOutOfRangeException(nameof(slowPeriods), slowPeriods,
                "Slow periods must be greater than the fast period for PVO.");
        }
    }
}
