namespace StaticSeries;

[TestClass]
public class PivotsSeriesTests : StaticSeriesTestBase
{
    [TestMethod]
    public override void DefaultParameters_ReturnsExpectedResults()
    {
        IReadOnlyList<PivotsResult> sut = Bars
            .ToPivots(4, 4);

        // proper quantities
        sut.Should().HaveCount(502);
        sut.Where(static x => x.HighPoint != null).Should().HaveCount(35);
        sut.Where(static x => x.HighTrend != null).Should().HaveCount(333);
        sut.Where(static x => x.HighLine != null).Should().HaveCount(338);
        sut.Where(static x => x.LowPoint != null).Should().HaveCount(34);
        sut.Where(static x => x.LowTrend != null).Should().HaveCount(328);
        sut.Where(static x => x.LowLine != null).Should().HaveCount(333);

        // sample values
        PivotsResult r3 = sut[3];
        r3.HighPoint.Should().BeNull();
        r3.HighTrend.Should().BeNull();
        r3.HighLine.Should().BeNull();
        r3.LowPoint.Should().BeNull();
        r3.LowTrend.Should().BeNull();
        r3.LowLine.Should().BeNull();

        PivotsResult r7 = sut[7];
        r7.HighPoint.Should().BeNull();
        r7.HighTrend.Should().BeNull();
        r7.HighLine.Should().BeNull();
        r7.LowPoint.Should().Be(212.53m);
        r7.LowTrend.Should().BeNull();
        r7.LowLine.Should().Be(212.53m);

        PivotsResult r120 = sut[120];
        r120.HighPoint.Should().Be(233.02m);
        r120.HighTrend.Should().Be(PivotTrend.Lh);
        r120.HighLine.Should().Be(233.02m);
        r120.LowPoint.Should().BeNull();
        r120.LowTrend.Should().Be(PivotTrend.Ll);
        ((double?)r120.LowLine).Should().BeApproximately((double)228.9671m, Money3);

        PivotsResult r180 = sut[180];
        r180.HighPoint.Should().Be(239.74m);
        r180.HighTrend.Should().Be(PivotTrend.Hh);
        r180.HighLine.Should().Be(239.74m);
        r180.LowPoint.Should().BeNull();
        r180.LowTrend.Should().Be(PivotTrend.Hl);
        ((double?)r180.LowLine).Should().BeApproximately((double)236.7050m, Money3);

        PivotsResult r250 = sut[250];
        r250.HighPoint.Should().BeNull();
        r250.HighTrend.Should().BeNull();
        r250.HighLine.Should().BeNull();
        r250.LowPoint.Should().Be(256.81m);
        r250.LowTrend.Should().BeNull();
        r250.LowLine.Should().BeNull();

        PivotsResult r472 = sut[472];
        r472.HighPoint.Should().BeNull();
        r472.HighTrend.Should().Be(PivotTrend.Lh);
        r472.HighLine.Should().Be(274.14m);
        r472.LowPoint.Should().BeNull();
        r472.LowTrend.Should().Be(PivotTrend.Hl);
        ((double?)r472.LowLine).Should().BeApproximately((double)255.8078m, Money3);

        PivotsResult r497 = sut[497];
        r497.HighPoint.Should().BeNull();
        r497.HighTrend.Should().BeNull();
        r497.HighLine.Should().BeNull();
        r497.LowPoint.Should().BeNull();
        r497.LowTrend.Should().BeNull();
        r497.LowLine.Should().BeNull();

        PivotsResult r498 = sut[498];
        r498.HighPoint.Should().BeNull();
        r498.HighTrend.Should().BeNull();
        r498.HighLine.Should().BeNull();
        r498.LowPoint.Should().BeNull();
        r498.LowTrend.Should().BeNull();
        r498.LowLine.Should().BeNull();
    }

    [TestMethod]
    public override void BadBars_DoesNotFail()
    {
        IReadOnlyList<PivotsResult> r = BadBars
            .ToPivots();

        r.Should().HaveCount(502);
    }

    [TestMethod]
    public override void NoBars_ReturnsEmpty()
    {
        IReadOnlyList<PivotsResult> r0 = Nobars
            .ToPivots();

        r0.Should().BeEmpty();

        IReadOnlyList<PivotsResult> r1 = Onebar
            .ToPivots();

        r1.Should().HaveCount(1);
    }

    [TestMethod]
    public void Condense_RemovesNullResults_ReturnsCondensed()
    {
        IReadOnlyList<PivotsResult> sut = Bars
            .ToPivots(4, 4)
            .Condense();
        sut.Should().HaveCount(67);
    }

    [TestMethod]
    public void Exceptions_InvalidParameters_ThrowsArgumentOutOfRangeException()
    {
        // bad left span
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(
            static () => Bars.ToPivots(1));

        // bad right span
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(
            static () => Bars.ToPivots(2, 1));

        // bad lookback window
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(
            static () => Bars.ToPivots(20, 10, 20, EndType.Close));
    }

    [TestMethod]
    public void Issue1950_WithKnownCsv_ProducesExpectedPivots()
    {
        IReadOnlyList<Bar> bars = Data.BarsFromCsv("_issue1950.pivots.csv");

        IReadOnlyList<PivotsResult> r = bars.ToPivots(maxTrendPeriods: 100);

        const string msg = "results size should match original bars size";

        r.Should().HaveCount(1430, msg);
        r.Should().HaveCount(bars.Count, msg);
        r.Where(static x => x.HighPoint is not null).Should().HaveCount(194);
        r.Where(static x => x.LowPoint is not null).Should().HaveCount(193);
    }
}
