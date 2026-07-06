namespace StaticSeries;

[TestClass]
public class WmaSeriesTests : StaticSeriesTestBase
{
    [TestMethod]
    public override void DefaultParameters_ReturnsExpectedResults()
    {
        IReadOnlyList<WmaResult> sut = Bars
            .ToWma(20);

        // proper quantities
        sut.Should().HaveCount(502);
        sut.Where(static x => x.Wma != null).Should().HaveCount(483);

        // sample values
        WmaResult r1 = sut[149];
        r1.Wma.Should().BeApproximately(235.5253, Money4);

        WmaResult r2 = sut[501];
        r2.Wma.Should().BeApproximately(246.5110, Money4);
    }

    [TestMethod]
    public void UseReusable_ClosePrice_ReturnsExpectedResult()
    {
        IReadOnlyList<WmaResult> sut = Bars
            .Use(CandlePart.Close)
            .ToWma(20);

        sut.Should().HaveCount(502);
        sut.Where(static x => x.Wma != null).Should().HaveCount(483);
    }

    [TestMethod]
    public void Chainee_FromSma_ReturnsExpectedResult()
    {
        IReadOnlyList<WmaResult> sut = Bars
            .ToSma(2)
            .ToWma(20);

        sut.Should().HaveCount(502);
        sut.Where(static x => x.Wma != null).Should().HaveCount(482);
    }

    [TestMethod]
    public void ChainFromResults_ToSma_ReturnsExpectedResult()
    {
        IReadOnlyList<SmaResult> sut = Bars
            .ToWma(20)
            .ToSma(10);

        sut.Should().HaveCount(502);
        sut.Where(static x => x.Sma != null).Should().HaveCount(474);
    }

    [TestMethod]
    public void Chaining_VersusDirectClose_ReturnsEquivalentResults()
    {
        IReadOnlyList<WmaResult> standard = Bars
            .ToWma(17);

        IReadOnlyList<WmaResult> sut = Bars
            .Use(CandlePart.Close)
            .ToWma(17);

        // assertions
        for (int i = 0; i < sut.Count; i++)
        {
            WmaResult s = standard[i];
            WmaResult c = sut[i];

            c.Timestamp.Should().Be(s.Timestamp);
            c.Wma.Should().Be(s.Wma);
        }
    }

    [TestMethod]
    public override void BadBars_DoesNotFail()
    {
        IReadOnlyList<WmaResult> r = BadBars
            .ToWma(15);

        r.Should().HaveCount(502);
        r.Where(static x => x.Wma is double v && double.IsNaN(v)).Should().BeEmpty();
    }

    [TestMethod]
    public override void NoBars_ReturnsEmpty()
    {
        IReadOnlyList<WmaResult> r0 = Nobars
            .ToWma(5);

        r0.Should().BeEmpty();

        IReadOnlyList<WmaResult> r1 = Onebar
            .ToWma(5);

        r1.Should().HaveCount(1);
    }

    [TestMethod]
    public void Removed_WithWarmupPeriods_TruncatesResults()
    {
        IReadOnlyList<WmaResult> sut = Bars
            .ToWma(20)
            .RemoveWarmupPeriods();

        // assertions
        sut.Should().HaveCount(502 - 19);

        WmaResult last = sut[^1];
        last.Wma.Should().BeApproximately(246.5110, Money4);
    }

    /// <summary>
    /// bad lookback period
    /// </summary>
    [TestMethod]
    public void Exceptions_InvalidLookback_ThrowsArgumentOutOfRangeException()
        => FluentActions
            .Invoking(static () => Bars.ToWma(0))
            .Should()
            .ThrowExactly<ArgumentOutOfRangeException>();
}
