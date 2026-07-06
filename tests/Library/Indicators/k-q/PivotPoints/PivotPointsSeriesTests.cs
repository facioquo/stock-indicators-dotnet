namespace StaticSeries;

[TestClass]
public class PivotPointsSeriesTests : StaticSeriesTestBase
{
    [TestMethod]
    public override void DefaultParameters_ReturnsExpectedResults()
    {
        const BarInterval barInterval = BarInterval.Month;
        const PivotPointType pointType = PivotPointType.Standard;

        IReadOnlyList<PivotPointsResult> sut = Bars
            .ToPivotPoints(barInterval, pointType);

        // proper quantities
        sut.Should().HaveCount(502);
        sut.Where(static x => x.PP != null).Should().HaveCount(482);

        // sample values
        PivotPointsResult r1 = sut[19];
        r1.PP.Should().BeNull();
        r1.S1.Should().BeNull();
        r1.S2.Should().BeNull();
        r1.S3.Should().BeNull();
        r1.S4.Should().BeNull();
        r1.R1.Should().BeNull();
        r1.R2.Should().BeNull();
        r1.R3.Should().BeNull();
        r1.R4.Should().BeNull();

        PivotPointsResult r2 = sut[20];
        r2.PP.Should().BeApproximately((double)214.5000m, Money3);
        r2.S1.Should().BeApproximately((double)211.9800m, Money3);
        r2.S2.Should().BeApproximately((double)209.0000m, Money3);
        r2.S3.Should().BeApproximately((double)206.4800m, Money3);
        r2.S4.Should().BeNull();
        r2.R1.Should().BeApproximately((double)217.4800m, Money3);
        r2.R2.Should().BeApproximately((double)220.0000m, Money3);
        r2.R3.Should().BeApproximately((double)222.9800m, Money3);
        r2.R4.Should().BeNull();

        PivotPointsResult r3 = sut[149];
        r3.PP.Should().BeApproximately((double)233.6400m, Money3);
        r3.S1.Should().BeApproximately((double)230.8100m, Money3);
        r3.S2.Should().BeApproximately((double)226.3300m, Money3);
        r3.S3.Should().BeApproximately((double)223.5000m, Money3);
        r3.S4.Should().BeNull();
        r3.R1.Should().BeApproximately((double)238.1200m, Money3);
        r3.R2.Should().BeApproximately((double)240.9500m, Money3);
        r3.R3.Should().BeApproximately((double)245.4300m, Money3);
        r3.R4.Should().BeNull();

        PivotPointsResult r4 = sut[250];
        r4.PP.Should().BeApproximately((double)251.2767m, Money3);
        r4.S1.Should().BeApproximately((double)247.6133m, Money3);
        r4.S2.Should().BeApproximately((double)241.2867m, Money3);
        r4.S3.Should().BeApproximately((double)237.6233m, Money3);
        r4.S4.Should().BeNull();
        r4.R1.Should().BeApproximately((double)257.6033m, Money3);
        r4.R2.Should().BeApproximately((double)261.2667m, Money3);
        r4.R3.Should().BeApproximately((double)267.5933m, Money3);
        r4.R4.Should().BeNull();

        PivotPointsResult r5 = sut[251];
        r5.PP.Should().BeApproximately((double)255.1967m, Money3);
        r5.S1.Should().BeApproximately((double)251.6933m, Money3);
        r5.S2.Should().BeApproximately((double)246.3667m, Money3);
        r5.S3.Should().BeApproximately((double)242.8633m, Money3);
        r5.S4.Should().BeNull();
        r5.R1.Should().BeApproximately((double)260.5233m, Money3);
        r5.R2.Should().BeApproximately((double)264.0267m, Money3);
        r5.R3.Should().BeApproximately((double)269.3533m, Money3);
        r5.R4.Should().BeNull();

        PivotPointsResult r6 = sut[501];
        r6.PP.Should().BeApproximately((double)266.6767m, Money3);
        r6.S1.Should().BeApproximately((double)258.9633m, Money3);
        r6.S2.Should().BeApproximately((double)248.9667m, Money3);
        r6.S3.Should().BeApproximately((double)241.2533m, Money3);
        r6.S4.Should().BeNull();
        r6.R1.Should().BeApproximately((double)276.6733m, Money3);
        r6.R2.Should().BeApproximately((double)284.3867m, Money3);
        r6.R3.Should().BeApproximately((double)294.3833m, Money3);
        r6.R4.Should().BeNull();
    }

    [TestMethod]
    public void Camarilla_AsPivotType_ReturnsExpectedResult()
    {
        const BarInterval barInterval = BarInterval.Week;
        const PivotPointType pointType = PivotPointType.Camarilla;

        IReadOnlyList<Bar> h = Data.GetDefault(38);
        IReadOnlyList<PivotPointsResult> sut = h.ToPivotPoints(barInterval, pointType);

        // proper quantities
        sut.Should().HaveCount(38);
        sut.Where(static x => x.PP != null).Should().HaveCount(33);

        // sample values
        PivotPointsResult r2 = sut[4];
        r2.R4.Should().BeNull();
        r2.R3.Should().BeNull();
        r2.PP.Should().BeNull();
        r2.S1.Should().BeNull();
        r2.S2.Should().BeNull();
        r2.R1.Should().BeNull();
        r2.R2.Should().BeNull();
        r2.S3.Should().BeNull();
        r2.S4.Should().BeNull();

        PivotPointsResult r3 = sut[5];
        r3.PP.Should().BeApproximately((double)271.0200m, Money3);
        r3.S1.Should().BeApproximately((double)270.13725m, Money5);
        r3.S2.Should().BeApproximately((double)269.2545m, Money3);
        r3.S3.Should().BeApproximately((double)268.3718m, Money3);
        r3.S4.Should().BeApproximately((double)265.7235m, Money3);
        r3.R1.Should().BeApproximately((double)271.9028m, Money3);
        r3.R2.Should().BeApproximately((double)272.7855m, Money3);
        r3.R3.Should().BeApproximately((double)273.66825m, Money5);
        r3.R4.Should().BeApproximately((double)276.3165m, Money3);

        PivotPointsResult r4 = sut[22];
        r4.PP.Should().BeApproximately((double)268.9600m, Money3);
        r4.S1.Should().BeApproximately((double)267.9819m, Money3);
        r4.S2.Should().BeApproximately((double)267.0038m, Money3);
        r4.S3.Should().BeApproximately((double)266.0258m, Money3);
        r4.S4.Should().BeApproximately((double)263.0915m, Money3);
        r4.R1.Should().BeApproximately((double)269.9381m, Money3);
        r4.R2.Should().BeApproximately((double)270.9162m, Money3);
        r4.R3.Should().BeApproximately((double)271.89425m, Money5);
        r4.R4.Should().BeApproximately((double)274.8285m, Money3);

        PivotPointsResult r5 = sut[23];
        r5.PP.Should().BeApproximately((double)257.1700m, Money3);
        r5.S1.Should().BeApproximately((double)255.5640m, Money3);
        r5.S2.Should().BeApproximately((double)253.9580m, Money3);
        r5.S3.Should().BeApproximately((double)252.3520m, Money3);
        r5.S4.Should().BeApproximately((double)247.5340m, Money3);
        r5.R1.Should().BeApproximately((double)258.7760m, Money3);
        r5.R2.Should().BeApproximately((double)260.3820m, Money3);
        r5.R3.Should().BeApproximately((double)261.9880m, Money3);
        r5.R4.Should().BeApproximately((double)266.8060m, Money3);

        PivotPointsResult r6 = sut[37];
        r6.PP.Should().BeApproximately((double)243.1500m, Money3);
        r6.S1.Should().BeApproximately((double)241.56325m, Money5);
        r6.S2.Should().BeApproximately((double)239.9765m, Money3);
        r6.S3.Should().BeApproximately((double)238.3898m, Money3);
        r6.S4.Should().BeApproximately((double)233.6295m, Money3);
        r6.R1.Should().BeApproximately((double)244.7368m, Money3);
        r6.R2.Should().BeApproximately((double)246.3235m, Money3);
        r6.R3.Should().BeApproximately((double)247.91025m, Money5);
        r6.R4.Should().BeApproximately((double)252.6705m, Money3);
    }

    [TestMethod]
    public void Demark_AsPivotType_ReturnsExpectedResult()
    {
        const BarInterval barInterval = BarInterval.Month;
        const PivotPointType pointType = PivotPointType.Demark;

        IReadOnlyList<PivotPointsResult> sut = Bars
            .ToPivotPoints(barInterval, pointType);

        // proper quantities
        sut.Should().HaveCount(502);
        sut.Where(static x => x.PP != null).Should().HaveCount(482);

        // sample values
        PivotPointsResult r1 = sut[19];
        r1.R4.Should().BeNull();
        r1.R3.Should().BeNull();
        r1.PP.Should().BeNull();
        r1.S1.Should().BeNull();
        r1.S2.Should().BeNull();
        r1.R1.Should().BeNull();
        r1.R2.Should().BeNull();
        r1.S3.Should().BeNull();
        r1.S4.Should().BeNull();

        PivotPointsResult r2 = sut[20];
        r2.R4.Should().BeNull();
        r2.R3.Should().BeNull();
        r2.PP.Should().BeApproximately((double)215.1300m, Money3);
        r2.S1.Should().BeApproximately((double)213.2400m, Money3);
        r2.S2.Should().BeNull();
        r2.R1.Should().BeApproximately((double)218.7400m, Money3);
        r2.R2.Should().BeNull();
        r2.S3.Should().BeNull();
        r2.S4.Should().BeNull();

        PivotPointsResult r3 = sut[149];
        r3.R4.Should().BeNull();
        r3.R3.Should().BeNull();
        r3.PP.Should().BeApproximately((double)234.3475m, Money3);
        r3.S1.Should().BeApproximately((double)232.2250m, Money3);
        r3.S2.Should().BeNull();
        r3.R1.Should().BeApproximately((double)239.5350m, Money3);
        r3.R2.Should().BeNull();
        r3.S3.Should().BeNull();
        r3.S4.Should().BeNull();

        PivotPointsResult r4 = sut[250];
        r4.R4.Should().BeNull();
        r4.R3.Should().BeNull();
        r4.PP.Should().BeApproximately((double)252.1925m, Money3);
        r4.S1.Should().BeApproximately((double)249.4450m, Money3);
        r4.S2.Should().BeNull();
        r4.R1.Should().BeApproximately((double)259.4350m, Money3);
        r4.R2.Should().BeNull();
        r4.S3.Should().BeNull();
        r4.S4.Should().BeNull();

        PivotPointsResult r5 = sut[251];
        r5.R4.Should().BeNull();
        r5.R3.Should().BeNull();
        r5.PP.Should().BeApproximately((double)256.0725m, Money3);
        r5.S1.Should().BeApproximately((double)253.4450m, Money3);
        r5.S2.Should().BeNull();
        r5.R1.Should().BeApproximately((double)262.2750m, Money3);
        r5.R2.Should().BeNull();
        r5.S3.Should().BeNull();
        r5.S4.Should().BeNull();

        PivotPointsResult r6 = sut[501];
        r6.R4.Should().BeNull();
        r6.R3.Should().BeNull();
        r6.PP.Should().BeApproximately((double)268.6050m, Money3);
        r6.S1.Should().BeApproximately((double)262.8200m, Money3);
        r6.S2.Should().BeNull();
        r6.R1.Should().BeApproximately((double)280.5300m, Money3);
        r6.R2.Should().BeNull();
        r6.S3.Should().BeNull();
        r6.S4.Should().BeNull();

        // special Demark case: test close = open
        WindowPoint d1 = PivotPoints.GetPivotPointDemark(125, 200, 100, 125);
        d1.PP.Should().Be(550d / 4);
    }

    [TestMethod]
    public void Fibonacci_AsPivotType_ReturnsExpectedResult()
    {
        const BarInterval barInterval = BarInterval.OneHour;
        const PivotPointType pointType = PivotPointType.Fibonacci;

        IReadOnlyList<Bar> h = Data.GetIntraday(300);
        IReadOnlyList<PivotPointsResult> sut = h.ToPivotPoints(barInterval, pointType);

        // proper quantities
        sut.Should().HaveCount(300);
        sut.Where(static x => x.PP != null).Should().HaveCount(241);

        // sample values
        PivotPointsResult r1 = sut[58];
        r1.R4.Should().BeNull();
        r1.R3.Should().BeNull();
        r1.PP.Should().BeNull();
        r1.S1.Should().BeNull();
        r1.S2.Should().BeNull();
        r1.R1.Should().BeNull();
        r1.R2.Should().BeNull();
        r1.S3.Should().BeNull();
        r1.S4.Should().BeNull();

        PivotPointsResult r2 = sut[59];
        r2.PP.Should().BeApproximately((double)368.4967m, Money3);
        r2.S1.Should().BeApproximately((double)367.9237m, Money3);
        r2.S2.Should().BeApproximately((double)367.5697m, Money3);
        r2.S3.Should().BeApproximately((double)366.9967m, Money3);
        r2.R1.Should().BeApproximately((double)369.0697m, Money3);
        r2.R2.Should().BeApproximately((double)369.4237m, Money3);
        r2.R3.Should().BeApproximately((double)369.9967m, Money3);

        PivotPointsResult r3 = sut[118];
        r3.PP.Should().BeApproximately((double)368.4967m, Money3);
        r3.S1.Should().BeApproximately((double)367.9237m, Money3);
        r3.S2.Should().BeApproximately((double)367.5697m, Money3);
        r3.S3.Should().BeApproximately((double)366.9967m, Money3);
        r3.R1.Should().BeApproximately((double)369.0697m, Money3);
        r3.R2.Should().BeApproximately((double)369.4237m, Money3);
        r3.R3.Should().BeApproximately((double)369.9967m, Money3);

        PivotPointsResult r4 = sut[119];
        r4.PP.Should().BeApproximately((double)369.0000m, Money3);
        r4.S1.Should().BeApproximately((double)368.5760m, Money3);
        r4.S2.Should().BeApproximately((double)368.3140m, Money3);
        r4.S3.Should().BeApproximately((double)367.8900m, Money3);
        r4.R1.Should().BeApproximately((double)369.4240m, Money3);
        r4.R2.Should().BeApproximately((double)369.6860m, Money3);
        r4.R3.Should().BeApproximately((double)370.1100m, Money3);

        PivotPointsResult r5 = sut[149];
        r5.PP.Should().BeApproximately((double)369.0000m, Money3);
        r5.S1.Should().BeApproximately((double)368.5760m, Money3);
        r5.S2.Should().BeApproximately((double)368.3140m, Money3);
        r5.S3.Should().BeApproximately((double)367.8900m, Money3);
        r5.R1.Should().BeApproximately((double)369.4240m, Money3);
        r5.R2.Should().BeApproximately((double)369.6860m, Money3);
        r5.R3.Should().BeApproximately((double)370.1100m, Money3);

        PivotPointsResult r6 = sut[299];
        r6.PP.Should().BeApproximately((double)368.8200m, Money3);
        r6.S1.Should().BeApproximately((double)367.5632m, Money3);
        r6.S2.Should().BeApproximately((double)366.7868m, Money3);
        r6.S3.Should().BeApproximately((double)365.5300m, Money3);
        r6.R1.Should().BeApproximately((double)370.0768m, Money3);
        r6.R2.Should().BeApproximately((double)370.8532m, Money3);
        r6.R3.Should().BeApproximately((double)372.1100m, Money3);
    }

    [TestMethod]
    public void Woodie_AsPivotType_ReturnsExpectedResult()
    {
        const BarInterval barInterval = BarInterval.Day;
        const PivotPointType pointType = PivotPointType.Woodie;

        IReadOnlyList<Bar> h = Data.GetIntraday();
        IReadOnlyList<PivotPointsResult> sut = h.ToPivotPoints(barInterval, pointType);

        // proper quantities
        sut.Should().HaveCount(1564);
        sut.Where(static x => x.PP != null).Should().HaveCount(1173);

        // sample values
        PivotPointsResult r2 = sut[390];
        r2.R4.Should().BeNull();
        r2.R3.Should().BeNull();
        r2.PP.Should().BeNull();
        r2.S1.Should().BeNull();
        r2.S2.Should().BeNull();
        r2.R1.Should().BeNull();
        r2.R2.Should().BeNull();
        r2.S3.Should().BeNull();
        r2.S4.Should().BeNull();

        PivotPointsResult r3 = sut[391];
        r3.PP.Should().BeApproximately((double)368.7875m, Money3);
        r3.S1.Should().BeApproximately((double)367.9850m, Money3);
        r3.S2.Should().BeApproximately((double)365.1175m, Money3);
        r3.S3.Should().BeApproximately((double)364.3150m, Money3);
        r3.R1.Should().BeApproximately((double)371.6550m, Money3);
        r3.R2.Should().BeApproximately((double)372.4575m, Money3);
        r3.R3.Should().BeApproximately((double)375.3250m, Money3);

        PivotPointsResult r4 = sut[1172];
        r4.PP.Should().BeApproximately((double)370.9769m, Money3);
        r4.S1.Should().BeApproximately((double)370.7938m, Money3);
        r4.S2.Should().BeApproximately((double)368.6845m, Money3);
        r4.S3.Should().BeApproximately((double)368.5014m, Money3);
        r4.R1.Should().BeApproximately((double)373.0862m, Money3);
        r4.R2.Should().BeApproximately((double)373.2693m, Money3);
        r4.R3.Should().BeApproximately((double)375.3786m, Money3);

        PivotPointsResult r5 = sut[1173];
        r5.PP.Should().BeApproximately((double)371.3625m, Money3);
        r5.S1.Should().BeApproximately((double)370.2650m, Money3);
        r5.S2.Should().BeApproximately((double)369.9525m, Money3);
        r5.S3.Should().BeApproximately((double)368.8550m, Money3);
        r5.R1.Should().BeApproximately((double)371.6750m, Money3);
        r5.R2.Should().BeApproximately((double)372.7725m, Money3);
        r5.R3.Should().BeApproximately((double)373.0850m, Money3);

        PivotPointsResult r6 = sut[1563];
        r6.PP.Should().BeApproximately((double)371.3625m, Money3);
        r6.S1.Should().BeApproximately((double)370.2650m, Money3);
        r6.S2.Should().BeApproximately((double)369.9525m, Money3);
        r6.S3.Should().BeApproximately((double)368.8550m, Money3);
        r6.R1.Should().BeApproximately((double)371.6750m, Money3);
        r6.R2.Should().BeApproximately((double)372.7725m, Money3);
        r6.R3.Should().BeApproximately((double)373.0850m, Money3);
    }

    [TestMethod]
    public override void BadBars_DoesNotFail()
    {
        IReadOnlyList<PivotPointsResult> r = BadBars
            .ToPivotPoints(BarInterval.Week);

        r.Should().HaveCount(502);
    }

    [TestMethod]
    public override void NoBars_ReturnsEmpty()
    {
        IReadOnlyList<PivotPointsResult> r0 = Nobars
            .ToPivotPoints(BarInterval.Week);

        r0.Should().BeEmpty();

        IReadOnlyList<PivotPointsResult> r1 = Onebar
            .ToPivotPoints(BarInterval.Week);

        r1.Should().HaveCount(1);
    }

    [TestMethod]
    public void Removed_WithWarmupPeriods_TruncatesResults()
    {
        const BarInterval barInterval = BarInterval.Month;
        const PivotPointType pointType = PivotPointType.Standard;

        IReadOnlyList<PivotPointsResult> sut = Bars
            .ToPivotPoints(barInterval, pointType)
            .RemoveWarmupPeriods();

        // assertions
        sut.Should().HaveCount(482);

        PivotPointsResult last = sut[^1];
        last.PP.Should().BeApproximately((double)266.6767m, Money3);
        last.S1.Should().BeApproximately((double)258.9633m, Money3);
        last.S2.Should().BeApproximately((double)248.9667m, Money3);
        last.S3.Should().BeApproximately((double)241.2533m, Money3);
        last.S4.Should().BeNull();
        last.R1.Should().BeApproximately((double)276.6733m, Money3);
        last.R2.Should().BeApproximately((double)284.3867m, Money3);
        last.R3.Should().BeApproximately((double)294.3833m, Money3);
        last.R4.Should().BeNull();
    }

    [TestMethod]
    public void Exceptions_InvalidParameters_ThrowsArgumentOutOfRangeException()
    {
        // bad pointtype size
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(
            static () => Bars
                .ToPivotPoints(BarInterval.Week, (PivotPointType)999));

        // bad window size
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(
            static () => Bars
                .ToPivotPoints(BarInterval.ThreeMinutes));
    }
}
