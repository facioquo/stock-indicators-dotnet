namespace StaticSeries;

[TestClass]
public class ParabolicSarSeriesTests : StaticSeriesTestBase
{
    [TestMethod]
    public override void DefaultParameters_ReturnsExpectedResults()
    {
        const double acclerationStep = 0.02;
        const double maxAccelerationFactor = 0.2;

        List<ParabolicSarResult> sut =
            Bars.ToParabolicSar(acclerationStep, maxAccelerationFactor)
                .ToList();

        // proper quantities
        sut.Should().HaveCount(502);
        sut.Where(static x => x.Sar != null).Should().HaveCount(488);

        // sample values
        ParabolicSarResult r14 = sut[14];
        r14.Sar.Should().Be(212.83);
        r14.IsReversal.Should().BeTrue();

        ParabolicSarResult r16 = sut[16];
        r16.Sar.Should().BeApproximately(212.9924, Money4);
        r16.IsReversal.Should().BeFalse();

        ParabolicSarResult r94 = sut[94];
        r94.Sar.Should().BeApproximately(228.3600, Money4);
        r94.IsReversal.Should().BeFalse();

        ParabolicSarResult r501 = sut[501];
        r501.Sar.Should().BeApproximately(229.7662, Money4);
        r501.IsReversal.Should().BeFalse();
    }

    [TestMethod]
    public void Extended_WithInitialStep_ReturnsExpectedResult()
    {
        const double acclerationStep = 0.02;
        const double maxAccelerationFactor = 0.2;
        const double initialStep = 0.01;

        List<ParabolicSarResult> sut =
            Bars.ToParabolicSar(
                acclerationStep, maxAccelerationFactor, initialStep)
                .ToList();

        // proper quantities
        sut.Should().HaveCount(502);
        sut.Where(static x => x.Sar != null).Should().HaveCount(488);

        // sample values
        ParabolicSarResult r14 = sut[14];
        r14.Sar.Should().Be(212.83);
        r14.IsReversal.Should().BeTrue();

        ParabolicSarResult r16 = sut[16];
        r16.Sar.Should().BeApproximately(212.9518, Money4);
        r16.IsReversal.Should().BeFalse();

        ParabolicSarResult r94 = sut[94];
        r94.Sar.Should().Be(228.36);
        r94.IsReversal.Should().BeFalse();

        ParabolicSarResult r486 = sut[486];
        r486.Sar.Should().BeApproximately(273.4148, Money4);
        r486.IsReversal.Should().BeFalse();

        ParabolicSarResult r501 = sut[501];
        r501.Sar.Should().Be(246.73);
        r501.IsReversal.Should().BeFalse();
    }

    [TestMethod]
    public void ChainFromResults_ToSma_ReturnsExpectedResult()
    {
        IReadOnlyList<SmaResult> sut = Bars
            .ToParabolicSar()
            .ToSma(10);

        sut.Should().HaveCount(502);
        sut.Where(static x => x.Sma != null).Should().HaveCount(479);
    }

    [TestMethod]
    public void InsufficientBars_WithTooFewBars_ReturnsEmptySar()
    {
        const double acclerationStep = 0.02;
        const double maxAccelerationFactor = 0.2;

        List<Bar> insufficientBars = Data.GetDefault()
            .OrderBy(static x => x.Timestamp)
            .Take(10)
            .ToList();

        IReadOnlyList<ParabolicSarResult> sut =
            insufficientBars
                .ToParabolicSar(acclerationStep, maxAccelerationFactor);

        // assertions

        // proper quantities
        sut.Should().HaveCount(10);
        sut.Where(static x => x.Sar != null).Should().BeEmpty();
    }

    [TestMethod]
    public override void BadBars_DoesNotFail()
    {
        IReadOnlyList<ParabolicSarResult> r = BadBars
            .ToParabolicSar(0.2, 0.2, 0.2);

        r.Should().HaveCount(502);
        r.Where(static x => x.Sar is double v && double.IsNaN(v)).Should().BeEmpty();
    }

    [TestMethod]
    public override void NoBars_ReturnsEmpty()
    {
        IReadOnlyList<ParabolicSarResult> r0 = Nobars
            .ToParabolicSar();

        r0.Should().BeEmpty();

        IReadOnlyList<ParabolicSarResult> r1 = Onebar
            .ToParabolicSar();

        r1.Should().HaveCount(1);
    }

    [TestMethod]
    public void Removed_WithWarmupPeriods_TruncatesResults()
    {
        const double acclerationStep = 0.02;
        const double maxAccelerationFactor = 0.2;

        IReadOnlyList<ParabolicSarResult> sut = Bars
            .ToParabolicSar(acclerationStep, maxAccelerationFactor)
            .RemoveWarmupPeriods();

        // assertions
        sut.Should().HaveCount(488);

        ParabolicSarResult last = sut[^1];
        last.Sar.Should().BeApproximately(229.7662, Money4);
        last.IsReversal.Should().BeFalse();
    }

    [TestMethod]
    public void Exceptions_InvalidParameters_ThrowsArgumentOutOfRangeException()
    {
        // bad acceleration step
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(
            static () => Bars.ToParabolicSar(0, 1));

        // insufficient acceleration step
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(
            static () => Bars.ToParabolicSar(0.02, 0));

        // step larger than factor
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(
            static () => Bars.ToParabolicSar(6, 2));

        // insufficient initial factor
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(
            static () => Bars.ToParabolicSar(0.02, 0.5, 0));
    }
}
