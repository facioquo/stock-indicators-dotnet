namespace StaticSeries;

[TestClass]
public class EmaSeriesTests : StaticSeriesTestBase
{
    [TestMethod]
    public void Increment_ValidInputs_ReturnsExpectedResult()
    {
        double ema = Ema.Increment(20, 217.5693, 222.10);

        ema.Should().BeApproximately(218.0008, Money4);
    }

    [TestMethod]
    public override void DefaultParameters_ReturnsExpectedResults()
    {
        IReadOnlyList<EmaResult> sut = Bars.ToEma(20);

        // proper quantities
        sut.Should().HaveCount(502);
        sut.Where(static x => x.Ema != null).Should().HaveCount(483);

        // sample values
        sut[29].Ema.Should().BeApproximately(216.6228, Money4);
        sut[249].Ema.Should().BeApproximately(255.3873, Money4);
        sut[501].Ema.Should().BeApproximately(249.3519, Money4);
    }

    [TestMethod]
    public void UsePart_OpenPrice_ReturnsExpectedResult()
    {
        IReadOnlyList<EmaResult> sut = Bars
            .Use(CandlePart.Open)
            .ToEma(20);

        // assertions

        // proper quantities
        // should always be the same number of results as there is bars
        sut.Should().HaveCount(502);
        sut.Where(static x => x.Ema != null).Should().HaveCount(483);

        // sample values
        sut[29].Ema.Should().BeApproximately(216.2643, Money4);
        sut[249].Ema.Should().BeApproximately(255.4875, Money4);
        sut[501].Ema.Should().BeApproximately(249.9157, Money4);
    }

    [TestMethod]
    public void UseReusable_ClosePrice_ReturnsExpectedResult()
    {
        IReadOnlyList<EmaResult> sut = Bars
            .Use(CandlePart.Close)
            .ToEma(20);

        sut.Should().HaveCount(502);
        sut.Where(static x => x.Ema != null).Should().HaveCount(483);
        sut.Where(static x => x.Ema is double.NaN).Should().BeEmpty();
    }

    [TestMethod]
    public void Chainee_FromSma_ReturnsExpectedResult()
    {
        IReadOnlyList<EmaResult> sut = Bars
            .ToSma(2)
            .ToEma(20);

        sut.Should().HaveCount(502);
        sut.Where(static x => x.Ema != null).Should().HaveCount(482);
        sut.Where(static x => x.Ema is double.NaN).Should().BeEmpty();
    }

    [TestMethod]
    public void ChainFromResults_ToSma_ReturnsExpectedResult()
    {
        IReadOnlyList<SmaResult> sut = Bars
            .ToEma(20)
            .ToSma(10);

        sut.Should().HaveCount(502);
        sut.Where(static x => x.Sma != null).Should().HaveCount(474);
        sut.Where(static x => x.Sma is double.NaN).Should().BeEmpty();
    }

    [TestMethod]
    public void ChaineeMore_FromRsi_ReturnsExpectedResult()
    {
        IReadOnlyList<EmaResult> sut = Bars
            .ToRsi()
            .ToEma(20);

        // assertions
        sut.Should().HaveCount(502);
        sut.Where(static x => x.Ema != null).Should().HaveCount(469);
        sut.Where(static x => x.Ema is double.NaN).Should().BeEmpty();

        // sample values
        sut[32].Ema.Should().BeNull();
        sut[33].Ema.Should().BeApproximately(67.4565, Money4);
        sut[249].Ema.Should().BeApproximately(70.4659, Money4);
        sut[501].Ema.Should().BeApproximately(37.0728, Money4);
    }

    [TestMethod]
    public override void BadBars_DoesNotFail()
    {
        IReadOnlyList<EmaResult> r = BadBars.ToEma(15);

        r.Should().HaveCount(502);
        r.Where(static x => x.Ema is double.NaN).Should().BeEmpty();
    }

    [TestMethod]
    public override void NoBars_ReturnsEmpty()
    {
        IReadOnlyList<EmaResult> r0 = Nobars.ToEma(10);

        r0.Should().BeEmpty();

        IReadOnlyList<EmaResult> r1 = Onebar.ToEma(10);

        r1.Should().HaveCount(1);
    }

    [TestMethod]
    public void Removed_WithWarmupPeriods_TruncatesResults()
    {
        IReadOnlyList<EmaResult> sut = Bars.ToEma(20)
            .RemoveWarmupPeriods();

        // assertions
        sut.Should().HaveCount(502 - (20 + 100));
        sut[^1].Ema.Should().BeApproximately(249.3519, Money4);
    }

    /// <summary>
    /// bad lookback period
    /// </summary>
    [TestMethod]
    public void Exceptions_InvalidLookback_ThrowsArgumentOutOfRangeException()
        => FluentActions
            .Invoking(static () => Bars.ToEma(0))
            .Should()
            .ThrowExactly<ArgumentOutOfRangeException>();
}
