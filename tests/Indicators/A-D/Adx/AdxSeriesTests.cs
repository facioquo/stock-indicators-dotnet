namespace StaticSeries;

[TestClass]
public class AdxSeriesTests : StaticSeriesTestBase
{
    [TestMethod]
    public override void DefaultParameters_ReturnsExpectedResults()
    {
        IReadOnlyList<AdxResult> sut = Bars.ToAdx();

        // proper quantities
        sut.Should().HaveCount(502);
        sut.Where(static x => x.Dx != null).Should().HaveCount(488);
        sut.Where(static x => x.Adx != null).Should().HaveCount(475);
        sut.Where(static x => x.Adxr != null).Should().HaveCount(461);

        // sample values
        AdxResult r13 = sut[13];
        r13.Pdi.Should().BeNull();
        r13.Mdi.Should().BeNull();
        r13.Dx.Should().BeNull();
        r13.Adx.Should().BeNull();

        AdxResult r14 = sut[14];
        r14.Pdi.Should().BeApproximately(21.9669, Money4);
        r14.Mdi.Should().BeApproximately(18.5462, Money4);
        r14.Dx.Should().BeApproximately(8.4433, Money4);
        r14.Adx.Should().BeNull();

        AdxResult r19 = sut[19];
        r19.Pdi.Should().BeApproximately(21.0361, Money4);
        r19.Mdi.Should().BeApproximately(25.0124, Money4);
        r19.Dx.Should().BeApproximately(8.6351, Money4);
        r19.Adx.Should().BeNull();

        AdxResult r26 = sut[26];
        r26.Adx.Should().BeNull();

        AdxResult r27 = sut[27];
        r27.Adx.Should().BeApproximately(15.9459, Money4);

        AdxResult r29 = sut[29];
        r29.Pdi.Should().BeApproximately(37.9719, Money4);
        r29.Mdi.Should().BeApproximately(14.1658, Money4);
        r29.Dx.Should().BeApproximately(45.6600, Money4);
        r29.Adx.Should().BeApproximately(19.7949, Money4);

        AdxResult r39 = sut[39];
        r39.Adxr.Should().BeNull();

        AdxResult r40 = sut[40];
        r40.Adxr.Should().BeNull();

        AdxResult r41 = sut[41];
        r41.Adxr.Should().NotBeNull();

        AdxResult r248 = sut[248];
        r248.Pdi.Should().BeApproximately(32.3167, Money4);
        r248.Mdi.Should().BeApproximately(18.2471, Money4);
        r248.Dx.Should().BeApproximately(27.8255, Money4);
        r248.Adx.Should().BeApproximately(30.5903, Money4);
        r248.Adxr.Should().NotBeNull();

        AdxResult r501 = sut[501];
        r501.Pdi.Should().BeApproximately(17.7565, Money4);
        r501.Mdi.Should().BeApproximately(31.1510, Money4);
        r501.Dx.Should().BeApproximately(27.3873, Money4);
        r501.Adx.Should().BeApproximately(34.2987, Money4);
        r501.Adxr.Should().NotBeNull();
    }

    [TestMethod]
    public void Results_AreAlwaysBounded()
    {
        IReadOnlyList<AdxResult> sut = Bars.ToAdx(14);
        sut.IsBetween(static x => x.Pdi, 0, 100);
        sut.IsBetween(static x => x.Mdi, 0, 100);
        sut.IsBetween(static x => x.Dx, 0, 100);
        sut.IsBetween(static x => x.Adx, 0, 100);
        sut.IsBetween(static x => x.Adxr, 0, 100);
    }

    [TestMethod]
    public void ChainFromResults_ToSma_ReturnsExpectedResult()
    {
        IReadOnlyList<SmaResult> sut = Bars
            .ToAdx()
            .ToSma(10);

        sut.Should().HaveCount(502);
        sut.Where(static x => x.Sma != null).Should().HaveCount(466);
    }

    [TestMethod]
    public override void BadBars_DoesNotFail()
    {
        IReadOnlyList<AdxResult> r = BadBars.ToAdx(20);

        r.Should().HaveCount(502);
        r.Where(static x => x.Adx is double v && double.IsNaN(v)).Should().BeEmpty();
    }

    [TestMethod]
    public void BigBarValues_DoesNotFail()
    {
        IReadOnlyList<AdxResult> r = BigBars.ToAdx(200);

        r.Should().HaveCount(1246);
    }

    [TestMethod]
    public override void NoBars_ReturnsEmpty()
    {
        IReadOnlyList<AdxResult> r0 = Nobars.ToAdx(5);

        r0.Should().BeEmpty();

        IReadOnlyList<AdxResult> r1 = Onebar.ToAdx(5);

        r1.Should().HaveCount(1);
    }

    [TestMethod]
    public void Issue859_HasInlineNaN_NaNsConverted()
    {
        // bars that produce in-sequence NaN values
        IReadOnlyList<Bar> bars = Data.BarsFromCsv("_issue0859.adx.nan.csv");

        IReadOnlyList<AdxResult> sut = bars.ToAdx();

        sut.Should().HaveCountGreaterThan(0);
        sut.Where(static x => x.Adx is double v && double.IsNaN(v)).Should().BeEmpty();
        sut.Should().HaveCount(595);
    }

    [TestMethod]
    public void Zeroes_WithZeroValues_DoesNotFail()
    {
        IReadOnlyList<AdxResult> r = ZeroesBars.ToAdx();

        r.Where(static x => x.Adx is double v && double.IsNaN(v)).Should().BeEmpty();
        r.Should().HaveCount(200);
    }

    [TestMethod]
    public void Removed_WithWarmupPeriods_TruncatesResults()
    {
        IReadOnlyList<AdxResult> sut = Bars
            .ToAdx()
            .RemoveWarmupPeriods();

        // assertions
        sut.Should().HaveCount(502 - ((2 * 14) + 100));

        AdxResult last = sut[^1];
        last.Pdi.Should().BeApproximately(17.7565, Money4);
        last.Mdi.Should().BeApproximately(31.1510, Money4);
        last.Dx.Should().BeApproximately(27.3873, Money4);
        last.Adx.Should().BeApproximately(34.2987, Money4);
    }

    [TestMethod] // bad lookback period
    public void Exceptions_InvalidLookback_ThrowsArgumentOutOfRangeException()
        => FluentActions
            .Invoking(static () => Bars.ToAdx(1))
            .Should()
            .ThrowExactly<ArgumentOutOfRangeException>();
}
