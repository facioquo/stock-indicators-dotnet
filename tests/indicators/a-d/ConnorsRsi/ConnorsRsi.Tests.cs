namespace Tests.Indicators;

[TestClass]
public class ConnorsRsiTests : TestBase
{
    [TestMethod]
    public void Standard()
    {
        int rsiPeriods = 3;
        int streakPeriods = 2;
        int rankPeriods = 100;
        int startPeriod = Math.Max(rsiPeriods, Math.Max(streakPeriods, rankPeriods)) + 2;

        List<ConnorsRsiResult> results1 =
            quotes.GetConnorsRsi(rsiPeriods, streakPeriods, rankPeriods)
            .ToList();

        // proper quantities
        Assert.HasCount(502, results1);
        Assert.AreEqual(502 - startPeriod + 1, results1.Count(static x => x.ConnorsRsi != null));

        // sample value
        ConnorsRsiResult r1 = results1[501];
        Assert.AreEqual(68.8087, r1.Rsi.Round(4));
        Assert.AreEqual(67.4899, r1.RsiStreak.Round(4));
        Assert.AreEqual(88.0000, r1.PercentRank.Round(4));
        Assert.AreEqual(74.7662, r1.ConnorsRsi.Round(4));

        // different parameters
        List<ConnorsRsiResult> results2 = quotes.GetConnorsRsi(14, 20, 10).ToList();
        ConnorsRsiResult r2 = results2[501];
        Assert.AreEqual(42.0773, r2.Rsi.Round(4));
        Assert.AreEqual(52.7386, r2.RsiStreak.Round(4));
        Assert.AreEqual(90.0000, r2.PercentRank.Round(4));
        Assert.AreEqual(61.6053, r2.ConnorsRsi.Round(4));
    }

    [TestMethod]
    public void PercentRankNotTruncatedWhenRankPeriodsDoesNotDivide100()
    {
        // percent rank must not be truncated by integer division
        List<ConnorsRsiResult> results = quotes
            .GetConnorsRsi(3, 2, 3)
            .ToList();

        double[] expectedRanks = [0d, 100d / 3, 200d / 3, 100d];

        List<ConnorsRsiResult> ranked = results
            .Where(static x => x.PercentRank != null)
            .ToList();

        Assert.IsNotEmpty(ranked);
        Assert.IsTrue(ranked.TrueForAll(x => expectedRanks.Contains((double)x.PercentRank!)));
        Assert.IsTrue(ranked.Exists(static x => x.PercentRank == 200d / 3));

        ConnorsRsiResult r = results.First(static x => x.PercentRank == 200d / 3 && x.ConnorsRsi != null);
        Assert.AreEqual(((r.Rsi + r.RsiStreak + r.PercentRank) / 3).Round(10), r.ConnorsRsi.Round(10));
    }

    [TestMethod]
    public void UseTuple()
    {
        List<ConnorsRsiResult> results = quotes
            .Use(CandlePart.Close)
            .GetConnorsRsi()
            .ToList();

        Assert.HasCount(502, results);
        Assert.AreEqual(401, results.Count(static x => x.ConnorsRsi != null));
    }

    [TestMethod]
    public void TupleNaN()
    {
        List<ConnorsRsiResult> r = tupleNanny
            .GetConnorsRsi()
            .ToList();

        Assert.HasCount(200, r);
        Assert.IsEmpty(r.Where(static x => x.ConnorsRsi is double v && double.IsNaN(v)));
    }

    [TestMethod]
    public void Chainee()
    {
        List<ConnorsRsiResult> results = quotes
            .GetSma(2)
            .GetConnorsRsi()
            .ToList();

        Assert.HasCount(502, results);
        Assert.AreEqual(400, results.Count(static x => x.ConnorsRsi != null));
    }

    [TestMethod]
    public void Chainor()
    {
        List<SmaResult> results = quotes
            .GetConnorsRsi()
            .GetSma(10)
            .ToList();

        Assert.HasCount(502, results);
        Assert.AreEqual(392, results.Count(static x => x.Sma != null));
    }

    [TestMethod]
    public void BadData()
    {
        List<ConnorsRsiResult> r = badQuotes
            .GetConnorsRsi(4, 3, 25)
            .ToList();

        Assert.HasCount(502, r);
        Assert.IsEmpty(r.Where(static x => x.Rsi is double v && double.IsNaN(v)));
    }

    [TestMethod]
    public void NoQuotes()
    {
        List<ConnorsRsiResult> r0 = noquotes
            .GetConnorsRsi()
            .ToList();

        Assert.IsEmpty(r0);

        List<ConnorsRsiResult> r1 = onequote
            .GetConnorsRsi()
            .ToList();

        Assert.HasCount(1, r1);
    }

    [TestMethod]
    public void Removed()
    {
        int rsiPeriods = 3;
        int streakPeriods = 2;
        int rankPeriods = 100;

        // TODO: I don't think this is right, inconsistent
        int removePeriods = Math.Max(rsiPeriods, Math.Max(streakPeriods, rankPeriods)) + 2;

        List<ConnorsRsiResult> results =
            quotes.GetConnorsRsi(rsiPeriods, streakPeriods, rankPeriods)
            .RemoveWarmupPeriods()
            .ToList();

        // assertions
        Assert.HasCount(502 - removePeriods + 1, results);

        ConnorsRsiResult last = results.LastOrDefault();
        Assert.AreEqual(68.8087, last.Rsi.Round(4));
        Assert.AreEqual(67.4899, last.RsiStreak.Round(4));
        Assert.AreEqual(88.0000, last.PercentRank.Round(4));
        Assert.AreEqual(74.7662, last.ConnorsRsi.Round(4));
    }

    [TestMethod]
    public void Exceptions()
    {
        // bad RSI period
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(
            static () => quotes.GetConnorsRsi(1, 2, 100));

        // bad Streak period
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(
            static () => quotes.GetConnorsRsi(3, 1, 100));

        // bad Rank period
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(
            static () => quotes.GetConnorsRsi(3, 2, 1));
    }
}
