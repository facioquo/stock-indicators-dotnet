namespace StaticSeries;

[TestClass]
public class RenkoSeriesTests : StaticSeriesTestBase
{
    /// <summary>
    /// close
    /// </summary>
    [TestMethod]
    public override void DefaultParameters_ReturnsExpectedResults()
    {
        IReadOnlyList<RenkoResult> sut = Bars
            .ToRenko(2.5m);

        // assertions

        sut.Should().HaveCount(112);
        sut.Where(static x => x.IsUp).Should().HaveCount(62);
        sut.Where(static x => !x.IsUp).Should().HaveCount(50);

        // sample values
        RenkoResult r0 = sut[0];
        r0.Open.Should().Be(213m);
        r0.High.Should().Be(216.89m);
        r0.Low.Should().Be(212.53m);
        r0.Close.Should().Be(215.5m);
        r0.Volume.Should().Be(1180981564m);
        r0.IsUp.Should().BeTrue();

        RenkoResult r5 = sut[5];
        r5.Open.Should().Be(225.5m);
        r5.High.Should().Be(228.15m);
        r5.Low.Should().Be(219.77m);
        r5.Close.Should().Be(228m);
        r5.Volume.Should().Be(4192959240m);
        r5.IsUp.Should().BeTrue();

        RenkoResult last = sut[^1];
        last.Open.Should().Be(240.5m);
        last.High.Should().Be(243.68m);
        last.Low.Should().Be(234.52m);
        last.Close.Should().Be(243m);
        last.Volume.Should().Be(189794032m);
        last.IsUp.Should().BeTrue();
    }

    [TestMethod]
    public void StandardHighLow_AsEndType_ReturnsExpectedResult()
    {
        IReadOnlyList<RenkoResult> sut = Bars
            .ToRenko(2.5m, EndType.HighLow);

        // assertions

        sut.Should().HaveCount(159);

        // sample values
        RenkoResult r0 = sut[0];
        r0.Open.Should().Be(213m);
        r0.High.Should().Be(216.89m);
        r0.Low.Should().Be(212.53m);
        r0.Close.Should().Be(215.5m);
        r0.Volume.Should().BeApproximately(1180981564m, 0.5m);
        r0.IsUp.Should().BeTrue();

        RenkoResult r25 = sut[25];
        r25.Open.Should().Be(270.5m);
        r25.High.Should().Be(273.16m);
        r25.Low.Should().Be(271.96m);
        r25.Close.Should().Be(273m);
        r25.Volume.Should().BeApproximately(100801672m, 0.5m);
        r25.IsUp.Should().BeTrue();

        RenkoResult last = sut[^1];
        last.Open.Should().Be(243m);
        last.High.Should().Be(246.73m);
        last.Low.Should().Be(241.87m);
        last.Close.Should().Be(245.5m);
        last.Volume.Should().BeApproximately(51999637m, 0.5m);
        last.IsUp.Should().BeTrue();
    }

    [TestMethod]
    public void Atr_WithAtrBrickSize_ReturnsExpectedResult()
    {
        IReadOnlyList<RenkoResult> sut = Bars
            .ToRenkoAtr(14);

        // proper quantities
        sut.Should().HaveCount(29);

        // sample values
        RenkoResult r0 = sut[0];
        ((double?)r0.Open).Should().BeApproximately((double)212.8m, Money3);
        ((double?)r0.High).Should().BeApproximately((double)220.19m, Money3);
        ((double?)r0.Low).Should().BeApproximately((double)212.53m, Money3);
        ((double?)r0.Close).Should().BeApproximately((double)218.9497m, Money3);
        r0.Volume.Should().BeApproximately(2090292272m, 0.5m);
        r0.IsUp.Should().BeTrue();

        RenkoResult last = sut[^1];
        ((double?)last.Open).Should().BeApproximately((double)237.3990m, Money3);
        ((double?)last.High).Should().BeApproximately((double)246.73m, Money3);
        ((double?)last.Low).Should().BeApproximately((double)229.42m, Money3);
        ((double?)last.Close).Should().BeApproximately((double)243.5487m, Money3);
        last.Volume.Should().BeApproximately(715446448m, 0.5m);
        last.IsUp.Should().BeTrue();
    }

    [TestMethod]
    public void UseAsBars_AsSmaInput_ReturnsExpectedResult()
    {
        IReadOnlyList<RenkoResult> renkoBars = Bars.ToRenko(2.5m);
        IReadOnlyList<SmaResult> renkoSma = renkoBars.ToSma(5);
        renkoSma.Where(static x => x.Sma != null).Should().HaveCount(108);
    }

    [TestMethod]
    public override void BadBars_DoesNotFail()
    {
        IReadOnlyList<RenkoResult> r = BadBars
            .ToRenko(100m);

        r.Count.Should().NotBe(0);
    }

    [TestMethod]
    public override void NoBars_ReturnsEmpty()
    {
        IReadOnlyList<RenkoResult> r0 = Nobars
            .ToRenko(0.01m);

        r0.Should().BeEmpty();
    }

    [TestMethod]
    public void Exceptions_InvalidParameters_ThrowsArgumentOutOfRangeException()
    {
        // bad arguments
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(
            static () => Bars.ToRenko(0));

        // bad end type
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(
            static () => Bars.ToRenko(2, (EndType)int.MaxValue));
    }
}
