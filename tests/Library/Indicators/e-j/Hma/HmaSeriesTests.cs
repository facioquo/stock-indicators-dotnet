namespace StaticSeries;

[TestClass]
public class HmaSeriesTests : StaticSeriesTestBase
{
    [TestMethod]
    public override void DefaultParameters_ReturnsExpectedResults()
    {
        IReadOnlyList<HmaResult> sut = Bars
            .ToHma(20);

        // proper quantities
        sut.Should().HaveCount(502);
        sut.Where(static x => x.Hma != null).Should().HaveCount(480);

        // sample values
        HmaResult r1 = sut[149];
        r1.Hma.Should().BeApproximately(236.0835, Money4);

        HmaResult r2 = sut[501];
        r2.Hma.Should().BeApproximately(235.6972, Money4);
    }

    [TestMethod]
    public void UseReusable_ClosePrice_ReturnsExpectedResult()
    {
        IReadOnlyList<HmaResult> sut = Bars
            .Use(CandlePart.Close)
            .ToHma(20);

        sut.Should().HaveCount(502);
        sut.Where(static x => x.Hma != null).Should().HaveCount(480);
    }

    [TestMethod]
    public void Chainee_FromSma_ReturnsExpectedResult()
    {
        IReadOnlyList<HmaResult> sut = Bars
            .ToSma(2)
            .ToHma(19);

        sut.Should().HaveCount(502);
        sut.Where(static x => x.Hma != null).Should().HaveCount(480);
    }

    [TestMethod]
    public void ChainFromResults_ToSma_ReturnsExpectedResult()
    {
        IReadOnlyList<SmaResult> sut = Bars
            .ToHma(20)
            .ToSma(10);

        sut.Should().HaveCount(502);
        sut.Where(static x => x.Sma != null).Should().HaveCount(471);
    }

    [TestMethod]
    public override void BadBars_DoesNotFail()
    {
        IReadOnlyList<HmaResult> r = BadBars
            .ToHma(15);

        r.Should().HaveCount(502);
        r.Where(static x => x.Hma is double v && double.IsNaN(v)).Should().BeEmpty();
    }

    [TestMethod]
    public override void NoBars_ReturnsEmpty()
    {
        IReadOnlyList<HmaResult> r0 = Nobars
            .ToHma(5);

        r0.Should().BeEmpty();

        IReadOnlyList<HmaResult> r1 = Onebar
            .ToHma(5);

        r1.Should().HaveCount(1);
    }

    [TestMethod]
    public void Removed_WithWarmupPeriods_TruncatesResults()
    {
        IReadOnlyList<HmaResult> sut = Bars
            .ToHma(20)
            .RemoveWarmupPeriods();

        // assertions
        sut.Should().HaveCount(480);

        HmaResult last = sut[^1];
        last.Hma.Should().BeApproximately(235.6972, Money4);
    }

    /// <summary>
    /// bad lookback period
    /// </summary>
    [TestMethod]
    public void Exceptions_InvalidLookback_ThrowsArgumentOutOfRangeException()
        => FluentActions
            .Invoking(static () => Bars.ToHma(1))
            .Should()
            .ThrowExactly<ArgumentOutOfRangeException>();
}
