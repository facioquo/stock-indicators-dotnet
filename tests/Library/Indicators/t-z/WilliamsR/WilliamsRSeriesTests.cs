namespace StaticSeries;

[TestClass]
public class WilliamsRSeriesTests : StaticSeriesTestBase
{
    [TestMethod]
    public override void DefaultParameters_ReturnsExpectedResults()
    {
        IReadOnlyList<WilliamsResult> sut = Bars
            .ToWilliamsR();

        // proper quantities
        sut.Should().HaveCount(502);
        sut.Where(static x => x.WilliamsR != null).Should().HaveCount(489);

        // sample values
        WilliamsResult r1 = sut[343];
        r1.WilliamsR.Should().BeApproximately(-19.8211, Money4);

        WilliamsResult r2 = sut[501];
        r2.WilliamsR.Should().BeApproximately(-52.0121, Money4);
    }

    [TestMethod]
    public void Results_AreAlwaysBounded()
    {
        IReadOnlyList<WilliamsResult> sut = Bars.ToWilliamsR(14);
        sut.IsBetween(static x => x.WilliamsR, -100, 0);
    }

    [TestMethod]
    public void ChainingFromResults_WorksAsExpected()
    {
        IReadOnlyList<SmaResult> sut = Bars
            .ToWilliamsR()
            .ToSma(10);

        sut.Should().HaveCount(502);
        sut.Where(static x => x.Sma != null).Should().HaveCount(480);
    }

    [TestMethod]
    public override void BadBars_DoesNotFail()
    {
        IReadOnlyList<WilliamsResult> sut = BadBars
            .ToWilliamsR(20);

        sut.Should().HaveCount(502);
        sut.Where(static x => x.WilliamsR is double v && double.IsNaN(v)).Should().BeEmpty();
    }

    [TestMethod]
    public override void NoBars_ReturnsEmpty()
    {
        IReadOnlyList<WilliamsResult> r0 = Nobars
            .ToWilliamsR();

        r0.Should().BeEmpty();

        IReadOnlyList<WilliamsResult> r1 = Onebar
            .ToWilliamsR();

        r1.Should().HaveCount(1);
    }

    [TestMethod]
    public void Removed_WithWarmupPeriods_TruncatesResults()
    {
        IReadOnlyList<WilliamsResult> sut = Bars
            .ToWilliamsR()
            .RemoveWarmupPeriods();

        // assertions
        sut.Should().HaveCount(502 - 13);

        WilliamsResult last = sut[^1];
        last.WilliamsR.Should().BeApproximately(-52.0121, Money4);
    }

    [TestMethod]
    public void Boundary_WithRandomBars_StaysWithinBounds()
    {
        IReadOnlyList<WilliamsResult> sut = Data
            .GetRandom(2500)
            .ToWilliamsR();

        sut.IsBetween(static x => x.WilliamsR, -100d, 0d);
    }

    [TestMethod]
    public void Issue1127_Original_BoundaryThreshold_Maintained()
    {
        // initialize
        IReadOnlyList<Bar> bars = Data.BarsFromCsv("_issue1127.williamr.original.csv");

        // get indicators
        IReadOnlyList<WilliamsResult> sut = bars
            .ToWilliamsR();

        sut.Should().HaveCountGreaterThan(0);
        sut.IsBetween(static x => x.WilliamsR, -100d, 0d);
    }

    [TestMethod]
    public void Issue1127_Revisit_BoundaryThreshold_Maintained()
    {
        // initialize
        IReadOnlyList<Bar> bars = Data.BarsFromCsv("_issue1127.williamr.revisit.csv");

        // get indicators
        IReadOnlyList<WilliamsResult> sut = bars
            .ToWilliamsR();

        sut.ToConsole(args: (nameof(WilliamsResult.WilliamsR), "F20"));

        sut.Should().HaveCountGreaterThan(0);
        sut.IsBetween(static x => x.WilliamsR, -100d, 0d);
    }

    /// <summary>
    /// bad lookback period
    /// </summary>
    [TestMethod]
    public void Exceptions_InvalidLookback_ThrowsArgumentOutOfRangeException()
        => FluentActions
            .Invoking(static () => Bars.ToWilliamsR(0))
            .Should()
            .ThrowExactly<ArgumentOutOfRangeException>();
}
