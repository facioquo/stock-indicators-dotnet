namespace StaticSeries;

[TestClass]
public class GatorSeriesTests : StaticSeriesTestBase
{
    [TestMethod]
    public override void DefaultParameters_ReturnsExpectedResults()
    {
        IReadOnlyList<GatorResult> sut = Bars
            .ToGator();

        // proper quantities
        sut.Should().HaveCount(502);
        sut.Where(static x => x.Upper != null).Should().HaveCount(482);
        sut.Where(static x => x.Lower != null).Should().HaveCount(490);
        sut.Where(static x => x.UpperIsExpanding != null).Should().HaveCount(481);
        sut.Where(static x => x.LowerIsExpanding != null).Should().HaveCount(489);

        // sample values
        GatorResult r11 = sut[11];
        r11.Upper.Should().BeNull();
        r11.Lower.Should().BeNull();
        r11.UpperIsExpanding.Should().BeNull();
        r11.LowerIsExpanding.Should().BeNull();

        GatorResult r12 = sut[12];
        r12.Upper.Should().BeNull();
        (r12.Lower.Value).Should().BeApproximately(-0.1402, Money4);
        r12.UpperIsExpanding.Should().BeNull();
        r12.LowerIsExpanding.Should().BeNull();

        GatorResult r13 = sut[13];
        r13.Upper.Should().BeNull();
        (r13.Lower.Value).Should().BeApproximately(-0.0406, Money4);
        r13.UpperIsExpanding.Should().BeNull();
        r13.LowerIsExpanding.Should().BeFalse();

        GatorResult r19 = sut[19];
        r19.Upper.Should().BeNull();
        (r19.Lower.Value).Should().BeApproximately(-1.0018, Money4);
        r19.UpperIsExpanding.Should().BeNull();
        r19.LowerIsExpanding.Should().BeTrue();

        GatorResult r20 = sut[20];
        (r20.Upper.Value).Should().BeApproximately(0.4004, Money4);
        (r20.Lower.Value).Should().BeApproximately(-1.0130, Money4);
        r20.UpperIsExpanding.Should().BeNull();
        r20.LowerIsExpanding.Should().BeTrue();

        GatorResult r21 = sut[21];
        (r21.Upper.Value).Should().BeApproximately(0.7298, Money4);
        (r21.Lower.Value).Should().BeApproximately(-0.6072, Money4);
        r21.UpperIsExpanding.Should().BeTrue();
        r21.LowerIsExpanding.Should().BeFalse();

        GatorResult r99 = sut[99];
        (r99.Upper.Value).Should().BeApproximately(0.5159, Money4);
        (r99.Lower.Value).Should().BeApproximately(-0.2320, Money4);
        r99.UpperIsExpanding.Should().BeFalse();
        r99.LowerIsExpanding.Should().BeTrue();

        GatorResult r249 = sut[249];
        (r249.Upper.Value).Should().BeApproximately(3.1317, Money4);
        (r249.Lower.Value).Should().BeApproximately(-1.8058, Money4);
        r249.UpperIsExpanding.Should().BeTrue();
        r249.LowerIsExpanding.Should().BeFalse();

        GatorResult r501 = sut[501];
        (r501.Upper.Value).Should().BeApproximately(7.4538, Money4);
        (r501.Lower.Value).Should().BeApproximately(-9.2399, Money4);
        r501.UpperIsExpanding.Should().BeTrue();
        r501.LowerIsExpanding.Should().BeTrue();
    }

    [TestMethod]
    public void FromAlligator_WithStandardBars_ReturnsExpectedResult()
    {
        IReadOnlyList<GatorResult> sut = Bars
            .ToAlligator()
            .ToGator();

        // proper quantities
        sut.Should().HaveCount(502);
        sut.Where(static x => x.Upper != null).Should().HaveCount(482);
        sut.Where(static x => x.Lower != null).Should().HaveCount(490);
        sut.Where(static x => x.UpperIsExpanding != null).Should().HaveCount(481);
        sut.Where(static x => x.LowerIsExpanding != null).Should().HaveCount(489);

        // sample values
        GatorResult r11 = sut[11];
        r11.Upper.Should().BeNull();
        r11.Lower.Should().BeNull();
        r11.UpperIsExpanding.Should().BeNull();
        r11.LowerIsExpanding.Should().BeNull();

        GatorResult r12 = sut[12];
        r12.Upper.Should().BeNull();
        (r12.Lower.Value).Should().BeApproximately(-0.1402, Money4);
        r12.UpperIsExpanding.Should().BeNull();
        r12.LowerIsExpanding.Should().BeNull();

        GatorResult r13 = sut[13];
        r13.Upper.Should().BeNull();
        (r13.Lower.Value).Should().BeApproximately(-0.0406, Money4);
        r13.UpperIsExpanding.Should().BeNull();
        r13.LowerIsExpanding.Should().BeFalse();

        GatorResult r19 = sut[19];
        r19.Upper.Should().BeNull();
        (r19.Lower.Value).Should().BeApproximately(-1.0018, Money4);
        r19.UpperIsExpanding.Should().BeNull();
        r19.LowerIsExpanding.Should().BeTrue();

        GatorResult r20 = sut[20];
        (r20.Upper.Value).Should().BeApproximately(0.4004, Money4);
        (r20.Lower.Value).Should().BeApproximately(-1.0130, Money4);
        r20.UpperIsExpanding.Should().BeNull();
        r20.LowerIsExpanding.Should().BeTrue();

        GatorResult r21 = sut[21];
        (r21.Upper.Value).Should().BeApproximately(0.7298, Money4);
        (r21.Lower.Value).Should().BeApproximately(-0.6072, Money4);
        r21.UpperIsExpanding.Should().BeTrue();
        r21.LowerIsExpanding.Should().BeFalse();

        GatorResult r99 = sut[99];
        (r99.Upper.Value).Should().BeApproximately(0.5159, Money4);
        (r99.Lower.Value).Should().BeApproximately(-0.2320, Money4);
        r99.UpperIsExpanding.Should().BeFalse();
        r99.LowerIsExpanding.Should().BeTrue();

        GatorResult r249 = sut[249];
        (r249.Upper.Value).Should().BeApproximately(3.1317, Money4);
        (r249.Lower.Value).Should().BeApproximately(-1.8058, Money4);
        r249.UpperIsExpanding.Should().BeTrue();
        r249.LowerIsExpanding.Should().BeFalse();

        GatorResult r501 = sut[501];
        (r501.Upper.Value).Should().BeApproximately(7.4538, Money4);
        (r501.Lower.Value).Should().BeApproximately(-9.2399, Money4);
        r501.UpperIsExpanding.Should().BeTrue();
        r501.LowerIsExpanding.Should().BeTrue();
    }

    [TestMethod]
    public void UseReusable_ClosePrice_ReturnsExpectedResult()
    {
        IReadOnlyList<GatorResult> sut = Bars
            .Use(CandlePart.Close)
            .ToGator();

        sut.Should().HaveCount(502);
        sut.Where(static x => x.Upper != null).Should().HaveCount(482);
    }

    [TestMethod]
    public void Chainee_FromSma_ReturnsExpectedResult()
    {
        IReadOnlyList<GatorResult> sut = Bars
            .ToSma(2)
            .ToGator();

        sut.Should().HaveCount(502);
        sut.Where(static x => x.Upper != null).Should().HaveCount(481);
    }

    [TestMethod]
    public override void BadBars_DoesNotFail()
    {
        IReadOnlyList<GatorResult> r = BadBars
            .ToGator();

        r.Should().HaveCount(502);
        r.Where(static x => x.Upper is double v && double.IsNaN(v)).Should().BeEmpty();
    }

    [TestMethod]
    public override void NoBars_ReturnsEmpty()
    {
        IReadOnlyList<GatorResult> r0 = Nobars
            .ToGator();

        r0.Should().BeEmpty();

        IReadOnlyList<GatorResult> r1 = Onebar
            .ToGator();

        r1.Should().HaveCount(1);
    }

    [TestMethod]
    public void Condense_WithNoThreshold_RemovesNullValues()
    {
        IReadOnlyList<GatorResult> sut = Bars
            .ToGator()
            .Condense();

        // assertions
        sut.Should().HaveCount(490);

        GatorResult last = sut[^1];
        (last.Upper.Value).Should().BeApproximately(7.4538, Money4);
        (last.Lower.Value).Should().BeApproximately(-9.2399, Money4);
        last.UpperIsExpanding.Should().BeTrue();
        last.LowerIsExpanding.Should().BeTrue();
    }

    [TestMethod]
    public void Removed_WithWarmupPeriods_TruncatesResults()
    {
        IReadOnlyList<GatorResult> sut = Bars
            .ToGator()
            .RemoveWarmupPeriods();

        // assertions
        sut.Should().HaveCount(502 - 150);

        GatorResult last = sut[^1];
        (last.Upper.Value).Should().BeApproximately(7.4538, Money4);
        (last.Lower.Value).Should().BeApproximately(-9.2399, Money4);
        last.UpperIsExpanding.Should().BeTrue();
        last.LowerIsExpanding.Should().BeTrue();
    }
}
