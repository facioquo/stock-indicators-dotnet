namespace StaticSeries;

[TestClass]
public class CmfSeriesTests : StaticSeriesTestBase
{
    [TestMethod]
    public override void DefaultParameters_ReturnsExpectedResults()
    {
        IReadOnlyList<CmfResult> sut = Bars
            .ToCmf();

        // proper quantities
        sut.Should().HaveCount(502);
        sut.Where(static x => x.Cmf != null).Should().HaveCount(483);

        // sample values
        CmfResult r1 = sut[49];
        r1.MoneyFlowMultiplier.Should().BeApproximately(0.5468, Money4);
        r1.MoneyFlowVolume.Should().BeApproximately(55609259, 0.005);
        r1.Cmf.Should().BeApproximately(0.350596, Money6);

        CmfResult r2 = sut[249];
        r2.MoneyFlowMultiplier.Should().BeApproximately(0.7778, Money4);
        r2.MoneyFlowVolume.Should().BeApproximately(36433792.89, 0.005);
        r2.Cmf.Should().BeApproximately(-0.040226, Money6);

        CmfResult r3 = sut[501];
        r3.MoneyFlowMultiplier.Should().BeApproximately(0.8052, Money4);
        r3.MoneyFlowVolume.Should().BeApproximately(118396116.25, 0.005);
        r3.Cmf.Should().BeApproximately(-0.123754, Money6);
    }

    [TestMethod]
    public void Results_AreAlwaysBounded()
    {
        IReadOnlyList<CmfResult> sut = Bars.ToCmf(20);
        sut.IsBetween(static x => x.Cmf, -1, 1);
    }

    [TestMethod]
    public void ChainFromResults_ToSma_ReturnsExpectedResult()
    {
        IReadOnlyList<SmaResult> sut = Bars
            .ToCmf()
            .ToSma(10);

        sut.Should().HaveCount(502);
        sut.Where(static x => x.Sma != null).Should().HaveCount(474);
    }

    [TestMethod]
    public override void BadBars_DoesNotFail()
    {
        IReadOnlyList<CmfResult> r = BadBars
            .ToCmf(15);

        r.Should().HaveCount(502);
        r.Where(static x => x.Cmf is double v && double.IsNaN(v)).Should().BeEmpty();
    }

    [TestMethod]
    public void BigBarValues_DoesNotFail()
    {
        IReadOnlyList<CmfResult> r = BigBars
            .ToCmf(150);

        r.Should().HaveCount(1246);
    }

    [TestMethod]
    public override void NoBars_ReturnsEmpty()
    {
        IReadOnlyList<CmfResult> r0 = Nobars
            .ToCmf();

        r0.Should().BeEmpty();

        IReadOnlyList<CmfResult> r1 = Onebar
            .ToCmf();

        r1.Should().HaveCount(1);
    }

    [TestMethod]
    public void Removed_WithWarmupPeriods_TruncatesResults()
    {
        IReadOnlyList<CmfResult> sut = Bars
            .ToCmf()
            .RemoveWarmupPeriods();

        // assertions
        sut.Should().HaveCount(502 - 19);

        CmfResult last = sut[^1];
        last.MoneyFlowMultiplier.Should().BeApproximately(0.8052, Money4);
        last.MoneyFlowVolume.Should().BeApproximately(118396116.25, 0.005);
        last.Cmf.Should().BeApproximately(-0.123754, Money6);
    }

    /// <summary>
    /// bad lookback period
    /// </summary>
    [TestMethod]
    public void Exceptions_InvalidLookback_ThrowsArgumentOutOfRangeException()
        => FluentActions
            .Invoking(static () => Bars.ToCmf(0))
            .Should()
            .ThrowExactly<ArgumentOutOfRangeException>();
}
