namespace Utilities;

[TestClass]
public class Candles : TestBaseWithPrecision
{
    [TestMethod]
    public void SortCandles()
    {
        IReadOnlyList<Bar> bars = Data.GetMismatch();

        // sort
        IReadOnlyList<CandleProperties> candles = bars
            .ToCandles();  // not sorted

        // proper quantities
        candles.Should().HaveCount(502);

        // sample values
        DateTime firstDate = DateTime.ParseExact("01/18/2016", "MM/dd/yyyy", invariantCulture);
        candles[0].Timestamp.Should().Be(firstDate);

        DateTime lastDate = DateTime.ParseExact("12/31/2018", "MM/dd/yyyy", invariantCulture);
        candles[^1].Timestamp.Should().Be(lastDate);

        DateTime spotDate = DateTime.ParseExact("03/16/2017", "MM/dd/yyyy", invariantCulture);
        candles[50].Timestamp.Should().Be(spotDate);
    }

    [TestMethod]
    public void CandleValues()
    {
        IReadOnlyList<CandleProperties> candles = Bars
            .ToCandles();

        // proper quantities
        candles.Should().HaveCount(502);

        // sample values
        CandleProperties r0 = candles[0];
        r0.Close.Should().Be(212.8m);
        r0.Size.Should().Be(1.83m);
        r0.Body.Should().Be(0.19m);
        r0.UpperWick.Should().Be(0.55m);
        r0.LowerWick.Should().Be(1.09m);
        r0.BodyPct.Should().BeApproximately(0.10383, Money5);
        r0.UpperWickPct.Should().BeApproximately(0.30055, Money5);
        r0.LowerWickPct.Should().BeApproximately(0.59563, Money5);
        r0.IsBullish.Should().BeTrue();
        r0.IsBearish.Should().BeFalse();

        CandleProperties r351 = candles[351];
        r351.Size.Should().Be(1.24m);
        r351.Body.Should().Be(0m);
        r351.UpperWick.Should().Be(0.69m);
        r351.LowerWick.Should().Be(0.55m);
        r351.BodyPct.Should().BeApproximately(0, Money5);
        r351.UpperWickPct.Should().BeApproximately(0.55645, Money5);
        r351.LowerWickPct.Should().BeApproximately(0.44355, Money5);
        r351.IsBullish.Should().BeFalse();
        r351.IsBearish.Should().BeFalse();

        CandleProperties r501 = candles[501];
        r501.Size.Should().Be(2.67m);
        r501.Body.Should().Be(0.36m);
        r501.UpperWick.Should().Be(0.26m);
        r501.LowerWick.Should().Be(2.05m);
        r501.BodyPct.Should().BeApproximately(0.13483, Money5);
        r501.UpperWickPct.Should().BeApproximately(0.09738, Money5);
        r501.LowerWickPct.Should().BeApproximately(0.76779, Money5);
        r501.IsBullish.Should().BeTrue();
        r501.IsBearish.Should().BeFalse();
    }

    [TestMethod]
    public void ToCandles()
    {
        IReadOnlyList<CandleProperties> candles
            = Bars.ToCandles();

        candles.Should().HaveCount(Bars.Count);
    }
}
