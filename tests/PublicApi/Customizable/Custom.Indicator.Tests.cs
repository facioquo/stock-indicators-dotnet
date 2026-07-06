using System.Globalization;
using Sut;

namespace Customization;

// CUSTOM INDICATORS

[TestClass, TestCategory("Integration")]
public class CustomIndicators
{
    private static readonly CultureInfo EnglishCulture = new("en-US", false);

    private static readonly IReadOnlyList<Bar> bars = Data.GetDefault();
    private static readonly IReadOnlyList<Bar> badBars = Data.GetBad();
    private static readonly IReadOnlyList<Bar> onebar = Data.GetDefault(1);
    private static readonly IReadOnlyList<Bar> nobars = [];

    [TestMethod]
    public void Standard()
    {
        IReadOnlyList<CustomReusable> results = bars
            .GetIndicator(20);

        // proper quantities
        Assert.HasCount(502, results);
        Assert.HasCount(483, results.Where(static x => x.Sma != null));

        // sample values
        Assert.IsNull(results[18].Sma);
        Assert.AreEqual(214.5250, Math.Round(results[19].Sma.Value, 4));
        Assert.AreEqual(215.0310, Math.Round(results[24].Sma.Value, 4));
        Assert.AreEqual(234.9350, Math.Round(results[149].Sma.Value, 4));
        Assert.AreEqual(255.5500, Math.Round(results[249].Sma.Value, 4));
        Assert.AreEqual(251.8600, Math.Round(results[501].Sma.Value, 4));
    }

    [TestMethod]
    public void CandlePartOpen()
    {
        IReadOnlyList<CustomReusable> results = bars
            .Use(CandlePart.Open)
            .GetIndicator(20);

        Assert.HasCount(502, results);
        Assert.HasCount(483, results.Where(static x => x.Sma != null));

        // sample values
        Assert.IsNull(results[18].Sma);
        Assert.AreEqual(214.3795, Math.Round(results[19].Sma.Value, 4));
        Assert.AreEqual(214.9535, Math.Round(results[24].Sma.Value, 4));
        Assert.AreEqual(234.8280, Math.Round(results[149].Sma.Value, 4));
        Assert.AreEqual(255.6915, Math.Round(results[249].Sma.Value, 4));
        Assert.AreEqual(253.1725, Math.Round(results[501].Sma.Value, 4));
    }

    [TestMethod]
    public void CandlePartVolume()
    {
        IReadOnlyList<CustomReusable> results = bars
            .Use(CandlePart.Volume)
            .GetIndicator(20);

        Assert.HasCount(502, results);
        Assert.HasCount(483, results.Where(static x => x.Sma != null));

        // sample values
        CustomReusable r24 = results[24];
        Assert.AreEqual(77293768.2, r24.Sma);

        CustomReusable r290 = results[290];
        Assert.AreEqual(157958070.8, r290.Sma);

        CustomReusable r501 = results[501];
        Assert.AreEqual(DateTime.ParseExact("12/31/2018", "MM/dd/yyyy", EnglishCulture), r501.Timestamp);
        Assert.AreEqual(163695200, r501.Sma);
    }

    [TestMethod]
    public void ChainingFromResults_WorksAsExpected()
    {
        IReadOnlyList<EmaResult> results = bars
            .GetIndicator(10)
            .ToEma(10);

        Assert.HasCount(502, results);
        Assert.HasCount(484, results.Where(static x => x.Ema != null));
    }

    [TestMethod]
    public void BarToSortedList()
    {
        IReadOnlyList<Bar> mismatch = Data.GetMismatch();

        IReadOnlyList<Bar> h = mismatch.ToSortedList();

        // proper quantities
        Assert.HasCount(502, h);

        // check first date
        DateTime firstDate = DateTime.ParseExact("01/18/2016", "MM/dd/yyyy", EnglishCulture);
        Assert.AreEqual(firstDate, h[0].Timestamp);

        // check last date
        DateTime lastDate = DateTime.ParseExact("12/31/2018", "MM/dd/yyyy", EnglishCulture);
        Assert.AreEqual(lastDate, h[^1].Timestamp);

        // spot check an out of sequence date
        DateTime spotDate = DateTime.ParseExact("03/16/2017", "MM/dd/yyyy", EnglishCulture);
        Assert.AreEqual(spotDate, h[50].Timestamp);
    }

    [TestMethod]
    public void NaN()
    {
        IReadOnlyList<CustomReusable> r = Data.GetBtcUsdNan()
            .GetIndicator(50);

        Assert.IsEmpty(r.Where(static x => x.Sma is double.NaN));
    }

    [TestMethod]
    public void BadData()
    {
        IReadOnlyList<CustomReusable> r = badBars
            .GetIndicator(15);

        Assert.HasCount(502, r);
        Assert.HasCount(0, r.Where(static x => x.Sma is double.NaN));
    }

    [TestMethod]
    public void NoBarsExist()
    {
        IReadOnlyList<CustomReusable> r0 = nobars
            .GetIndicator(5);

        Assert.IsEmpty(r0);

        IReadOnlyList<CustomReusable> r1 = onebar
            .GetIndicator(5);

        Assert.HasCount(1, r1);
    }

    [TestMethod]
    public void Removed()
    {
        IReadOnlyList<CustomReusable> results = bars
            .GetIndicator(20)
            .RemoveWarmupPeriods(19);

        Assert.HasCount(502 - 19, results);
        Assert.AreEqual(251.8600, Math.Round(results[^1].Sma.Value, 4));
    }

    // bad lookback period
    [TestMethod]
    public void Exceptions()
        => Assert.ThrowsExactly<ArgumentOutOfRangeException>(
            static () => bars.GetIndicator(0));
}
