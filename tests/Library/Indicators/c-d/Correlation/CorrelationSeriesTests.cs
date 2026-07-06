namespace StaticSeries;

[TestClass]
public class CorrelationSeriesTests : StaticSeriesTestBase
{
    [TestMethod]
    public override void DefaultParameters_ReturnsExpectedResults()
    {
        IReadOnlyList<CorrResult> sut = Bars
            .ToCorrelation(OtherBars, 20);

        // proper quantities
        // should always be the same number of sut as there is bars
        sut.Should().HaveCount(502);
        sut.Where(static x => x.Correlation != null).Should().HaveCount(483);

        // sample values
        CorrResult r18 = sut[18];
        r18.Correlation.Should().BeNull();
        r18.RSquared.Should().BeNull();

        CorrResult r19 = sut[19];
        r19.Correlation.Should().BeApproximately(0.6933, Money4);
        r19.RSquared.Should().BeApproximately(0.4806, Money4);

        CorrResult r257 = sut[257];
        r257.Correlation.Should().BeApproximately(-0.1347, Money4);
        r257.RSquared.Should().BeApproximately(0.0181, Money4);

        CorrResult r501 = sut[501];
        r501.Correlation.Should().BeApproximately(0.8460, Money4);
        r501.RSquared.Should().BeApproximately(0.7157, Money4);
    }

    [TestMethod]
    public void SwappingHistories_ReturnsSameResults()
    {
        const int lookbackPeriods = 20;

        IReadOnlyList<CorrResult> sutA = Bars
            .ToCorrelation(OtherBars, lookbackPeriods);

        IReadOnlyList<CorrResult> sutB = OtherBars
            .ToCorrelation(Bars, lookbackPeriods);

        sutA.Should().HaveCount(502);
        sutB.Should().HaveCount(502);

        sutA.Should().BeEquivalentTo(
            expectation: sutB,
            config: options => options
                .WithStrictOrdering()
                .ComparingByMembers<CorrResult>()
                .Excluding(r => r.VarianceA)
                .Excluding(r => r.VarianceB),
            because: "Correlation is bi-directional");

        sutA.Select(a => a.VarianceA).Should().BeEquivalentTo(
            expectation: sutB.Select(b => b.VarianceB),
            config: options => options
                .WithStrictOrdering(),
            because: "Variance vectors must map inversely");

        sutB.Select(b => b.VarianceA).Should().BeEquivalentTo(
            expectation: sutA.Select(a => a.VarianceB),
            config: options => options
                .WithStrictOrdering(),
            because: "Variance vectors must map inversely");
    }

    [TestMethod]
    public void Results_AreAlwaysBounded()
    {
        IReadOnlyList<CorrResult> sut = Bars.ToCorrelation(OtherBars, 20);
        sut.IsBetween(static x => x.Correlation, -1, 1);
        sut.IsBetween(static x => x.RSquared, 0, 1);
    }

    [TestMethod]
    public void UseReusable_ClosePrice_ReturnsExpectedResult()
    {
        IReadOnlyList<CorrResult> sut = Bars
            .Use(CandlePart.Close)
            .ToCorrelation(OtherBars.Use(CandlePart.Close), 20);

        sut.Should().HaveCount(502);
        sut.Where(static x => x.Correlation != null).Should().HaveCount(483);
    }

    [TestMethod]
    public void ChainFromResults_ToSma_ReturnsExpectedResult()
    {
        IReadOnlyList<SmaResult> sut = Bars
            .ToCorrelation(OtherBars, 20)
            .ToSma(10);

        sut.Should().HaveCount(502);
        sut.Where(static x => x.Sma != null).Should().HaveCount(474);
    }

    [TestMethod]
    public void Chainee_FromSma_ReturnsExpectedResult()
    {
        IReadOnlyList<CorrResult> sut = Bars
            .ToSma(2)
            .ToCorrelation(OtherBars.ToSma(2), 20);

        sut.Should().HaveCount(502);
        sut.Where(static x => x.Correlation != null).Should().HaveCount(482);
        sut.Where(static x => x.Correlation is double v && double.IsNaN(v)).Should().BeEmpty();
    }

    [TestMethod]
    public override void BadBars_DoesNotFail()
    {
        IReadOnlyList<CorrResult> r = BadBars
            .ToCorrelation(BadBars, 15);

        r.Should().HaveCount(502);
        r.Where(static x => x.Correlation is double v && double.IsNaN(v)).Should().BeEmpty();
    }

    [TestMethod]
    public void BigBarValues_DoesNotFail()
    {
        IReadOnlyList<CorrResult> r = BigBars
            .ToCorrelation(BigBars, 150);

        r.Should().HaveCount(1246);
    }

    [TestMethod]
    public override void NoBars_ReturnsEmpty()
    {
        IReadOnlyList<CorrResult> r0 = Nobars
            .ToCorrelation(Nobars, 10);

        r0.Should().BeEmpty();

        IReadOnlyList<CorrResult> r1 = Onebar
            .ToCorrelation(Onebar, 10);

        r1.Should().HaveCount(1);
    }

    [TestMethod]
    public void Removed_WithWarmupPeriods_TruncatesResults()
    {
        IReadOnlyList<CorrResult> sut = Bars
            .ToCorrelation(OtherBars, 20)
            .RemoveWarmupPeriods();

        // assertions
        sut.Should().HaveCount(502 - 19);

        CorrResult last = sut[^1];
        last.Correlation.Should().BeApproximately(0.8460, Money4);
        last.RSquared.Should().BeApproximately(0.7157, Money4);
    }

    [TestMethod]
    public void Exceptions_InvalidParameters_ThrowsExpectedException()
    {
        // bad lookback period
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(
            () => Bars.ToCorrelation(OtherBars, 0));

        // bad eval bars
        IReadOnlyList<Bar> eval = Data.GetCompare(300);
        Assert.ThrowsExactly<InvalidBarsException>(
            () => Bars.ToCorrelation(eval, 30));

        // mismatched bars
        Assert.ThrowsExactly<InvalidBarsException>(
            () => MismatchBars.ToCorrelation(OtherBars, 20));
    }
}
