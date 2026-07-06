namespace StaticSeries;

[TestClass]
public class UlcerIndexSeriesTests : StaticSeriesTestBase
{
    [TestMethod]
    public override void DefaultParameters_ReturnsExpectedResults()
    {
        IReadOnlyList<UlcerIndexResult> sut = Bars
            .ToUlcerIndex();

        // proper quantities
        sut.Should().HaveCount(502);
        sut.Where(static x => x.UlcerIndex != null).Should().HaveCount(489);

        // sample value
        UlcerIndexResult r = sut[501];
        r.UlcerIndex.Should().BeApproximately(5.7255, Money4);
    }

    [TestMethod]
    public void UseReusable_ClosePrice_ReturnsExpectedResult()
    {
        IReadOnlyList<UlcerIndexResult> sut = Bars
            .Use(CandlePart.Close)
            .ToUlcerIndex();

        sut.Should().HaveCount(502);
        sut.Where(static x => x.UlcerIndex != null).Should().HaveCount(489);
    }

    [TestMethod]
    public void Chainee_FromSma_ReturnsExpectedResult()
    {
        IReadOnlyList<UlcerIndexResult> sut = Bars
            .ToSma(2)
            .ToUlcerIndex();

        sut.Should().HaveCount(502);
        sut.Where(static x => x.UlcerIndex != null).Should().HaveCount(488);
    }

    [TestMethod]
    public void ChainFromResults_ToSma_ReturnsExpectedResult()
    {
        IReadOnlyList<SmaResult> sut = Bars
            .ToUlcerIndex()
            .ToSma(10);

        sut.Should().HaveCount(502);
        sut.Where(static x => x.Sma != null).Should().HaveCount(480);
    }

    [TestMethod]
    public override void BadBars_DoesNotFail()
    {
        IReadOnlyList<UlcerIndexResult> r = BadBars
            .ToUlcerIndex(15);

        r.Should().HaveCount(502);
        r.Where(static x => x.UlcerIndex is double.NaN).Should().BeEmpty();
    }

    [TestMethod]
    public override void NoBars_ReturnsEmpty()
    {
        IReadOnlyList<UlcerIndexResult> r0 = Nobars
            .ToUlcerIndex();

        r0.Should().BeEmpty();

        IReadOnlyList<UlcerIndexResult> r1 = Onebar
            .ToUlcerIndex();

        r1.Should().HaveCount(1);
    }

    [TestMethod]
    public void Removed_WithWarmupPeriods_TruncatesResults()
    {
        IReadOnlyList<UlcerIndexResult> sut = Bars
            .ToUlcerIndex()
            .RemoveWarmupPeriods();

        // assertions
        sut.Should().HaveCount(502 - 13);

        UlcerIndexResult last = sut[^1];
        last.UlcerIndex.Should().BeApproximately(5.7255, Money4);
    }

    /// <summary>
    /// bad lookback period
    /// </summary>
    [TestMethod]
    public void Exceptions_InvalidLookback_ThrowsArgumentOutOfRangeException()
        => FluentActions
            .Invoking(static () => Bars.ToUlcerIndex(0))
            .Should()
            .ThrowExactly<ArgumentOutOfRangeException>();
}
