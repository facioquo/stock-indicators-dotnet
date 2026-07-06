namespace StaticSeries;

[TestClass]
public class BollingerBandsSeriesTests : StaticSeriesTestBase
{
    [TestMethod]
    public override void DefaultParameters_ReturnsExpectedResults()
    {
        IReadOnlyList<BollingerBandsResult> sut =
            Bars.ToBollingerBands();

        // proper quantities
        sut.Should().HaveCount(502);
        sut.Where(static x => x.Sma != null).Should().HaveCount(483);
        sut.Where(static x => x.UpperBand != null).Should().HaveCount(483);
        sut.Where(static x => x.LowerBand != null).Should().HaveCount(483);
        sut.Where(static x => x.PercentB != null).Should().HaveCount(483);
        sut.Where(static x => x.ZScore != null).Should().HaveCount(483);
        sut.Where(static x => x.Width != null).Should().HaveCount(483);

        // sample values
        BollingerBandsResult r1 = sut[249];
        r1.Sma.Should().BeApproximately(255.5500, Money4);
        r1.UpperBand.Should().BeApproximately(259.5642, Money4);
        r1.LowerBand.Should().BeApproximately(251.5358, Money4);
        r1.PercentB.Should().BeApproximately(0.803923, Money6);
        r1.ZScore.Should().BeApproximately(1.215692, Money6);
        r1.Width.Should().BeApproximately(0.031416, Money6);

        BollingerBandsResult r2 = sut[501];
        r2.Sma.Should().BeApproximately(251.8600, Money4);
        r2.UpperBand.Should().BeApproximately(273.7004, Money4);
        r2.LowerBand.Should().BeApproximately(230.0196, Money4);
        r2.PercentB.Should().BeApproximately(0.349362, Money6);
        r2.ZScore.Should().BeApproximately(-0.602552, Money6);
        r2.Width.Should().BeApproximately(0.173433, Money6);
    }

    [TestMethod]
    public void UseReusable_ClosePrice_ReturnsExpectedResult()
    {
        IReadOnlyList<BollingerBandsResult> sut = Bars
            .Use(CandlePart.Close)
            .ToBollingerBands();

        sut.Should().HaveCount(502);
        sut.Where(static x => x.Sma != null).Should().HaveCount(483);
    }

    [TestMethod]
    public void Chainee_FromSma_ReturnsExpectedResult()
    {
        IReadOnlyList<BollingerBandsResult> sut = Bars
            .ToSma(2)
            .ToBollingerBands();

        sut.Should().HaveCount(502);
        sut.Where(static x => x.UpperBand != null).Should().HaveCount(482);
    }

    [TestMethod]
    public void ChainFromResults_ToSma_ReturnsExpectedResult()
    {
        IReadOnlyList<SmaResult> sut = Bars
            .ToBollingerBands()
            .ToSma(10);

        sut.Should().HaveCount(502);
        sut.Where(static x => x.Sma != null).Should().HaveCount(474);
    }

    [TestMethod]
    public override void BadBars_DoesNotFail()
    {
        IReadOnlyList<BollingerBandsResult> r = BadBars
            .ToBollingerBands(15, 3);

        r.Should().HaveCount(502);
        r.Where(static x => x.UpperBand is double v && double.IsNaN(v)).Should().BeEmpty();
    }

    [TestMethod]
    public override void NoBars_ReturnsEmpty()
    {
        IReadOnlyList<BollingerBandsResult> r0 = Nobars
            .ToBollingerBands();

        r0.Should().BeEmpty();

        IReadOnlyList<BollingerBandsResult> r1 = Onebar
            .ToBollingerBands();

        r1.Should().HaveCount(1);
    }

    [TestMethod]
    public void Removed_WithWarmupPeriods_TruncatesResults()
    {
        IReadOnlyList<BollingerBandsResult> sut = Bars
            .ToBollingerBands()
            .RemoveWarmupPeriods();

        // assertions
        sut.Should().HaveCount(502 - 19);

        BollingerBandsResult last = sut[^1];
        last.Sma.Should().BeApproximately(251.8600, Money4);
        last.UpperBand.Should().BeApproximately(273.7004, Money4);
        last.LowerBand.Should().BeApproximately(230.0196, Money4);
        last.PercentB.Should().BeApproximately(0.349362, Money6);
        last.ZScore.Should().BeApproximately(-0.602552, Money6);
        last.Width.Should().BeApproximately(0.173433, Money6);
    }

    [TestMethod]
    public void Exceptions_InvalidParameters_ThrowsArgumentOutOfRangeException()
    {
        // bad lookback period
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(
            static () => Bars.ToBollingerBands(1));

        // bad standard deviation
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(
            static () => Bars.ToBollingerBands(2, 0));
    }
}
