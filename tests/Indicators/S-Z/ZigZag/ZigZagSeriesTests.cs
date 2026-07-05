namespace StaticSeries;

[TestClass]
public class ZigZagSeriesTests : StaticSeriesTestBase
{
    /// <summary>
    /// on Close
    /// </summary>
    [TestMethod]
    public override void DefaultParameters_ReturnsExpectedResults()
    {
        IReadOnlyList<ZigZagResult> sut =
            Bars.ToZigZag(EndType.Close, 3);

        // proper quantities
        sut.Should().HaveCount(502);
        sut.Where(static x => x.ZigZag != null).Should().HaveCount(234);
        sut.Where(static x => x.RetraceHigh != null).Should().HaveCount(234);
        sut.Where(static x => x.RetraceLow != null).Should().HaveCount(221);
        sut.Where(static x => x.PointType != null).Should().HaveCount(14);

        // sample values
        ZigZagResult r0 = sut[249];
        r0.ZigZag.Should().BeNull();
        r0.RetraceHigh.Should().BeNull();
        r0.RetraceLow.Should().BeNull();
        r0.PointType.Should().BeNull();

        ZigZagResult r1 = sut[277];
        r1.ZigZag.Should().Be(248.13m);
        r1.RetraceHigh.Should().Be(272.248m);
        r1.RetraceLow.Should().Be(248.13m);
        r1.PointType.Should().Be("L");

        ZigZagResult r2 = sut[483];
        r2.ZigZag.Should().Be(272.52m);
        r2.RetraceHigh.Should().Be(272.52m);
        r2.RetraceLow.Should().Be(248.799m);
        r2.PointType.Should().Be("H");

        ZigZagResult r3 = sut[439];
        ((double?)r3.ZigZag).Should().BeApproximately((double)276.0133m, Money3);
        ((double?)r3.RetraceHigh).Should().BeApproximately((double)280.9158m, Money3);
        ((double?)r3.RetraceLow).Should().BeApproximately((double)264.5769m, Money3);
        r3.PointType.Should().BeNull();

        ZigZagResult r4 = sut[500];
        ((double?)r4.ZigZag).Should().BeApproximately((double)241.4575m, Money3);
        ((double?)r4.RetraceHigh).Should().BeApproximately((double)246.7933m, Money3);
        r4.RetraceLow.Should().BeNull();
        r4.PointType.Should().BeNull();

        ZigZagResult r5 = sut[501];
        r5.ZigZag.Should().Be(245.28m);
        r5.RetraceHigh.Should().Be(245.28m);
        r5.RetraceLow.Should().BeNull();
        r5.PointType.Should().BeNull();
    }

    [TestMethod]
    public void StandardHighLow_WithHighLowEndType_ReturnsExpectedResult()
    {
        IReadOnlyList<ZigZagResult> sut =
            Bars.ToZigZag(EndType.HighLow, 3);

        // proper quantities
        sut.Should().HaveCount(502);
        sut.Where(static x => x.ZigZag != null).Should().HaveCount(463);
        sut.Where(static x => x.RetraceHigh != null).Should().HaveCount(463);
        sut.Where(static x => x.RetraceLow != null).Should().HaveCount(442);
        sut.Where(static x => x.PointType != null).Should().HaveCount(30);

        // sample values
        ZigZagResult r38 = sut[38];
        r38.ZigZag.Should().BeNull();
        r38.RetraceHigh.Should().BeNull();
        r38.RetraceLow.Should().BeNull();
        r38.PointType.Should().BeNull();

        ZigZagResult r277 = sut[277];
        r277.ZigZag.Should().Be(252.9550m);
        ((double?)r277.RetraceHigh).Should().BeApproximately((double)262.8054m, Money3);
        ((double?)r277.RetraceLow).Should().BeApproximately((double)245.4467m, Money3);
        r277.PointType.Should().BeNull();

        ZigZagResult r316 = sut[316];
        r316.ZigZag.Should().Be(249.48m);
        r316.RetraceHigh.Should().Be(258.34m);
        r316.RetraceLow.Should().Be(249.48m);
        r316.PointType.Should().Be("L");

        ZigZagResult r456 = sut[456];
        ((double?)r456.ZigZag).Should().BeApproximately((double)261.3325m, Money3);
        ((double?)r456.RetraceHigh).Should().BeApproximately((double)274.3419m, Money3);
        ((double?)r456.RetraceLow).Should().BeApproximately((double)256.1050m, Money3);
        r456.PointType.Should().BeNull();

        ZigZagResult r500 = sut[500];
        ((double?)r500.ZigZag).Should().BeApproximately((double)240.1667m, Money3);
        ((double?)r500.RetraceHigh).Should().BeApproximately((double)246.95083m, Money5);
        r500.RetraceLow.Should().BeNull();
        r500.PointType.Should().BeNull();

        ZigZagResult r501 = sut[501];
        r501.ZigZag.Should().Be(245.54m);
        r501.RetraceHigh.Should().Be(245.54m);
        r501.RetraceLow.Should().BeNull();
        r501.PointType.Should().BeNull();
    }

    [TestMethod]
    public void ChainingFromResults_WorksAsExpected()
    {
        IReadOnlyList<SmaResult> sut = Bars
            .ToZigZag(EndType.Close, 3)
            .ToSma(10);

        sut.Should().HaveCount(502);
        sut.Where(static x => x.Sma != null).Should().HaveCount(225);
    }

    [TestMethod]
    public void NoEntry_ThresholdNeverMet_ReturnsExpected()
    {
        // thresholds are never met
        IReadOnlyList<Bar> bars = Data.BarsFromJson("_issue0616.zigzag.thresholds.json");

        IReadOnlyList<ZigZagResult> sut = bars
            .ToZigZag();

        sut.Should().HaveCountGreaterThan(0);
        sut.Where(static x => x.PointType != null).Should().BeEmpty();
    }

    [TestMethod]
    public override void BadBars_DoesNotFail()
    {
        IReadOnlyList<ZigZagResult> r1 = BadBars
            .ToZigZag();

        r1.Should().HaveCount(502);

        IReadOnlyList<ZigZagResult> r2 = BadBars
            .ToZigZag(EndType.HighLow);

        r2.Should().HaveCount(502);
    }

    [TestMethod]
    public override void NoBars_ReturnsEmpty()
    {
        IReadOnlyList<ZigZagResult> r0 = Nobars
            .ToZigZag();

        r0.Should().BeEmpty();

        IReadOnlyList<ZigZagResult> r1 = Onebar
            .ToZigZag();

        r1.Should().HaveCount(1);
    }

    [TestMethod]
    public void Condense_RemovesNullResults_ReturnsCondensed()
    {
        IReadOnlyList<ZigZagResult> sut = Bars
            .ToZigZag(EndType.Close, 3)
            .Condense();

        // assertions
        sut.Should().HaveCount(14);
    }

    [TestMethod]
    public void Exceptions_InvalidParameters_ThrowsArgumentOutOfRangeException()
    {
        // bad lookback period
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(
            static () => Bars.ToZigZag(EndType.Close, 0));

        // bad end type
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(
            static () => Bars.ToZigZag((EndType)int.MaxValue, 2));
    }

    [TestMethod]
    public void Issue0616_SchrodingerScenario_HighAndLowThresholdMet_IsDeterministic()
    {
        IReadOnlyList<Bar> bars = Data.BarsFromJson("_issue0616.zigzag.schrodinger.json");

        IReadOnlyList<ZigZagResult> r1 = bars.ToZigZag(EndType.Close, 0.25m).ToList();
        r1.Should().HaveCount(342);

        // first period has High/Low that exceeds threhold
        // where it is both a H and L pivot simultaenously
        IReadOnlyList<ZigZagResult> r2 = bars.ToZigZag(EndType.HighLow, 3).ToList();
        r2.Should().HaveCount(342);
    }

    [TestMethod]
    public void Issue0632_ThresholdNeverMet_ReturnsExpected()
    {
        // thresholds are never met
        IReadOnlyList<Bar> bars = Data.BarsFromJson("_issue0632.zigzag.thresholds.json");

        IReadOnlyList<ZigZagResult> sut = bars
            .ToZigZag();

        sut.Should().HaveCount(17);
    }

    [TestMethod]
    public void Issue1949_UserErrors()
    {
        IReadOnlyList<Bar> bars = Data.BarsFromCsv("_issue1949.zigzag.csv");

        IReadOnlyList<ZigZagResult> r = bars
            .ToZigZag(endType: EndType.HighLow, percentChange: 5.0m);

        const string msg = "results size should match original bars size";

        r.Should().HaveCount(1430, msg);
        r.Should().HaveCount(bars.Count, msg);
        r.Where(static x => x.ZigZag is not null).Should().HaveCount(726);
        r.Where(static x => x.PointType is not null).Should().HaveCount(1);
        r[704].ZigZag.Should().Be(75540.8m);
        r[704].PointType.Should().Be("L");
    }
}
