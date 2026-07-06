namespace StaticSeries;

[TestClass]
public class StdDevChannelsSeriesTests : StaticSeriesTestBase
{
    [TestMethod]
    public override void DefaultParameters_ReturnsExpectedResults()
    {
        const int lookbackPeriods = 20;
        const double standardDeviations = 2;

        IReadOnlyList<StdDevChannelsResult> sut =
            Bars.ToStdDevChannels(lookbackPeriods, standardDeviations);

        // proper quantities
        sut.Should().HaveCount(502);
        sut.Where(static x => x.Centerline != null).Should().HaveCount(500);
        sut.Where(static x => x.UpperChannel != null).Should().HaveCount(500);
        sut.Where(static x => x.LowerChannel != null).Should().HaveCount(500);

        // sample value
        StdDevChannelsResult r1 = sut[1];
        r1.Centerline.Should().BeNull();
        r1.UpperChannel.Should().BeNull();
        r1.LowerChannel.Should().BeNull();
        r1.BreakPoint.Should().BeFalse();

        StdDevChannelsResult r2 = sut[2];
        r2.Centerline.Should().BeApproximately(213.7993, Money4);
        r2.UpperChannel.Should().BeApproximately(215.7098, Money4);
        r2.LowerChannel.Should().BeApproximately(211.8888, Money4);
        r2.BreakPoint.Should().BeTrue();

        StdDevChannelsResult r3 = sut[141];
        r3.Centerline.Should().BeApproximately(236.1744, Money4);
        r3.UpperChannel.Should().BeApproximately(240.4784, Money4);
        r3.LowerChannel.Should().BeApproximately(231.8704, Money4);
        r3.BreakPoint.Should().BeFalse();

        StdDevChannelsResult r4 = sut[142];
        r4.Centerline.Should().BeApproximately(236.3269, Money4);
        r4.UpperChannel.Should().BeApproximately(239.5585, Money4);
        r4.LowerChannel.Should().BeApproximately(233.0953, Money4);
        r4.BreakPoint.Should().BeTrue();

        StdDevChannelsResult r5 = sut[249];
        r5.Centerline.Should().BeApproximately(259.6044, Money4);
        r5.UpperChannel.Should().BeApproximately(267.5754, Money4);
        r5.LowerChannel.Should().BeApproximately(251.6333, Money4);
        r5.BreakPoint.Should().BeFalse();

        StdDevChannelsResult r6 = sut[482];
        r6.Centerline.Should().BeApproximately(267.9069, Money4);
        r6.UpperChannel.Should().BeApproximately(289.7473, Money4);
        r6.LowerChannel.Should().BeApproximately(246.0664, Money4);
        r6.BreakPoint.Should().BeTrue();

        StdDevChannelsResult r7 = sut[501];
        r7.Centerline.Should().BeApproximately(235.8131, Money4);
        r7.UpperChannel.Should().BeApproximately(257.6536, Money4);
        r7.LowerChannel.Should().BeApproximately(213.9727, Money4);
        r7.BreakPoint.Should().BeFalse();
    }

    [TestMethod]
    public void FullHistory_AsLookbackPeriods_ReturnsLinearRegression()
    {
        // full history linear regression

        IReadOnlyList<StdDevChannelsResult> sut =
            Bars.ToStdDevChannels(Bars.Count);

        // proper quantities
        sut.Should().HaveCount(502);
        sut.Where(static x => x.Centerline != null).Should().HaveCount(502);
        sut.Where(static x => x.UpperChannel != null).Should().HaveCount(502);
        sut.Where(static x => x.LowerChannel != null).Should().HaveCount(502);
        sut.Where(static x => !x.BreakPoint).Should().HaveCount(501);

        // sample value
        StdDevChannelsResult r1 = sut[0];
        r1.Centerline.Should().BeApproximately(219.2605, Money4);
        r1.UpperChannel.Should().BeApproximately(258.7104, Money4);
        r1.LowerChannel.Should().BeApproximately(179.8105, Money4);
        r1.BreakPoint.Should().BeTrue();

        StdDevChannelsResult r2 = sut[249];
        r2.Centerline.Should().BeApproximately(249.3814, Money4);
        r2.UpperChannel.Should().BeApproximately(288.8314, Money4);
        r2.LowerChannel.Should().BeApproximately(209.9315, Money4);

        StdDevChannelsResult r3 = sut[501];
        r3.Centerline.Should().BeApproximately(279.8653, Money4);
        r3.UpperChannel.Should().BeApproximately(319.3152, Money4);
        r3.LowerChannel.Should().BeApproximately(240.4153, Money4);
    }

    [TestMethod]
    public void UseReusable_ClosePrice_ReturnsExpectedResult()
    {
        IReadOnlyList<StdDevChannelsResult> sut = Bars
            .Use(CandlePart.Close)
            .ToStdDevChannels();

        sut.Should().HaveCount(502);
        sut.Where(static x => x.Centerline != null).Should().HaveCount(500);
    }

    [TestMethod]
    public void Chainee_FromSma_ReturnsExpectedResult()
    {
        IReadOnlyList<StdDevChannelsResult> sut = Bars
            .ToSma(2)
            .ToStdDevChannels();

        sut.Should().HaveCount(502);
        sut.Where(static x => x.Centerline != null).Should().HaveCount(500);
    }

    [TestMethod]
    public override void BadBars_DoesNotFail()
    {
        IReadOnlyList<StdDevChannelsResult> r = BadBars
            .ToStdDevChannels();

        r.Should().HaveCount(502);
        r.Where(static x => x.UpperChannel is double v && double.IsNaN(v)).Should().BeEmpty();
    }

    [TestMethod]
    public override void NoBars_ReturnsEmpty()
    {
        IReadOnlyList<StdDevChannelsResult> r0 = Nobars
            .ToStdDevChannels();

        r0.Should().BeEmpty();

        IReadOnlyList<StdDevChannelsResult> r1 = Onebar
            .ToStdDevChannels();

        r1.Should().HaveCount(1);
    }

    [TestMethod]
    public void Condense_RemovesNullResults_ReturnsCondensed()
    {
        const int lookbackPeriods = 20;
        const double standardDeviations = 2;

        IReadOnlyList<StdDevChannelsResult> sut = Bars
            .ToStdDevChannels(lookbackPeriods, standardDeviations)
            .Condense();

        // assertions
        sut.Should().HaveCount(500);
        StdDevChannelsResult last = sut[^1];
        last.Centerline.Should().BeApproximately(235.8131, Money4);
        last.UpperChannel.Should().BeApproximately(257.6536, Money4);
        last.LowerChannel.Should().BeApproximately(213.9727, Money4);
        last.BreakPoint.Should().BeFalse();
    }

    [TestMethod]
    public void Removed_WithWarmupPeriods_TruncatesResults()
    {
        const int lookbackPeriods = 20;
        const double standardDeviations = 2;

        IReadOnlyList<StdDevChannelsResult> sut = Bars
            .ToStdDevChannels(lookbackPeriods, standardDeviations)
            .RemoveWarmupPeriods();

        // assertions
        sut.Should().HaveCount(500);
        StdDevChannelsResult last = sut[^1];
        last.Centerline.Should().BeApproximately(235.8131, Money4);
        last.UpperChannel.Should().BeApproximately(257.6536, Money4);
        last.LowerChannel.Should().BeApproximately(213.9727, Money4);
        last.BreakPoint.Should().BeFalse();
    }

    [TestMethod]
    public void Exceptions_InvalidParameters_ThrowsArgumentOutOfRangeException()
    {
        // bad lookback period
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(
            static () => Bars.ToStdDevChannels(0));

        // bad standard deviations
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(
            static () => Bars.ToStdDevChannels(20, 0));
    }
}
