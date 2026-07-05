namespace StaticSeries;

[TestClass]
public class UltimateSeriesTests : StaticSeriesTestBase
{
    [TestMethod]
    public override void DefaultParameters_ReturnsExpectedResults()
    {
        IReadOnlyList<UltimateResult> sut = Bars
            .ToUltimate();

        // proper quantities
        sut.Should().HaveCount(502);
        sut.Where(static x => x.Ultimate != null).Should().HaveCount(474);

        // sample values
        UltimateResult r1 = sut[74];
        r1.Ultimate.Should().BeApproximately(51.7770, Money4);

        UltimateResult r2 = sut[249];
        r2.Ultimate.Should().BeApproximately(45.3121, Money4);

        UltimateResult r3 = sut[501];
        r3.Ultimate.Should().BeApproximately(49.5257, Money4);
    }

    [TestMethod]
    public void Results_AreAlwaysBounded()
    {
        IReadOnlyList<UltimateResult> sut = Bars.ToUltimate(7, 14, 28);
        sut.IsBetween(static x => x.Ultimate, 0, 100);
    }

    [TestMethod]
    public void Boundary_WithRandomBars_StaysWithinBounds()
    {
        IReadOnlyList<UltimateResult> sut = Data
            .GetRandom(2500)
            .ToUltimate(7, 14, 28);

        sut.IsBetween(static x => x.Ultimate, 0d, 100d);
    }

    [TestMethod]
    public void ChainingFromResults_WorksAsExpected()
    {
        IReadOnlyList<SmaResult> sut = Bars
            .ToUltimate()
            .ToSma(10);

        sut.Should().HaveCount(502);
        sut.Where(static x => x.Sma != null).Should().HaveCount(465);
    }

    [TestMethod]
    public override void BadBars_DoesNotFail()
    {
        IReadOnlyList<UltimateResult> r = BadBars
            .ToUltimate(1, 2, 3);

        r.Should().HaveCount(502);
        r.Where(static x => x.Ultimate is double.NaN).Should().BeEmpty();
    }

    [TestMethod]
    public override void NoBars_ReturnsEmpty()
    {
        IReadOnlyList<UltimateResult> r0 = Nobars
            .ToUltimate();

        r0.Should().BeEmpty();

        IReadOnlyList<UltimateResult> r1 = Onebar
            .ToUltimate();

        r1.Should().HaveCount(1);
    }

    [TestMethod]
    public void Removed_WithWarmupPeriods_TruncatesResults()
    {
        IReadOnlyList<UltimateResult> sut = Bars
            .ToUltimate()
            .RemoveWarmupPeriods();

        // assertions
        sut.Should().HaveCount(502 - 28);

        UltimateResult last = sut[^1];
        last.Ultimate.Should().BeApproximately(49.5257, Money4);
    }

    [TestMethod]
    public void Exceptions_InvalidParameters_ThrowsArgumentOutOfRangeException()
    {
        // bad short period
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(
            static () => Bars.ToUltimate(0));

        // bad middle period
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(
            static () => Bars.ToUltimate(7, 6));

        // bad long period
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(
            static () => Bars.ToUltimate(7, 14, 11));
    }
}
