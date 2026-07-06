namespace StaticSeries;

[TestClass]
public class CciSeriesTests : StaticSeriesTestBase
{
    [TestMethod]
    public override void DefaultParameters_ReturnsExpectedResults()
    {
        IReadOnlyList<CciResult> sut = Bars
            .ToCci();

        // proper quantities
        sut.Should().HaveCount(502);
        sut.Where(static x => x.Cci != null).Should().HaveCount(483);

        // sample value
        CciResult r = sut[501];
        r.Cci.Should().BeApproximately(-52.9946, Money4);
    }

    [TestMethod]
    public void ChainFromResults_ToSma_ReturnsExpectedResult()
    {
        IReadOnlyList<SmaResult> sut = Bars
            .ToCci()
            .ToSma(10);

        sut.Should().HaveCount(502);
        sut.Where(static x => x.Sma != null).Should().HaveCount(474);
    }

    [TestMethod]
    public override void BadBars_DoesNotFail()
    {
        IReadOnlyList<CciResult> r = BadBars
            .ToCci(15);

        r.Should().HaveCount(502);
        r.Where(static x => x.Cci is double.NaN).Should().BeEmpty();
    }

    [TestMethod]
    public override void NoBars_ReturnsEmpty()
    {
        IReadOnlyList<CciResult> r0 = Nobars
            .ToCci();

        r0.Should().BeEmpty();

        IReadOnlyList<CciResult> r1 = Onebar
            .ToCci();

        r1.Should().HaveCount(1);
    }

    [TestMethod]
    public void Removed_WithWarmupPeriods_TruncatesResults()
    {
        IReadOnlyList<CciResult> sut = Bars
            .ToCci()
            .RemoveWarmupPeriods();

        // assertions
        sut.Should().HaveCount(502 - 19);

        CciResult last = sut[^1];
        last.Cci.Should().BeApproximately(-52.9946, Money4);
    }

    /// <summary>
    /// bad lookback period
    /// </summary>
    [TestMethod]
    public void Exceptions_InvalidLookback_ThrowsArgumentOutOfRangeException()
        => FluentActions
            .Invoking(static () => Bars.ToCci(0))
            .Should()
            .ThrowExactly<ArgumentOutOfRangeException>();
}
