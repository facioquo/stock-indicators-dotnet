namespace StaticSeries;

[TestClass]
public class PvoSeriesTests : StaticSeriesTestBase
{
    [TestMethod]
    public override void DefaultParameters_ReturnsExpectedResults()
    {
        const int fastPeriods = 12;
        const int slowPeriods = 26;
        const int signalPeriods = 9;

        IReadOnlyList<PvoResult> sut =
            Bars.ToPvo(fastPeriods, slowPeriods, signalPeriods);

        // proper quantities
        sut.Should().HaveCount(502);
        sut.Where(static x => x.Pvo != null).Should().HaveCount(477);
        sut.Where(static x => x.Signal != null).Should().HaveCount(469);
        sut.Where(static x => x.Histogram != null).Should().HaveCount(469);

        // sample values
        PvoResult r1 = sut[24];
        r1.Pvo.Should().BeNull();
        r1.Signal.Should().BeNull();
        r1.Histogram.Should().BeNull();

        PvoResult r2 = sut[33];
        r2.Pvo.Should().BeApproximately(1.5795, Money4);
        r2.Signal.Should().BeApproximately(-3.5530, Money4);
        r2.Histogram.Should().BeApproximately(5.1325, Money4);

        PvoResult r3 = sut[149];
        r3.Pvo.Should().BeApproximately(-7.1910, Money4);
        r3.Signal.Should().BeApproximately(-5.1159, Money4);
        r3.Histogram.Should().BeApproximately(-2.0751, Money4);

        PvoResult r4 = sut[249];
        r4.Pvo.Should().BeApproximately(-6.3667, Money4);
        r4.Signal.Should().BeApproximately(1.7333, Money4);
        r4.Histogram.Should().BeApproximately(-8.1000, Money4);

        PvoResult r5 = sut[501];
        r5.Pvo.Should().BeApproximately(10.4395, Money4);
        r5.Signal.Should().BeApproximately(12.2681, Money4);
        r5.Histogram.Should().BeApproximately(-1.8286, Money4);
    }

    [TestMethod]
    public void ChainFromResults_ToSma_ReturnsExpectedResult()
    {
        IReadOnlyList<SmaResult> sut = Bars
            .ToPvo()
            .ToSma(10);

        sut.Should().HaveCount(502);
        sut.Where(static x => x.Sma != null).Should().HaveCount(468);
    }

    [TestMethod]
    public override void BadBars_DoesNotFail()
    {
        IReadOnlyList<PvoResult> r = BadBars
            .ToPvo(10, 20, 5);

        r.Should().HaveCount(502);
        r.Where(static x => x.Pvo is double v && double.IsNaN(v)).Should().BeEmpty();
    }

    [TestMethod]
    public override void NoBars_ReturnsEmpty()
    {
        IReadOnlyList<PvoResult> r0 = Nobars
            .ToPvo();

        r0.Should().BeEmpty();

        IReadOnlyList<PvoResult> r1 = Onebar
            .ToPvo();

        r1.Should().HaveCount(1);
    }

    [TestMethod]
    public void Removed_WithWarmupPeriods_TruncatesResults()
    {
        const int fastPeriods = 12;
        const int slowPeriods = 26;
        const int signalPeriods = 9;

        IReadOnlyList<PvoResult> sut = Bars
            .ToPvo(fastPeriods, slowPeriods, signalPeriods)
            .RemoveWarmupPeriods();

        // assertions
        sut.Should().HaveCount(502 - (slowPeriods + signalPeriods + 250));

        PvoResult last = sut[^1];
        last.Pvo.Should().BeApproximately(10.4395, Money4);
        last.Signal.Should().BeApproximately(12.2681, Money4);
        last.Histogram.Should().BeApproximately(-1.8286, Money4);
    }

    [TestMethod]
    public void Exceptions_InvalidParameters_ThrowsArgumentOutOfRangeException()
    {
        // bad fast period
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(
            static () => Bars.ToPvo(0));

        // bad slow periods must be larger than faster period
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(
            static () => Bars.ToPvo(12, 12));

        // bad signal period
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(
            static () => Bars.ToPvo(12, 26, -1));
    }
}
