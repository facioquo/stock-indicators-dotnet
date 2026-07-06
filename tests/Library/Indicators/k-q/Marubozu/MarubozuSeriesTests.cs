namespace StaticSeries;

[TestClass]
public class MarubozuSeriesTests : StaticSeriesTestBase
{
    [TestMethod]
    public override void DefaultParameters_ReturnsExpectedResults()
    {
        IReadOnlyList<CandleResult> sut = Bars
            .ToMarubozu();

        // proper quantities
        sut.Should().HaveCount(502);
        sut.Where(static x => x.Match != Match.None).Should().HaveCount(6);

        // sample values
        CandleResult r31 = sut[31];
        r31.Price.Should().BeNull();
        ((int)r31.Match).Should().Be(0);

        CandleResult r32 = sut[32];
        r32.Price.Should().Be(222.10m);
        r32.Match.Should().Be(Match.BullSignal);

        CandleResult r33 = sut[33];
        r33.Price.Should().BeNull();
        r33.Match.Should().Be(Match.None);

        CandleResult r34 = sut[34];
        r34.Price.Should().BeNull();
        r34.Match.Should().Be(Match.None);

        CandleResult r274 = sut[274];
        r274.Price.Should().BeNull();
        r274.Match.Should().Be(Match.None);

        CandleResult r277 = sut[277];
        r277.Price.Should().Be(248.13m);
        r277.Match.Should().Be(Match.BearSignal);
    }

    [TestMethod]
    public override void BadBars_DoesNotFail()
    {
        IReadOnlyList<CandleResult> r = BadBars
            .ToMarubozu();

        r.Should().HaveCount(502);
    }

    [TestMethod]
    public override void NoBars_ReturnsEmpty()
    {
        IReadOnlyList<CandleResult> r0 = Nobars
            .ToMarubozu();

        r0.Should().BeEmpty();

        IReadOnlyList<CandleResult> r1 = Onebar
            .ToMarubozu();

        r1.Should().HaveCount(1);
    }

    [TestMethod]
    public void Condense_RemovesNullResults_ReturnsCondensed()
    {
        IReadOnlyList<CandleResult> sut = Bars
            .ToMarubozu()
            .Condense();

        sut.Should().HaveCount(6);
    }

    [TestMethod]
    public void Exceptions_InvalidMinBodyPercent_ThrowsArgumentOutOfRangeException()
    {
        // bad minimum body percent values
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(
            static () => Bars.ToMarubozu(79.9));

        Assert.ThrowsExactly<ArgumentOutOfRangeException>(
            static () => Bars.ToMarubozu(100.1));
    }
}
