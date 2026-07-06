namespace StaticSeries;

[TestClass]
public class StdDevSeriesTests : StaticSeriesTestBase
{
    [TestMethod]
    public override void DefaultParameters_ReturnsExpectedResults()
    {
        IReadOnlyList<StdDevResult> sut = Bars
            .ToStdDev(10);

        // proper quantities
        sut.Should().HaveCount(502);
        sut.Where(static x => x.StdDev != null).Should().HaveCount(493);
        sut.Where(static x => x.ZScore != null).Should().HaveCount(493);

        // sample values
        StdDevResult r1 = sut[8];
        r1.StdDev.Should().BeNull();
        r1.Mean.Should().BeNull();
        r1.ZScore.Should().BeNull();

        StdDevResult r2 = sut[9];
        r2.StdDev.Should().BeApproximately(0.5020, Money4);
        r2.Mean.Should().BeApproximately(214.0140, Money4);
        r2.ZScore.Should().BeApproximately(-0.525917, Money6);

        StdDevResult r3 = sut[249];
        r3.StdDev.Should().BeApproximately(0.9827, Money4);
        r3.Mean.Should().BeApproximately(257.2200, Money4);
        r3.ZScore.Should().BeApproximately(0.783563, Money6);

        StdDevResult r4 = sut[501];
        r4.StdDev.Should().BeApproximately(5.4738, Money4);
        r4.Mean.Should().BeApproximately(242.4100, Money4);
        r4.ZScore.Should().BeApproximately(0.524312, Money6);
    }

    [TestMethod]
    public void UseReusable_ClosePrice_ReturnsExpectedResult()
    {
        IReadOnlyList<StdDevResult> sut = Bars
            .Use(CandlePart.Close)
            .ToStdDev(10);

        sut.Should().HaveCount(502);
        sut.Where(static x => x.StdDev != null).Should().HaveCount(493);
    }

    [TestMethod]
    public void Chainee_FromSma_ReturnsExpectedResult()
    {
        IReadOnlyList<StdDevResult> sut = Bars
            .ToSma(2)
            .ToStdDev(10);

        sut.Should().HaveCount(502);
        sut.Where(static x => x.StdDev != null).Should().HaveCount(492);
    }

    [TestMethod]
    public void ChainFromResults_ToSma_ReturnsExpectedResult()
    {
        IReadOnlyList<SmaResult> sut = Bars
            .ToStdDev(10)
            .ToSma(10);

        sut.Should().HaveCount(502);
        sut.Where(static x => x.Sma != null).Should().HaveCount(484);
    }

    [TestMethod]
    public override void BadBars_DoesNotFail()
    {
        IReadOnlyList<StdDevResult> r = BadBars
            .ToStdDev(15);

        r.Should().HaveCount(502);
        r.Where(static x => x.StdDev is double.NaN).Should().BeEmpty();
    }

    [TestMethod]
    public void BigBarValues_DoesNotFail()
    {
        IReadOnlyList<StdDevResult> r = BigBars
            .ToStdDev(200);

        r.Should().HaveCount(1246);
    }

    [TestMethod]
    public override void NoBars_ReturnsEmpty()
    {
        IReadOnlyList<StdDevResult> r0 = Nobars
            .ToStdDev(10);

        r0.Should().BeEmpty();

        IReadOnlyList<StdDevResult> r1 = Onebar
            .ToStdDev(10);

        r1.Should().HaveCount(1);
    }

    [TestMethod]
    public void Removed_WithWarmupPeriods_TruncatesResults()
    {
        IReadOnlyList<StdDevResult> sut = Bars
            .ToStdDev(10)
            .RemoveWarmupPeriods();

        // assertions
        sut.Should().HaveCount(502 - 9);

        StdDevResult last = sut[^1];
        last.StdDev.Should().BeApproximately(5.4738, Money4);
        last.Mean.Should().BeApproximately(242.4100, Money4);
        last.ZScore.Should().BeApproximately(0.524312, Money6);
    }

    [TestMethod] // bad lookback period
    public void Exceptions_InvalidLookback_ThrowsArgumentOutOfRangeException()
        => FluentActions
            .Invoking(static () => Bars.ToStdDev(1))
            .Should()
            .ThrowExactly<ArgumentOutOfRangeException>();
}
