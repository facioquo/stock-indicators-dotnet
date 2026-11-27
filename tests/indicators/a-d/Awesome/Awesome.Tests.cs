namespace Tests.Indicators;

[TestClass]
public class AwesomeTests : TestBase
{
    [TestMethod]
    public void Standard()
    {
        List<AwesomeResult> results = quotes
            .GetAwesome(5, 34)
            .ToList();

        // proper quantities
        Assert.HasCount(502, results);
        Assert.AreEqual(469, results.Count(static x => x.Oscillator != null));

        // sample values
        AwesomeResult r1 = results[32];
        Assert.IsNull(r1.Oscillator);
        Assert.IsNull(r1.Normalized);

        AwesomeResult r2 = results[33];
        Assert.AreEqual(5.4756, r2.Oscillator.Round(4));
        Assert.AreEqual(2.4548, r2.Normalized.Round(4));

        AwesomeResult r3 = results[249];
        Assert.AreEqual(5.0618, r3.Oscillator.Round(4));
        Assert.AreEqual(1.9634, r3.Normalized.Round(4));

        AwesomeResult r4 = results[501];
        Assert.AreEqual(-17.7692, r4.Oscillator.Round(4));
        Assert.AreEqual(-7.2763, r4.Normalized.Round(4));
    }

    [TestMethod]
    public void UseTuple()
    {
        List<AwesomeResult> results = quotes
            .Use(CandlePart.Close)
            .GetAwesome()
            .ToList();

        Assert.HasCount(502, results);
        Assert.AreEqual(469, results.Count(static x => x.Oscillator != null));
    }

    [TestMethod]
    public void TupleNaN()
    {
        List<AwesomeResult> r = tupleNanny
            .GetAwesome()
            .ToList();

        Assert.HasCount(200, r);
        Assert.IsEmpty(r.Where(static x => x.Oscillator is double v && double.IsNaN(v)));
    }

    [TestMethod]
    public void Chainee()
    {
        List<AwesomeResult> results = quotes
            .GetSma(2)
            .GetAwesome()
            .ToList();

        Assert.HasCount(502, results);
        Assert.AreEqual(468, results.Count(static x => x.Oscillator != null));
    }

    [TestMethod]
    public void Chainor()
    {
        List<SmaResult> results = quotes
            .GetAwesome()
            .GetSma(10)
            .ToList();

        Assert.HasCount(502, results);
        Assert.AreEqual(460, results.Count(static x => x.Sma != null));
    }

    [TestMethod]
    public void BadData()
    {
        List<AwesomeResult> r = badQuotes
            .GetAwesome()
            .ToList();

        Assert.HasCount(502, r);
        Assert.IsEmpty(r.Where(static x => x.Oscillator is double v && double.IsNaN(v)));
    }

    [TestMethod]
    public void NoQuotes()
    {
        List<AwesomeResult> r0 = noquotes
            .GetAwesome()
            .ToList();

        Assert.IsEmpty(r0);

        List<AwesomeResult> r1 = onequote
            .GetAwesome()
            .ToList();

        Assert.HasCount(1, r1);
    }

    [TestMethod]
    public void Removed()
    {
        List<AwesomeResult> results = quotes
            .GetAwesome(5, 34)
            .RemoveWarmupPeriods()
            .ToList();

        // assertions
        Assert.HasCount(502 - 33, results);

        AwesomeResult last = results.LastOrDefault();
        Assert.AreEqual(-17.7692, last.Oscillator.Round(4));
        Assert.AreEqual(-7.2763, last.Normalized.Round(4));
    }

    [TestMethod]
    public void Exceptions()
    {
        // bad fast period
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(
            static () => quotes.GetAwesome(0, 34))
            .ParamName.Should().Be("fastPeriods");

        // bad slow period
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(
            static () => quotes.GetAwesome(25, 25));
    }
}
