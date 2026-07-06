namespace StaticSeries;

[TestClass]
public class VortexSeriesTests : StaticSeriesTestBase
{
    [TestMethod]
    public override void DefaultParameters_ReturnsExpectedResults()
    {
        IReadOnlyList<VortexResult> sut = Bars
            .ToVortex(14);

        // proper quantities
        sut.Should().HaveCount(502);
        sut.Where(static x => x.Pvi != null).Should().HaveCount(488);

        // sample values
        VortexResult r1 = sut[13];
        r1.Pvi.Should().BeNull();
        r1.Nvi.Should().BeNull();

        VortexResult r2 = sut[14];
        r2.Pvi.Should().BeApproximately(1.0460, Money4);
        r2.Nvi.Should().BeApproximately(0.8119, Money4);

        VortexResult r3 = sut[29];
        r3.Pvi.Should().BeApproximately(1.1300, Money4);
        r3.Nvi.Should().BeApproximately(0.7393, Money4);

        VortexResult r4 = sut[249];
        r4.Pvi.Should().BeApproximately(1.1558, Money4);
        r4.Nvi.Should().BeApproximately(0.6634, Money4);

        VortexResult r5 = sut[501];
        r5.Pvi.Should().BeApproximately(0.8712, Money4);
        r5.Nvi.Should().BeApproximately(1.1163, Money4);
    }

    [TestMethod]
    public override void BadBars_DoesNotFail()
    {
        IReadOnlyList<VortexResult> r = BadBars
            .ToVortex(20);

        r.Should().HaveCount(502);
        r.Where(static x => x.Pvi is double v && double.IsNaN(v)).Should().BeEmpty();
    }

    [TestMethod]
    public override void NoBars_ReturnsEmpty()
    {
        IReadOnlyList<VortexResult> r0 = Nobars
            .ToVortex(5);

        r0.Should().BeEmpty();

        IReadOnlyList<VortexResult> r1 = Onebar
            .ToVortex(5);

        r1.Should().HaveCount(1);
    }

    [TestMethod]
    public void Condense_RemovesNullResults_ReturnsCondensed()
    {
        IReadOnlyList<VortexResult> sut = Bars
            .ToVortex(14)
            .Condense();

        // assertions
        sut.Should().HaveCount(502 - 14);

        VortexResult last = sut[^1];
        last.Pvi.Should().BeApproximately(0.8712, Money4);
        last.Nvi.Should().BeApproximately(1.1163, Money4);
    }

    [TestMethod]
    public void Removed_WithWarmupPeriods_TruncatesResults()
    {
        IReadOnlyList<VortexResult> sut = Bars
            .ToVortex(14)
            .RemoveWarmupPeriods();

        // assertions
        sut.Should().HaveCount(502 - 14);

        VortexResult last = sut[^1];
        last.Pvi.Should().BeApproximately(0.8712, Money4);
        last.Nvi.Should().BeApproximately(1.1163, Money4);
    }

    /// <summary>
    /// bad lookback period
    /// </summary>
    [TestMethod]
    public void Exceptions_InvalidLookback_ThrowsArgumentOutOfRangeException()
        => FluentActions
            .Invoking(static () => Bars.ToVortex(1))
            .Should()
            .ThrowExactly<ArgumentOutOfRangeException>();
}
