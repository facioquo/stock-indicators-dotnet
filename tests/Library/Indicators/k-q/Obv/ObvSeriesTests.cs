namespace StaticSeries;

[TestClass]
public class ObvSeriesTests : StaticSeriesTestBase
{
    [TestMethod]
    public override void DefaultParameters_ReturnsExpectedResults()
    {
        IReadOnlyList<ObvResult> sut = Bars
            .ToObv();

        // proper quantities
        sut.Should().HaveCount(502);

        // sample values
        ObvResult r1 = sut[249];
        r1.Obv.Should().Be(1780918888);

        ObvResult r2 = sut[501];
        r2.Obv.Should().Be(539843504);
    }

    [TestMethod]
    public void ChainFromResults_ToSma_ReturnsExpectedResult()
    {
        IReadOnlyList<SmaResult> sut = Bars
            .ToObv()
            .ToSma(10);

        sut.Should().HaveCount(502);
        sut.Where(static x => x.Sma != null).Should().HaveCount(493);
    }

    [TestMethod]
    public override void BadBars_DoesNotFail()
    {
        IReadOnlyList<ObvResult> r = BadBars
            .ToObv();

        r.Should().HaveCount(502);
        r.Where(static x => double.IsNaN(x.Obv)).Should().BeEmpty();
    }

    [TestMethod]
    public void BigBarValues_WithLargeNumbers_DoesNotFail()
    {
        IReadOnlyList<ObvResult> r = BigBars
            .ToObv();

        r.Should().HaveCount(1246);
    }

    [TestMethod]
    public override void NoBars_ReturnsEmpty()
    {
        IReadOnlyList<ObvResult> r0 = Nobars
            .ToObv();

        r0.Should().BeEmpty();

        IReadOnlyList<ObvResult> r1 = Onebar
            .ToObv();

        r1.Should().HaveCount(1);
    }
}
