namespace StaticSeries;

[TestClass]
public class HeikinAshiSeriesTests : StaticSeriesTestBase
{
    [TestMethod]
    public override void DefaultParameters_ReturnsExpectedResults()
    {
        IReadOnlyList<HeikinAshiResult> sut = Bars
            .ToHeikinAshi();

        // proper quantities
        sut.Should().HaveCount(502);

        // sample value
        HeikinAshiResult r = sut[501];
        ((double?)r.Open).Should().BeApproximately((double)241.3018m, Money3);
        ((double?)r.High).Should().BeApproximately((double)245.54m, Money3);
        ((double?)r.Low).Should().BeApproximately((double)241.3018m, Money3);
        ((double?)r.Close).Should().BeApproximately((double)244.6525m, Money3);
        r.Volume.Should().Be(147031456m);
    }

    [TestMethod]
    public void UseAsBars_WhenChained_ReturnsExpectedResult()
    {
        IReadOnlyList<HeikinAshiResult> haBars = Bars.ToHeikinAshi();
        IReadOnlyList<SmaResult> haSma = haBars.ToSma(5);
        Assert.AreEqual(498, haSma.Count(static x => x.Sma != null));
    }

    [TestMethod]
    public override void BadBars_DoesNotFail()
    {
        IReadOnlyList<HeikinAshiResult> r = BadBars
            .ToHeikinAshi();

        r.Should().HaveCount(502);
    }

    [TestMethod]
    public override void NoBars_ReturnsEmpty()
    {
        IReadOnlyList<HeikinAshiResult> r0 = Nobars
            .ToHeikinAshi();

        r0.Should().BeEmpty();

        IReadOnlyList<HeikinAshiResult> r1 = Onebar
            .ToHeikinAshi();

        r1.Should().HaveCount(1);
    }
}
