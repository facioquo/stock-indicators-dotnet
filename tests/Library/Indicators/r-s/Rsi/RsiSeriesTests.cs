namespace StaticSeries;

[TestClass]
public class RsiSeriesTests : StaticSeriesTestBase
{
    [TestMethod]
    public override void DefaultParameters_ReturnsExpectedResults()
    {
        IReadOnlyList<RsiResult> sut = Bars
            .ToRsi();

        // proper quantities
        sut.Should().HaveCount(502);
        sut.Where(static x => x.Rsi != null).Should().HaveCount(488);

        // sample values
        sut[13].Rsi.Should().BeNull();
        sut[14].Rsi.Should().BeApproximately(62.0541, Money4);
        sut[249].Rsi.Should().BeApproximately(70.9368, Money4);
        sut[501].Rsi.Should().BeApproximately(42.0773, Money4);
    }

    [TestMethod]
    public void Results_WithAnyInput_AreAlwaysBounded()
    {
        IReadOnlyList<RsiResult> sut = Bars.ToRsi(14);
        sut.IsBetween(static x => x.Rsi, 0, 100);
    }

    [TestMethod]
    public void Boundary_WithRandomBars_StaysWithinBounds()
    {
        IReadOnlyList<RsiResult> sut = Data
            .GetRandom(2500)
            .ToRsi(14);

        sut.IsBetween(static x => x.Rsi, 0d, 100d);
    }

    [TestMethod]
    public void SmallLookback_WithOnePeriod_ReturnsExpectedResult()
    {
        const int lookbackPeriods = 1;
        IReadOnlyList<RsiResult> sut = Bars
            .ToRsi(lookbackPeriods);

        // proper quantities
        sut.Should().HaveCount(502);
        sut.Where(static x => x.Rsi != null).Should().HaveCount(501);

        // sample values
        sut[28].Rsi.Should().Be(100);
        sut[52].Rsi.Should().Be(0);
    }

    [TestMethod]
    public void CryptoData_WithBitcoinBars_ReturnsExpectedCount()
    {
        IReadOnlyList<Bar> btc = Data.GetBitcoin();

        IReadOnlyList<RsiResult> sut = btc
            .ToRsi(1);

        sut.Should().HaveCount(1246);
    }

    [TestMethod]
    public void UseReusable_ClosePrice_ReturnsExpectedResult()
    {
        IReadOnlyList<RsiResult> sut = Bars
            .Use(CandlePart.Close)
            .ToRsi();

        sut.Should().HaveCount(502);
        sut.Where(static x => x.Rsi != null).Should().HaveCount(488);
    }

    [TestMethod]
    public void Chainee_FromSma_ReturnsExpectedResult()
    {
        IReadOnlyList<RsiResult> sut = Bars
            .ToSma(2)
            .ToRsi();

        sut.Should().HaveCount(502);
        sut.Where(static x => x.Rsi != null).Should().HaveCount(487);
    }

    [TestMethod]
    public void ChainFromResults_ToSma_ReturnsExpectedResult()
    {
        IReadOnlyList<SmaResult> sut = Bars
            .ToRsi()
            .ToSma(10);

        sut.Should().HaveCount(502);
        sut.Where(static x => x.Sma != null).Should().HaveCount(479);
    }

    [TestMethod]
    public void NaN_WithNanInputs_DoesNotProduceNaN()
    {
        IReadOnlyList<RsiResult> sut = Data.GetBtcUsdNan()
            .ToRsi();

        sut.Where(static x => x.Rsi is double v && double.IsNaN(v)).Should().BeEmpty();
    }

    [TestMethod]
    public override void BadBars_DoesNotFail()
    {
        IReadOnlyList<RsiResult> sut = BadBars
            .ToRsi(20);

        sut.Should().HaveCount(502);
        sut.Where(static x => x.Rsi is double v && double.IsNaN(v)).Should().BeEmpty();
    }

    [TestMethod]
    public override void NoBars_ReturnsEmpty()
    {
        IReadOnlyList<RsiResult> r0 = Nobars
            .ToRsi();

        r0.Should().BeEmpty();

        IReadOnlyList<RsiResult> r1 = Onebar
            .ToRsi();

        r1.Should().HaveCount(1);
    }

    [TestMethod]
    public void Removed_WithWarmupPeriods_TruncatesResults()
    {
        IReadOnlyList<RsiResult> sut = Bars
            .ToRsi()
            .RemoveWarmupPeriods();

        // assertions
        sut.Should().HaveCount(502 - (10 * 14));
        sut[^1].Rsi.Should().BeApproximately(42.0773, Money4);
    }

    /// <summary>
    /// bad lookback period
    /// </summary>
    [TestMethod]
    public void Exceptions_InvalidLookback_ThrowsArgumentOutOfRangeException()
        => FluentActions
            .Invoking(static () => Bars.ToRsi(0))
            .Should()
            .ThrowExactly<ArgumentOutOfRangeException>();
}
