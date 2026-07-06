namespace StaticSeries;

[TestClass]
public class HurstSeriesTests : StaticSeriesTestBase
{
    [TestMethod]
    public override void DefaultParameters_ReturnsExpectedResults()
    {
        IReadOnlyList<HurstResult> sut = LongestBars
            .ToHurst(LongestBars.Count - 1);

        // assertions

        // proper quantities
        sut.Should().HaveCount(15821);
        sut.Where(static x => x.HurstExponent != null).Should().HaveCount(1);
        sut.Where(static x => x.HurstExponentAL != null).Should().HaveCount(1);

        // sample value
        HurstResult r15820 = sut[15820];
        r15820.HurstExponent.Should().BeApproximately(0.479755, Money6);
        r15820.HurstExponentAL.Should().BeApproximately(0.471156, Money6);
    }

    [TestMethod]
    public void Results_WithAnyInput_AreAlwaysBounded()
    {
        IReadOnlyList<HurstResult> sut = Bars.ToHurst(100);
        sut.IsBetween(static x => x.HurstExponent, 0, 1);
        sut.IsBetween(static x => x.HurstExponentAL, 0, 1);
    }

    [TestMethod]
    public void StirlingBoundary_ReturnsExpectedResults()
    {
        // lookbackPeriods=500 produces chunk sizes [500, 250, 125, 62, 31, 15]
        // that straddle the n=340 Stirling/exact-gamma branch boundary.
        IReadOnlyList<HurstResult> sut = Bars.ToHurst(500);

        sut.Should().HaveCount(502);
        sut.Count(static x => x.HurstExponent != null).Should().Be(2);
        sut.Count(static x => x.HurstExponentAL != null).Should().Be(2);

        HurstResult last = sut[^1];
        last.HurstExponent.Should().BeApproximately(0.568000, Money6);
        last.HurstExponentAL.Should().BeApproximately(0.516229, Money6);
    }

    [TestMethod]
    public void UseReusable_ClosePrice_ReturnsExpectedResult()
    {
        IReadOnlyList<HurstResult> sut = Bars
            .Use(CandlePart.Close)
            .ToHurst();

        sut.Should().HaveCount(502);
        sut.Count(static x => x.HurstExponent != null).Should().Be(402);
        sut.Count(static x => x.HurstExponentAL != null).Should().Be(402);

        HurstResult last = sut[^1];
        last.HurstExponent.Should().BeApproximately(0.564643, Money6);
        last.HurstExponentAL.Should().BeApproximately(0.497004, Money6);
    }

    [TestMethod]
    public void ChainFromResults_ToSma_ReturnsExpectedResult()
    {
        IReadOnlyList<SmaResult> sut = Bars
            .ToHurst()
            .ToSma(10);

        sut.Should().HaveCount(502);
        Assert.AreEqual(393, sut.Count(static x => x.Sma != null));
    }

    [TestMethod]
    public void Chainee_FromSma_ReturnsExpectedResult()
    {
        IReadOnlyList<HurstResult> sut = Bars
            .ToSma(10)
            .ToHurst();

        sut.Should().HaveCount(502);
        Assert.AreEqual(393, sut.Count(static x => x.HurstExponent != null));
    }

    [TestMethod]
    public override void BadBars_DoesNotFail()
    {
        IReadOnlyList<HurstResult> r = BadBars
            .ToHurst(150);

        r.Should().HaveCount(502);
        r.Where(static x => x.HurstExponent is double v && double.IsNaN(v)).Should().BeEmpty();
        r.Where(static x => x.HurstExponentAL is double v && double.IsNaN(v)).Should().BeEmpty();
    }

    [TestMethod]
    public override void NoBars_ReturnsEmpty()
    {
        IReadOnlyList<HurstResult> r0 = Nobars
            .ToHurst();

        r0.Should().BeEmpty();

        IReadOnlyList<HurstResult> r1 = Onebar
            .ToHurst();

        r1.Should().HaveCount(1);
    }

    [TestMethod]
    public void Removed_WithWarmupPeriods_TruncatesResults()
    {
        IReadOnlyList<HurstResult> sut = LongestBars.ToHurst(LongestBars.Count - 1)
            .RemoveWarmupPeriods();

        // assertions
        sut.Should().HaveCount(1);

        HurstResult last = sut[^1];
        last.HurstExponent.Should().BeApproximately(0.479755, Money6);
        last.HurstExponentAL.Should().BeApproximately(0.471156, Money6);
    }

    /// <summary>
    /// bad lookback period
    /// </summary>
    [TestMethod]
    public void Exceptions_InvalidLookback_ThrowsArgumentOutOfRangeException()
        => FluentActions
            .Invoking(static () => Bars.ToHurst(19))
            .Should()
            .ThrowExactly<ArgumentOutOfRangeException>();
}
