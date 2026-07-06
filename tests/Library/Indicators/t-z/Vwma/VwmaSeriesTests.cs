namespace StaticSeries;

[TestClass]
public class VwmaSeriesTests : StaticSeriesTestBase
{
    [TestMethod]
    public override void DefaultParameters_ReturnsExpectedResults()
    {
        IReadOnlyList<VwmaResult> sut = Bars
            .ToVwma(10);

        // proper quantities
        sut.Should().HaveCount(502);
        sut.Where(static x => x.Vwma != null).Should().HaveCount(493);

        // sample values
        VwmaResult r8 = sut[8];
        r8.Vwma.Should().BeNull();

        sut[9].Vwma.Should().BeApproximately(213.981942, Money6);
        sut[24].Vwma.Should().BeApproximately(215.899211, Money6);
        sut[99].Vwma.Should().BeApproximately(226.302760, Money6);
        sut[249].Vwma.Should().BeApproximately(257.053654, Money6);
        sut[501].Vwma.Should().BeApproximately(242.101548, Money6);
    }

    [TestMethod]
    public void ChainingFromResults_WorksAsExpected()
    {
        IReadOnlyList<SmaResult> sut = Bars
            .ToVwma(10)
            .ToSma(10);

        sut.Should().HaveCount(502);
        sut.Where(static x => x.Sma != null).Should().HaveCount(484);
    }

    [TestMethod]
    public override void BadBars_DoesNotFail()
    {
        IReadOnlyList<VwmaResult> r = BadBars
            .ToVwma(15);

        r.Should().HaveCount(502);
        r.Where(static x => x.Vwma is double v && double.IsNaN(v)).Should().BeEmpty();
    }

    [TestMethod]
    public override void NoBars_ReturnsEmpty()
    {
        IReadOnlyList<VwmaResult> r0 = Nobars
            .ToVwma(4);

        r0.Should().BeEmpty();

        IReadOnlyList<VwmaResult> r1 = Onebar
            .ToVwma(4);

        r1.Should().HaveCount(1);
    }

    [TestMethod]
    public void Removed_WithWarmupPeriods_TruncatesResults()
    {
        IReadOnlyList<VwmaResult> sut = Bars
            .ToVwma(10)
            .RemoveWarmupPeriods();

        // assertions
        sut.Should().HaveCount(502 - 9);

        VwmaResult last = sut[^1];
        last.Vwma.Should().BeApproximately(242.101548, Money6);
    }

    /// <summary>
    /// bad lookback period
    /// </summary>
    [TestMethod]
    public void Exceptions_InvalidLookback_ThrowsArgumentOutOfRangeException()
        => FluentActions
            .Invoking(static () => Bars.ToVwma(0))
            .Should()
            .ThrowExactly<ArgumentOutOfRangeException>();
}
