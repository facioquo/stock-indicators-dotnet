namespace StaticSeries;

[TestClass]
public class SuperTrendSeriesTests : StaticSeriesTestBase
{
    [TestMethod]
    public override void DefaultParameters_ReturnsExpectedResults()
    {
        const int lookbackPeriods = 14;
        const double multiplier = 3;

        IReadOnlyList<SuperTrendResult> sut = Bars
            .ToSuperTrend(lookbackPeriods, multiplier);

        // proper quantities
        sut.Should().HaveCount(502);
        sut.Where(static x => x.SuperTrend != null).Should().HaveCount(488);

        // sample values
        SuperTrendResult r13 = sut[13];
        r13.SuperTrend.Should().BeNull();
        r13.UpperBand.Should().BeNull();
        r13.LowerBand.Should().BeNull();

        SuperTrendResult r14 = sut[14];
        ((double?)r14.SuperTrend).Should().BeApproximately((double)210.6157m, Money3);
        r14.UpperBand.Should().BeNull();
        r14.LowerBand.Should().Be(r14.SuperTrend);

        SuperTrendResult r151 = sut[151];
        ((double?)r151.SuperTrend).Should().BeApproximately((double)232.8520m, Money3);
        r151.UpperBand.Should().BeNull();
        r151.LowerBand.Should().Be(r151.SuperTrend);

        SuperTrendResult r152 = sut[152];
        ((double?)r152.SuperTrend).Should().BeApproximately((double)237.6436m, Money3);
        r152.UpperBand.Should().Be(r152.SuperTrend);
        r152.LowerBand.Should().BeNull();

        SuperTrendResult r249 = sut[249];
        ((double?)r249.SuperTrend).Should().BeApproximately((double)253.8008m, Money3);
        r249.UpperBand.Should().BeNull();
        r249.LowerBand.Should().Be(r249.SuperTrend);

        SuperTrendResult r501 = sut[501];
        ((double?)r501.SuperTrend).Should().BeApproximately((double)250.7954m, Money3);
        r501.UpperBand.Should().Be(r501.SuperTrend);
        r501.LowerBand.Should().BeNull();
    }

    [TestMethod]
    public void Bitcoin_WithBtcData_ReturnsExpectedResult()
    {
        IReadOnlyList<Bar> h = Data.GetBitcoin();

        IReadOnlyList<SuperTrendResult> sut = h
            .ToSuperTrend();

        sut.Should().HaveCount(1246);

        SuperTrendResult r = sut[1208];
        ((double?)r.LowerBand).Should().BeApproximately((double)16242.2704m, Money3);
    }

    [TestMethod]
    public override void BadBars_DoesNotFail()
    {
        IReadOnlyList<SuperTrendResult> r = BadBars
            .ToSuperTrend(7);

        r.Should().HaveCount(502);
    }

    [TestMethod]
    public override void NoBars_ReturnsEmpty()
    {
        IReadOnlyList<SuperTrendResult> r0 = Nobars
            .ToSuperTrend();

        r0.Should().BeEmpty();

        IReadOnlyList<SuperTrendResult> r1 = Onebar
            .ToSuperTrend();

        r1.Should().HaveCount(1);
    }

    [TestMethod]
    public void Condense_RemovesNullResults_ReturnsCondensed()
    {
        const int lookbackPeriods = 14;
        const double multiplier = 3;

        IReadOnlyList<SuperTrendResult> sut = Bars
            .ToSuperTrend(lookbackPeriods, multiplier)
            .Condense();

        // assertions
        sut.Should().HaveCount(488);

        SuperTrendResult last = sut[^1];
        ((double?)last.SuperTrend).Should().BeApproximately((double)250.7954m, Money3);
        last.UpperBand.Should().Be(last.SuperTrend);
        last.LowerBand.Should().BeNull();
    }

    [TestMethod]
    public void Removed_WithWarmupPeriods_TruncatesResults()
    {
        const int lookbackPeriods = 14;
        const double multiplier = 3;

        IReadOnlyList<SuperTrendResult> sut = Bars
            .ToSuperTrend(lookbackPeriods, multiplier)
            .RemoveWarmupPeriods();

        // assertions
        sut.Should().HaveCount(488);

        SuperTrendResult last = sut[^1];
        ((double?)last.SuperTrend).Should().BeApproximately((double)250.7954m, Money3);
        last.UpperBand.Should().Be(last.SuperTrend);
        last.LowerBand.Should().BeNull();
    }

    [TestMethod]
    public void Exceptions_InvalidParameters_ThrowsArgumentOutOfRangeException()
    {
        // bad lookback period
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(
            static () => Bars.ToSuperTrend(1));

        // bad multiplier
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(
            static () => Bars.ToSuperTrend(7, 0));
    }
}
