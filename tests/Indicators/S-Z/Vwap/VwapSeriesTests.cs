namespace StaticSeries;

[TestClass]
public class VwapSeriesTests : StaticSeriesTestBase
{
    private static readonly IReadOnlyList<Bar> intraday = Data.GetIntraday()
        .OrderBy(static x => x.Timestamp)
        .Take(391)
        .ToList();

    [TestMethod]
    public override void DefaultParameters_ReturnsExpectedResults()
    {
        IReadOnlyList<VwapResult> sut = intraday.ToVwap();

        // proper quantities
        sut.Should().HaveCount(391);
        sut.Where(static x => x.Vwap != null).Should().HaveCount(391);

        // sample values
        VwapResult r1 = sut[0];
        r1.Vwap.Should().BeApproximately(367.4800, Money4);

        VwapResult r2 = sut[1];
        r2.Vwap.Should().BeApproximately(367.4223, Money4);

        VwapResult r3 = sut[369];
        r3.Vwap.Should().BeApproximately(367.9494, Money4);

        VwapResult r4 = sut[390];
        r4.Vwap.Should().BeApproximately(368.1804, Money4);
    }

    [TestMethod]
    public void WithStartDate_AsAnchorDate_ReturnsExpectedResult()
    {
        DateTime startDate =
            DateTime.ParseExact("2020-12-15 10:00", "yyyy-MM-dd h:mm", invariantCulture);

        IReadOnlyList<VwapResult> sut = intraday
            .ToVwap(startDate);

        // proper quantities
        sut.Should().HaveCount(391);
        sut.Where(static x => x.Vwap != null).Should().HaveCount(361);

        // sample values
        VwapResult r1 = sut[29];
        r1.Vwap.Should().BeNull();

        VwapResult r2 = sut[30];
        r2.Vwap.Should().BeApproximately(366.8100, Money4);

        VwapResult r3 = sut[369];
        r3.Vwap.Should().BeApproximately(368.0511, Money4);

        VwapResult r4 = sut[390];
        r4.Vwap.Should().BeApproximately(368.2908, Money4);
    }

    [TestMethod]
    public void ChainFromResults_ToSma_ReturnsExpectedResult()
    {
        IReadOnlyList<SmaResult> sut = Bars
            .ToVwap()
            .ToSma(10);

        sut.Should().HaveCount(502);
        sut.Where(static x => x.Sma != null).Should().HaveCount(493);
    }

    [TestMethod]
    public override void BadBars_DoesNotFail()
    {
        IReadOnlyList<VwapResult> r = BadBars
            .ToVwap();

        r.Should().HaveCount(502);
        r.Where(static x => x.Vwap is double v && double.IsNaN(v)).Should().BeEmpty();
    }

    [TestMethod]
    public override void NoBars_ReturnsEmpty()
    {
        IReadOnlyList<VwapResult> r0 = Nobars
            .ToVwap();

        r0.Should().BeEmpty();

        IReadOnlyList<VwapResult> r1 = Onebar
            .ToVwap();

        r1.Should().HaveCount(1);
    }

    [TestMethod]
    public void Removed_WithWarmupPeriods_TruncatesResults()
    {
        // no start date
        IReadOnlyList<VwapResult> sut = intraday
            .ToVwap()
            .RemoveWarmupPeriods();

        // assertions
        sut.Should().HaveCount(391);

        VwapResult last = sut[^1];
        last.Vwap.Should().BeApproximately(368.1804, Money4);

        // with start date
        DateTime startDate =
        DateTime.ParseExact("2020-12-15 10:00", "yyyy-MM-dd h:mm", invariantCulture);

        IReadOnlyList<VwapResult> sdResults = intraday
            .ToVwap(startDate)
            .RemoveWarmupPeriods();

        // assertions
        sdResults.Should().HaveCount(361);

        VwapResult sdLast = sdResults[^1];
        sdLast.Vwap.Should().BeApproximately(368.2908, Money4);
    }

    [TestMethod]
    public void Exceptions_InvalidStartDate_ThrowsArgumentOutOfRangeException()
    {
        // bad SMA period
        DateTime startDate =
            DateTime.ParseExact("2000-12-15", "yyyy-MM-dd", invariantCulture);

        Assert.ThrowsExactly<ArgumentOutOfRangeException>(
            () => intraday.ToVwap(startDate));
    }
}
