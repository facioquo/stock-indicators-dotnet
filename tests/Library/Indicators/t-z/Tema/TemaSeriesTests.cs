namespace StaticSeries;

[TestClass]
public class TemaSeriesTests : StaticSeriesTestBase
{
    [TestMethod]
    public override void DefaultParameters_ReturnsExpectedResults()
    {
        IReadOnlyList<TemaResult> sut = Bars
            .ToTema(20);

        // proper quantities
        sut.Should().HaveCount(502);
        sut.Where(static x => x.Tema != null).Should().HaveCount(483);

        // sample values
        TemaResult r25 = sut[25];
        r25.Tema.Should().BeApproximately(216.1441, Money4);

        TemaResult r67 = sut[67];
        r67.Tema.Should().BeApproximately(222.9562, Money4);

        TemaResult r249 = sut[249];
        r249.Tema.Should().BeApproximately(258.6208, Money4);

        TemaResult r501 = sut[501];
        r501.Tema.Should().BeApproximately(238.7690, Money4);
    }

    [TestMethod]
    public void UseReusable_ClosePrice_ReturnsExpectedResult()
    {
        IReadOnlyList<TemaResult> sut = Bars
            .Use(CandlePart.Close)
            .ToTema(20);

        sut.Should().HaveCount(502);
        sut.Where(static x => x.Tema != null).Should().HaveCount(483);
    }

    [TestMethod]
    public void Chainee_FromSma_ReturnsExpectedResult()
    {
        IReadOnlyList<TemaResult> sut = Bars
            .ToSma(2)
            .ToTema(20);

        sut.Should().HaveCount(502);
        sut.Where(static x => x.Tema != null).Should().HaveCount(482);
    }

    [TestMethod]
    public void ChainFromResults_ToSma_ReturnsExpectedResult()
    {
        IReadOnlyList<SmaResult> sut = Bars
            .ToTema(20)
            .ToSma(10);

        sut.Should().HaveCount(502);
        sut.Where(static x => x.Sma != null).Should().HaveCount(474);
    }

    [TestMethod]
    public override void BadBars_DoesNotFail()
    {
        IReadOnlyList<TemaResult> r = BadBars
            .ToTema(15);

        r.Should().HaveCount(502);
        r.Where(static x => x.Tema is double v && double.IsNaN(v)).Should().BeEmpty();
    }

    [TestMethod]
    public override void NoBars_ReturnsEmpty()
    {
        IReadOnlyList<TemaResult> r0 = Nobars
            .ToTema(5);

        r0.Should().BeEmpty();

        IReadOnlyList<TemaResult> r1 = Onebar
            .ToTema(5);

        r1.Should().HaveCount(1);
    }

    [TestMethod]
    public void Removed_WithWarmupPeriods_TruncatesResults()
    {
        IReadOnlyList<TemaResult> sut = Bars
            .ToTema(20)
            .RemoveWarmupPeriods();

        // assertions
        sut.Should().HaveCount(502 - ((3 * 20) + 100));

        TemaResult last = sut[^1];
        last.Tema.Should().BeApproximately(238.7690, Money4);
    }

    /// <summary>
    /// bad lookback period
    /// </summary>
    [TestMethod]
    public void Exceptions_InvalidLookback_ThrowsArgumentOutOfRangeException()
        => FluentActions
            .Invoking(static () => Bars.ToTema(0))
            .Should()
            .ThrowExactly<ArgumentOutOfRangeException>();
}
