namespace StaticSeries;

[TestClass]
public class HtTrendlineSeriesTests : StaticSeriesTestBase
{
    [TestMethod]
    public override void DefaultParameters_ReturnsExpectedResults()
    {
        IReadOnlyList<HtlResult> sut = Bars
            .ToHtTrendline();

        // proper quantities
        // should always be the same number of sut as there is bars
        sut.Should().HaveCount(502);
        sut.Where(static x => x.DcPeriods != null).Should().HaveCount(495);
        sut.Where(static x => x.Trendline != null).Should().HaveCount(502);
        sut.Where(static x => x.SmoothPrice != null).Should().HaveCount(496);

        // sample values
        HtlResult r5 = sut[5];
        r5.DcPeriods.Should().BeNull();
        r5.Trendline.Should().Be(214.205);
        r5.SmoothPrice.Should().BeNull();

        HtlResult r6 = sut[6];
        r6.DcPeriods.Should().BeNull();
        r6.Trendline.Should().Be(213.84);
        r6.SmoothPrice.Should().Be(214.071);

        HtlResult r7 = sut[7];
        r7.DcPeriods.Should().Be(1);

        HtlResult r11 = sut[11];
        r11.DcPeriods.Should().Be(3);
        r11.Trendline.Should().BeApproximately(213.9502, Money4);
        r11.SmoothPrice.Should().BeApproximately(213.8460, Money4);

        HtlResult r25 = sut[25];
        r25.DcPeriods.Should().Be(14);
        r25.Trendline.Should().BeApproximately(215.3948, Money4);
        r25.SmoothPrice.Should().BeApproximately(216.3365, Money4);

        HtlResult r149 = sut[149];
        r149.DcPeriods.Should().Be(24);
        r149.Trendline.Should().BeApproximately(233.9410, Money4);
        r149.SmoothPrice.Should().BeApproximately(235.8570, Money4);

        HtlResult r249 = sut[249];
        r249.DcPeriods.Should().Be(25);
        r249.Trendline.Should().BeApproximately(253.8788, Money4);
        r249.SmoothPrice.Should().BeApproximately(257.5825, Money4);

        HtlResult r501 = sut[501];
        r501.DcPeriods.Should().Be(20);
        r501.Trendline.Should().BeApproximately(252.2172, Money4);
        r501.SmoothPrice.Should().BeApproximately(242.3435, Money4);
    }

    [TestMethod]
    public void UseReusable_ClosePrice_ReturnsExpectedResult()
    {
        IReadOnlyList<HtlResult> sut = Bars
            .Use(CandlePart.Close)
            .ToHtTrendline();

        sut.Should().HaveCount(502);
        sut.Where(static x => x.Trendline != null).Should().HaveCount(502);
    }

    [TestMethod]
    public void Chainee_FromSma_ReturnsExpectedResult()
    {
        IReadOnlyList<HtlResult> sut = Bars
            .ToSma(2)
            .ToHtTrendline();

        sut.Should().HaveCount(502);
        sut.Where(static x => x.Trendline != null).Should().HaveCount(501);
    }

    [TestMethod]
    public void ChainFromResults_ToSma_ReturnsExpectedResult()
    {
        IReadOnlyList<SmaResult> sut = Bars
            .ToHtTrendline()
            .ToSma(10);

        sut.Should().HaveCount(502);
        sut.Where(static x => x.Sma != null).Should().HaveCount(493);
    }

    [TestMethod]
    public override void BadBars_DoesNotFail()
    {
        IReadOnlyList<HtlResult> r = BadBars
            .ToHtTrendline();

        r.Should().HaveCount(502);
        r.Where(static x => x.Trendline is double v && double.IsNaN(v)).Should().BeEmpty();
    }

    [TestMethod]
    public void Removed_WithWarmupPeriods_TruncatesResults()
    {
        IReadOnlyList<HtlResult> sut = Bars
            .ToHtTrendline()
            .RemoveWarmupPeriods();

        // assertions
        sut.Should().HaveCount(502 - 100);

        HtlResult last = sut[^1];
        last.Trendline.Should().BeApproximately(252.2172, Money4);
        last.SmoothPrice.Should().BeApproximately(242.3435, Money4);
    }

    [TestMethod]
    public void PennyData_WithSmallPrices_ReturnsExpectedResult()
    {
        IReadOnlyList<Bar> penny = Data.GetPenny();

        IReadOnlyList<HtlResult> r = penny
            .ToHtTrendline();

        r.Should().HaveCount(533);
    }

    [TestMethod]
    public override void NoBars_ReturnsEmpty()
    {
        IReadOnlyList<HtlResult> r0 = Nobars
            .ToHtTrendline();

        r0.Should().BeEmpty();

        IReadOnlyList<HtlResult> r1 = Onebar
            .ToHtTrendline();

        r1.Should().HaveCount(1);
    }
}
