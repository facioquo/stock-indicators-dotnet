namespace StaticSeries;

[TestClass]
public class IchimokuSeriesTests : StaticSeriesTestBase
{
    [TestMethod]
    public override void DefaultParameters_ReturnsExpectedResults()
    {
        const int tenkanPeriods = 9;
        const int kijunPeriods = 26;
        const int senkouBPeriods = 52;

        IReadOnlyList<IchimokuResult> sut = Bars
            .ToIchimoku(tenkanPeriods, kijunPeriods, senkouBPeriods);

        // proper quantities
        sut.Should().HaveCount(502);
        sut.Where(static x => x.TenkanSen != null).Should().HaveCount(494);
        sut.Where(static x => x.KijunSen != null).Should().HaveCount(477);
        sut.Where(static x => x.SenkouSpanA != null).Should().HaveCount(451);
        sut.Where(static x => x.SenkouSpanB != null).Should().HaveCount(425);
        sut.Where(static x => x.ChikouSpan != null).Should().HaveCount(476);

        // sample values
        IchimokuResult r1 = sut[51];
        r1.TenkanSen.Should().Be(224.465);
        r1.KijunSen.Should().Be(221.94);
        r1.SenkouSpanA.Should().Be(214.83249999999998);
        r1.SenkouSpanB.Should().BeNull();
        r1.ChikouSpan.Should().Be(226.35);

        IchimokuResult r2 = sut[249];
        r2.TenkanSen.Should().Be(257.15);
        r2.KijunSen.Should().Be(253.08499999999998);
        r2.SenkouSpanA.Should().Be(246.3125);
        r2.SenkouSpanB.Should().Be(241.685);
        r2.ChikouSpan.Should().Be(259.21);

        IchimokuResult r3 = sut[475];
        r3.TenkanSen.Should().Be(265.575);
        r3.KijunSen.Should().Be(263.965);
        r3.SenkouSpanA.Should().Be(274.9475);
        r3.SenkouSpanB.Should().Be(274.95000000000005);
        r3.ChikouSpan.Should().Be(245.28);

        IchimokuResult r4 = sut[501];
        r4.TenkanSen.Should().Be(241.26);
        r4.KijunSen.Should().Be(251.505);
        r4.SenkouSpanA.Should().Be(264.77);
        r4.SenkouSpanB.Should().Be(269.82);
        r4.ChikouSpan.Should().BeNull();
    }

    [TestMethod]
    public void Extended_WithMoreBars_ReturnsExpectedResult()
    {
        IReadOnlyList<IchimokuResult> sut = Bars
            .ToIchimoku(3, 13, 40, 0, 0);

        sut.Should().HaveCount(502);
    }

    [TestMethod]
    public override void BadBars_DoesNotFail()
    {
        IReadOnlyList<IchimokuResult> r = BadBars
            .ToIchimoku();

        r.Should().HaveCount(502);
    }

    [TestMethod]
    public override void NoBars_ReturnsEmpty()
    {
        IReadOnlyList<IchimokuResult> r0 = Nobars
            .ToIchimoku();

        r0.Should().BeEmpty();

        IReadOnlyList<IchimokuResult> r1 = Onebar
            .ToIchimoku();

        r1.Should().HaveCount(1);
    }

    [TestMethod]
    public void Condense_WithNoThreshold_RemovesNullValues()
    {
        IReadOnlyList<IchimokuResult> sut = Bars
            .ToIchimoku()
            .Condense();

        sut.Should().HaveCount(502);
    }

    [TestMethod]
    public void Exceptions_InvalidLookback_ThrowsArgumentOutOfRangeException()
    {
        // bad signal period
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(
            static () => Bars.ToIchimoku(0));

        // bad short span period
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(
            static () => Bars.ToIchimoku(9, 0));

        // bad long span period
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(
            static () => Bars.ToIchimoku(9, 26, 26));

        // invalid offsets
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(
            static () => Bars.ToIchimoku(9, 26, 52, -1));

        Assert.ThrowsExactly<ArgumentOutOfRangeException>(
            static () => Bars.ToIchimoku(9, 26, 52, -1, 12));

        Assert.ThrowsExactly<ArgumentOutOfRangeException>(
            static () => Bars.ToIchimoku(9, 26, 52, 12, -1));
    }
}
