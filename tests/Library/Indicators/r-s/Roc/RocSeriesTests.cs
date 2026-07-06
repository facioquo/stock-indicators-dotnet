namespace StaticSeries;

[TestClass]
public class RocSeriesTests : StaticSeriesTestBase
{
    [TestMethod]
    public override void DefaultParameters_ReturnsExpectedResults()
    {
        IReadOnlyList<RocResult> sut = Bars
            .ToRoc(20);

        // proper quantities
        sut.Should().HaveCount(502);
        sut.Where(static x => x.Momentum != null).Should().HaveCount(482);
        sut.Where(static x => x.Roc != null).Should().HaveCount(482);

        // sample values
        RocResult r49 = sut[49];
        r49.Momentum.Should().BeApproximately(4.96, Money4);
        r49.Roc.Should().BeApproximately(2.2465, Money4);

        RocResult r249 = sut[249];
        r249.Momentum.Should().BeApproximately(6.25, Money4);
        r249.Roc.Should().BeApproximately(2.4827, Money4);

        RocResult r501 = sut[501];
        r501.Momentum.Should().BeApproximately(-22.05, Money4);
        r501.Roc.Should().BeApproximately(-8.2482, Money4);
    }

    [TestMethod]
    public void UseReusable_ClosePrice_ReturnsExpectedResult()
    {
        IReadOnlyList<RocResult> sut = Bars
            .Use(CandlePart.Close)
            .ToRoc(20);

        sut.Should().HaveCount(502);
        sut.Where(static x => x.Roc != null).Should().HaveCount(482);
    }

    [TestMethod]
    public void Chainee_FromSma_ReturnsExpectedResult()
    {
        IReadOnlyList<RocResult> sut = Bars
            .ToSma(2)
            .ToRoc(20);

        sut.Should().HaveCount(502);
        sut.Where(static x => x.Roc != null).Should().HaveCount(481);
    }

    [TestMethod]
    public void ChainFromResults_ToSma_ReturnsExpectedResult()
    {
        IReadOnlyList<SmaResult> sut = Bars
            .ToRoc(20)
            .ToSma(10);

        sut.Should().HaveCount(502);
        sut.Where(static x => x.Sma != null).Should().HaveCount(473);
    }

    [TestMethod]
    public override void BadBars_DoesNotFail()
    {
        IReadOnlyList<RocResult> r = BadBars
            .ToRoc(35);

        r.Should().HaveCount(502);
        r.Where(static x => x.Roc is double.NaN).Should().BeEmpty();
    }

    [TestMethod]
    public override void NoBars_ReturnsEmpty()
    {
        IReadOnlyList<RocResult> r0 = Nobars
            .ToRoc(5);

        r0.Should().BeEmpty();

        IReadOnlyList<RocResult> r1 = Onebar
            .ToRoc(5);

        r1.Should().HaveCount(1);
    }

    [TestMethod]
    public void Removed_WithWarmupPeriods_TruncatesResults()
    {
        IReadOnlyList<RocResult> sut = Bars
            .ToRoc(20)
            .RemoveWarmupPeriods();

        // assertions
        sut.Should().HaveCount(502 - 20);

        RocResult last = sut[^1];
        last.Roc.Should().BeApproximately(-8.2482, Money4);
    }

    [TestMethod] // bad lookback period
    public void Exceptions_InvalidLookback_ThrowsArgumentOutOfRangeException()
        => FluentActions
            .Invoking(static () => Bars.ToRoc(0))
            .Should()
            .ThrowExactly<ArgumentOutOfRangeException>();
}
