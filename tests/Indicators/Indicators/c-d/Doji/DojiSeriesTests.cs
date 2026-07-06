namespace StaticSeries;

[TestClass]
public class DojiSeriesTests : StaticSeriesTestBase
{
    [TestMethod]
    public override void DefaultParameters_ReturnsExpectedResults()
    {
        IReadOnlyList<CandleResult> sut = Bars
            .ToDoji();

        // proper quantities
        sut.Should().HaveCount(502);
        sut.Where(static x => x.Match != Match.None).Should().HaveCount(112);

        // sample values
        CandleResult r1 = sut[1];
        r1.Price.Should().BeNull();
        ((int)r1.Match).Should().Be(0);

        CandleResult r23 = sut[23];
        r23.Price.Should().Be(216.28m);
        r23.Match.Should().Be(Match.Neutral);

        CandleResult r46 = sut[46];
        r46.Price.Should().BeNull();
        r46.Match.Should().Be(Match.None);

        CandleResult r392 = sut[392];
        r392.Price.Should().BeNull();
        r392.Match.Should().Be(Match.None);

        CandleResult r451 = sut[451];
        r451.Price.Should().Be(273.64m);
        ((int)r451.Match).Should().Be(1);

        CandleResult r477 = sut[477];
        r477.Price.Should().Be(256.86m);
        r477.Match.Should().Be(Match.Neutral);
    }

    [TestMethod]
    public override void BadBars_DoesNotFail()
    {
        IReadOnlyList<CandleResult> r = BadBars
            .ToDoji();

        r.Should().HaveCount(502);
    }

    [TestMethod]
    public override void NoBars_ReturnsEmpty()
    {
        IReadOnlyList<CandleResult> r0 = Nobars
            .ToDoji();

        r0.Should().BeEmpty();

        IReadOnlyList<CandleResult> r1 = Onebar
            .ToDoji();

        r1.Should().HaveCount(1);
    }

    [TestMethod]
    public void Condense_RemovesNullResults_ReturnsCondensed()
    {
        IReadOnlyList<CandleResult> sut = Bars
            .ToDoji()
            .Condense();

        sut.Should().HaveCount(112);
    }

    [TestMethod]
    public void Exceptions_InvalidMaxPriceChange_ThrowsArgumentOutOfRangeException()
    {
        // bad maximum change value
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(
            static () => Bars.ToDoji(-0.00001));

        Assert.ThrowsExactly<ArgumentOutOfRangeException>(
            static () => Bars.ToDoji(0.50001));
    }
}
