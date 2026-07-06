namespace StaticSeries;

[TestClass]
public class AlligatorSeriesTests : StaticSeriesTestBase
{
    [TestMethod]
    public override void DefaultParameters_ReturnsExpectedResults()
    {
        IReadOnlyList<AlligatorResult> sut = Bars
            .ToAlligator();

        // proper quantities
        sut.Should().HaveCount(502);
        sut.Where(static x => x.Jaw != null).Should().HaveCount(482);
        sut.Where(static x => x.Teeth != null).Should().HaveCount(490);
        sut.Where(static x => x.Lips != null).Should().HaveCount(495);

        // starting calculations at proper index
        sut[19].Jaw.Should().BeNull();
        sut[20].Jaw.Should().NotBeNull();

        sut[11].Teeth.Should().BeNull();
        sut[12].Teeth.Should().NotBeNull();

        sut[6].Lips.Should().BeNull();
        sut[7].Lips.Should().NotBeNull();

        // sample values
        sut[20].Jaw.Should().BeApproximately(213.81269, Money5);
        sut[21].Jaw.Should().BeApproximately(213.79287, Money5);
        sut[99].Jaw.Should().BeApproximately(225.60571, Money5);
        sut[501].Jaw.Should().BeApproximately(260.98953, Money5);

        sut[12].Teeth.Should().BeApproximately(213.699375, Money6);
        sut[13].Teeth.Should().BeApproximately(213.80008, Money5);
        sut[99].Teeth.Should().BeApproximately(226.12157, Money5);
        sut[501].Teeth.Should().BeApproximately(253.53576, Money5);

        sut[7].Lips.Should().BeApproximately(213.63500, Money5);
        sut[8].Lips.Should().BeApproximately(213.74900, Money5);
        sut[99].Lips.Should().BeApproximately(226.35353, Money5);
        sut[501].Lips.Should().BeApproximately(244.29591, Money5);
    }

    [TestMethod]
    public void Chainee_FromSma_ReturnsExpectedResult()
    {
        IReadOnlyList<AlligatorResult> sut = Bars
            .ToSma(2)
            .ToAlligator();

        sut.Should().HaveCount(502);
        sut.Where(static x => x.Jaw != null).Should().HaveCount(481);
    }

    [TestMethod]
    public override void BadBars_DoesNotFail()
    {
        IReadOnlyList<AlligatorResult> r = BadBars
            .ToAlligator(3, 3, 2, 1, 1, 1);

        r.Should().HaveCount(502);
        r.Where(static x => x.Jaw is double.NaN).Should().BeEmpty();
    }

    [TestMethod]
    public override void NoBars_ReturnsEmpty()
    {
        IReadOnlyList<AlligatorResult> r0 = Nobars
            .ToAlligator();

        r0.Should().BeEmpty();

        IReadOnlyList<AlligatorResult> r1 = Onebar
            .ToAlligator();

        r1.Should().HaveCount(1);
    }

    [TestMethod]
    public void Condense_RemovesNullResults_ReturnsCondensed()
    {
        IReadOnlyList<AlligatorResult> sut = Bars
            .ToAlligator()
            .Condense();

        sut.Should().HaveCount(495);

        AlligatorResult last = sut[^1];
        last.Jaw.Should().BeApproximately(260.98953, Money5);
        last.Teeth.Should().BeApproximately(253.53576, Money5);
        last.Lips.Should().BeApproximately(244.29591, Money5);
    }

    [TestMethod]
    public void Removed_WithWarmupPeriods_TruncatesResults()
    {
        IReadOnlyList<AlligatorResult> sut = Bars
            .ToAlligator()
            .RemoveWarmupPeriods();

        sut.Should().HaveCount(502 - 21 - 250);

        AlligatorResult last = sut[^1];
        last.Jaw.Should().BeApproximately(260.98953, Money5);
        last.Teeth.Should().BeApproximately(253.53576, Money5);
        last.Lips.Should().BeApproximately(244.29591, Money5);
    }

    [TestMethod]
    public void Equality_BetweenInstances_ComparesCorrectly()
    {
        AlligatorResult r1 = new(EvalDate, 1d, null, null);
        AlligatorResult r2 = new(EvalDate, 1d, null, null);
        AlligatorResult r3 = new(EvalDate, 2d, null, null); // abberent

        Assert.IsTrue(Equals(r1, r2));
        Assert.IsFalse(Equals(r1, r3));
    }

    [TestMethod]
    public void Exceptions_InvalidParameters_ThrowsArgumentOutOfRangeException()
    {
        // bad jaw lookback periods
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(
            static () => Bars.ToAlligator(13, 8, 13));

        // bad teeth lookback periods
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(
            static () => Bars.ToAlligator(13, 8, 8, 5, 8));

        // bad lips lookback periods
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(
            static () => Bars.ToAlligator(13, 8, 8, 5, 0));

        // bad jaw offset periods
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(
            static () => Bars.ToAlligator(13, 0));

        // bad teeth offset periods
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(
            static () => Bars.ToAlligator(13, 8, 8, 0));

        // bad lips offset periods
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(
            static () => Bars.ToAlligator(13, 8, 8, 5, 5, 0));

        // bad jaw + offset periods
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(
            static () => Bars.ToAlligator(13, 8, 12, 11));

        // bad teeth + offset periods
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(
            static () => Bars.ToAlligator(13, 8, 8, 5, 7, 7));
    }
}
