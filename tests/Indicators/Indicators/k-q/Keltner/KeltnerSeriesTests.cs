namespace StaticSeries;

[TestClass]
public class KeltnerSeriesTests : StaticSeriesTestBase
{
    [TestMethod]
    public override void DefaultParameters_ReturnsExpectedResults()
    {
        const int emaPeriods = 20;
        const int multiplier = 2;
        const int atrPeriods = 10;

        IReadOnlyList<KeltnerResult> sut = Bars
            .ToKeltner(emaPeriods, multiplier, atrPeriods);

        // proper quantities
        sut.Should().HaveCount(502);

        int warmupPeriod = 502 - Math.Max(emaPeriods, atrPeriods) + 1;
        sut.Where(static x => x.Centerline != null).Should().HaveCount(warmupPeriod);
        sut.Where(static x => x.UpperBand != null).Should().HaveCount(warmupPeriod);
        sut.Where(static x => x.LowerBand != null).Should().HaveCount(warmupPeriod);
        sut.Where(static x => x.Width != null).Should().HaveCount(warmupPeriod);

        // sample value
        KeltnerResult r1 = sut[485];
        r1.UpperBand.Should().BeApproximately(275.4260, Money4);
        r1.Centerline.Should().BeApproximately(265.4599, Money4);
        r1.LowerBand.Should().BeApproximately(255.4938, Money4);
        r1.Width.Should().BeApproximately(0.075085, Money6);

        KeltnerResult r2 = sut[501];
        r2.UpperBand.Should().BeApproximately(262.1873, Money4);
        r2.Centerline.Should().BeApproximately(249.3519, Money4);
        r2.LowerBand.Should().BeApproximately(236.5165, Money4);
        r2.Width.Should().BeApproximately(0.102950, Money6);
    }

    [TestMethod]
    public override void BadBars_DoesNotFail()
    {
        IReadOnlyList<KeltnerResult> r = BadBars
            .ToKeltner(10, 3, 15);

        r.Should().HaveCount(502);
        r.Where(static x => x.UpperBand is double v && double.IsNaN(v)).Should().BeEmpty();
    }

    [TestMethod]
    public override void NoBars_ReturnsEmpty()
    {
        IReadOnlyList<KeltnerResult> r0 = Nobars
            .ToKeltner();

        r0.Should().BeEmpty();

        IReadOnlyList<KeltnerResult> r1 = Onebar
            .ToKeltner();

        r1.Should().HaveCount(1);
    }

    [TestMethod]
    public void Condense_WithNoThreshold_RemovesNullValues()
    {
        const int emaPeriods = 20;
        const int multiplier = 2;
        const int atrPeriods = 10;

        IReadOnlyList<KeltnerResult> sut = Bars
            .ToKeltner(emaPeriods, multiplier, atrPeriods)
            .Condense();

        // assertions
        sut.Should().HaveCount(483);

        KeltnerResult last = sut[^1];
        last.UpperBand.Should().BeApproximately(262.1873, Money4);
        last.Centerline.Should().BeApproximately(249.3519, Money4);
        last.LowerBand.Should().BeApproximately(236.5165, Money4);
        last.Width.Should().BeApproximately(0.102950, Money6);
    }

    [TestMethod]
    public void Removed_WithWarmupPeriods_TruncatesResults()
    {
        const int emaPeriods = 20;
        const int multiplier = 2;
        const int atrPeriods = 10;
        int n = Math.Max(emaPeriods, atrPeriods);

        IReadOnlyList<KeltnerResult> sut = Bars
            .ToKeltner(emaPeriods, multiplier, atrPeriods)
            .RemoveWarmupPeriods();

        // assertions
        Assert.HasCount(502 - Math.Max(2 * n, n + 100), sut);

        KeltnerResult last = sut[^1];
        last.UpperBand.Should().BeApproximately(262.1873, Money4);
        last.Centerline.Should().BeApproximately(249.3519, Money4);
        last.LowerBand.Should().BeApproximately(236.5165, Money4);
        last.Width.Should().BeApproximately(0.102950, Money6);
    }

    [TestMethod]
    public void Exceptions_InvalidLookback_ThrowsArgumentOutOfRangeException()
    {
        // bad EMA period
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(
            static () => Bars.ToKeltner(1));

        // bad ATR period
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(
            static () => Bars.ToKeltner(20, 2, 1));

        // bad multiplier
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(
            static () => Bars.ToKeltner(20, 0));
    }
}
