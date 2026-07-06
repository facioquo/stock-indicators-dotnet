namespace StaticSeries;

[TestClass]
public class MaEnvelopesSeriesTests : StaticSeriesTestBase
{
    /// <summary>
    /// SMA
    /// </summary>
    [TestMethod]
    public override void DefaultParameters_ReturnsExpectedResults()
    {
        IReadOnlyList<MaEnvelopeResult> sut =
            Bars.ToMaEnvelopes(20);

        // proper quantities
        sut.Should().HaveCount(502);
        sut.Where(static x => x.Centerline != null).Should().HaveCount(483);

        // sample values
        MaEnvelopeResult r1 = sut[24];
        r1.Centerline.Should().BeApproximately(215.0310, Money4);
        r1.UpperEnvelope.Should().BeApproximately(220.4068, Money4);
        r1.LowerEnvelope.Should().BeApproximately(209.6552, Money4);

        MaEnvelopeResult r2 = sut[249];
        r2.Centerline.Should().BeApproximately(255.5500, Money4);
        r2.UpperEnvelope.Should().BeApproximately(261.9388, Money4);
        r2.LowerEnvelope.Should().BeApproximately(249.16125, Money5);

        MaEnvelopeResult r3 = sut[501];
        r3.Centerline.Should().BeApproximately(251.8600, Money4);
        r3.UpperEnvelope.Should().BeApproximately(258.1565, Money4);
        r3.LowerEnvelope.Should().BeApproximately(245.5635, Money4);
    }

    [TestMethod]
    public void Alma_AsMaType_ReturnsExpectedResult()
    {
        IReadOnlyList<MaEnvelopeResult> sut =
            Bars.ToMaEnvelopes(10, 2.5, MaType.ALMA);

        // proper quantities
        sut.Should().HaveCount(502);
        sut.Where(static x => x.Centerline != null).Should().HaveCount(493);

        // sample values
        MaEnvelopeResult r1 = sut[24];
        r1.Centerline.Should().BeApproximately(216.0619, Money4);
        r1.UpperEnvelope.Should().BeApproximately(221.4635, Money4);
        r1.LowerEnvelope.Should().BeApproximately(210.6604, Money4);

        MaEnvelopeResult r2 = sut[249];
        r2.Centerline.Should().BeApproximately(257.5787, Money4);
        r2.UpperEnvelope.Should().BeApproximately(264.0182, Money4);
        r2.LowerEnvelope.Should().BeApproximately(251.1393, Money4);

        MaEnvelopeResult r3 = sut[501];
        r3.Centerline.Should().BeApproximately(242.1871, Money4);
        r3.UpperEnvelope.Should().BeApproximately(248.2418, Money4);
        r3.LowerEnvelope.Should().BeApproximately(236.1324, Money4);
    }

    [TestMethod]
    public void Dema_AsMaType_ReturnsExpectedResult()
    {
        IReadOnlyList<MaEnvelopeResult> sut =
            Bars.ToMaEnvelopes(20, 2.5, MaType.DEMA);

        // proper quantities
        sut.Should().HaveCount(502);
        sut.Where(static x => x.Centerline != null).Should().HaveCount(483);

        // sample values
        MaEnvelopeResult r1 = sut[38];
        r1.Centerline.Should().BeApproximately(223.4594, Money4);
        r1.UpperEnvelope.Should().BeApproximately(229.0459, Money4);
        r1.LowerEnvelope.Should().BeApproximately(217.8730, Money4);

        MaEnvelopeResult r2 = sut[249];
        r2.Centerline.Should().BeApproximately(258.4452, Money4);
        r2.UpperEnvelope.Should().BeApproximately(264.9064, Money4);
        r2.LowerEnvelope.Should().BeApproximately(251.9841, Money4);

        MaEnvelopeResult r3 = sut[501];
        r3.Centerline.Should().BeApproximately(241.1677, Money4);
        r3.UpperEnvelope.Should().BeApproximately(247.1969, Money4);
        r3.LowerEnvelope.Should().BeApproximately(235.1385, Money4);
    }

    [TestMethod]
    public void Epma_AsMaType_ReturnsExpectedResult()
    {
        IReadOnlyList<MaEnvelopeResult> sut =
            Bars.ToMaEnvelopes(20, 2.5, MaType.EPMA);

        // proper quantities
        sut.Should().HaveCount(502);
        sut.Where(static x => x.Centerline != null).Should().HaveCount(483);

        // sample values
        MaEnvelopeResult r1 = sut[24];
        r1.Centerline.Should().BeApproximately(216.2859, Money4);
        r1.UpperEnvelope.Should().BeApproximately(221.6930, Money4);
        r1.LowerEnvelope.Should().BeApproximately(210.8787, Money4);

        MaEnvelopeResult r2 = sut[249];
        r2.Centerline.Should().BeApproximately(258.5179, Money4);
        r2.UpperEnvelope.Should().BeApproximately(264.9808, Money4);
        r2.LowerEnvelope.Should().BeApproximately(252.0549, Money4);

        MaEnvelopeResult r3 = sut[501];
        r3.Centerline.Should().BeApproximately(235.8131, Money4);
        r3.UpperEnvelope.Should().BeApproximately(241.7085, Money4);
        r3.LowerEnvelope.Should().BeApproximately(229.9178, Money4);
    }

    [TestMethod]
    public void Ema_AsMaType_ReturnsExpectedResult()
    {
        IReadOnlyList<MaEnvelopeResult> sut =
            Bars.ToMaEnvelopes(20, 2.5, MaType.EMA);

        // proper quantities
        sut.Should().HaveCount(502);
        sut.Where(static x => x.Centerline != null).Should().HaveCount(483);

        // sample values
        MaEnvelopeResult r1 = sut[24];
        r1.Centerline.Should().BeApproximately(215.0920, Money4);
        r1.UpperEnvelope.Should().BeApproximately(220.4693, Money4);
        r1.LowerEnvelope.Should().BeApproximately(209.7147, Money4);

        MaEnvelopeResult r2 = sut[249];
        r2.Centerline.Should().BeApproximately(255.3873, Money4);
        r2.UpperEnvelope.Should().BeApproximately(261.7719, Money4);
        r2.LowerEnvelope.Should().BeApproximately(249.0026, Money4);

        MaEnvelopeResult r3 = sut[501];
        r3.Centerline.Should().BeApproximately(249.3519, Money4);
        r3.UpperEnvelope.Should().BeApproximately(255.5857, Money4);
        r3.LowerEnvelope.Should().BeApproximately(243.1181, Money4);
    }

    [TestMethod]
    public void Hma_AsMaType_ReturnsExpectedResult()
    {
        IReadOnlyList<MaEnvelopeResult> sut =
            Bars.ToMaEnvelopes(20, 2.5, MaType.HMA);

        // proper quantities
        sut.Should().HaveCount(502);
        sut.Where(static x => x.Centerline != null).Should().HaveCount(480);

        // sample values
        MaEnvelopeResult r2 = sut[149];
        r2.Centerline.Should().BeApproximately(236.0835, Money4);
        r2.UpperEnvelope.Should().BeApproximately(241.9856, Money4);
        r2.LowerEnvelope.Should().BeApproximately(230.1814, Money4);

        MaEnvelopeResult r3 = sut[501];
        r3.Centerline.Should().BeApproximately(235.6972, Money4);
        r3.UpperEnvelope.Should().BeApproximately(241.5897, Money4);
        r3.LowerEnvelope.Should().BeApproximately(229.8048, Money4);
    }

    [TestMethod]
    public void Smma_AsMaType_ReturnsExpectedResult()
    {
        IReadOnlyList<MaEnvelopeResult> sut =
            Bars.ToMaEnvelopes(20, 2.5, MaType.SMMA);

        // proper quantities
        sut.Should().HaveCount(502);
        sut.Where(static x => x.Centerline != null).Should().HaveCount(483);

        // sample values
        MaEnvelopeResult r1 = sut[24];
        r1.Centerline.Should().BeApproximately(214.8433, Money4);
        r1.UpperEnvelope.Should().BeApproximately(220.2144, Money4);
        r1.LowerEnvelope.Should().BeApproximately(209.4722, Money4);

        MaEnvelopeResult r2 = sut[249];
        r2.Centerline.Should().BeApproximately(252.5574, Money4);
        r2.UpperEnvelope.Should().BeApproximately(258.8714, Money4);
        r2.LowerEnvelope.Should().BeApproximately(246.2435, Money4);

        MaEnvelopeResult r3 = sut[501];
        r3.Centerline.Should().BeApproximately(255.6746, Money4);
        r3.UpperEnvelope.Should().BeApproximately(262.0665, Money4);
        r3.LowerEnvelope.Should().BeApproximately(249.2828, Money4);
    }

    [TestMethod]
    public void Tema_AsMaType_ReturnsExpectedResult()
    {
        IReadOnlyList<MaEnvelopeResult> sut =
            Bars.ToMaEnvelopes(20, 2.5, MaType.TEMA);

        // proper quantities
        sut.Should().HaveCount(502);
        sut.Where(static x => x.Centerline != null).Should().HaveCount(483);

        // sample values
        MaEnvelopeResult r1 = sut[57];
        r1.Centerline.Should().BeApproximately(222.6349, Money4);
        r1.UpperEnvelope.Should().BeApproximately(228.2008, Money4);
        r1.LowerEnvelope.Should().BeApproximately(217.0690, Money4);

        MaEnvelopeResult r2 = sut[249];
        r2.Centerline.Should().BeApproximately(258.6208, Money4);
        r2.UpperEnvelope.Should().BeApproximately(265.0863, Money4);
        r2.LowerEnvelope.Should().BeApproximately(252.1553, Money4);

        MaEnvelopeResult r3 = sut[501];
        r3.Centerline.Should().BeApproximately(238.7690, Money4);
        r3.UpperEnvelope.Should().BeApproximately(244.7382, Money4);
        r3.LowerEnvelope.Should().BeApproximately(232.7998, Money4);
    }

    [TestMethod]
    public void Wma_AsMaType_ReturnsExpectedResult()
    {
        IReadOnlyList<MaEnvelopeResult> sut =
            Bars.ToMaEnvelopes(20, 2.5, MaType.WMA);

        // proper quantities
        sut.Should().HaveCount(502);
        sut.Where(static x => x.Centerline != null).Should().HaveCount(483);

        // sample values
        MaEnvelopeResult r1 = sut[149];
        r1.Centerline.Should().BeApproximately(235.5253, Money4);
        r1.UpperEnvelope.Should().BeApproximately(241.4135, Money4);
        r1.LowerEnvelope.Should().BeApproximately(229.6372, Money4);

        MaEnvelopeResult r2 = sut[501];
        r2.Centerline.Should().BeApproximately(246.5110, Money4);
        r2.UpperEnvelope.Should().BeApproximately(252.6738, Money4);
        r2.LowerEnvelope.Should().BeApproximately(240.3483, Money4);
    }

    [TestMethod]
    public void UseReusable_ClosePrice_ReturnsExpectedResult()
    {
        IReadOnlyList<MaEnvelopeResult> sut = Bars
            .Use(CandlePart.Close)
            .ToMaEnvelopes(10);

        sut.Should().HaveCount(502);
        sut.Where(static x => x.Centerline != null).Should().HaveCount(493);
    }

    [TestMethod]
    public void Chainee_FromSma_ReturnsExpectedResult()
    {
        IReadOnlyList<MaEnvelopeResult> sut = Bars
            .ToSma(2)
            .ToMaEnvelopes(10);

        sut.Should().HaveCount(502);
        sut.Where(static x => x.Centerline != null).Should().HaveCount(492);
    }

    [TestMethod]
    public override void BadBars_DoesNotFail()
    {
        IReadOnlyList<MaEnvelopeResult> a = BadBars
            .ToMaEnvelopes(5, 2.5, MaType.ALMA);

        a.Should().HaveCount(502);

        IReadOnlyList<MaEnvelopeResult> d = BadBars
            .ToMaEnvelopes(5, 2.5, MaType.DEMA);

        d.Should().HaveCount(502);

        IReadOnlyList<MaEnvelopeResult> p = BadBars
            .ToMaEnvelopes(5, 2.5, MaType.EPMA);

        p.Should().HaveCount(502);

        IReadOnlyList<MaEnvelopeResult> e = BadBars
            .ToMaEnvelopes(5, 2.5, MaType.EMA);

        e.Should().HaveCount(502);

        IReadOnlyList<MaEnvelopeResult> h = BadBars
            .ToMaEnvelopes(5, 2.5, MaType.HMA);

        h.Should().HaveCount(502);

        IReadOnlyList<MaEnvelopeResult> s = BadBars
            .ToMaEnvelopes(5);

        s.Should().HaveCount(502);

        IReadOnlyList<MaEnvelopeResult> t = BadBars
            .ToMaEnvelopes(5, 2.5, MaType.TEMA);

        t.Should().HaveCount(502);

        IReadOnlyList<MaEnvelopeResult> w = BadBars
            .ToMaEnvelopes(5, 2.5, MaType.WMA);

        w.Should().HaveCount(502);
    }

    [TestMethod]
    public override void NoBars_ReturnsEmpty()
    {
        IReadOnlyList<MaEnvelopeResult> r0 = Nobars
            .ToMaEnvelopes(10);

        r0.Should().BeEmpty();

        IReadOnlyList<MaEnvelopeResult> r1 = Onebar
            .ToMaEnvelopes(10);

        r1.Should().HaveCount(1);
    }

    [TestMethod]
    public void Condense_RemovesNullResults_ReturnsCondensed()
    {
        IReadOnlyList<MaEnvelopeResult> sut = Bars
            .ToMaEnvelopes(20)
            .Condense();

        sut.Should().HaveCount(483);
    }

    [TestMethod]
    public void Exceptions_InvalidParameters_ThrowsArgumentOutOfRangeException()
    {
        // bad offset period
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(
            static () => Bars.ToMaEnvelopes(14, 0));

        // bad MA period
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(
            static () => Bars.ToMaEnvelopes(14, 5, MaType.KAMA));

        // note: insufficient bars is tested elsewhere
    }
}
