namespace Tests.Indicators;

[TestClass]
public class PivotsTests : TestBase
{
    [TestMethod]
    public void Standard()
    {
        List<PivotsResult> results = quotes
            .GetPivots(4, 4, 20, EndType.HighLow)
            .ToList();

        // proper quantities
        Assert.HasCount(502, results);
        Assert.HasCount(35, results.Where(static x => x.HighPoint != null));
        Assert.HasCount(333, results.Where(static x => x.HighTrend != null));
        Assert.HasCount(338, results.Where(static x => x.HighLine != null));
        Assert.HasCount(34, results.Where(static x => x.LowPoint != null));
        Assert.HasCount(328, results.Where(static x => x.LowTrend != null));
        Assert.HasCount(333, results.Where(static x => x.LowLine != null));

        // sample values
        PivotsResult r3 = results[3];
        Assert.IsNull(r3.HighPoint);
        Assert.IsNull(r3.HighTrend);
        Assert.IsNull(r3.HighLine);
        Assert.IsNull(r3.LowPoint);
        Assert.IsNull(r3.LowTrend);
        Assert.IsNull(r3.LowLine);

        PivotsResult r7 = results[7];
        Assert.IsNull(r7.HighPoint);
        Assert.IsNull(r7.HighTrend);
        Assert.IsNull(r7.HighLine);
        Assert.AreEqual(212.53m, r7.LowPoint);
        Assert.IsNull(r7.LowTrend);
        Assert.AreEqual(212.53m, r7.LowLine);

        PivotsResult r120 = results[120];
        Assert.AreEqual(233.02m, r120.HighPoint);
        Assert.AreEqual(PivotTrend.LH, r120.HighTrend);
        Assert.AreEqual(233.02m, r120.HighLine);
        Assert.IsNull(r120.LowPoint);
        Assert.AreEqual(PivotTrend.LL, r120.LowTrend);
        Assert.AreEqual(228.9671m, r120.LowLine.Round(4));

        PivotsResult r180 = results[180];
        Assert.AreEqual(239.74m, r180.HighPoint);
        Assert.AreEqual(PivotTrend.HH, r180.HighTrend);
        Assert.AreEqual(239.74m, r180.HighLine);
        Assert.IsNull(r180.LowPoint);
        Assert.AreEqual(PivotTrend.HL, r180.LowTrend);
        Assert.AreEqual(236.7050m, r180.LowLine.Round(4));

        PivotsResult r250 = results[250];
        Assert.IsNull(r250.HighPoint);
        Assert.IsNull(r250.HighTrend);
        Assert.IsNull(r250.HighLine);
        Assert.AreEqual(256.81m, r250.LowPoint);
        Assert.IsNull(r250.LowTrend);
        Assert.IsNull(r250.LowLine);

        PivotsResult r472 = results[472];
        Assert.IsNull(r472.HighPoint);
        Assert.AreEqual(PivotTrend.LH, r472.HighTrend);
        Assert.AreEqual(274.14m, r472.HighLine);
        Assert.IsNull(r472.LowPoint);
        Assert.AreEqual(PivotTrend.HL, r472.LowTrend);
        Assert.AreEqual(255.8078m, r472.LowLine.Round(4));

        PivotsResult r497 = results[497];
        Assert.IsNull(r497.HighPoint);
        Assert.IsNull(r497.HighTrend);
        Assert.IsNull(r497.HighLine);
        Assert.IsNull(r497.LowPoint);
        Assert.IsNull(r497.LowTrend);
        Assert.IsNull(r497.LowLine);

        PivotsResult r498 = results[498];
        Assert.IsNull(r498.HighPoint);
        Assert.IsNull(r498.HighTrend);
        Assert.IsNull(r498.HighLine);
        Assert.IsNull(r498.LowPoint);
        Assert.IsNull(r498.LowTrend);
        Assert.IsNull(r498.LowLine);
    }

    [TestMethod]
    public void BadData()
    {
        List<PivotsResult> r = badQuotes
            .GetPivots()
            .ToList();

        Assert.HasCount(502, r);
    }

    [TestMethod]
    public void NoQuotes()
    {
        List<PivotsResult> r0 = noquotes
            .GetPivots()
            .ToList();

        Assert.IsEmpty(r0);

        List<PivotsResult> r1 = onequote
            .GetPivots()
            .ToList();

        Assert.HasCount(1, r1);
    }

    [TestMethod]
    public void Condense()
    {
        List<PivotsResult> r = quotes
            .GetPivots(4, 4, 20, EndType.HighLow)
            .Condense()
            .ToList();

        Assert.HasCount(67, r);
    }

    [TestMethod]
    public void Exceptions()
    {
        // bad left span
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(
            static () => quotes.GetPivots(1));

        // bad right span
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(
            static () => quotes.GetPivots(2, 1));

        // bad lookback window
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(
            static () => quotes.GetPivots(20, 10, 20, EndType.Close))
            .ParamName.Should().Be("maxTrendPeriods");
    }

    [TestMethod]
    public void Issue1950()
    {
        IOrderedEnumerable<Quote> quotes = File.ReadAllLines("m-r/Pivots/data.issue1950.csv")
            .Skip(1)
            .Select(Importer.QuoteFromCsv)
            .OrderByDescending(static x => x.Date);

        List<PivotsResult> r = quotes
            .GetPivots(maxTrendPeriods: 100)
            .ToList();

        string msg = "results size should match original quotes size";

        r.Should().HaveCount(1430, msg);
        r.Should().HaveCount(quotes.Count(), msg);
        r.Where(static x => x.HighPoint is not null).Should().HaveCount(194);
        r.Where(static x => x.LowPoint is not null).Should().HaveCount(193);
    }
}
