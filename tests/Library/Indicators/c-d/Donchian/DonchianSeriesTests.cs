namespace StaticSeries;

[TestClass]
public class DonchianSeriesTests : StaticSeriesTestBase
{
    [TestMethod]
    public override void DefaultParameters_ReturnsExpectedResults()
    {
        IReadOnlyList<DonchianResult> sut = Bars
            .ToDonchian();

        // proper quantities
        sut.Should().HaveCount(502);
        sut.Where(static x => x.Centerline != null).Should().HaveCount(482);
        sut.Where(static x => x.UpperBand != null).Should().HaveCount(482);
        sut.Where(static x => x.LowerBand != null).Should().HaveCount(482);
        sut.Where(static x => x.Width != null).Should().HaveCount(482);

        // sample values
        DonchianResult r1 = sut[19];
        r1.Centerline.Should().BeNull();
        r1.UpperBand.Should().BeNull();
        r1.LowerBand.Should().BeNull();
        r1.Width.Should().BeNull();

        DonchianResult r2 = sut[20];
        r2.Centerline.Should().BeApproximately((double)214.2700m, Money3);
        r2.UpperBand.Should().BeApproximately((double)217.0200m, Money3);
        r2.LowerBand.Should().BeApproximately((double)211.5200m, Money3);
        r2.Width.Should().BeApproximately((double)0.025669m, Money6);

        DonchianResult r3 = sut[249];
        r3.Centerline.Should().BeApproximately((double)254.2850m, Money3);
        r3.UpperBand.Should().BeApproximately((double)258.7000m, Money3);
        r3.LowerBand.Should().BeApproximately((double)249.8700m, Money3);
        r3.Width.Should().BeApproximately((double)0.034725m, Money6);

        DonchianResult r4 = sut[485];
        r4.Centerline.Should().BeApproximately((double)265.5350m, Money3);
        r4.UpperBand.Should().BeApproximately((double)274.3900m, Money3);
        r4.LowerBand.Should().BeApproximately((double)256.6800m, Money3);
        r4.Width.Should().BeApproximately((double)0.066696m, Money6);

        DonchianResult r5 = sut[501];
        r5.Centerline.Should().BeApproximately((double)251.5050m, Money3);
        r5.UpperBand.Should().BeApproximately((double)273.5900m, Money3);
        r5.LowerBand.Should().BeApproximately((double)229.4200m, Money3);
        r5.Width.Should().BeApproximately((double)0.175623m, Money6);
    }

    [TestMethod]
    public override void BadBars_DoesNotFail()
    {
        IReadOnlyList<DonchianResult> r = BadBars
            .ToDonchian(15);

        r.Should().HaveCount(502);
    }

    [TestMethod]
    public override void NoBars_ReturnsEmpty()
    {
        IReadOnlyList<DonchianResult> r0 = Nobars
            .ToDonchian();

        r0.Should().BeEmpty();

        IReadOnlyList<DonchianResult> r1 = Onebar
            .ToDonchian();

        r1.Should().HaveCount(1);
    }

    [TestMethod]
    public void Condense_RemovesNullResults_ReturnsCondensed()
    {
        IReadOnlyList<DonchianResult> sut = Bars
            .ToDonchian()
            .Condense();

        // assertions
        sut.Should().HaveCount(502 - 20);

        DonchianResult last = sut[^1];
        last.Centerline.Should().BeApproximately((double)251.5050m, Money3);
        last.UpperBand.Should().BeApproximately((double)273.5900m, Money3);
        last.LowerBand.Should().BeApproximately((double)229.4200m, Money3);
        last.Width.Should().BeApproximately((double)0.175623m, Money6);
    }

    [TestMethod]
    public void Removed_WithWarmupPeriods_TruncatesResults()
    {
        IReadOnlyList<DonchianResult> sut = Bars
            .ToDonchian()
            .RemoveWarmupPeriods();

        // assertions
        sut.Should().HaveCount(502 - 20);

        DonchianResult last = sut[^1];
        last.Centerline.Should().BeApproximately((double)251.5050m, Money3);
        last.UpperBand.Should().BeApproximately((double)273.5900m, Money3);
        last.LowerBand.Should().BeApproximately((double)229.4200m, Money3);
        last.Width.Should().BeApproximately((double)0.175623m, Money6);
    }

    /// <summary>
    /// bad lookback period
    /// </summary>
    [TestMethod]
    public void Exceptions_InvalidLookback_ThrowsArgumentOutOfRangeException()
        => FluentActions
            .Invoking(static () => Bars.ToDonchian(0))
            .Should()
            .ThrowExactly<ArgumentOutOfRangeException>();
}
