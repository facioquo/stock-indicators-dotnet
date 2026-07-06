namespace StaticSeries;

[TestClass]
public class MfiSeriesTests : StaticSeriesTestBase
{
    [TestMethod]
    public override void DefaultParameters_ReturnsExpectedResults()
    {
        IReadOnlyList<MfiResult> sut = Bars
            .ToMfi();

        // proper quantities
        sut.Should().HaveCount(502);
        sut.Where(static x => x.Mfi != null).Should().HaveCount(488);

        // sample values
        MfiResult r1 = sut[439];
        r1.Mfi.Should().BeApproximately(69.0622, Money4);

        MfiResult r2 = sut[501];
        r2.Mfi.Should().BeApproximately(39.9494, Money4);
    }

    [TestMethod]
    public void Results_WithAnyInput_AreAlwaysBounded()
    {
        IReadOnlyList<MfiResult> sut = Bars.ToMfi(14);
        sut.IsBetween(static x => x.Mfi, 0, 100);
    }

    [TestMethod]
    public void Boundary_WithRandomBars_StaysWithinBounds()
    {
        IReadOnlyList<MfiResult> sut = Data
            .GetRandom(2500)
            .ToMfi(14);

        sut.IsBetween(static x => x.Mfi, 0d, 100d);
    }

    [TestMethod]
    public void ChainFromResults_ToSma_ReturnsExpectedResult()
    {
        IReadOnlyList<SmaResult> sut = Bars
            .ToMfi()
            .ToSma(10);

        sut.Should().HaveCount(502);
        sut.Where(static x => x.Sma != null).Should().HaveCount(479);
    }

    [TestMethod]
    public void SmallLookback_WithFourPeriods_ReturnsExpectedResult()
    {
        const int lookbackPeriods = 4;

        IReadOnlyList<MfiResult> sut = Bars
            .ToMfi(lookbackPeriods);

        // proper quantities
        sut.Should().HaveCount(502);
        sut.Where(static x => x.Mfi != null).Should().HaveCount(498);

        // sample values
        MfiResult r1 = sut[31];
        r1.Mfi.Should().BeApproximately(100, Money4);

        MfiResult r2 = sut[43];
        r2.Mfi.Should().BeApproximately(0, Money4);
    }

    [TestMethod]
    public override void BadBars_DoesNotFail()
    {
        IReadOnlyList<MfiResult> r = BadBars
            .ToMfi(15);

        r.Should().HaveCount(502);
        r.Where(static x => x.Mfi is double v && double.IsNaN(v)).Should().BeEmpty();
    }

    [TestMethod]
    public override void NoBars_ReturnsEmpty()
    {
        IReadOnlyList<MfiResult> r0 = Nobars
            .ToMfi();

        r0.Should().BeEmpty();

        IReadOnlyList<MfiResult> r1 = Onebar
            .ToMfi();

        r1.Should().HaveCount(1);
    }

    [TestMethod]
    public void Removed_WithWarmupPeriods_TruncatesResults()
    {
        const int lookbackPeriods = 14;

        IReadOnlyList<MfiResult> sut = Bars
            .ToMfi(lookbackPeriods)
            .RemoveWarmupPeriods();

        // assertions
        sut.Should().HaveCount(502 - 14);

        MfiResult last = sut[^1];
        last.Mfi.Should().BeApproximately(39.9494, Money4);
    }

    /// <summary>
    /// bad lookback period
    /// </summary>
    [TestMethod]
    public void Exceptions_InvalidLookback_ThrowsArgumentOutOfRangeException()
        => FluentActions
            .Invoking(static () => Bars.ToMfi(1))
            .Should()
            .ThrowExactly<ArgumentOutOfRangeException>();
}
