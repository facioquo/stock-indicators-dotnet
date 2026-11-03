namespace StaticSeries;

[TestClass]
public class TtmSqueezeSeriesTests : StaticSeriesTestBase
{
    [TestMethod]
    public override void DefaultParameters_ReturnsExpectedResults()
    {
        IReadOnlyList<TtmSqueezeResult> sut = Bars
            .ToTtmSqueeze();

        // proper quantities
        sut.Should().HaveCount(502);
        sut.Where(static x => x.Momentum != null).Should().HaveCount(464);
        sut.Where(static x => x.IsSqueezeOn != null).Should().HaveCount(482);
        sut.Where(static x => x.IsSqueezeOn == true).Should().HaveCount(54);

        // sample values
        TtmSqueezeResult r19 = sut[19];
        r19.Momentum.Should().BeNull();
        r19.IsSqueezeOn.Should().BeNull();

        TtmSqueezeResult r20 = sut[20];
        r20.Momentum.Should().BeNull();
        r20.IsSqueezeOn.Should().BeTrue();

        TtmSqueezeResult r37 = sut[37];
        r37.Momentum.Should().BeNull();
        r37.IsSqueezeOn.Should().BeFalse();

        TtmSqueezeResult r38 = sut[38];
        r38.Momentum.Should().BeApproximately(5.680836, Money6);
        r38.IsSqueezeOn.Should().BeFalse();

        TtmSqueezeResult r39 = sut[39];
        r39.Momentum.Should().BeApproximately(5.950029, Money6);

        TtmSqueezeResult r100 = sut[100];
        r100.Momentum.Should().BeApproximately(1.040725, Money6);

        TtmSqueezeResult r249 = sut[249];
        r249.Momentum.Should().BeApproximately(3.495654, Money6);

        TtmSqueezeResult r501 = sut[501];
        r501.Momentum.Should().BeApproximately(-16.305979, Money6);
        r501.IsSqueezeOn.Should().BeFalse();
    }

    [TestMethod]
    public void ChainFromResults_ToSma_ReturnsExpectedResult()
    {
        IReadOnlyList<SmaResult> sut = Bars
            .ToTtmSqueeze()
            .ToSma(10);

        sut.Should().HaveCount(502);
        sut.Where(static x => x.Sma != null).Should().HaveCount(455);
    }

    [TestMethod]
    public void SmallLookback_WithMinimalPeriods_ReturnsExpectedResult()
    {
        IReadOnlyList<TtmSqueezeResult> sut = Bars
            .ToTtmSqueeze(2);

        sut.Should().HaveCount(502);
        sut.Where(static x => x.Momentum != null).Should().HaveCount(500);
        sut.Where(static x => x.IsSqueezeOn != null).Should().HaveCount(500);
    }

    [TestMethod]
    public override void BadBars_DoesNotFail()
    {
        IReadOnlyList<TtmSqueezeResult> r = BadBars
            .ToTtmSqueeze();

        r.Should().HaveCount(502);
        r.Where(static x => x.Momentum is double v && double.IsNaN(v)).Should().BeEmpty();
    }

    [TestMethod]
    public override void NoBars_ReturnsEmpty()
    {
        IReadOnlyList<TtmSqueezeResult> r0 = Nobars
            .ToTtmSqueeze();

        r0.Should().BeEmpty();

        IReadOnlyList<TtmSqueezeResult> r1 = Onebar
            .ToTtmSqueeze();

        r1.Should().HaveCount(1);
    }

    [TestMethod]
    public void Removed_WithWarmupPeriods_TruncatesResults()
    {
        IReadOnlyList<TtmSqueezeResult> sut = Bars
            .ToTtmSqueeze()
            .RemoveWarmupPeriods();

        // assertions
        sut.Should().HaveCount(502 - 38);

        TtmSqueezeResult last = sut[^1];
        last.Momentum.Should().BeApproximately(-16.305979, Money6);
    }

    [TestMethod]
    public void Exceptions_InvalidLookback_ThrowsArgumentOutOfRangeException()
        => FluentActions
            .Invoking(static () => Bars.ToTtmSqueeze(1))
            .Should()
            .ThrowExactly<ArgumentOutOfRangeException>()
            .WithParameterName("lookbackPeriods");

    [TestMethod]
    public void Exceptions_InvalidStandardDeviations_ThrowsArgumentOutOfRangeException()
        => FluentActions
            .Invoking(static () => Bars.ToTtmSqueeze(20, 0))
            .Should()
            .ThrowExactly<ArgumentOutOfRangeException>()
            .WithParameterName("standardDeviations");

    [TestMethod]
    public void Exceptions_InvalidMultiplier_ThrowsArgumentOutOfRangeException()
        => FluentActions
            .Invoking(static () => Bars.ToTtmSqueeze(20, 2, 0))
            .Should()
            .ThrowExactly<ArgumentOutOfRangeException>()
            .WithParameterName("multiplier");
}
