namespace Skender.Stock.Indicators;

// MOVING AVERAGE CONVERGENCE/DIVERGENCE (MACD) OSCILLATOR (SERIES)
public static partial class Indicator
{
    internal static List<MacdResult> CalcMacd(
        this List<(DateTime, double)> tpList,
        int fastPeriods,
        int slowPeriods,
        int signalPeriods)
    {
        // check parameter arguments
        ValidateMacd(fastPeriods, slowPeriods, signalPeriods);

        // initialize
        List<EmaResult> emaFast = tpList.CalcEma(fastPeriods);
        List<EmaResult> emaSlow = tpList.CalcEma(slowPeriods);

        int length = tpList.Count;
        List<(DateTime, double)> emaDiff = [];
        List<MacdResult> results = new(length);

        // roll through quotes
        for (int i = 0; i < length; i++)
        {
            (DateTime date, double _) = tpList[i];
            EmaResult df = emaFast[i];
            EmaResult ds = emaSlow[i];

            MacdResult r = new(date) {
                FastEma = df.Ema,
                SlowEma = ds.Ema
            };
            results.Add(r);

            if (i >= slowPeriods - 1)
            {
                double macd = (df.Ema - ds.Ema).Null2NaN();
                r.Macd = macd.NaN2Null();

                // temp data for interim EMA of macd
                (DateTime, double) diff = (date, macd);

                emaDiff.Add(diff);
            }
        }

        // add signal and histogram to result
        List<EmaResult> emaSignal = CalcEma(emaDiff, signalPeriods);

        for (int d = slowPeriods - 1; d < length; d++)
        {
            MacdResult r = results[d];
            EmaResult ds = emaSignal[d + 1 - slowPeriods];

            r.Signal = ds.Ema.NaN2Null();
            r.Histogram = (r.Macd - r.Signal).NaN2Null();
        }

        return results;
    }

    // parameter validation
    private static void ValidateMacd(
        int fastPeriods,
        int slowPeriods,
        int signalPeriods)
    {
        // check parameter arguments
        if (fastPeriods <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(fastPeriods), fastPeriods,
                "Fast periods must be greater than 0 for MACD.");
        }

        if (signalPeriods <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(signalPeriods), signalPeriods,
                "Signal periods must be greater than 0 for MACD.");
        }

        if (slowPeriods <= fastPeriods)
        {
            throw new ArgumentOutOfRangeException(nameof(slowPeriods), slowPeriods,
                "Slow periods must be greater than the fast period for MACD.");
        }
    }
}
