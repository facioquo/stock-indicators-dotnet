namespace StaticSeries;

[TestClass]
public class PmoSeriesTests : StaticSeriesTestBase
{
    [TestMethod]
    public override void DefaultParameters_ReturnsExpectedResults()
    {
        IReadOnlyList<PmoResult> sut = Bars
            .ToPmo();

        // proper quantities
        sut.Should().HaveCount(502);
        sut.Where(static x => x.Pmo != null).Should().HaveCount(448);
        sut.Where(static x => x.Signal != null).Should().HaveCount(439);

        // sample values
        PmoResult r1 = sut[92];
        r1.Pmo.Should().BeApproximately(0.6159, Money4);
        r1.Signal.Should().BeApproximately(0.5582, Money4);

        PmoResult r2 = sut[501];
        r2.Pmo.Should().BeApproximately(-2.7016, Money4);
        r2.Signal.Should().BeApproximately(-2.3117, Money4);
    }

    [TestMethod]
    public void UseReusable_ClosePrice_ReturnsExpectedResult()
    {
        IReadOnlyList<PmoResult> sut = Bars
            .Use(CandlePart.Close)
            .ToPmo();

        sut.Should().HaveCount(502);
        sut.Where(static x => x.Pmo != null).Should().HaveCount(448);
    }

    [TestMethod]
    public void Chainee_FromSma_ReturnsExpectedResult()
    {
        IReadOnlyList<PmoResult> sut = Bars
            .ToSma(2)
            .ToPmo();

        sut.Should().HaveCount(502);
        sut.Where(static x => x.Pmo != null).Should().HaveCount(447);
    }

    [TestMethod]
    public void ChainFromResults_ToSma_ReturnsExpectedResult()
    {
        IReadOnlyList<SmaResult> sut = Bars
            .ToPmo()
            .ToSma(10);

        sut.Should().HaveCount(502);
        sut.Where(static x => x.Sma != null).Should().HaveCount(439);
    }

    [TestMethod]
    public override void BadBars_DoesNotFail()
    {
        IReadOnlyList<PmoResult> r = BadBars
            .ToPmo(25, 15, 5);

        r.Should().HaveCount(502);
        r.Where(static x => x.Pmo is double v && double.IsNaN(v)).Should().BeEmpty();
    }

    [TestMethod]
    public override void NoBars_ReturnsEmpty()
    {
        IReadOnlyList<PmoResult> r0 = Nobars
            .ToPmo();

        r0.Should().BeEmpty();

        IReadOnlyList<PmoResult> r1 = Onebar
            .ToPmo();

        r1.Should().HaveCount(1);
    }

    [TestMethod]
    public void Removed_WithWarmupPeriods_TruncatesResults()
    {
        IReadOnlyList<PmoResult> sut = Bars
            .ToPmo()
            .RemoveWarmupPeriods();

        // assertions
        sut.Should().HaveCount(502 - (35 + 20 + 250));

        PmoResult last = sut[^1];
        last.Pmo.Should().BeApproximately(-2.7016, Money4);
        last.Signal.Should().BeApproximately(-2.3117, Money4);
    }

    [TestMethod]
    public void Exceptions_InvalidParameters_ThrowsArgumentOutOfRangeException()
    {
        // bad time period
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(
            static () => Bars.ToPmo(1));

        // bad smoothing period
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(
            static () => Bars.ToPmo(5, 0));

        // bad signal period
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(
            static () => Bars.ToPmo(5, 5, 0));
    }
}
