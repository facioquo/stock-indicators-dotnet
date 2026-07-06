namespace StaticSeries;

[TestClass]
public class PrsSeriesTests : StaticSeriesTestBase
{
    [TestMethod]
    public override void DefaultParameters_ReturnsExpectedResults()
    {
        const int lookbackPeriods = 30;

        IReadOnlyList<PrsResult> sut = OtherBars
            .ToPrs(Bars, lookbackPeriods);

        // proper quantities
        sut.Should().HaveCount(502);
        sut.Where(static x => x.Prs != null).Should().HaveCount(502);

        // sample values
        PrsResult r1 = sut[8];
        r1.Prs.Should().BeApproximately(1.108340, Money6);
        r1.PrsPercent.Should().BeNull();

        PrsResult r2 = sut[249];
        r2.Prs.Should().BeApproximately(1.222373, Money6);
        r2.PrsPercent.Should().BeApproximately(-0.023089, Money6);

        PrsResult r3 = sut[501];
        r3.Prs.Should().BeApproximately(1.356817, Money6);
        r3.PrsPercent.Should().BeApproximately(0.037082, Money6);
    }

    [TestMethod]
    public void UseReusable_ClosePrice_ReturnsExpectedResult()
    {
        IReadOnlyList<PrsResult> sut = OtherBars
            .Use(CandlePart.Close)
            .ToPrs(Bars.Use(CandlePart.Close), 20);

        sut.Should().HaveCount(502);
        sut.Where(static x => x.Prs != null).Should().HaveCount(502);
    }

    [TestMethod]
    public void ChainFromResults_ToSma_ReturnsExpectedResult()
    {
        IReadOnlyList<SmaResult> sut = OtherBars
            .ToPrs(Bars, 20)
            .ToSma(10);

        sut.Should().HaveCount(502);
        sut.Where(static x => x.Sma != null).Should().HaveCount(493);
    }

    [TestMethod]
    public void Chainee_FromSma_ReturnsExpectedResult()
    {
        IReadOnlyList<PrsResult> sut = Bars
            .ToSma(2)
            .ToPrs(OtherBars.ToSma(2), 20);

        sut.Should().HaveCount(502);
        sut.Where(static x => x.Prs != null).Should().HaveCount(501);
        sut.Where(static x => x.Prs is double.NaN).Should().BeEmpty();
    }

    [TestMethod]
    public override void BadBars_DoesNotFail()
    {
        IReadOnlyList<PrsResult> r = BadBars
            .ToPrs(BadBars, 15);

        r.Should().HaveCount(502);
        r.Where(static x => x.Prs is double.NaN).Should().BeEmpty();
    }

    [TestMethod]
    public override void NoBars_ReturnsEmpty()
    {
        IReadOnlyList<PrsResult> r0 = Nobars
            .ToPrs(Nobars);

        r0.Should().BeEmpty();

        IReadOnlyList<PrsResult> r1 = Onebar
            .ToPrs(Onebar);

        r1.Should().HaveCount(1);
    }

    [TestMethod]
    public void Exceptions_InvalidParameters_ThrowsArgumentOutOfRangeException()
    {
        // bad lookback period
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(
            static () => OtherBars.ToPrs(Bars, 0));

        // insufficient bars
        Assert.ThrowsExactly<InvalidBarsException>(
            static () => Data.GetCompare(13).ToPrs(Bars, 14));

        // insufficient eval bars
        Assert.ThrowsExactly<InvalidBarsException>(
            static () => Data.GetCompare(300).ToPrs(Bars, 14));

        // mismatch bars
        Assert.ThrowsExactly<InvalidBarsException>(
            static () => OtherBars.ToPrs(MismatchBars, 14));
    }
}
