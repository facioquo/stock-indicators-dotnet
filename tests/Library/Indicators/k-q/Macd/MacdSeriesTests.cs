namespace StaticSeries;

[TestClass]
public class MacdSeriesTests : StaticSeriesTestBase
{
    [TestMethod]
    public override void DefaultParameters_ReturnsExpectedResults()
    {
        const int fastPeriods = 12;
        const int slowPeriods = 26;
        const int signalPeriods = 9;

        IReadOnlyList<MacdResult> sut =
            Bars.ToMacd(fastPeriods, slowPeriods, signalPeriods);

        // proper quantities
        sut.Should().HaveCount(502);
        sut.Where(static x => x.Macd != null).Should().HaveCount(477);
        sut.Where(static x => x.Signal != null).Should().HaveCount(469);
        sut.Where(static x => x.Histogram != null).Should().HaveCount(469);

        // sample values
        MacdResult r49 = sut[49];
        r49.Macd.Should().BeApproximately(1.7203, Money4);
        r49.Signal.Should().BeApproximately(1.9675, Money4);
        r49.Histogram.Should().BeApproximately(-0.2472, Money4);
        r49.FastEma.Should().BeApproximately(224.1840, Money4);
        r49.SlowEma.Should().BeApproximately(222.4637, Money4);

        MacdResult r249 = sut[249];
        r249.Macd.Should().BeApproximately(2.2353, Money4);
        r249.Signal.Should().BeApproximately(2.3141, Money4);
        r249.Histogram.Should().BeApproximately(-0.0789, Money4);
        r249.FastEma.Should().BeApproximately(256.6780, Money4);
        r249.SlowEma.Should().BeApproximately(254.4428, Money4);

        MacdResult r501 = sut[501];
        r501.Macd.Should().BeApproximately(-6.2198, Money4);
        r501.Signal.Should().BeApproximately(-5.8569, Money4);
        r501.Histogram.Should().BeApproximately(-0.3629, Money4);
        r501.FastEma.Should().BeApproximately(245.4957, Money4);
        r501.SlowEma.Should().BeApproximately(251.7155, Money4);
    }

    [TestMethod]
    public void UseReusable_ClosePrice_ReturnsExpectedResult()
    {
        IReadOnlyList<MacdResult> sut = Bars
            .Use(CandlePart.Close)
            .ToMacd();

        sut.Should().HaveCount(502);
        sut.Where(static x => x.Macd != null).Should().HaveCount(477);
    }

    [TestMethod]
    public void Chainee_FromSma_ReturnsExpectedResult()
    {
        IReadOnlyList<MacdResult> sut = Bars
            .ToSma(2)
            .ToMacd();

        sut.Should().HaveCount(502);
        sut.Where(static x => x.Macd != null).Should().HaveCount(476);
    }

    [TestMethod]
    public void ChainFromResults_ToSma_ReturnsExpectedResult()
    {
        IReadOnlyList<SmaResult> sut = Bars
            .ToMacd()
            .ToSma(10);

        sut.Should().HaveCount(502);
        sut.Where(static x => x.Sma != null).Should().HaveCount(468);
    }

    [TestMethod]
    public override void BadBars_DoesNotFail()
    {
        IReadOnlyList<MacdResult> r = BadBars
            .ToMacd(10, 20, 5);

        r.Should().HaveCount(502);
        r.Where(static x => x.Macd is double v && double.IsNaN(v)).Should().BeEmpty();
    }

    [TestMethod]
    public override void NoBars_ReturnsEmpty()
    {
        IReadOnlyList<MacdResult> r0 = Nobars
            .ToMacd();

        r0.Should().BeEmpty();

        IReadOnlyList<MacdResult> r1 = Onebar
            .ToMacd();

        r1.Should().HaveCount(1);
    }

    [TestMethod]
    public void Removed_WithWarmupPeriods_TruncatesResults()
    {
        const int fastPeriods = 12;
        const int slowPeriods = 26;
        const int signalPeriods = 9;

        IReadOnlyList<MacdResult> sut = Bars
            .ToMacd(fastPeriods, slowPeriods, signalPeriods)
            .RemoveWarmupPeriods();

        // assertions
        sut.Should().HaveCount(502 - (slowPeriods + signalPeriods + 250));

        MacdResult last = sut[^1];
        last.Macd.Should().BeApproximately(-6.2198, Money4);
        last.Signal.Should().BeApproximately(-5.8569, Money4);
        last.Histogram.Should().BeApproximately(-0.3629, Money4);
    }

    [TestMethod]
    public void Exceptions_InvalidParameters_ThrowsArgumentOutOfRangeException()
    {
        // bad fast period
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(
            static () => Bars.ToMacd(0));

        // bad slow periods must be larger than faster period
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(
            static () => Bars.ToMacd(12, 12));

        // bad signal period
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(
            static () => Bars.ToMacd(12, 26, -1));
    }
}
