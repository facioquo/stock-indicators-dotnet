namespace StaticSeries;

[TestClass]
public class StarcBandsSeriesTests : StaticSeriesTestBase
{
    [TestMethod]
    public override void DefaultParameters_ReturnsExpectedResults()
    {
        const int smaPeriods = 20;
        const int multiplier = 2;
        const int atrPeriods = 14;

        IReadOnlyList<StarcBandsResult> sut = Bars
            .ToStarcBands(smaPeriods, multiplier, atrPeriods);

        // proper quantities
        sut.Should().HaveCount(502);
        sut.Where(static x => x.Centerline != null).Should().HaveCount(483);
        sut.Where(static x => x.UpperBand != null).Should().HaveCount(483);
        sut.Where(static x => x.LowerBand != null).Should().HaveCount(483);

        // sample value
        StarcBandsResult r1 = sut[18];
        r1.Centerline.Should().BeNull();
        r1.UpperBand.Should().BeNull();
        r1.LowerBand.Should().BeNull();

        StarcBandsResult r19 = sut[19];
        r19.Centerline.Should().BeApproximately(214.5250, Money4);
        r19.UpperBand.Should().BeApproximately(217.2345, Money4);
        r19.LowerBand.Should().BeApproximately(211.8155, Money4);

        StarcBandsResult r249 = sut[249];
        r249.Centerline.Should().BeApproximately(255.5500, Money4);
        r249.UpperBand.Should().BeApproximately(258.2261, Money4);
        r249.LowerBand.Should().BeApproximately(252.8739, Money4);

        StarcBandsResult r485 = sut[485];
        r485.Centerline.Should().BeApproximately(265.4855, Money4);
        r485.UpperBand.Should().BeApproximately(275.1161, Money4);
        r485.LowerBand.Should().BeApproximately(255.8549, Money4);

        StarcBandsResult r501 = sut[501];
        r501.Centerline.Should().BeApproximately(251.8600, Money4);
        r501.UpperBand.Should().BeApproximately(264.1595, Money4);
        r501.LowerBand.Should().BeApproximately(239.5605, Money4);
    }

    [TestMethod]
    public override void BadBars_DoesNotFail()
    {
        IReadOnlyList<StarcBandsResult> r = BadBars
            .ToStarcBands(10, 3, 15);

        r.Should().HaveCount(502);
        r.Where(static x => x.UpperBand is double v && double.IsNaN(v)).Should().BeEmpty();
    }

    [TestMethod]
    public override void NoBars_ReturnsEmpty()
    {
        IReadOnlyList<StarcBandsResult> r0 = Nobars
            .ToStarcBands(10);

        r0.Should().BeEmpty();

        IReadOnlyList<StarcBandsResult> r1 = Onebar
            .ToStarcBands(10);

        r1.Should().HaveCount(1);
    }

    [TestMethod]
    public void Condense_RemovesNullResults_ReturnsCondensed()
    {
        const int smaPeriods = 20;
        const int multiplier = 2;
        const int atrPeriods = 14;
        int lookbackPeriods = Math.Max(smaPeriods, atrPeriods);

        IReadOnlyList<StarcBandsResult> sut = Bars
            .ToStarcBands(smaPeriods, multiplier, atrPeriods)
            .Condense();

        // assertions
        sut.Should().HaveCount(502 - lookbackPeriods + 1);

        StarcBandsResult last = sut[^1];
        last.Centerline.Should().BeApproximately(251.8600, Money4);
        last.UpperBand.Should().BeApproximately(264.1595, Money4);
        last.LowerBand.Should().BeApproximately(239.5605, Money4);
    }

    [TestMethod]
    public void Removed_WithWarmupPeriods_TruncatesResults()
    {
        const int smaPeriods = 20;
        const int multiplier = 2;
        const int atrPeriods = 14;
        int lookbackPeriods = Math.Max(smaPeriods, atrPeriods);

        IReadOnlyList<StarcBandsResult> sut = Bars
            .ToStarcBands(smaPeriods, multiplier, atrPeriods)
            .RemoveWarmupPeriods();

        // assertions
        sut.Should().HaveCount(502 - (lookbackPeriods + 150));

        StarcBandsResult last = sut[^1];
        last.Centerline.Should().BeApproximately(251.8600, Money4);
        last.UpperBand.Should().BeApproximately(264.1595, Money4);
        last.LowerBand.Should().BeApproximately(239.5605, Money4);
    }

    [TestMethod]
    public void Exceptions_InvalidParameters_ThrowsArgumentOutOfRangeException()
    {
        // bad EMA period
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(
            static () => Bars.ToStarcBands(1));

        // bad ATR period
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(
            static () => Bars.ToStarcBands(20, 2, 1));

        // bad multiplier
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(
            static () => Bars.ToStarcBands(20, 0));
    }
}
