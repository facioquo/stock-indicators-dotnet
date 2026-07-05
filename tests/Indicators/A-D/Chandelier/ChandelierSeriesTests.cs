namespace StaticSeries;

[TestClass]
public class ChandelierSeriesTests : StaticSeriesTestBase
{
    [TestMethod]
    public override void DefaultParameters_ReturnsExpectedResults()
    {
        const int lookbackPeriods = 22;

        IReadOnlyList<ChandelierResult> longResult =
            Bars.ToChandelier(lookbackPeriods);

        // proper quantities
        longResult.Should().HaveCount(502);
        longResult.Where(static x => x.ChandelierExit != null).Should().HaveCount(480);

        // sample values (long)
        ChandelierResult a = longResult[501];
        a.ChandelierExit.Should().BeApproximately(256.5860, Money4);

        ChandelierResult b = longResult[492];
        b.ChandelierExit.Should().BeApproximately(259.0480, Money4);

        // short
        IReadOnlyList<ChandelierResult> shortResult =
            Bars.ToChandelier(lookbackPeriods, 3, Direction.Short);

        ChandelierResult c = shortResult[501];
        c.ChandelierExit.Should().BeApproximately(246.4240, Money4);
    }

    [TestMethod]
    public void ChainFromResults_ToSma_ReturnsExpectedResult()
    {
        IReadOnlyList<SmaResult> sut = Bars
            .ToChandelier()
            .ToSma(10);

        sut.Should().HaveCount(502);
        sut.Where(static x => x.Sma != null).Should().HaveCount(471);
    }

    [TestMethod]
    public override void BadBars_DoesNotFail()
    {
        IReadOnlyList<ChandelierResult> r = BadBars
            .ToChandelier(15, 2);

        r.Should().HaveCount(502);
        r.Where(static x => x.ChandelierExit is double.NaN).Should().BeEmpty();
    }

    [TestMethod]
    public override void NoBars_ReturnsEmpty()
    {
        IReadOnlyList<ChandelierResult> r0 = Nobars
            .ToChandelier();

        r0.Should().BeEmpty();

        IReadOnlyList<ChandelierResult> r1 = Onebar
            .ToChandelier();

        r1.Should().HaveCount(1);
    }

    [TestMethod]
    public void Removed_WithWarmupPeriods_TruncatesResults()
    {
        IReadOnlyList<ChandelierResult> sut = Bars
            .ToChandelier()
            .RemoveWarmupPeriods();

        // assertions
        sut.Should().HaveCount(502 - 22);

        ChandelierResult last = sut[^1];
        last.ChandelierExit.Should().BeApproximately(256.5860, Money4);
    }

    [TestMethod]
    public void Exceptions_InvalidParameters_ThrowsArgumentOutOfRangeException()
    {
        // bad lookback period
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(
            static () => Bars.ToChandelier(0));

        // bad multiplier
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(
            static () => Bars.ToChandelier(25, 0));

        // bad type
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(
            static () => Bars.ToChandelier(25, 2, (Direction)int.MaxValue));
    }
}
