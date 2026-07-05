namespace StaticSeries;

[TestClass]
public class AtrStopSeriesTests : StaticSeriesTestBase
{
    [TestMethod]
    public override void DefaultParameters_ReturnsExpectedResults()
    {
        const int lookbackPeriods = 21;
        const double multiplier = 3;

        IReadOnlyList<AtrStopResult> sut = Bars
            .ToAtrStop(lookbackPeriods, multiplier);

        // proper quantities
        sut.Should().HaveCount(502);
        sut.Where(static x => x.AtrStop != null).Should().HaveCount(481);

        // sample values
        AtrStopResult r20 = sut[20];
        r20.AtrStop.Should().BeNull();
        r20.BuyStop.Should().BeNull();
        r20.SellStop.Should().BeNull();

        AtrStopResult r21 = sut[21];
        r21.AtrStop.Should().BeApproximately(211.13, Money4);
        r21.BuyStop.Should().BeNull();
        r21.SellStop.Should().Be(r21.AtrStop);

        AtrStopResult r151 = sut[151];
        r151.AtrStop.Should().BeApproximately(232.7861, Money4);
        r151.BuyStop.Should().BeNull();
        r151.SellStop.Should().Be(r151.AtrStop);

        AtrStopResult r152 = sut[152];
        r152.AtrStop.Should().BeApproximately(236.3913, Money4);
        r152.BuyStop.Should().Be(r152.AtrStop);
        r152.SellStop.Should().BeNull();

        AtrStopResult r249 = sut[249];
        r249.AtrStop.Should().BeApproximately(253.8863, Money4);
        r249.BuyStop.Should().BeNull();
        r249.SellStop.Should().Be(r249.AtrStop);

        AtrStopResult r501 = sut[501];
        r501.AtrStop.Should().BeApproximately(246.3232, Money4);
        r501.BuyStop.Should().Be(r501.AtrStop);
        r501.SellStop.Should().BeNull();
    }

    [TestMethod]
    public void HighLow_WithHighLowEndType_ReturnsExpectedResult()
    {
        const int lookbackPeriods = 21;
        const double multiplier = 3;

        IReadOnlyList<AtrStopResult> sut = Bars
            .ToAtrStop(lookbackPeriods, multiplier, EndType.HighLow);

        // proper quantities
        sut.Should().HaveCount(502);
        sut.Where(static x => x.AtrStop != null).Should().HaveCount(481);

        // sample values
        AtrStopResult r20 = sut[20];
        r20.AtrStop.Should().BeNull();
        r20.BuyStop.Should().BeNull();
        r20.SellStop.Should().BeNull();

        AtrStopResult r21 = sut[21];
        r21.AtrStop.Should().BeApproximately(210.23, Money4);
        r21.BuyStop.Should().BeNull();
        r21.SellStop.Should().Be(r21.AtrStop);

        AtrStopResult r69 = sut[69];
        r69.AtrStop.Should().BeApproximately(221.0594, Money4);
        r69.BuyStop.Should().BeNull();
        r69.SellStop.Should().Be(r69.AtrStop);

        AtrStopResult r70 = sut[70];
        r70.AtrStop.Should().BeApproximately(226.4624, Money4);
        r70.BuyStop.Should().Be(r70.AtrStop);
        r70.SellStop.Should().BeNull();

        AtrStopResult r249 = sut[249];
        r249.AtrStop.Should().BeApproximately(253.4863, Money4);
        r249.BuyStop.Should().BeNull();
        r249.SellStop.Should().Be(r249.AtrStop);

        AtrStopResult r501 = sut[501];
        r501.AtrStop.Should().BeApproximately(252.6932, Money4);
        r501.BuyStop.Should().Be(r501.AtrStop);
        r501.SellStop.Should().BeNull();
    }

    [TestMethod]
    public override void BadBars_DoesNotFail()
    {
        IReadOnlyList<AtrStopResult> r = BadBars
            .ToAtrStop(7);

        r.Should().HaveCount(502);
    }

    [TestMethod]
    public override void NoBars_ReturnsEmpty()
    {
        IReadOnlyList<AtrStopResult> r0 = Nobars
            .ToAtrStop();

        r0.Should().BeEmpty();

        IReadOnlyList<AtrStopResult> r1 = Onebar
            .ToAtrStop();

        r1.Should().HaveCount(1);
    }

    [TestMethod]
    public void Condense_RemovesNullResults_ReturnsCondensed()
    {
        const int lookbackPeriods = 21;
        const double multiplier = 3;

        IReadOnlyList<AtrStopResult> sut = Bars
            .ToAtrStop(lookbackPeriods, multiplier)
            .Condense();

        // assertions
        sut.Should().HaveCount(481);

        AtrStopResult last = sut[^1];
        last.AtrStop.Should().BeApproximately(246.3232, Money4);
        last.BuyStop.Should().Be(last.AtrStop);
        last.SellStop.Should().BeNull();
    }

    [TestMethod]
    public void Removed_WithWarmupPeriods_TruncatesResults()
    {
        const int lookbackPeriods = 21;
        const double multiplier = 3;

        IReadOnlyList<AtrStopResult> sut = Bars
            .ToAtrStop(lookbackPeriods, multiplier)
            .RemoveWarmupPeriods();

        // assertions
        sut.Should().HaveCount(481);

        AtrStopResult last = sut[^1];
        last.AtrStop.Should().BeApproximately(246.3232, Money4);
        last.BuyStop.Should().Be(last.AtrStop);
        last.SellStop.Should().BeNull();
    }

    [TestMethod]
    public void Exceptions_InvalidParameters_ThrowsArgumentOutOfRangeException()
    {
        // bad lookback period
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(
            static () => Bars.ToAtrStop(1));

        // bad multiplier
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(
            static () => Bars.ToAtrStop(7, 0));
    }
}
