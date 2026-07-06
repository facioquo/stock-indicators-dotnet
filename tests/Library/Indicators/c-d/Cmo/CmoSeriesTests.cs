namespace StaticSeries;

[TestClass]
public class CmoSeriesTests : StaticSeriesTestBase
{
    [TestMethod]
    public override void DefaultParameters_ReturnsExpectedResults()
    {
        IReadOnlyList<CmoResult> sut = Bars
            .ToCmo(14);

        // proper quantities
        sut.Should().HaveCount(502);
        sut.Where(static x => x.Cmo != null).Should().HaveCount(488);

        // sample values
        CmoResult r13 = sut[13];
        r13.Cmo.Should().BeNull();

        CmoResult r14 = sut[14];
        r14.Cmo.Should().BeApproximately(24.1081, Money4);

        CmoResult r249 = sut[249];
        r249.Cmo.Should().BeApproximately(48.9614, Money4);

        CmoResult r501 = sut[501];
        r501.Cmo.Should().BeApproximately(-26.7502, Money4);
    }

    [TestMethod]
    public void Results_AreAlwaysBounded()
    {
        IReadOnlyList<CmoResult> sut = Bars.ToCmo(14);
        sut.IsBetween(static x => x.Cmo, -100, 100);
    }

    [TestMethod]
    public void UseReusable_ClosePrice_ReturnsExpectedResult()
    {
        IReadOnlyList<CmoResult> sut = Bars
            .Use(CandlePart.Close)
            .ToCmo(14);

        sut.Should().HaveCount(502);
        sut.Where(static x => x.Cmo != null).Should().HaveCount(488);
    }

    [TestMethod]
    public void Chainee_FromSma_ReturnsExpectedResult()
    {
        IReadOnlyList<CmoResult> sut = Bars
            .ToSma(2)
            .ToCmo(20);

        sut.Should().HaveCount(502);
        sut.Where(static x => x.Cmo != null).Should().HaveCount(481);
    }

    [TestMethod]
    public void ChainFromResults_ToSma_ReturnsExpectedResult()
    {
        IReadOnlyList<SmaResult> sut = Bars
            .ToCmo(20)
            .ToSma(10);

        sut.Should().HaveCount(502);
        sut.Where(static x => x.Sma != null).Should().HaveCount(473);
    }

    [TestMethod]
    public override void BadBars_DoesNotFail()
    {
        IReadOnlyList<CmoResult> r = BadBars
            .ToCmo(35);

        r.Should().HaveCount(502);
        r.Where(static x => x.Cmo is double.NaN).Should().BeEmpty();
    }

    [TestMethod]
    public override void NoBars_ReturnsEmpty()
    {
        IReadOnlyList<CmoResult> r0 = Nobars
            .ToCmo(5);

        r0.Should().BeEmpty();

        IReadOnlyList<CmoResult> r1 = Onebar
            .ToCmo(5);

        r1.Should().HaveCount(1);
    }

    [TestMethod]
    public void Removed_WithWarmupPeriods_TruncatesResults()
    {
        IReadOnlyList<CmoResult> sut = Bars
            .ToCmo(14)
            .RemoveWarmupPeriods();

        // assertions
        sut.Should().HaveCount(488);

        CmoResult last = sut[^1];
        last.Cmo.Should().BeApproximately(-26.7502, Money4);
    }

    /// <summary>
    /// bad lookback period
    /// </summary>
    [TestMethod]
    public void Exceptions_InvalidLookback_ThrowsArgumentOutOfRangeException()
        => FluentActions
            .Invoking(static () => Bars.ToCmo(0))
            .Should()
            .ThrowExactly<ArgumentOutOfRangeException>();

    [TestMethod]
    public void FlatPrices_AcrossEntireWindow_ReturnsZero()
    {
        // construct a 30-bar series where every close is identical
        // (no price change anywhere). CMO should be 0 across the
        // entire post-warmup window since sH + sL == 0 (no movement,
        // so neither gains nor losses accumulate).
        DateTime t0 = new(2025, 1, 1);
        List<Bar> flat = new(30);
        for (int i = 0; i < 30; i++)
        {
            flat.Add(new Bar(
                Timestamp: t0.AddDays(i),
                Open: 100m,
                High: 100m,
                Low: 100m,
                Close: 100m,
                Volume: 1000m));
        }

        IReadOnlyList<CmoResult> sut = flat.ToCmo(14);

        sut.Should().HaveCount(30);
        sut[0].Cmo.Should().BeNull();           // first record always null
        sut[13].Cmo.Should().BeNull();          // still warming up

        // post-warmup: every bar within an all-flat window must report 0
        for (int i = 14; i < 30; i++)
        {
            sut[i].Cmo.Should().Be(0d, $"flat window at index {i} should produce 0");
        }
    }

    [TestMethod]
    public void FlatThenMoving_ReturnsValuesOnceRealMovementEnters()
    {
        // 20 flat bars (no movement) then 14 ascending bars. The CMO
        // should be 0 while the lookback window is fully flat, then
        // climb toward +100 as the lookback window fills with up-ticks
        // (sH grows, sL stays 0). Validates that an in-window zero-change
        // tick is treated as a non-issue rather than producing NaN or null.
        DateTime t0 = new(2025, 1, 1);
        List<Bar> series = new(34);
        for (int i = 0; i < 20; i++)
        {
            series.Add(new Bar(
                Timestamp: t0.AddDays(i),
                Open: 100m, High: 100m, Low: 100m, Close: 100m,
                Volume: 1000m));
        }

        for (int i = 0; i < 14; i++)
        {
            decimal close = 100m + ((i + 1) * 1m);
            series.Add(new Bar(
                Timestamp: t0.AddDays(20 + i),
                Open: close - 1m, High: close, Low: close - 1m, Close: close,
                Volume: 1000m));
        }

        IReadOnlyList<CmoResult> sut = series.ToCmo(14);

        sut.Should().HaveCount(34);
        sut[19].Cmo.Should().Be(0d, "still inside the all-flat 14-bar window");
        sut[33].Cmo.Should().Be(100d, "last bar's 14-period window is entirely up-ticks");
        sut.Where(static x => x.Cmo is double.NaN).Should().BeEmpty(
            "zero-change ticks must never propagate to NaN results");
    }
}
