namespace Utilities;

[TestClass]
[TestCategory("Utilities")]
public class BarIntervalCodes : TestBase
{
    [TestMethod]
    public void ToCode_ForEachInterval_ReturnsCanonicalCode()
    {
        Assert.AreEqual("1m", BarInterval.OneMinute.ToCode());
        Assert.AreEqual("5m", BarInterval.FiveMinutes.ToCode());
        Assert.AreEqual("1h", BarInterval.OneHour.ToCode());
        Assert.AreEqual("1d", BarInterval.Day.ToCode());
        Assert.AreEqual("1w", BarInterval.Week.ToCode());
        Assert.AreEqual("1mo", BarInterval.Month.ToCode());
    }

    [TestMethod]
    public void ToBarInterval_WithCanonicalCode_RoundTrips()
    {
        foreach (BarInterval interval in Enum.GetValues<BarInterval>())
        {
            string code = interval.ToCode();
            Assert.AreEqual(interval, code.ToBarInterval());
        }
    }

    [TestMethod]
    public void ToBarInterval_WithAliasesAndCasing_ReturnsExpected()
    {
        Assert.AreEqual(BarInterval.FiveMinutes, "5min".ToBarInterval());
        Assert.AreEqual(BarInterval.FiveMinutes, "5MINUTES".ToBarInterval());
        Assert.AreEqual(BarInterval.Day, "1day".ToBarInterval());
        Assert.AreEqual(BarInterval.Day, " day ".ToBarInterval());
        Assert.AreEqual(BarInterval.OneHour, "1HR".ToBarInterval());
    }

    [TestMethod]
    public void ToCode_WithUndefinedInterval_ThrowsArgumentOutOfRangeException()
        => Assert.ThrowsExactly<ArgumentOutOfRangeException>(
            static () => ((BarInterval)99).ToCode());

    [TestMethod]
    public void ToBarInterval_WithUnknownCode_ThrowsArgumentException()
        => Assert.ThrowsExactly<ArgumentException>(static () => "13x".ToBarInterval());

    [TestMethod]
    public void TryToBarInterval_WithNullOrEmpty_ReturnsFalse()
    {
        Assert.IsFalse(BarIntervals.TryToBarInterval(null, out _));
        Assert.IsFalse(BarIntervals.TryToBarInterval("", out _));
        Assert.IsFalse(BarIntervals.TryToBarInterval("nope", out _));
    }
}
