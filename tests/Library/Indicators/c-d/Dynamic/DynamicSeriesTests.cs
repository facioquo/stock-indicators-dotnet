namespace StaticSeries;

[TestClass]
public class DynamicSeriesTests : StaticSeriesTestBase
{
    [TestMethod]
    public override void DefaultParameters_ReturnsExpectedResults()
    {
        IReadOnlyList<DynamicResult> sut = Bars
            .ToDynamic(14);

        // assertions
        sut.Should().HaveCount(502);
        sut.Where(static x => x.Dynamic != null).Should().HaveCount(501);

        // sample values
        DynamicResult r1 = sut[1];
        r1.Dynamic.Should().BeApproximately(212.9465, Money4);

        DynamicResult r25 = sut[25];
        r25.Dynamic.Should().BeApproximately(215.4801, Money4);

        DynamicResult r250 = sut[250];
        r250.Dynamic.Should().BeApproximately(256.0554, Money4);

        DynamicResult r501 = sut[501];
        r501.Dynamic.Should().BeApproximately(245.7356, Money4);
    }

    [TestMethod]
    public void UseReusable_ClosePrice_ReturnsExpectedResult()
    {
        IReadOnlyList<DynamicResult> sut = Bars
            .Use(CandlePart.Close)
            .ToDynamic(20);

        sut.Should().HaveCount(502);
        sut.Where(static x => x.Dynamic != null).Should().HaveCount(501);
        sut.Where(static x => x.Dynamic is double v && double.IsNaN(v)).Should().BeEmpty();
    }

    [TestMethod]
    public void Chainee_FromSma_ReturnsExpectedResult()
    {
        IReadOnlyList<DynamicResult> sut = Bars
            .ToSma(10)
            .ToDynamic(14);

        sut.Should().HaveCount(502);
        sut.Where(static x => x.Dynamic != null).Should().HaveCount(492);
    }

    [TestMethod]
    public void ChainFromResults_ToSma_ReturnsExpectedResult()
    {
        IReadOnlyList<SmaResult> sut = Bars
            .ToDynamic(14)
            .ToSma(10);

        sut.Should().HaveCount(502);
        sut.Where(static x => x.Sma != null).Should().HaveCount(492);
    }

    [TestMethod]
    public override void BadBars_DoesNotFail()
    {
        IReadOnlyList<DynamicResult> r = BadBars
            .ToDynamic(15);

        r.Should().HaveCount(502);
        r.Where(static x => x.Dynamic is double v && double.IsNaN(v)).Should().BeEmpty();
    }

    [TestMethod]
    public override void NoBars_ReturnsEmpty()
    {
        IReadOnlyList<DynamicResult> r0 = Nobars
            .ToDynamic(14);

        r0.Should().BeEmpty();

        IReadOnlyList<DynamicResult> r1 = Onebar
            .ToDynamic(14);

        r1.Should().HaveCount(1);
    }

    [TestMethod]
    public void Exceptions_InvalidParameters_ThrowsArgumentOutOfRangeException()
    {
        // bad lookback period
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(
            static () => Bars.ToDynamic(0));

        // bad k-factor
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(
            static () => Bars.ToDynamic(14, 0));
    }
}
