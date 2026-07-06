namespace StaticSeries;

[TestClass]
public class FractalSeriesTests : StaticSeriesTestBase
{
    /// <summary>
    /// Span 2
    /// </summary>
    [TestMethod]
    public override void DefaultParameters_ReturnsExpectedResults()
    {
        IReadOnlyList<FractalResult> sut = Bars
            .ToFractal();

        // proper quantities
        sut.Should().HaveCount(502);
        sut.Where(static x => x.FractalBear != null).Should().HaveCount(63);
        sut.Where(static x => x.FractalBull != null).Should().HaveCount(71);

        // sample values
        FractalResult r1 = sut[1];
        r1.FractalBear.Should().BeNull();
        r1.FractalBull.Should().BeNull();

        FractalResult r2 = sut[3];
        r2.FractalBear.Should().Be(215.17m);
        r2.FractalBull.Should().BeNull();

        FractalResult r3 = sut[133];
        r3.FractalBear.Should().Be(234.53m);
        r3.FractalBull.Should().BeNull();

        FractalResult r4 = sut[180];
        r4.FractalBear.Should().Be(239.74m);
        r4.FractalBull.Should().Be(238.52m);

        FractalResult r5 = sut[250];
        r5.FractalBear.Should().BeNull();
        r5.FractalBull.Should().Be(256.81m);

        FractalResult r6 = sut[500];
        r6.FractalBear.Should().BeNull();
        r6.FractalBull.Should().BeNull();
    }

    [TestMethod]
    public void StandardSpan4_WithSpanFour_ReturnsExpectedResult()
    {
        IReadOnlyList<FractalResult> sut = Bars
            .ToFractal(4, 4);

        // proper quantities
        sut.Should().HaveCount(502);
        Assert.AreEqual(35, sut.Count(static x => x.FractalBear != null));
        Assert.AreEqual(34, sut.Count(static x => x.FractalBull != null));

        // sample values
        FractalResult r1 = sut[3];
        r1.FractalBear.Should().BeNull();
        r1.FractalBull.Should().BeNull();

        FractalResult r2 = sut[7];
        r2.FractalBear.Should().BeNull();
        r2.FractalBull.Should().Be(212.53m);

        FractalResult r3 = sut[120];
        r3.FractalBear.Should().Be(233.02m);
        r3.FractalBull.Should().BeNull();

        FractalResult r4 = sut[180];
        r4.FractalBear.Should().Be(239.74m);
        r4.FractalBull.Should().BeNull();

        FractalResult r5 = sut[250];
        r5.FractalBear.Should().BeNull();
        r5.FractalBull.Should().Be(256.81m);

        FractalResult r6 = sut[500];
        r6.FractalBear.Should().BeNull();
        r6.FractalBull.Should().BeNull();
    }

    [TestMethod]
    public override void BadBars_DoesNotFail()
    {
        IReadOnlyList<FractalResult> r = BadBars
            .ToFractal();

        r.Should().HaveCount(502);
    }

    [TestMethod]
    public override void NoBars_ReturnsEmpty()
    {
        IReadOnlyList<FractalResult> r0 = Nobars
            .ToFractal();

        r0.Should().BeEmpty();

        IReadOnlyList<FractalResult> r1 = Onebar
            .ToFractal();

        r1.Should().HaveCount(1);
    }

    [TestMethod]
    public void Condense_WithNoThreshold_RemovesNullValues()
    {
        IReadOnlyList<FractalResult> sut = Bars
            .ToFractal()
            .Condense();

        sut.Should().HaveCount(129);
    }

    /// <summary>
    /// bad window span
    /// </summary>
    [TestMethod]
    public void Exceptions_InvalidLookback_ThrowsArgumentOutOfRangeException()
        => FluentActions
            .Invoking(static () => Bars.ToFractal(1))
            .Should()
            .ThrowExactly<ArgumentOutOfRangeException>();
}
