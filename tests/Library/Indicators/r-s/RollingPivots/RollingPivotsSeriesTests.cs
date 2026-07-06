namespace StaticSeries;

[TestClass]
public class RollingPivotsSeriesTests : StaticSeriesTestBase
{
    [TestMethod]
    public override void DefaultParameters_ReturnsExpectedResults()
    {
        const int windowPeriods = 11;
        const int offsetPeriods = 9;
        const PivotPointType pointType = PivotPointType.Standard;

        IReadOnlyList<RollingPivotsResult> sut =
            Bars.ToRollingPivots(windowPeriods, offsetPeriods, pointType);

        // proper quantities
        sut.Should().HaveCount(502);
        sut.Where(static x => x.PP != null).Should().HaveCount(482);

        // sample values
        RollingPivotsResult r1 = sut[19];
        r1.PP.Should().BeNull();
        r1.S1.Should().BeNull();
        r1.S2.Should().BeNull();
        r1.S3.Should().BeNull();
        r1.S4.Should().BeNull();
        r1.R1.Should().BeNull();
        r1.R2.Should().BeNull();
        r1.R3.Should().BeNull();
        r1.R4.Should().BeNull();

        RollingPivotsResult r2 = sut[20];
        r2.PP.Should().BeApproximately((double)213.6367m, Money3);
        r2.S1.Should().BeApproximately((double)212.1033m, Money3);
        r2.S2.Should().BeApproximately((double)209.9867m, Money3);
        r2.S3.Should().BeApproximately((double)208.4533m, Money3);
        r2.S4.Should().BeNull();
        r2.R1.Should().BeApproximately((double)215.7533m, Money3);
        r2.R2.Should().BeApproximately((double)217.2867m, Money3);
        r2.R3.Should().BeApproximately((double)219.4033m, Money3);
        r2.R4.Should().BeNull();

        RollingPivotsResult r3 = sut[149];
        r3.PP.Should().BeApproximately((double)233.6333m, Money3);
        r3.S1.Should().BeApproximately((double)231.3567m, Money3);
        r3.S2.Should().BeApproximately((double)227.3733m, Money3);
        r3.S3.Should().BeApproximately((double)225.0967m, Money3);
        r3.S4.Should().BeNull();
        r3.R1.Should().BeApproximately((double)237.6167m, Money3);
        r3.R2.Should().BeApproximately((double)239.8933m, Money3);
        r3.R3.Should().BeApproximately((double)243.8767m, Money3);
        r3.R4.Should().BeNull();

        RollingPivotsResult r4 = sut[249];
        r4.PP.Should().BeApproximately((double)253.9533m, Money3);
        r4.S1.Should().BeApproximately((double)251.5267m, Money3);
        r4.S2.Should().BeApproximately((double)247.4433m, Money3);
        r4.S3.Should().BeApproximately((double)245.0167m, Money3);
        r4.S4.Should().BeNull();
        r4.R1.Should().BeApproximately((double)258.0367m, Money3);
        r4.R2.Should().BeApproximately((double)260.4633m, Money3);
        r4.R3.Should().BeApproximately((double)264.5467m, Money3);
        r4.R4.Should().BeNull();

        RollingPivotsResult r5 = sut[501];
        r5.PP.Should().BeApproximately((double)260.0267m, Money3);
        r5.S1.Should().BeApproximately((double)246.4633m, Money3);
        r5.S2.Should().BeApproximately((double)238.7767m, Money3);
        r5.S3.Should().BeApproximately((double)225.2133m, Money3);
        r5.S4.Should().BeNull();
        r5.R1.Should().BeApproximately((double)267.7133m, Money3);
        r5.R2.Should().BeApproximately((double)281.2767m, Money3);
        r5.R3.Should().BeApproximately((double)288.9633m, Money3);
        r5.R4.Should().BeNull();
    }

    [TestMethod]
    public void Camarilla_AsPivotType_ReturnsExpectedResult()
    {
        const int windowPeriods = 10;
        const int offsetPeriods = 0;
        const PivotPointType pointType = PivotPointType.Camarilla;

        IReadOnlyList<Bar> h = Data.GetDefault(38);

        IReadOnlyList<RollingPivotsResult> sut = h
            .ToRollingPivots(windowPeriods, offsetPeriods, pointType);

        // proper quantities
        sut.Should().HaveCount(38);
        sut.Where(static x => x.PP != null).Should().HaveCount(28);

        // sample values
        RollingPivotsResult r1 = sut[9];
        r1.R4.Should().BeNull();
        r1.R3.Should().BeNull();
        r1.PP.Should().BeNull();
        r1.S1.Should().BeNull();
        r1.S2.Should().BeNull();
        r1.R1.Should().BeNull();
        r1.R2.Should().BeNull();
        r1.S3.Should().BeNull();
        r1.S4.Should().BeNull();

        RollingPivotsResult r2 = sut[10];
        r2.PP.Should().BeApproximately((double)267.0800m, Money3);
        r2.S1.Should().BeApproximately((double)265.8095m, Money3);
        r2.S2.Should().BeApproximately((double)264.5390m, Money3);
        r2.S3.Should().BeApproximately((double)263.2685m, Money3);
        r2.S4.Should().BeApproximately((double)259.4570m, Money3);
        r2.R1.Should().BeApproximately((double)268.3505m, Money3);
        r2.R2.Should().BeApproximately((double)269.6210m, Money3);
        r2.R3.Should().BeApproximately((double)270.8915m, Money3);
        r2.R4.Should().BeApproximately((double)274.7030m, Money3);

        RollingPivotsResult r3 = sut[22];
        r3.PP.Should().BeApproximately((double)263.2900m, Money3);
        r3.S1.Should().BeApproximately((double)261.6840m, Money3);
        r3.S2.Should().BeApproximately((double)260.0780m, Money3);
        r3.S3.Should().BeApproximately((double)258.4720m, Money3);
        r3.S4.Should().BeApproximately((double)253.6540m, Money3);
        r3.R1.Should().BeApproximately((double)264.8960m, Money3);
        r3.R2.Should().BeApproximately((double)266.5020m, Money3);
        r3.R3.Should().BeApproximately((double)268.1080m, Money3);
        r3.R4.Should().BeApproximately((double)272.9260m, Money3);

        RollingPivotsResult r4 = sut[23];
        r4.PP.Should().BeApproximately((double)257.1700m, Money3);
        r4.S1.Should().BeApproximately((double)255.5640m, Money3);
        r4.S2.Should().BeApproximately((double)253.9580m, Money3);
        r4.S3.Should().BeApproximately((double)252.3520m, Money3);
        r4.S4.Should().BeApproximately((double)247.5340m, Money3);
        r4.R1.Should().BeApproximately((double)258.7760m, Money3);
        r4.R2.Should().BeApproximately((double)260.3820m, Money3);
        r4.R3.Should().BeApproximately((double)261.9880m, Money3);
        r4.R4.Should().BeApproximately((double)266.8060m, Money3);

        RollingPivotsResult r5 = sut[37];
        r5.PP.Should().BeApproximately((double)243.1500m, Money3);
        r5.S1.Should().BeApproximately((double)240.5650m, Money3);
        r5.S2.Should().BeApproximately((double)237.9800m, Money3);
        r5.S3.Should().BeApproximately((double)235.3950m, Money3);
        r5.S4.Should().BeApproximately((double)227.6400m, Money3);
        r5.R1.Should().BeApproximately((double)245.7350m, Money3);
        r5.R2.Should().BeApproximately((double)248.3200m, Money3);
        r5.R3.Should().BeApproximately((double)250.9050m, Money3);
        r5.R4.Should().BeApproximately((double)258.6600m, Money3);
    }

    [TestMethod]
    public void Demark_AsPivotType_ReturnsExpectedResult()
    {
        const int windowPeriods = 10;
        const int offsetPeriods = 10;
        const PivotPointType pointType = PivotPointType.Demark;

        IReadOnlyList<RollingPivotsResult> sut = Bars
            .ToRollingPivots(windowPeriods, offsetPeriods, pointType);

        // proper quantities
        sut.Should().HaveCount(502);
        sut.Where(static x => x.PP != null).Should().HaveCount(482);

        // sample values
        RollingPivotsResult r1 = sut[19];
        r1.PP.Should().BeNull();
        r1.S1.Should().BeNull();
        r1.S2.Should().BeNull();
        r1.S3.Should().BeNull();
        r1.S4.Should().BeNull();
        r1.R1.Should().BeNull();
        r1.R2.Should().BeNull();
        r1.R3.Should().BeNull();
        r1.R4.Should().BeNull();

        RollingPivotsResult r2 = sut[20];
        r2.PP.Should().BeApproximately((double)212.9900m, Money3);
        r2.S1.Should().BeApproximately((double)210.8100m, Money3);
        r2.R1.Should().BeApproximately((double)214.4600m, Money3);
        r2.R2.Should().BeNull();
        r2.R3.Should().BeNull();
        r2.R4.Should().BeNull();
        r2.S2.Should().BeNull();
        r2.S3.Should().BeNull();
        r2.S4.Should().BeNull();

        RollingPivotsResult r3 = sut[149];
        r3.PP.Should().BeApproximately((double)232.6525m, Money3);
        r3.S1.Should().BeApproximately((double)229.3950m, Money3);
        r3.R1.Should().BeApproximately((double)235.6550m, Money3);
        r3.R2.Should().BeNull();
        r3.R3.Should().BeNull();
        r3.R4.Should().BeNull();
        r3.S2.Should().BeNull();
        r3.S3.Should().BeNull();
        r3.S4.Should().BeNull();

        RollingPivotsResult r4 = sut[250];
        r4.PP.Should().BeApproximately((double)252.9325m, Money3);
        r4.S1.Should().BeApproximately((double)249.4850m, Money3);
        r4.R1.Should().BeApproximately((double)255.9950m, Money3);
        r4.R2.Should().BeNull();
        r4.R3.Should().BeNull();
        r4.R4.Should().BeNull();
        r4.S2.Should().BeNull();
        r4.S3.Should().BeNull();
        r4.S4.Should().BeNull();

        RollingPivotsResult r5 = sut[251];
        r5.PP.Should().BeApproximately((double)252.6700m, Money3);
        r5.S1.Should().BeApproximately((double)248.9600m, Money3);
        r5.R1.Should().BeApproximately((double)255.4700m, Money3);
        r5.R2.Should().BeNull();
        r5.R3.Should().BeNull();
        r5.R4.Should().BeNull();
        r5.S2.Should().BeNull();
        r5.S3.Should().BeNull();
        r5.S4.Should().BeNull();

        RollingPivotsResult r6 = sut[501];
        r6.PP.Should().BeApproximately((double)264.6125m, Money3);
        r6.S1.Should().BeApproximately((double)255.6350m, Money3);
        r6.R1.Should().BeApproximately((double)276.8850m, Money3);
        r6.R2.Should().BeNull();
        r6.R3.Should().BeNull();
        r6.R4.Should().BeNull();
        r6.S2.Should().BeNull();
        r6.S3.Should().BeNull();
        r6.S4.Should().BeNull();
    }

    [TestMethod]
    public void Fibonacci_AsPivotType_ReturnsExpectedResult()
    {
        const int windowPeriods = 44;
        const int offsetPeriods = 15;
        const PivotPointType pointType = PivotPointType.Fibonacci;

        IReadOnlyList<Bar> h = Data.GetIntraday(300);

        IReadOnlyList<RollingPivotsResult> sut =
            h.ToRollingPivots(windowPeriods, offsetPeriods, pointType);

        // proper quantities
        sut.Should().HaveCount(300);
        sut.Where(static x => x.PP != null).Should().HaveCount(241);

        // sample values
        RollingPivotsResult r1 = sut[58];
        r1.R4.Should().BeNull();
        r1.R3.Should().BeNull();
        r1.PP.Should().BeNull();
        r1.S1.Should().BeNull();
        r1.S2.Should().BeNull();
        r1.R1.Should().BeNull();
        r1.R2.Should().BeNull();
        r1.S3.Should().BeNull();
        r1.S4.Should().BeNull();

        RollingPivotsResult r2 = sut[59];
        r2.PP.Should().BeApproximately((double)368.4283m, Money3);
        r2.S1.Should().BeApproximately((double)367.8553m, Money3);
        r2.S2.Should().BeApproximately((double)367.5013m, Money3);
        r2.S3.Should().BeApproximately((double)366.9283m, Money3);
        r2.R1.Should().BeApproximately((double)369.0013m, Money3);
        r2.R2.Should().BeApproximately((double)369.3553m, Money3);
        r2.R3.Should().BeApproximately((double)369.9283m, Money3);

        RollingPivotsResult r3 = sut[118];
        r3.PP.Should().BeApproximately((double)369.1573m, Money3);
        r3.S1.Should().BeApproximately((double)368.7333m, Money3);
        r3.S2.Should().BeApproximately((double)368.4713m, Money3);
        r3.S3.Should().BeApproximately((double)368.0473m, Money3);
        r3.R1.Should().BeApproximately((double)369.5813m, Money3);
        r3.R2.Should().BeApproximately((double)369.8433m, Money3);
        r3.R3.Should().BeApproximately((double)370.2673m, Money3);

        RollingPivotsResult r4 = sut[119];
        r4.PP.Should().BeApproximately((double)369.1533m, Money3);
        r4.S1.Should().BeApproximately((double)368.7293m, Money3);
        r4.S2.Should().BeApproximately((double)368.4674m, Money3);
        r4.S3.Should().BeApproximately((double)368.0433m, Money3);
        r4.R1.Should().BeApproximately((double)369.5774m, Money3);
        r4.R2.Should().BeApproximately((double)369.8393m, Money3);
        r4.R3.Should().BeApproximately((double)370.2633m, Money3);

        RollingPivotsResult r5 = sut[149];
        r5.PP.Should().BeApproximately((double)369.0183m, Money3);
        r5.S1.Should().BeApproximately((double)368.6593m, Money3);
        r5.S2.Should().BeApproximately((double)368.4374m, Money3);
        r5.S3.Should().BeApproximately((double)368.0783m, Money3);
        r5.R1.Should().BeApproximately((double)369.3774m, Money3);
        r5.R2.Should().BeApproximately((double)369.5993m, Money3);
        r5.R3.Should().BeApproximately((double)369.9583m, Money3);

        RollingPivotsResult r6 = sut[299];
        r6.PP.Should().BeApproximately((double)367.7567m, Money3);
        r6.S1.Should().BeApproximately((double)367.3174m, Money3);
        r6.S2.Should().BeApproximately((double)367.0460m, Money3);
        r6.S3.Should().BeApproximately((double)366.6067m, Money3);
        r6.R1.Should().BeApproximately((double)368.1960m, Money3);
        r6.R2.Should().BeApproximately((double)368.4674m, Money3);
        r6.R3.Should().BeApproximately((double)368.9067m, Money3);
    }

    [TestMethod]
    public void Woodie_AsPivotType_ReturnsExpectedResult()
    {
        const int windowPeriods = 375;
        const int offsetPeriods = 16;
        const PivotPointType pointType = PivotPointType.Woodie;

        IReadOnlyList<Bar> h = Data.GetIntraday();

        IReadOnlyList<RollingPivotsResult> sut = h
            .ToRollingPivots(windowPeriods, offsetPeriods, pointType);

        // proper quantities
        sut.Should().HaveCount(1564);
        sut.Where(static x => x.PP != null).Should().HaveCount(1173);

        // sample values
        RollingPivotsResult r2 = sut[390];
        r2.R4.Should().BeNull();
        r2.R3.Should().BeNull();
        r2.PP.Should().BeNull();
        r2.S1.Should().BeNull();
        r2.S2.Should().BeNull();
        r2.R1.Should().BeNull();
        r2.R2.Should().BeNull();
        r2.S3.Should().BeNull();
        r2.S4.Should().BeNull();

        RollingPivotsResult r3 = sut[391];
        r3.PP.Should().BeApproximately((double)368.7850m, Money3);
        r3.S1.Should().BeApproximately((double)367.9901m, Money3);
        r3.S2.Should().BeApproximately((double)365.1252m, Money3);
        r3.S3.Should().BeApproximately((double)364.3303m, Money3);
        r3.R1.Should().BeApproximately((double)371.6499m, Money3);
        r3.R2.Should().BeApproximately((double)372.4448m, Money3);
        r3.R3.Should().BeApproximately((double)375.3097m, Money3);

        RollingPivotsResult r4 = sut[1172];
        r4.PP.Should().BeApproximately((double)371.75m, Money3);
        r4.S1.Should().BeApproximately((double)371.04m, Money3);
        r4.S2.Should().BeApproximately((double)369.35m, Money3);
        r4.S3.Should().BeApproximately((double)368.64m, Money3);
        r4.R1.Should().BeApproximately((double)373.44m, Money3);
        r4.R2.Should().BeApproximately((double)374.15m, Money3);
        r4.R3.Should().BeApproximately((double)375.84m, Money3);

        RollingPivotsResult r5 = sut[1173];
        r5.PP.Should().BeApproximately((double)371.3625m, Money3);
        r5.S1.Should().BeApproximately((double)370.2650m, Money3);
        r5.S2.Should().BeApproximately((double)369.9525m, Money3);
        r5.S3.Should().BeApproximately((double)368.8550m, Money3);
        r5.R1.Should().BeApproximately((double)371.6750m, Money3);
        r5.R2.Should().BeApproximately((double)372.7725m, Money3);
        r5.R3.Should().BeApproximately((double)373.0850m, Money3);

        RollingPivotsResult r6 = sut[1563];
        r6.PP.Should().BeApproximately((double)369.38m, Money3);
        r6.S1.Should().BeApproximately((double)366.52m, Money3);
        r6.S2.Should().BeApproximately((double)364.16m, Money3);
        r6.S3.Should().BeApproximately((double)361.30m, Money3);
        r6.R1.Should().BeApproximately((double)371.74m, Money3);
        r6.R2.Should().BeApproximately((double)374.60m, Money3);
        r6.R3.Should().BeApproximately((double)376.96m, Money3);
    }

    [TestMethod]
    public override void BadBars_DoesNotFail()
    {
        IReadOnlyList<RollingPivotsResult> r = BadBars
            .ToRollingPivots(5, 5);

        r.Should().HaveCount(502);
    }

    [TestMethod]
    public override void NoBars_ReturnsEmpty()
    {
        IReadOnlyList<RollingPivotsResult> r0 = Nobars
            .ToRollingPivots(5, 2);

        r0.Should().BeEmpty();

        IReadOnlyList<RollingPivotsResult> r1 = Onebar
            .ToRollingPivots(5, 2);

        r1.Should().HaveCount(1);
    }

    [TestMethod]
    public void Removed_WithWarmupPeriods_TruncatesResults()
    {
        const int windowPeriods = 11;
        const int offsetPeriods = 9;
        const PivotPointType pointType = PivotPointType.Standard;

        IReadOnlyList<RollingPivotsResult> sut = Bars
            .ToRollingPivots(windowPeriods, offsetPeriods, pointType)
            .RemoveWarmupPeriods();

        // assertions
        sut.Should().HaveCount(502 - (windowPeriods + offsetPeriods));

        RollingPivotsResult last = sut[^1];
        last.PP.Should().BeApproximately((double)260.0267m, Money3);
        last.S1.Should().BeApproximately((double)246.4633m, Money3);
        last.S2.Should().BeApproximately((double)238.7767m, Money3);
        last.S3.Should().BeApproximately((double)225.2133m, Money3);
        last.S4.Should().BeNull();
        last.R1.Should().BeApproximately((double)267.7133m, Money3);
        last.R2.Should().BeApproximately((double)281.2767m, Money3);
        last.R3.Should().BeApproximately((double)288.9633m, Money3);
        last.R4.Should().BeNull();
    }

    [TestMethod]
    public void Exceptions_InvalidParameters_ThrowsArgumentOutOfRangeException()
    {
        // bad window period
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(
            static () => Bars.ToRollingPivots(0, 10));

        // bad offset period
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(
            static () => Bars.ToRollingPivots(10, -1));
    }
}
