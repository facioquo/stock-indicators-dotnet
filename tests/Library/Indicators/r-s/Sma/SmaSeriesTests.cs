namespace StaticSeries;

[TestClass]
public partial class SmaSeriesTests : StaticSeriesTestBase
{
    [TestMethod]
    public override void DefaultParameters_ReturnsExpectedResults()
    {
        IReadOnlyList<SmaResult> sut = Bars
            .ToSma(20);

        // proper quantities
        sut.Should().HaveCount(502);
        sut.Where(static x => x.Sma != null).Should().HaveCount(483);
        sut.Should().HaveCount(expectedDefault.Length);

        // sample values
        sut[18].Sma.Should().BeNull();
        sut[19].Sma.Should().BeApproximately(214.525, Money12);
        sut[24].Sma.Should().BeApproximately(215.031, Money12);
        sut[149].Sma.Should().BeApproximately(234.935, Money12);
        sut[249].Sma.Should().BeApproximately(255.550, Money12);
        sut[501].Sma.Should().BeApproximately(251.860, Money12);

        // matches manual calculations with high precision
        for (int i = 0; i < Bars.Count; i++)
        {
            sut[i].Timestamp.Should().Be(Bars[i].Timestamp);

            if (double.IsNaN(expectedDefault[i]))
            {
                sut[i].Sma.Should().BeNull();
                continue;
            }

            sut[i].Sma.Should().BeApproximately(expectedDefault[i], Money12);
        }
    }

    [TestMethod]
    public void LongestBars_ReturnsExpectedResults()
    {
        IReadOnlyList<SmaResult> sut = LongestBars
            .ToSma(20);

        // proper quantities
        sut.Should().HaveCount(15821);
        sut.Where(static x => x.Sma != null).Should().HaveCount(15802);
        sut.Should().HaveCount(expectedLongest.Length);

        // matches manual calculations with high precision
        for (int i = 0; i < LongestBars.Count; i++)
        {
            sut[i].Timestamp.Should().Be(LongestBars[i].Timestamp);

            if (double.IsNaN(expectedLongest[i]))
            {
                sut[i].Sma.Should().BeNull();
                continue;
            }

            sut[i].Sma.Should().BeApproximately(expectedLongest[i], Money10);
        }
    }

    [TestMethod]
    public void CandlePartOpen_UseOpenPart_ReturnsExpectedResult()
    {
        IReadOnlyList<SmaResult> sut = Bars
            .Use(CandlePart.Open)
            .ToSma(20);

        sut.Should().HaveCount(502);
        sut.Where(static x => x.Sma != null).Should().HaveCount(483);

        // sample values
        sut[18].Sma.Should().BeNull();
        sut[19].Sma.Should().BeApproximately(214.3795, Money4);
        sut[24].Sma.Should().BeApproximately(214.9535, Money4);
        sut[149].Sma.Should().BeApproximately(234.8280, Money4);
        sut[249].Sma.Should().BeApproximately(255.6915, Money4);
        sut[501].Sma.Should().BeApproximately(253.1725, Money4);
    }

    [TestMethod]
    public void CandlePartVolume_UseVolumePart_ReturnsExpectedResult()
    {
        IReadOnlyList<SmaResult> sut = Bars
            .Use(CandlePart.Volume)
            .ToSma(20);

        sut.Should().HaveCount(502);
        sut.Where(static x => x.Sma != null).Should().HaveCount(483);

        // sample values
        SmaResult r24 = sut[24];
        r24.Sma.Should().Be(77293768.2);

        SmaResult r290 = sut[290];
        r290.Sma.Should().Be(157958070.8);

        SmaResult r501 = sut[501];
        Assert.AreEqual(DateTime.ParseExact("12/31/2018", "MM/dd/yyyy", invariantCulture), r501.Timestamp);
        r501.Sma.Should().Be(163695200);
    }

    [TestMethod]
    public void ChainFromResults_ToEma_ReturnsExpectedResult()
    {
        IReadOnlyList<EmaResult> sut = Bars
            .ToSma(10)
            .ToEma(10);

        sut.Should().HaveCount(502);
        sut.Where(static x => x.Ema != null).Should().HaveCount(484);
    }

    [TestMethod]
    public void NaN_WithBtcUsdData_DoesNotProduceNaN()
    {
        IReadOnlyList<SmaResult> r = Data.GetBtcUsdNan()
            .ToSma(50);

        r.Where(static x => x.Sma is double.NaN).Should().BeEmpty();
    }

    [TestMethod]
    public override void BadBars_DoesNotFail()
    {
        IReadOnlyList<SmaResult> r = BadBars
            .ToSma(15);

        r.Should().HaveCount(502);
        r.Where(static x => x.Sma is double.NaN).Should().BeEmpty();
    }

    [TestMethod]
    public override void NoBars_ReturnsEmpty()
    {
        IReadOnlyList<SmaResult> r0 = Nobars
            .ToSma(5);

        r0.Should().BeEmpty();

        IReadOnlyList<SmaResult> r1 = Onebar
            .ToSma(5);

        r1.Should().HaveCount(1);
    }

    [TestMethod]
    public void Removed_WithWarmupPeriods_TruncatesResults()
    {
        IReadOnlyList<SmaResult> sut = Bars
            .ToSma(20)
            .RemoveWarmupPeriods();

        // assertions
        sut.Should().HaveCount(502 - 19);
        sut[^1].Sma.Should().BeApproximately(251.8600, Money4);
    }

    [TestMethod]
    public void Equality_BetweenInstances_ComparesCorrectly()
    {
        SmaResult r1 = new(Timestamp: EvalDate, Sma: 1d);

        SmaResult r2 = new(Timestamp: EvalDate, Sma: 1d);

        SmaResult r3 = new(Timestamp: EvalDate, Sma: 2d);

        Assert.IsTrue(Equals(r1, r2));
        Assert.IsFalse(Equals(r1, r3));

        Assert.IsTrue(r1.Equals(r2));
        Assert.IsFalse(r1.Equals(r3));

        (r1 == r2).Should().BeTrue();
        (r1 == r3).Should().BeFalse();

        (r1 != r2).Should().BeFalse();
        (r1 != r3).Should().BeTrue();
    }

    /// <summary>
    /// bad lookback period
    /// </summary>
    [TestMethod]
    public void Exceptions_InvalidLookback_ThrowsArgumentOutOfRangeException()
        => FluentActions
            .Invoking(static () => Bars.ToSma(0))
            .Should()
            .ThrowExactly<ArgumentOutOfRangeException>();
}
