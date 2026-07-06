namespace StaticSeries;

[TestClass]
public class StcSeriesTests : StaticSeriesTestBase
{
    [TestMethod]
    public override void DefaultParameters_ReturnsExpectedResults()
    {
        const int cyclePeriods = 9;
        const int fastPeriods = 12;
        const int slowPeriods = 26;

        IReadOnlyList<StcResult> sut = Bars
            .ToStc(cyclePeriods, fastPeriods, slowPeriods);

        // proper quantities
        sut.Should().HaveCount(502);
        sut.Where(static x => x.Stc != null).Should().HaveCount(467);

        // sample values
        StcResult r34 = sut[34];
        r34.Stc.Should().BeNull();

        StcResult r35 = sut[35];
        r35.Stc.Should().Be(100d);

        StcResult r49 = sut[49];
        r49.Stc.Should().BeApproximately(0.8370, Money4);

        StcResult r249 = sut[249];
        r249.Stc.Should().BeApproximately(27.7340, Money4);

        StcResult last = sut[^1];
        last.Stc.Should().BeApproximately(19.2544, Money4);
    }

    [TestMethod]
    public void UseReusable_ClosePrice_ReturnsExpectedResult()
    {
        IReadOnlyList<StcResult> sut = Bars
            .Use(CandlePart.Close)
            .ToStc(9, 12, 26);

        sut.Should().HaveCount(502);
        sut.Where(static x => x.Stc != null).Should().HaveCount(467);
    }

    [TestMethod]
    public void Chainee_FromSma_ReturnsExpectedResult()
    {
        IReadOnlyList<StcResult> sut = Bars
            .ToSma(2)
            .ToStc(9, 12, 26);

        sut.Should().HaveCount(502);
        sut.Where(static x => x.Stc != null).Should().HaveCount(466);
    }

    [TestMethod]
    public void ChainFromResults_ToSma_ReturnsExpectedResult()
    {
        IReadOnlyList<SmaResult> sut = Bars
            .ToStc(9, 12, 26)
            .ToSma(10);

        sut.Should().HaveCount(502);
        sut.Where(static x => x.Sma != null).Should().HaveCount(458);
    }

    [TestMethod]
    public override void BadBars_DoesNotFail()
    {
        IReadOnlyList<StcResult> r = BadBars
            .ToStc();

        r.Should().HaveCount(502);
        r.Where(static x => x.Stc is double v && double.IsNaN(v)).Should().BeEmpty();
    }

    [TestMethod]
    public override void NoBars_ReturnsEmpty()
    {
        IReadOnlyList<StcResult> r0 = Nobars
            .ToStc();

        r0.Should().BeEmpty();

        IReadOnlyList<StcResult> r1 = Onebar
            .ToStc();

        r1.Should().HaveCount(1);
    }

    [TestMethod]
    public void Issue1107_Magic58_IsNotOutOfRange()
    {
        // stochastic SMMA variant initialization bug

        RandomGbm bars = new(58);

        IReadOnlyList<StcResult> sut = bars
            .ToStc();

        sut.Should().HaveCount(58);
    }

    [TestMethod]
    public void Removed_WithWarmupPeriods_TruncatesResults()
    {
        const int cyclePeriods = 9;
        const int fastPeriods = 12;
        const int slowPeriods = 26;

        IReadOnlyList<StcResult> sut = Bars
            .ToStc(cyclePeriods, fastPeriods, slowPeriods)
            .RemoveWarmupPeriods();

        // assertions
        sut.Should().HaveCount(502 - (slowPeriods + cyclePeriods + 250));

        StcResult last = sut[^1];
        last.Stc.Should().BeApproximately(19.2544, Money4);
    }

    [TestMethod]
    public void Results_AreAlwaysBounded()
    {
        IReadOnlyList<StcResult> sut = Bars.ToStc(9, 12, 26);
        sut.IsBetween(static x => x.Stc, 0, 100);
    }

    [TestMethod]
    public void Exceptions_InvalidParameters_ThrowsArgumentOutOfRangeException()
    {
        // bad fast period
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(
            static () => Bars.ToStc(9, 0, 26));

        // bad slow periods must be larger than faster period
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(
            static () => Bars.ToStc(9, 12, 12));

        // bad signal period
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(
            static () => Bars.ToStc(-1, 12, 26));
    }
}
