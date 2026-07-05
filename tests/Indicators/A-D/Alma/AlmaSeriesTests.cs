namespace StaticSeries;

[TestClass]
public class AlmaSeriesTests : StaticSeriesTestBase
{
    [TestMethod]
    public override void DefaultParameters_ReturnsExpectedResults()
    {
        const int lookbackPeriods = 10;
        const double offset = 0.85;
        const double sigma = 6;

        IReadOnlyList<AlmaResult> sut = Bars
            .ToAlma(lookbackPeriods, offset, sigma);

        // proper quantities
        sut.Should().HaveCount(502);
        sut.Where(static x => x.Alma != null).Should().HaveCount(493);

        // sample values
        AlmaResult r1 = sut[8];
        r1.Alma.Should().BeNull();

        AlmaResult r2 = sut[9];
        r2.Alma.Should().BeApproximately(214.1839, Money4);

        AlmaResult r3 = sut[24];
        r3.Alma.Should().BeApproximately(216.0619, Money4);

        AlmaResult r4 = sut[149];
        r4.Alma.Should().BeApproximately(235.8609, Money4);

        AlmaResult r5 = sut[249];
        r5.Alma.Should().BeApproximately(257.5787, Money4);

        AlmaResult r6 = sut[501];
        r6.Alma.Should().BeApproximately(242.1871, Money4);
    }

    [TestMethod]
    public void UseReusable_ClosePrice_ReturnsExpectedResult()
    {
        IReadOnlyList<AlmaResult> sut = Bars
            .Use(CandlePart.Close)
            .ToAlma(10);

        sut.Should().HaveCount(502);
        sut.Where(static x => x.Alma != null).Should().HaveCount(493);

        AlmaResult last = sut[^1];
        last.Alma.Should().BeApproximately(242.1871, Money4);
    }

    [TestMethod]
    public void Chainee_FromSma_ReturnsExpectedResult()
    {
        IReadOnlyList<AlmaResult> sut = Bars
            .ToSma(2)
            .ToAlma(10);

        sut.Should().HaveCount(502);
        sut.Where(static x => x.Alma != null).Should().HaveCount(492);
    }

    [TestMethod]
    public void ChainFromResults_ToSma_ReturnsExpectedResult()
    {
        const int lookbackPeriods = 10;
        const double offset = 0.85;
        const double sigma = 6;

        IReadOnlyList<SmaResult> sut = Bars
            .ToAlma(lookbackPeriods, offset, sigma)
            .ToSma(10);

        sut.Should().HaveCount(502);
        sut.Where(static x => x.Sma != null).Should().HaveCount(484);
    }

    [TestMethod]
    public void NaN_WithNaNInputs_HandlesGracefully()
    {
        IReadOnlyList<AlmaResult> r1
            = Data.GetBtcUsdNan().ToAlma();

        r1.Where(static x => x.Alma is double.NaN).Should().BeEmpty();

        IReadOnlyList<AlmaResult> r2
            = Data.GetBtcUsdNan().ToAlma(20);

        r2.Where(static x => x.Alma is double.NaN).Should().BeEmpty();
    }

    [TestMethod]
    public override void BadBars_DoesNotFail()
    {
        IReadOnlyList<AlmaResult> r = BadBars.ToAlma(14, 0.5, 3);

        r.Should().HaveCount(502);
        r.Where(static x => x.Alma is double.NaN).Should().BeEmpty();
    }

    [TestMethod]
    public override void NoBars_ReturnsEmpty()
    {
        IReadOnlyList<AlmaResult> r0 = Nobars.ToAlma();

        r0.Should().BeEmpty();

        IReadOnlyList<AlmaResult> r1 = Onebar.ToAlma();

        r1.Should().HaveCount(1);
    }

    [TestMethod]
    public void Removed_WithWarmupPeriods_TruncatesResults()
    {
        IReadOnlyList<AlmaResult> sut = Bars
            .ToAlma(10)
            .RemoveWarmupPeriods();

        // assertions
        sut.Should().HaveCount(502 - 9);

        AlmaResult last = sut[^1];
        last.Alma.Should().BeApproximately(242.1871, Money4);
    }

    [TestMethod]
    public void Exceptions_InvalidParameters_ThrowsArgumentOutOfRangeException()
    {
        // bad lookback period
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(
            static () => Bars.ToAlma(0, 1, 5));

        // bad offset
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(
            static () => Bars.ToAlma(15, 1.1, 3));

        // bad sigma
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(
            static () => Bars.ToAlma(10, 0.5, 0));
    }
}
