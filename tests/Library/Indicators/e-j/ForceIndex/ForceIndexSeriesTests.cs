namespace StaticSeries;

[TestClass]
public class ForceIndexSeriesTests : StaticSeriesTestBase
{
    [TestMethod]
    public override void DefaultParameters_ReturnsExpectedResults()
    {
        IReadOnlyList<ForceIndexResult> r = Bars.ToForceIndex(13).ToList();

        // proper quantities
        r.Should().HaveCount(502);
        r.Where(static x => x.ForceIndex != null).Should().HaveCount(489);

        // sample values
        r[12].ForceIndex.Should().BeNull();

        Assert.AreEqual(10668240.778, Math.Round(r[13].ForceIndex.Value, 3));
        Assert.AreEqual(15883211.364, Math.Round(r[24].ForceIndex.Value, 3));
        Assert.AreEqual(7598218.196, Math.Round(r[149].ForceIndex.Value, 3));
        Assert.AreEqual(23612118.994, Math.Round(r[249].ForceIndex.Value, 3));
        Assert.AreEqual(-16824018.428, Math.Round(r[501].ForceIndex.Value, 3));
    }

    [TestMethod]
    public void ChainFromResults_ToSma_ReturnsExpectedResult()
    {
        IReadOnlyList<SmaResult> sut = Bars
            .ToForceIndex(13)
            .ToSma(10);

        sut.Should().HaveCount(502);
        sut.Where(static x => x.Sma != null).Should().HaveCount(480);
    }

    [TestMethod]
    public override void BadBars_DoesNotFail()
    {
        IReadOnlyList<ForceIndexResult> r = BadBars
            .ToForceIndex();

        r.Should().HaveCount(502);
        r.Where(static x => x.ForceIndex is double v && double.IsNaN(v)).Should().BeEmpty();
    }

    [TestMethod]
    public override void NoBars_ReturnsEmpty()
    {
        IReadOnlyList<ForceIndexResult> r0 = Nobars
            .ToForceIndex(5);

        r0.Should().BeEmpty();

        IReadOnlyList<ForceIndexResult> r1 = Onebar
            .ToForceIndex(5);

        r1.Should().HaveCount(1);
    }

    [TestMethod]
    public void Removed_WithWarmupPeriods_TruncatesResults()
    {
        IReadOnlyList<ForceIndexResult> sut = Bars
            .ToForceIndex(13)
            .RemoveWarmupPeriods();

        // assertions
        sut.Should().HaveCount(502 - (13 + 100));

        ForceIndexResult last = sut[^1];
        Assert.AreEqual(-16824018.428, Math.Round(last.ForceIndex.Value, 3));
    }

    /// <summary>
    /// bad lookback period
    /// </summary>
    [TestMethod]
    public void Exceptions_InvalidLookback_ThrowsArgumentOutOfRangeException()
        => FluentActions
            .Invoking(static () => Bars.ToForceIndex(0))
            .Should()
            .ThrowExactly<ArgumentOutOfRangeException>();
}
