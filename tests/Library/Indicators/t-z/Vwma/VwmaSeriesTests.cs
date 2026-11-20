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

    [TestMethod]
    [DataRow(CandlePart.Open)]
    [DataRow(CandlePart.High)]
    [DataRow(CandlePart.Low)]
    [DataRow(CandlePart.Close)]
    [DataRow(CandlePart.HL2)]
    [DataRow(CandlePart.HLC3)]
    [DataRow(CandlePart.OC2)]
    [DataRow(CandlePart.OHL3)]
    [DataRow(CandlePart.OHLC4)]
    [DataRow(CandlePart.Volume)]
    public void ToVwma_SelectedCandlePart_MatchesWeightedReference(
        CandlePart candlePart)
    {
        (double first, double second) = candlePart switch {
            CandlePart.Open => (40d / 3d, 27.5),
            CandlePart.High => (52d / 3d, 31.5),
            CandlePart.Low => (34d / 3d, 25.5),
            CandlePart.Close => (46d / 3d, 29.5),
            CandlePart.HL2 => (43d / 3d, 28.5),
            CandlePart.HLC3 => (44d / 3d, 173d / 6d),
            CandlePart.OC2 => (43d / 3d, 28.5),
            CandlePart.OHL3 => (14d, 169d / 6d),
            CandlePart.OHLC4 => (43d / 3d, 28.5),
            CandlePart.Volume => (5d / 3d, 2.5),
            _ => throw new ArgumentOutOfRangeException(nameof(candlePart))
        };

        DateTime day = new(2026, 1, 1);
        IReadOnlyList<Bar> bars = [
            new(day, 10m, 14m, 8m, 12m, 2m),
            new(day.AddDays(1), 20m, 24m, 18m, 22m, 1m),
            new(day.AddDays(2), 30m, 34m, 28m, 32m, 3m)
        ];

        IReadOnlyList<VwmaResult> actual = bars.ToVwma(2, candlePart);

        // For Open: (10*2 + 20*1)/3, then (20*1 + 30*3)/4.
        actual[0].Vwma.Should().BeNull();
        actual[1].Vwma.Should().BeApproximately(first, Money12);
        actual[2].Vwma.Should().BeApproximately(second, Money12);
    }

    [TestMethod]
    public void ToVwma_ExplicitClose_PreservesExistingOverload()
        => Bars.ToVwma(14, CandlePart.Close).IsExactly(Bars.ToVwma(14));

    [TestMethod]
    public void ToVwma_InvalidCandlePart_ThrowsArgumentOutOfRangeException()
        => FluentActions.Invoking(() => Bars.ToVwma(14, (CandlePart)(-1)))
            .Should().ThrowExactly<ArgumentOutOfRangeException>()
            .WithParameterName("candlePart");
}
