namespace StaticSeries;

[TestClass]
public class DemaSeriesTests : StaticSeriesTestBase
{
    [TestMethod]
    public override void DefaultParameters_ReturnsExpectedResults()
    {
        IReadOnlyList<DemaResult> sut = Bars
            .ToDema(20);

        // proper quantities
        sut.Should().HaveCount(502);
        sut.Where(static x => x.Dema != null).Should().HaveCount(483);

        // sample values
        DemaResult r25 = sut[25];
        r25.Dema.Should().BeApproximately(215.7605, Money4);

        DemaResult r51 = sut[51];
        r51.Dema.Should().BeApproximately(225.8259, Money4);

        DemaResult r249 = sut[249];
        r249.Dema.Should().BeApproximately(258.4452, Money4);

        DemaResult r251 = sut[501];
        r251.Dema.Should().BeApproximately(241.1677, Money4);
    }

    [TestMethod]
    public void UseReusable_ClosePrice_ReturnsExpectedResult()
    {
        IReadOnlyList<DemaResult> sut = Bars
            .Use(CandlePart.Close)
            .ToDema(20);

        sut.Should().HaveCount(502);
        sut.Where(static x => x.Dema != null).Should().HaveCount(483);
    }

    [TestMethod]
    public void Chainee_FromSma_ReturnsExpectedResult()
    {
        IReadOnlyList<DemaResult> sut = Bars
            .ToSma(2)
            .ToDema(20);

        sut.Should().HaveCount(502);
        sut.Where(static x => x.Dema != null).Should().HaveCount(482);
    }

    [TestMethod]
    public void ChainFromResults_ToSma_ReturnsExpectedResult()
    {
        IReadOnlyList<SmaResult> sut = Bars
            .ToDema(20)
            .ToSma(10);

        sut.Should().HaveCount(502);
        sut.Where(static x => x.Sma != null).Should().HaveCount(474);
    }

    [TestMethod]
    public override void BadBars_DoesNotFail()
    {
        IReadOnlyList<DemaResult> r = BadBars
            .ToDema(15);

        r.Should().HaveCount(502);
        r.Where(static x => x.Dema is double v && double.IsNaN(v)).Should().BeEmpty();
    }

    [TestMethod]
    public override void NoBars_ReturnsEmpty()
    {
        IReadOnlyList<DemaResult> r0 = Nobars
            .ToDema(5);

        r0.Should().BeEmpty();

        IReadOnlyList<DemaResult> r1 = Onebar
            .ToDema(5);

        r1.Should().HaveCount(1);
    }

    [TestMethod]
    public void Removed_WithWarmupPeriods_TruncatesResults()
    {
        IReadOnlyList<DemaResult> sut = Bars
            .ToDema(20)
            .RemoveWarmupPeriods();

        // assertions
        sut.Should().HaveCount(502 - (40 + 100));

        DemaResult last = sut[^1];
        last.Dema.Should().BeApproximately(241.1677, Money4);
    }

    /// <summary>
    /// bad lookback period
    /// </summary>
    [TestMethod]
    public void Exceptions_InvalidLookback_ThrowsArgumentOutOfRangeException() =>
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(
            static () => Bars.ToDema(0));
}
