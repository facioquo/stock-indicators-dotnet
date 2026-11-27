namespace Tests.Indicators;

[TestClass]
public class UltimateTests : TestBase
{
    [TestMethod]
    public void Standard()
    {
        List<UltimateResult> results = quotes
            .GetUltimate(7, 14, 28)
            .ToList();

        // proper quantities
        Assert.HasCount(502, results);
        Assert.HasCount(474, results.Where(static x => x.Ultimate != null));

        // sample values
        UltimateResult r1 = results[74];
        Assert.AreEqual(51.7770, r1.Ultimate.Round(4));

        UltimateResult r2 = results[249];
        Assert.AreEqual(45.3121, r2.Ultimate.Round(4));

        UltimateResult r3 = results[501];
        Assert.AreEqual(49.5257, r3.Ultimate.Round(4));
    }

    [TestMethod]
    public void Chainor()
    {
        List<SmaResult> results = quotes
            .GetUltimate()
            .GetSma(10)
            .ToList();

        Assert.HasCount(502, results);
        Assert.HasCount(465, results.Where(static x => x.Sma != null));
    }

    [TestMethod]
    public void BadData()
    {
        List<UltimateResult> r = badQuotes
            .GetUltimate(1, 2, 3)
            .ToList();

        Assert.HasCount(502, r);
        Assert.IsEmpty(r.Where(static x => x.Ultimate is double v && double.IsNaN(v)));
    }

    [TestMethod]
    public void NoQuotes()
    {
        List<UltimateResult> r0 = noquotes
            .GetUltimate()
            .ToList();

        Assert.IsEmpty(r0);

        List<UltimateResult> r1 = onequote
            .GetUltimate()
            .ToList();

        Assert.HasCount(1, r1);
    }

    [TestMethod]
    public void Removed()
    {
        List<UltimateResult> results = quotes
            .GetUltimate(7, 14, 28)
            .RemoveWarmupPeriods()
            .ToList();

        // assertions
        Assert.HasCount(502 - 28, results);

        UltimateResult last = results.LastOrDefault();
        Assert.AreEqual(49.5257, last.Ultimate.Round(4));
    }

    [TestMethod]
    public void Exceptions()
    {
        // bad short period
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(
            static () => quotes.GetUltimate(0))
            .ParamName.Should().Be("shortPeriods");

        Assert.ThrowsExactly<ArgumentOutOfRangeException>(
            static () => quotes.GetUltimate(7, 0, 28))
            .ParamName.Should().Be("middleAverage");

        Assert.ThrowsExactly<ArgumentOutOfRangeException>(
            static () => quotes.GetUltimate(7, 14, 0))
            .ParamName.Should().Be("longPeriods");

        // bad middle period
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(
            static () => quotes.GetUltimate(7, 6));

        // bad long period
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(
            static () => quotes.GetUltimate(7, 14, 11));
    }
}
