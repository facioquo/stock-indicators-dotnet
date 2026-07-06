namespace StaticSeries;

[TestClass]
public class SmaAnalysisSeriesTests : StaticSeriesTestBase
{
    [TestMethod]
    public override void DefaultParameters_ReturnsExpectedResults()
    {
        IReadOnlyList<SmaAnalysisResult> sut = Bars
            .ToSmaAnalysis(20);

        // proper quantities
        sut.Should().HaveCount(502);
        sut.Where(static x => x.Sma != null).Should().HaveCount(483);

        // sample value
        SmaAnalysisResult r = sut[501];
        r.Sma.Should().BeApproximately(251.86, Money6);
        r.Mad.Should().BeApproximately(9.450000, Money6);
        r.Mse.Should().BeApproximately(119.25102, Money6);
        r.Mape.Should().BeApproximately(0.037637, Money6);
    }

    [TestMethod]
    public void UseReusable_ClosePrice_ReturnsExpectedResult()
    {
        IReadOnlyList<SmaAnalysisResult> sut = Bars
            .Use(CandlePart.Close)
            .ToSmaAnalysis(20);

        sut.Should().HaveCount(502);
        sut.Where(static x => x.Sma != null).Should().HaveCount(483);
    }

    [TestMethod]
    public void Chainee_FromSma_ReturnsExpectedResult()
    {
        IReadOnlyList<SmaAnalysisResult> sut = Bars
            .ToSma(2)
            .ToSmaAnalysis(20);

        sut.Should().HaveCount(502);
        sut.Where(static x => x.Sma != null).Should().HaveCount(482);
    }

    [TestMethod]
    public void ChainFromResults_ToEma_ReturnsExpectedResult()
    {
        IReadOnlyList<EmaResult> sut = Bars
            .ToSmaAnalysis(10)
            .ToEma(10);

        sut.Should().HaveCount(502);
        sut.Where(static x => x.Ema != null).Should().HaveCount(484);
    }

    [TestMethod]
    public override void BadBars_DoesNotFail()
    {
        IReadOnlyList<SmaAnalysisResult> r = BadBars
            .ToSmaAnalysis(15);

        r.Should().HaveCount(502);
        r.Where(static x => x.Mape is double.NaN).Should().BeEmpty();
    }

    [TestMethod]
    public override void NoBars_ReturnsEmpty()
    {
        IReadOnlyList<SmaAnalysisResult> r0 = Nobars
            .ToSmaAnalysis(6);

        r0.Should().BeEmpty();

        IReadOnlyList<SmaAnalysisResult> r1 = Onebar
            .ToSmaAnalysis(6);

        r1.Should().HaveCount(1);
    }

    [TestMethod]
    public void Removed_WithWarmupPeriods_TruncatesResults()
    {
        IReadOnlyList<SmaAnalysisResult> sut = Bars
            .ToSmaAnalysis(20)
            .RemoveWarmupPeriods();

        // assertions
        sut.Should().HaveCount(502 - 19);
        (sut[^1].Sma.Value).Should().BeApproximately(251.8600, Money4);
    }

    /// <summary>
    /// bad lookback period
    /// </summary>
    [TestMethod]
    public void Exceptions_InvalidLookback_ThrowsArgumentOutOfRangeException()
        => FluentActions
            .Invoking(static () => Bars.ToSmaAnalysis(0))
            .Should()
            .ThrowExactly<ArgumentOutOfRangeException>();
}
