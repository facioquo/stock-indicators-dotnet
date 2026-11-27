namespace Tests.Indicators;

[TestClass]
public class MacdTests : TestBase
{
    [TestMethod]
    public void Standard()
    {
        int fastPeriods = 12;
        int slowPeriods = 26;
        int signalPeriods = 9;

        List<MacdResult> results =
            quotes.GetMacd(fastPeriods, slowPeriods, signalPeriods)
            .ToList();

        // proper quantities
        Assert.HasCount(502, results);
        Assert.AreEqual(477, results.Count(static x => x.Macd != null));
        Assert.AreEqual(469, results.Count(static x => x.Signal != null));
        Assert.AreEqual(469, results.Count(static x => x.Histogram != null));

        // sample values
        MacdResult r49 = results[49];
        Assert.AreEqual(1.7203, r49.Macd.Round(4));
        Assert.AreEqual(1.9675, r49.Signal.Round(4));
        Assert.AreEqual(-0.2472, r49.Histogram.Round(4));
        Assert.AreEqual(224.1840, r49.FastEma.Round(4));
        Assert.AreEqual(222.4637, r49.SlowEma.Round(4));

        MacdResult r249 = results[249];
        Assert.AreEqual(2.2353, r249.Macd.Round(4));
        Assert.AreEqual(2.3141, r249.Signal.Round(4));
        Assert.AreEqual(-0.0789, r249.Histogram.Round(4));
        Assert.AreEqual(256.6780, r249.FastEma.Round(4));
        Assert.AreEqual(254.4428, r249.SlowEma.Round(4));

        MacdResult r501 = results[501];
        Assert.AreEqual(-6.2198, r501.Macd.Round(4));
        Assert.AreEqual(-5.8569, r501.Signal.Round(4));
        Assert.AreEqual(-0.3629, r501.Histogram.Round(4));
        Assert.AreEqual(245.4957, r501.FastEma.Round(4));
        Assert.AreEqual(251.7155, r501.SlowEma.Round(4));
    }

    [TestMethod]
    public void UseTuple()
    {
        List<MacdResult> results = quotes
            .Use(CandlePart.Close)
            .GetMacd()
            .ToList();

        Assert.HasCount(502, results);
        Assert.AreEqual(477, results.Count(static x => x.Macd != null));
    }

    [TestMethod]
    public void TupleNaN()
    {
        List<MacdResult> r = tupleNanny
            .GetMacd()
            .ToList();

        Assert.HasCount(200, r);
        Assert.IsEmpty(r.Where(static x => x.Macd is double v && double.IsNaN(v)));
    }

    [TestMethod]
    public void Chainee()
    {
        List<MacdResult> results = quotes
            .GetSma(2)
            .GetMacd()
            .ToList();

        Assert.HasCount(502, results);
        Assert.AreEqual(476, results.Count(static x => x.Macd != null));
    }

    [TestMethod]
    public void Chainor()
    {
        List<SmaResult> results = quotes
            .GetMacd()
            .GetSma(10)
            .ToList();

        Assert.HasCount(502, results);
        Assert.AreEqual(468, results.Count(static x => x.Sma != null));
    }

    [TestMethod]
    public void BadData()
    {
        List<MacdResult> r = badQuotes
            .GetMacd(10, 20, 5)
            .ToList();

        Assert.HasCount(502, r);
        Assert.IsEmpty(r.Where(static x => x.Macd is double v && double.IsNaN(v)));
    }

    [TestMethod]
    public void NoQuotes()
    {
        List<MacdResult> r0 = noquotes
            .GetMacd()
            .ToList();

        Assert.IsEmpty(r0);

        List<MacdResult> r1 = onequote
            .GetMacd()
            .ToList();

        Assert.HasCount(1, r1);
    }

    [TestMethod]
    public void Removed()
    {
        int fastPeriods = 12;
        int slowPeriods = 26;
        int signalPeriods = 9;

        List<MacdResult> results = quotes
            .GetMacd(fastPeriods, slowPeriods, signalPeriods)
            .RemoveWarmupPeriods()
            .ToList();

        // assertions
        Assert.HasCount(502 - (slowPeriods + signalPeriods + 250), results);

        MacdResult last = results.LastOrDefault();
        Assert.AreEqual(-6.2198, last.Macd.Round(4));
        Assert.AreEqual(-5.8569, last.Signal.Round(4));
        Assert.AreEqual(-0.3629, last.Histogram.Round(4));
    }

    [TestMethod]
    public void Exceptions()
    {
        // bad fast period
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(
            static () => quotes.GetMacd(0, 26, 9));

        // bad slow periods must be larger than faster period
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(
            static () => quotes.GetMacd(12, 12, 9));

        // bad signal period
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(
            static () => quotes.GetMacd(12, 26, -1));

        Assert.ThrowsExactly<ArgumentOutOfRangeException>(
            static () => quotes.GetMacd(12, 26, 0))
            .ParamName.Should().Be("signalPeriods");
    }
}
