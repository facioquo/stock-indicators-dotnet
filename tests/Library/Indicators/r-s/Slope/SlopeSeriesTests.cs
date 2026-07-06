namespace StaticSeries;

[TestClass]
public class SlopeSeriesTests : StaticSeriesTestBase
{
    [TestMethod]
    public override void DefaultParameters_ReturnsExpectedResults()
    {
        const int n = 20;
        IReadOnlyList<SlopeResult> sut = Bars
            .ToSlope(n);

        // proper quantities
        sut.Should().HaveCount(502);
        sut.Where(static x => x.Slope != null).Should().HaveCount(483);
        sut.Where(static x => x.StdDev != null).Should().HaveCount(483);
        sut.Where(static x => x.Line != null).Should().HaveCount(20);

        // warmup periods should be null
        sut.Take(n - 1).Should().AllSatisfy(
            static r => {
                r.Slope.Should().BeNull();
                r.Intercept.Should().BeNull();
                r.StdDev.Should().BeNull();
                r.RSquared.Should().BeNull();
                r.Line.Should().BeNull();
            });

        // sample values
        SlopeResult r1 = sut[249];
        r1.Slope.Should().BeApproximately(0.312406, Money6);
        r1.Intercept.Should().BeApproximately(180.4164, Money3);
        r1.RSquared.Should().BeApproximately(0.8056, Money3);
        r1.StdDev.Should().BeApproximately(2.0071, Money3);
        r1.Line.Should().BeNull();

        SlopeResult r2 = sut[482];
        r2.Slope.Should().BeApproximately(-0.337015, Money6);
        r2.Intercept.Should().BeApproximately(425.1111, Money3);
        r2.RSquared.Should().BeApproximately(0.1730, Money3);
        r2.StdDev.Should().BeApproximately(4.6719, Money3);
        ((double?)r2.Line).Should().BeApproximately((double)267.9069m, Money3);

        SlopeResult r3 = sut[501];
        r3.Slope.Should().BeApproximately(-1.689143, Money6);
        r3.Intercept.Should().BeApproximately(1083.7629, Money3);
        r3.RSquared.Should().BeApproximately(0.7955, Money3);
        r3.StdDev.Should().BeApproximately(10.9202, Money3);
        ((double?)r3.Line).Should().BeApproximately((double)235.8131m, Money3);
    }

    [TestMethod]
    public void UseReusable_ClosePrice_ReturnsExpectedResult()
    {
        IReadOnlyList<SlopeResult> sut = Bars
            .Use(CandlePart.Close)
            .ToSlope(20);

        sut.Should().HaveCount(502);
        sut.Where(static x => x.Slope != null).Should().HaveCount(483);
    }

    [TestMethod]
    public void Chainee_FromSma_ReturnsExpectedResult()
    {
        IReadOnlyList<SlopeResult> sut = Bars
            .ToSma(2)
            .ToSlope(20);

        sut.Should().HaveCount(502);
        sut.Where(static x => x.Slope != null).Should().HaveCount(482);
    }

    [TestMethod]
    public void ChainFromResults_ToSma_ReturnsExpectedResult()
    {
        IReadOnlyList<SmaResult> sut = Bars
            .ToSlope(20)
            .ToSma(10);

        sut.Should().HaveCount(502);
        sut.Where(static x => x.Sma != null).Should().HaveCount(474);
    }

    [TestMethod]
    public override void BadBars_DoesNotFail()
    {
        IReadOnlyList<SlopeResult> r = BadBars
            .ToSlope(15);

        r.Should().HaveCount(502);
        r.Where(static x => x.Slope is double v && double.IsNaN(v)).Should().BeEmpty();
    }

    [TestMethod]
    public void BigBarValues_DoesNotFail()
    {
        IReadOnlyList<SlopeResult> r = BigBars
            .ToSlope(250);

        r.Should().HaveCount(1246);
    }

    [TestMethod]
    public override void NoBars_ReturnsEmpty()
    {
        IReadOnlyList<SlopeResult> r0 = Nobars
            .ToSlope(5);

        r0.Should().BeEmpty();

        IReadOnlyList<SlopeResult> r1 = Onebar
            .ToSlope(5);

        r1.Should().HaveCount(1);
    }

    [TestMethod]
    public void Removed_WithWarmupPeriods_TruncatesResults()
    {
        IReadOnlyList<SlopeResult> sut = Bars
            .ToSlope(20)
            .RemoveWarmupPeriods();

        // assertions
        sut.Should().HaveCount(502 - 19);

        SlopeResult last = sut[^1];
        last.Slope.Should().BeApproximately(-1.689143, Money6);
        last.Intercept.Should().BeApproximately(1083.7629, Money3);
        last.RSquared.Should().BeApproximately(0.7955, Money3);
        last.StdDev.Should().BeApproximately(10.9202, Money3);
        ((double?)last.Line).Should().BeApproximately((double)235.8131m, Money3);
    }

    /// <summary>
    /// bad lookback period
    /// </summary>
    [TestMethod]
    public void Exceptions_InvalidLookback_ThrowsArgumentOutOfRangeException()
        => FluentActions
            .Invoking(static () => Bars.ToSlope(1))
            .Should()
            .ThrowExactly<ArgumentOutOfRangeException>();
}
