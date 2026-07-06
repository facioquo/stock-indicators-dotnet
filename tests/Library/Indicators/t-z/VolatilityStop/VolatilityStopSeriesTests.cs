namespace StaticSeries;

[TestClass]
public class VolatilityStopSeriesTests : StaticSeriesTestBase
{
    [TestMethod]
    public override void DefaultParameters_ReturnsExpectedResults()
    {
        IReadOnlyList<VolatilityStopResult> sut =
            Bars.ToVolatilityStop(14);

        // proper quantities
        sut.Should().HaveCount(502);
        sut.Where(static x => x.Sar != null).Should().HaveCount(448);

        // sample values
        VolatilityStopResult r53 = sut[53];
        r53.Sar.Should().BeNull();
        r53.IsStop.Should().BeNull();
        r53.LowerBand.Should().BeNull();
        r53.UpperBand.Should().BeNull();

        VolatilityStopResult r54 = sut[54];
        r54.Sar.Should().BeApproximately(226.2118, Money4);
        r54.IsStop.Should().BeFalse();
        r54.UpperBand.Should().BeApproximately(226.2118, Money4);
        r54.LowerBand.Should().BeNull();

        VolatilityStopResult r55 = sut[55];
        r55.Sar.Should().BeApproximately(226.2124, Money4);
        r55.IsStop.Should().BeFalse();
        r55.UpperBand.Should().BeApproximately(226.2124, Money4);
        r55.LowerBand.Should().BeNull();

        VolatilityStopResult r168 = sut[168];
        r168.IsStop.Should().BeTrue();

        VolatilityStopResult r282 = sut[282];
        r282.Sar.Should().BeApproximately(261.8687, Money4);
        r282.IsStop.Should().BeTrue();
        r282.UpperBand.Should().BeApproximately(261.8687, Money4);
        r282.LowerBand.Should().BeNull();

        VolatilityStopResult r283 = sut[283];
        r283.Sar.Should().BeApproximately(249.3219, Money4);
        r283.IsStop.Should().BeFalse();
        r283.LowerBand.Should().BeApproximately(249.3219, Money4);
        r283.UpperBand.Should().BeNull();

        VolatilityStopResult r284 = sut[284];
        r284.Sar.Should().BeApproximately(249.7460, Money4);
        r284.IsStop.Should().BeFalse();
        r284.LowerBand.Should().BeApproximately(249.7460, Money4);
        r284.UpperBand.Should().BeNull();

        VolatilityStopResult last = sut[^1];
        last.Sar.Should().BeApproximately(249.2423, Money4);
        last.IsStop.Should().BeFalse();
        last.UpperBand.Should().BeApproximately(249.2423, Money4);
        last.LowerBand.Should().BeNull();
    }

    [TestMethod]
    public void ChainingFromResults_WorksAsExpected()
    {
        IReadOnlyList<SmaResult> sut = Bars
            .ToVolatilityStop()
            .ToSma(10);

        sut.Should().HaveCount(502);
        sut.Where(static x => x.Sma != null).Should().HaveCount(439);
    }

    [TestMethod]
    public override void BadBars_DoesNotFail()
    {
        IReadOnlyList<VolatilityStopResult> r = BadBars
            .ToVolatilityStop();

        r.Should().HaveCount(502);
        r.Where(static x => x.Sar is double v && double.IsNaN(v)).Should().BeEmpty();
    }

    [TestMethod]
    public override void NoBars_ReturnsEmpty()
    {
        IReadOnlyList<VolatilityStopResult> r0 = Nobars
            .ToVolatilityStop();

        r0.Should().BeEmpty();

        IReadOnlyList<VolatilityStopResult> r1 = Onebar
            .ToVolatilityStop();

        r1.Should().HaveCount(1);
    }

    [TestMethod]
    public void Removed_WithWarmupPeriods_TruncatesResults()
    {
        IReadOnlyList<VolatilityStopResult> sut = Bars
            .ToVolatilityStop(14)
            .RemoveWarmupPeriods();

        // assertions
        sut.Should().HaveCount(402);

        VolatilityStopResult last = sut[^1];
        last.Sar.Should().BeApproximately(249.2423, Money4);
        last.IsStop.Should().BeFalse();
    }

    [TestMethod]
    public void Exceptions_InvalidParameters_ThrowsArgumentOutOfRangeException()
    {
        // bad lookback period
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(
            static () => Bars.ToVolatilityStop(1));

        // bad multiplier
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(
            static () => Bars.ToVolatilityStop(20, 0));
    }
}
