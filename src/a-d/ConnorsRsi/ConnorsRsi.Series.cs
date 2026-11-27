namespace Skender.Stock.Indicators;

// CONNORS RSI (SERIES)
public static partial class Indicator
{
    internal static List<ConnorsRsiResult> CalcConnorsRsi(
        this List<(DateTime, double)> tpList,
        int rsiPeriods,
        int streakPeriods,
        int rankPeriods)
    {
        // check parameter arguments
        ValidateConnorsRsi(rsiPeriods, streakPeriods, rankPeriods);

        // initialize
        List<ConnorsRsiResult> results = tpList.CalcStreak(rsiPeriods, rankPeriods);
        int startPeriod = Math.Max(rsiPeriods, Math.Max(streakPeriods, rankPeriods)) + 2;
        int length = results.Count;

        // RSI of streak
        List<(DateTime Date, double Streak)> bdStreak = results
            .Remove(Math.Min(length, 1))
            .Select(static x => (x.Date, (double)x.Streak))
            .ToList();

        List<RsiResult> rsiStreak = CalcRsi(bdStreak, streakPeriods);

        // compose final results
        for (int p = streakPeriods + 2; p < length; p++)
        {
            ConnorsRsiResult r = results[p];
            RsiResult k = rsiStreak[p - 1];

            r.RsiStreak = k.Rsi;

            if (p + 1 >= startPeriod)
            {
                r.ConnorsRsi = (r.Rsi + r.RsiStreak + r.PercentRank) / 3;
            }
        }

        return results;
    }

    // calculate baseline streak and rank
    private static List<ConnorsRsiResult> CalcStreak(
        this List<(DateTime Date, double Streak)> tpList,
        int rsiPeriods,
        int rankPeriods)
    {
        // initialize
        List<RsiResult> rsiResults = CalcRsi(tpList, rsiPeriods);

        int length = tpList.Count;
        List<ConnorsRsiResult> results = new(length);
        double[] gain = new double[length];

        double lastClose = double.NaN;
        int streak = 0;

        // compose interim results
        for (int i = 0; i < length; i++)
        {
            (DateTime date, double value) = tpList[i];

            ConnorsRsiResult r = new(date) {
                Rsi = rsiResults[i].Rsi
            };
            results.Add(r);

            // bypass for first record
            if (i == 0)
            {
                lastClose = value;
                continue;
            }

            // streak of up or down
            if (value == lastClose)
            {
                streak = 0;
            }
            else if (value > lastClose)
            {
                if (streak >= 0)
                {
                    streak++;
                }
                else
                {
                    streak = 1;
                }
            }
            else // h.Value < lastClose
            {
                if (streak <= 0)
                {
                    streak--;
                }
                else
                {
                    streak = -1;
                }
            }

            r.Streak = streak;

            // percentile rank
            gain[i] = (lastClose <= 0) ? double.NaN
                    : (value - lastClose) / lastClose;

            if (i + 1 > rankPeriods)
            {
                int qty = 0;
                for (int p = i - rankPeriods; p <= i; p++)
                {
                    if (gain[p] < gain[i])
                    {
                        qty++;
                    }
                }

                r.PercentRank = 100d * qty / rankPeriods;
            }

            lastClose = value;
        }

        return results;
    }

    // parameter validation
    private static void ValidateConnorsRsi(
        int rsiPeriods,
        int streakPeriods,
        int rankPeriods)
    {
        // check parameter arguments
        if (rsiPeriods <= 1)
        {
            throw new ArgumentOutOfRangeException(nameof(rsiPeriods), rsiPeriods,
                "RSI period for Close price must be greater than 1 for ConnorsRsi.");
        }

        if (streakPeriods <= 1)
        {
            throw new ArgumentOutOfRangeException(nameof(streakPeriods), streakPeriods,
                "RSI period for Streak must be greater than 1 for ConnorsRsi.");
        }

        if (rankPeriods <= 1)
        {
            throw new ArgumentOutOfRangeException(nameof(rankPeriods), rankPeriods,
                "Percent Rank periods must be greater than 1 for ConnorsRsi.");
        }
    }
}
