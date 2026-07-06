namespace StaticSeries;

[TestClass]
public class AroonSeriesTests : StaticSeriesTestBase
{
    [TestMethod]
    public override void DefaultParameters_ReturnsExpectedResults()
    {
        IReadOnlyList<AroonResult> sut = Bars
            .ToAroon();

        // proper quantities
        sut.Should().HaveCount(502);
        sut.Where(static x => x.AroonUp != null).Should().HaveCount(477);
        sut.Where(static x => x.AroonDown != null).Should().HaveCount(477);
        sut.Where(static x => x.Oscillator != null).Should().HaveCount(477);

        // sample values
        AroonResult r1 = sut[210];
        r1.AroonUp.Should().Be(100);
        r1.AroonDown.Should().Be(000);
        r1.Oscillator.Should().Be(100);

        AroonResult r2 = sut[293];
        r2.AroonUp.Should().Be(0);
        r2.AroonDown.Should().Be(40);
        r2.Oscillator.Should().Be(-40);

        AroonResult r3 = sut[298];
        r3.AroonUp.Should().Be(0);
        r3.AroonDown.Should().Be(20);
        r3.Oscillator.Should().Be(-20);

        AroonResult r4 = sut[458];
        r4.AroonUp.Should().Be(0);
        r4.AroonDown.Should().Be(100);
        r4.Oscillator.Should().Be(-100);

        AroonResult r5 = sut[501];
        r5.AroonUp.Should().Be(28);
        r5.AroonDown.Should().Be(88);
        r5.Oscillator.Should().Be(-60);
    }

    [TestMethod]
    public void Results_AreAlwaysBounded()
    {
        IReadOnlyList<AroonResult> sut = Bars.ToAroon(25);
        sut.IsBetween(static x => x.AroonUp, 0, 100);
        sut.IsBetween(static x => x.AroonDown, 0, 100);
        sut.IsBetween(static x => x.Oscillator, -100, 100);
    }

    [TestMethod]
    public void Boundary_WithRandomBars_StaysWithinBounds()
    {
        IReadOnlyList<AroonResult> sut = Data
            .GetRandom(2500)
            .ToAroon(25);

        sut.IsBetween(static x => x.AroonUp, 0d, 100d);
        sut.IsBetween(static x => x.AroonDown, 0d, 100d);
        sut.IsBetween(static x => x.Oscillator, -100d, 100d);
    }

    [TestMethod]
    public void ChainFromResults_ToSma_ReturnsExpectedResult()
    {
        IReadOnlyList<SmaResult> sut = Bars
            .ToAroon()
            .ToSma(10);

        sut.Should().HaveCount(502);
        sut.Where(static x => x.Sma != null).Should().HaveCount(468);
    }

    [TestMethod]
    public override void BadBars_DoesNotFail()
    {
        IReadOnlyList<AroonResult> r = BadBars
            .ToAroon(20);

        r.Should().HaveCount(502);
        r.Where(static x => x.Oscillator is double v && double.IsNaN(v)).Should().BeEmpty();
    }

    [TestMethod]
    public override void NoBars_ReturnsEmpty()
    {
        IReadOnlyList<AroonResult> r0 = Nobars
            .ToAroon();

        r0.Should().BeEmpty();

        IReadOnlyList<AroonResult> r1 = Onebar
            .ToAroon();

        r1.Should().HaveCount(1);
    }

    [TestMethod]
    public void Removed_WithWarmupPeriods_TruncatesResults()
    {
        IReadOnlyList<AroonResult> sut = Bars
            .ToAroon()
            .RemoveWarmupPeriods();

        // assertions
        sut.Should().HaveCount(502 - 25);

        AroonResult last = sut[^1];
        last.AroonUp.Should().Be(28);
        last.AroonDown.Should().Be(88);
        last.Oscillator.Should().Be(-60);
    }

    /// <summary>
    /// bad lookback period
    /// </summary>
    [TestMethod]
    public void Exceptions_InvalidLookback_ThrowsArgumentOutOfRangeException()
        => FluentActions
            .Invoking(static () => Bars.ToAroon(0))
            .Should()
            .ThrowExactly<ArgumentOutOfRangeException>();
}
