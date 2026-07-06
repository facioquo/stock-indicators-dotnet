namespace StaticSeries;

[TestClass]
public class ChopSeriesTests : StaticSeriesTestBase
{
    [TestMethod]
    public override void DefaultParameters_ReturnsExpectedResults()
    {
        IReadOnlyList<ChopResult> sut = Bars
            .ToChop();

        // proper quantities
        sut.Should().HaveCount(502);
        sut.Where(static x => x.Chop != null).Should().HaveCount(488);

        // sample values
        ChopResult r1 = sut[13];
        r1.Chop.Should().BeNull();

        ChopResult r2 = sut[14];
        r2.Chop.Should().BeApproximately(69.9967, Money4);

        ChopResult r3 = sut[249];
        r3.Chop.Should().BeApproximately(41.8499, Money4);

        ChopResult r4 = sut[501];
        r4.Chop.Should().BeApproximately(38.6526, Money4);
    }

    [TestMethod]
    public void ChainFromResults_ToSma_ReturnsExpectedResult()
    {
        IReadOnlyList<SmaResult> sut = Bars
            .ToChop()
            .ToSma(10);

        sut.Should().HaveCount(502);
        sut.Where(static x => x.Sma != null).Should().HaveCount(479);
    }

    [TestMethod]
    public void SmallLookback_WithMinimalPeriods_ReturnsExpectedResult()
    {
        const int lookbackPeriods = 2;
        IReadOnlyList<ChopResult> sut = Bars
            .ToChop(lookbackPeriods);

        // proper quantities
        sut.Should().HaveCount(502);
        sut.Where(static x => x.Chop != null).Should().HaveCount(500);
    }

    [TestMethod]
    public void Results_AreAlwaysBounded()
    {
        IReadOnlyList<ChopResult> sut = Bars.ToChop(14);
        sut.IsBetween(static x => x.Chop, 0, 100);
    }

    [TestMethod]
    public override void BadBars_DoesNotFail()
    {
        IReadOnlyList<ChopResult> r = BadBars
            .ToChop(20);

        r.Should().HaveCount(502);
        r.Where(static x => x.Chop is double v && double.IsNaN(v)).Should().BeEmpty();
    }

    [TestMethod]
    public override void NoBars_ReturnsEmpty()
    {
        IReadOnlyList<ChopResult> r0 = Nobars
            .ToChop();

        r0.Should().BeEmpty();

        IReadOnlyList<ChopResult> r1 = Onebar
            .ToChop();

        r1.Should().HaveCount(1);
    }

    [TestMethod]
    public void Removed_WithWarmupPeriods_TruncatesResults()
    {
        IReadOnlyList<ChopResult> sut = Bars
            .ToChop()
            .RemoveWarmupPeriods();

        // assertions
        sut.Should().HaveCount(502 - 14);

        ChopResult last = sut[^1];
        last.Chop.Should().BeApproximately(38.6526, Money4);
    }

    /// <summary>
    /// bad lookback period
    /// </summary>
    [TestMethod]
    public void Exceptions_InvalidLookback_ThrowsArgumentOutOfRangeException()
        => FluentActions
            .Invoking(static () => Bars.ToChop(1))
            .Should()
            .ThrowExactly<ArgumentOutOfRangeException>();
}
