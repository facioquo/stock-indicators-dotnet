namespace StaticSeries;

[TestClass]
public class ElderRaySeriesTests : StaticSeriesTestBase
{
    [TestMethod]
    public override void DefaultParameters_ReturnsExpectedResults()
    {
        IReadOnlyList<ElderRayResult> sut = Bars
            .ToElderRay();

        // proper quantities
        sut.Should().HaveCount(502);
        sut.Where(static x => x.BullPower != null).Should().HaveCount(490);
        sut.Where(static x => x.BearPower != null).Should().HaveCount(490);

        // sample values
        ElderRayResult r1 = sut[11];
        r1.Ema.Should().BeNull();
        r1.BullPower.Should().BeNull();
        r1.BearPower.Should().BeNull();

        ElderRayResult r2 = sut[12];
        r2.Ema.Should().BeApproximately(214.0000, Money4);
        r2.BullPower.Should().BeApproximately(0.7500, Money4);
        r2.BearPower.Should().BeApproximately(-0.5100, Money4);

        ElderRayResult r3 = sut[24];
        r3.Ema.Should().BeApproximately(215.5426, Money4);
        r3.BullPower.Should().BeApproximately(1.4274, Money4);
        r3.BearPower.Should().BeApproximately(0.5474, Money4);

        ElderRayResult r4 = sut[149];
        r4.Ema.Should().BeApproximately(235.3970, Money4);
        r4.BullPower.Should().BeApproximately(0.9430, Money4);
        r4.BearPower.Should().BeApproximately(0.4730, Money4);

        ElderRayResult r5 = sut[249];
        r5.Ema.Should().BeApproximately(256.5206, Money4);
        r5.BullPower.Should().BeApproximately(1.5194, Money4);
        r5.BearPower.Should().BeApproximately(1.0694, Money4);

        ElderRayResult r6 = sut[501];
        r6.Ema.Should().BeApproximately(246.0129, Money4);
        r6.BullPower.Should().BeApproximately(-0.4729, Money4);
        r6.BearPower.Should().BeApproximately(-3.1429, Money4);
    }

    [TestMethod]
    public void ChainFromResults_ToSma_ReturnsExpectedResult()
    {
        IReadOnlyList<SmaResult> sut = Bars
            .ToElderRay()
            .ToSma(10);

        sut.Should().HaveCount(502);
        sut.Where(static x => x.Sma != null).Should().HaveCount(481);
    }

    [TestMethod]
    public override void BadBars_DoesNotFail()
    {
        IReadOnlyList<ElderRayResult> r = BadBars
            .ToElderRay();

        r.Should().HaveCount(502);
        r.Where(static x => x.BullPower is double v && double.IsNaN(v)).Should().BeEmpty();
    }

    [TestMethod]
    public override void NoBars_ReturnsEmpty()
    {
        IReadOnlyList<ElderRayResult> r0 = Nobars
            .ToElderRay();

        r0.Should().BeEmpty();

        IReadOnlyList<ElderRayResult> r1 = Onebar
            .ToElderRay();

        r1.Should().HaveCount(1);
    }

    [TestMethod]
    public void Removed_WithWarmupPeriods_TruncatesResults()
    {
        IReadOnlyList<ElderRayResult> sut = Bars
            .ToElderRay()
            .RemoveWarmupPeriods();

        // assertions
        sut.Should().HaveCount(502 - (100 + 13));

        ElderRayResult last = sut[^1];
        last.Ema.Should().BeApproximately(246.0129, Money4);
        last.BullPower.Should().BeApproximately(-0.4729, Money4);
        last.BearPower.Should().BeApproximately(-3.1429, Money4);
    }

    /// <summary>
    /// bad lookback period
    /// </summary>
    [TestMethod]
    public void Exceptions_InvalidLookback_ThrowsArgumentOutOfRangeException()
        => FluentActions
            .Invoking(static () => Bars.ToElderRay(0))
            .Should()
            .ThrowExactly<ArgumentOutOfRangeException>();
}
