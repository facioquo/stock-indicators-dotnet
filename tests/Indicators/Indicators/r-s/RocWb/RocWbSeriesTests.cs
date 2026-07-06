namespace StaticSeries;

[TestClass]
public class RocWbSeriesTests : StaticSeriesTestBase
{
    [TestMethod]
    public override void DefaultParameters_ReturnsExpectedResults()
    {
        IReadOnlyList<RocWbResult> sut = Bars
            .ToRocWb(20, 3, 20);

        // proper quantities
        sut.Should().HaveCount(502);
        sut.Where(static x => x.Roc != null).Should().HaveCount(482);
        sut.Where(static x => x.RocEma != null).Should().HaveCount(480);
        sut.Where(static x => x.UpperBand != null).Should().HaveCount(463);
        sut.Where(static x => x.LowerBand != null).Should().HaveCount(463);

        // sample values
        RocWbResult r19 = sut[19];
        r19.Roc.Should().BeNull();
        r19.RocEma.Should().BeNull();
        r19.UpperBand.Should().BeNull();
        r19.LowerBand.Should().BeNull();

        RocWbResult r20 = sut[20];
        (r20.Roc.Value).Should().BeApproximately(1.0573, Money4);
        r20.RocEma.Should().BeNull();
        r20.UpperBand.Should().BeNull();
        r20.LowerBand.Should().BeNull();

        RocWbResult r22 = sut[22];
        (r22.RocEma.Value).Should().BeApproximately(0.9617, Money4);
        r22.UpperBand.Should().BeNull();
        r22.LowerBand.Should().BeNull();

        RocWbResult r23 = sut[23];
        (r23.RocEma.Value).Should().BeApproximately(0.8582, Money4);
        r23.UpperBand.Should().BeNull();
        r23.LowerBand.Should().BeNull();

        RocWbResult r38 = sut[38];
        (r38.RocEma.Value).Should().BeApproximately(3.6872, Money4);
        r38.UpperBand.Should().BeNull();
        r38.LowerBand.Should().BeNull();

        RocWbResult r39 = sut[39];
        (r39.RocEma.Value).Should().BeApproximately(4.5348, Money4);
        (r39.UpperBand.Value).Should().BeApproximately(3.0359, Money4);
        (r39.LowerBand.Value).Should().BeApproximately(-3.0359, Money4);

        RocWbResult r49 = sut[49];
        (r49.RocEma.Value).Should().BeApproximately(2.3147, Money4);
        (r49.UpperBand.Value).Should().BeApproximately(3.6761, Money4);

        RocWbResult r149 = sut[149];
        (r149.UpperBand.Value).Should().BeApproximately(1.7377, Money4);

        RocWbResult r249 = sut[249];
        (r249.UpperBand.Value).Should().BeApproximately(3.0683, Money4);

        RocWbResult r501 = sut[501];
        (r501.Roc.Value).Should().BeApproximately(-8.2482, Money4);
        (r501.RocEma.Value).Should().BeApproximately(-8.3390, Money4);
        (r501.UpperBand.Value).Should().BeApproximately(6.1294, Money4);
        (r501.LowerBand.Value).Should().BeApproximately(-6.1294, Money4);
    }

    [TestMethod]
    public void UseReusable_ClosePrice_ReturnsExpectedResult()
    {
        IReadOnlyList<RocWbResult> sut = Bars
            .Use(CandlePart.Close)
            .ToRocWb(20, 3, 20);

        sut.Should().HaveCount(502);
        sut.Where(static x => x.Roc != null).Should().HaveCount(482);
    }

    [TestMethod]
    public void Chainee_FromSma_ReturnsExpectedResult()
    {
        IReadOnlyList<RocWbResult> sut = Bars
            .ToSma(2)
            .ToRocWb(20, 3, 20);

        sut.Should().HaveCount(502);
        sut.Where(static x => x.Roc != null).Should().HaveCount(481);
    }

    [TestMethod]
    public void ChainFromResults_ToSma_ReturnsExpectedResult()
    {
        IReadOnlyList<SmaResult> sut = Bars
            .ToRocWb(20, 3, 20)
            .ToSma(10);

        sut.Should().HaveCount(502);
        sut.Where(static x => x.Sma != null).Should().HaveCount(473);
    }

    [TestMethod]
    public override void BadBars_DoesNotFail()
    {
        IReadOnlyList<RocWbResult> r = BadBars
            .ToRocWb(35, 3, 35);

        r.Should().HaveCount(502);
        r.Where(static x => x.Roc is double v && double.IsNaN(v)).Should().BeEmpty();
    }

    [TestMethod]
    public override void NoBars_ReturnsEmpty()
    {
        IReadOnlyList<RocWbResult> r0 = Nobars
            .ToRocWb(5, 3, 2);

        r0.Should().BeEmpty();

        IReadOnlyList<RocWbResult> r1 = Onebar
            .ToRocWb(5, 3, 2);

        r1.Should().HaveCount(1);
    }

    [TestMethod]
    public void Removed_WithWarmupPeriods_TruncatesResults()
    {
        IReadOnlyList<RocWbResult> sut = Bars
            .ToRocWb(20, 3, 20)
            .RemoveWarmupPeriods();

        // assertions
        sut.Should().HaveCount(502 - (20 + 3 + 100));

        RocWbResult last = sut[^1];
        (last.Roc.Value).Should().BeApproximately(-8.2482, Money4);
        (last.RocEma.Value).Should().BeApproximately(-8.3390, Money4);
        (last.UpperBand.Value).Should().BeApproximately(6.1294, Money4);
        (last.LowerBand.Value).Should().BeApproximately(-6.1294, Money4);
    }

    [TestMethod]
    public void Exceptions_InvalidParameters_ThrowsArgumentOutOfRangeException()
    {
        // bad lookback period
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(
            static () => Bars.ToRocWb(0, 3, 12));

        // bad EMA period
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(
            static () => Bars.ToRocWb(14, 0, 14));

        // bad STDDEV period
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(
            static () => Bars.ToRocWb(15, 3, 16));
    }
}
