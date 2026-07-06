namespace StaticSeries;

[TestClass]
public class TsiSeriesTests : StaticSeriesTestBase
{
    [TestMethod]
    public override void DefaultParameters_ReturnsExpectedResults()
    {
        IReadOnlyList<TsiResult> sut = Bars
            .ToTsi();

        // proper quantities
        sut.Should().HaveCount(502);
        sut.Where(static x => x.Tsi != null).Should().HaveCount(465);
        sut.Where(static x => x.Signal != null).Should().HaveCount(459);

        // sample values
        TsiResult r2 = sut[37];
        r2.Tsi.Should().BeApproximately(53.1204, Money4);
        r2.Signal.Should().BeNull();

        TsiResult r3A = sut[43];
        r3A.Tsi.Should().BeApproximately(46.0960, Money4);
        r3A.Signal.Should().BeApproximately(51.6916, Money4);

        TsiResult r3B = sut[44];
        r3B.Tsi.Should().BeApproximately(42.5121, Money4);
        r3B.Signal.Should().BeApproximately(49.3967, Money4);

        TsiResult r4 = sut[149];
        r4.Tsi.Should().BeApproximately(29.0936, Money4);
        r4.Signal.Should().BeApproximately(28.0134, Money4);

        TsiResult r5 = sut[249];
        r5.Tsi.Should().BeApproximately(41.9232, Money4);
        r5.Signal.Should().BeApproximately(42.4063, Money4);

        TsiResult r6 = sut[501];
        r6.Tsi.Should().BeApproximately(-28.3513, Money4);
        r6.Signal.Should().BeApproximately(-29.3597, Money4);
    }

    [TestMethod]
    public void Results_AreAlwaysBounded()
    {
        IReadOnlyList<TsiResult> sut = Bars.ToTsi(25, 13, 7);
        sut.IsBetween(static x => x.Tsi, -100, 100);
        sut.IsBetween(static x => x.Signal, -100, 100);
    }

    [TestMethod]
    public void UseReusable_ClosePrice_ReturnsExpectedResult()
    {
        IReadOnlyList<TsiResult> sut = Bars
            .Use(CandlePart.Close)
            .ToTsi();

        sut.Should().HaveCount(502);
        sut.Where(static x => x.Tsi != null).Should().HaveCount(465);
    }

    [TestMethod]
    public void Chainee_FromSma_ReturnsExpectedResult()
    {
        IReadOnlyList<TsiResult> sut = Bars
            .ToSma(2)
            .ToTsi();

        sut.Should().HaveCount(502);
        sut.Where(static x => x.Tsi != null).Should().HaveCount(464);
    }

    [TestMethod]
    public void ChainFromResults_ToSma_ReturnsExpectedResult()
    {
        IReadOnlyList<SmaResult> sut = Bars
            .ToTsi()
            .ToSma(10);

        sut.Should().HaveCount(502);
        sut.Where(static x => x.Sma != null).Should().HaveCount(456);
    }

    [TestMethod]
    public override void BadBars_DoesNotFail()
    {
        IReadOnlyList<TsiResult> r = BadBars
            .ToTsi();

        r.Should().HaveCount(502);
        r.Where(static x => x.Tsi is double v && double.IsNaN(v)).Should().BeEmpty();
    }

    [TestMethod]
    public void BigBarValues_DoesNotFail()
    {
        IReadOnlyList<TsiResult> r = BigBars
            .ToTsi();

        r.Should().HaveCount(1246);
    }

    [TestMethod]
    public override void NoBars_ReturnsEmpty()
    {
        IReadOnlyList<TsiResult> r0 = Nobars
            .ToTsi();

        r0.Should().BeEmpty();

        IReadOnlyList<TsiResult> r1 = Onebar
            .ToTsi();

        r1.Should().HaveCount(1);
    }

    [TestMethod]
    public void Removed_WithWarmupPeriods_TruncatesResults()
    {
        IReadOnlyList<TsiResult> sut = Bars
            .ToTsi()
            .RemoveWarmupPeriods();

        // assertions
        sut.Should().HaveCount(502 - (25 + 13 + 250));

        TsiResult last = sut[^1];
        last.Tsi.Should().BeApproximately(-28.3513, Money4);
        last.Signal.Should().BeApproximately(-29.3597, Money4);
    }

    [TestMethod]
    public void Exceptions_InvalidParameters_ThrowsArgumentOutOfRangeException()
    {
        // bad lookback period
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(
            static () => Bars.ToTsi(0));

        // bad smoothing period
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(
            static () => Bars.ToTsi(25, 0));

        // bad signal period
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(
            static () => Bars.ToTsi(25, 13, -1));
    }
}
