namespace StaticSeries;

[TestClass]
public class EpmaSeriesTests : StaticSeriesTestBase
{
    [TestMethod]
    public override void DefaultParameters_ReturnsExpectedResults()
    {
        IReadOnlyList<EpmaResult> sut = Bars
            .ToEpma(20);

        // proper quantities
        sut.Should().HaveCount(502);
        sut.Where(static x => x.Epma != null).Should().HaveCount(483);

        // sample values
        EpmaResult r1 = sut[18];
        r1.Epma.Should().BeNull();

        EpmaResult r2 = sut[19];
        r2.Epma.Should().BeApproximately(215.6189, Money4);

        EpmaResult r3 = sut[149];
        r3.Epma.Should().BeApproximately(236.7060, Money4);

        EpmaResult r4 = sut[249];
        r4.Epma.Should().BeApproximately(258.5179, Money4);

        EpmaResult r5 = sut[501];
        r5.Epma.Should().BeApproximately(235.8131, Money4);
    }

    [TestMethod]
    public void UseReusable_ClosePrice_ReturnsExpectedResult()
    {
        IReadOnlyList<EpmaResult> sut = Bars
            .Use(CandlePart.Close)
            .ToEpma(20);

        sut.Should().HaveCount(502);
        sut.Where(static x => x.Epma != null).Should().HaveCount(483);
    }

    [TestMethod]
    public void Chainee_FromSma_ReturnsExpectedResult()
    {
        IReadOnlyList<EpmaResult> sut = Bars
            .ToSma(2)
            .ToEpma(20);

        sut.Should().HaveCount(502);
        sut.Where(static x => x.Epma != null).Should().HaveCount(482);
    }

    [TestMethod]
    public void ChainFromResults_ToSma_ReturnsExpectedResult()
    {
        IReadOnlyList<SmaResult> sut = Bars
            .ToEpma(20)
            .ToSma(10);

        sut.Should().HaveCount(502);
        sut.Where(static x => x.Sma != null).Should().HaveCount(474);
    }

    [TestMethod]
    public override void BadBars_DoesNotFail()
    {
        IReadOnlyList<EpmaResult> r = BadBars
            .ToEpma(15);

        r.Should().HaveCount(502);
        r.Where(static x => x.Epma is double v && double.IsNaN(v)).Should().BeEmpty();
    }

    [TestMethod]
    public override void NoBars_ReturnsEmpty()
    {
        IReadOnlyList<EpmaResult> r0 = Nobars
            .ToEpma(5);

        r0.Should().BeEmpty();

        IReadOnlyList<EpmaResult> r1 = Onebar
            .ToEpma(5);

        r1.Should().HaveCount(1);
    }

    [TestMethod]
    public void Removed_WithWarmupPeriods_TruncatesResults()
    {
        IReadOnlyList<EpmaResult> sut = Bars
            .ToEpma(20)
            .RemoveWarmupPeriods();

        // assertions
        sut.Should().HaveCount(502 - 19);

        EpmaResult last = sut[^1];
        last.Epma.Should().BeApproximately(235.8131, Money4);
    }

    /// <summary>
    /// bad lookback period
    /// </summary>
    [TestMethod]
    public void Exceptions_InvalidLookback_ThrowsArgumentOutOfRangeException()
        => FluentActions
            .Invoking(static () => Bars.ToEpma(0))
            .Should()
            .ThrowExactly<ArgumentOutOfRangeException>();
}
