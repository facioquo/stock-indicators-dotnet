namespace StaticSeries;

[TestClass]
public class RenkoAtrSeriesTests : StaticSeriesTestBase
{
    /// <summary>
    /// Default parameters (atrPeriods = 14, endType = Close)
    /// </summary>
    [TestMethod]
    public override void DefaultParameters_ReturnsExpectedResults()
    {
        IReadOnlyList<RenkoResult> sut = Bars
            .ToRenkoAtr();

        // assertions
        sut.Should().HaveCount(29);
        sut.Where(static x => x.IsUp).Should().HaveCount(17);
        sut.Where(static x => !x.IsUp).Should().HaveCount(12);

        // sample values (first brick)
        RenkoResult r0 = sut[0];
        r0.Timestamp.Should().Be(DateTime.Parse("2017-02-13", invariantCulture));
        r0.Open.Should().Be(212.8m);
        r0.High.Should().Be(220.19m);
        r0.Low.Should().Be(212.53m);
        r0.Volume.Should().Be(2090292272m);
        r0.IsUp.Should().BeTrue();

        // last brick
        RenkoResult last = sut[^1];
        last.Timestamp.Should().Be(DateTime.Parse("2018-12-31", invariantCulture));
        last.High.Should().Be(246.73m);
        last.Low.Should().Be(229.42m);
        last.Volume.Should().Be(715446448m);
        last.IsUp.Should().BeTrue();
    }

    [TestMethod]
    public void WithHighLowEndType_AsEndType_ReturnsExpectedResult()
    {
        IReadOnlyList<RenkoResult> sut = Bars
            .ToRenkoAtr(14, EndType.HighLow);

        // assertions - HighLow produces more bricks than Close
        sut.Count.Should().BeGreaterThan(29);

        // sample values
        RenkoResult r0 = sut[0];
        r0.Open.Should().Be(212.8m);
        r0.Volume.Should().BeGreaterThan(0);
        r0.IsUp.Should().BeTrue();

        RenkoResult last = sut[^1];
        last.IsUp.Should().BeTrue();
    }

    [TestMethod]
    public void WithCustomAtrPeriod_AsAtrPeriods_ReturnsExpectedResult()
    {
        IReadOnlyList<RenkoResult> sut = Bars
            .ToRenkoAtr(20);

        // assertions - different ATR period results in different brick size and count
        sut.Count.Should().BeGreaterThan(0);
        sut.Where(static x => x.IsUp).Should().NotBeEmpty();
        sut.Where(static x => !x.IsUp).Should().NotBeEmpty();
    }

    [TestMethod]
    public void Results_WithDefaults_HaveRequiredValues()
    {
        IReadOnlyList<RenkoResult> results = Bars
            .ToRenkoAtr();

        results.Should().NotBeEmpty();

        foreach (RenkoResult r in results)
        {
            r.Timestamp.Should().BeAfter(DateTime.MinValue);
            r.Open.Should().NotBe(0);
            r.High.Should().NotBe(0);
            r.Low.Should().NotBe(0);
            r.Close.Should().NotBe(0);
            r.High.Should().BeGreaterOrEqualTo(r.Low);
        }
    }

    [TestMethod]
    public void Results_WithAnyInput_AreAlwaysBounded()
    {
        IReadOnlyList<RenkoResult> results = Bars
            .ToRenkoAtr();

        foreach (RenkoResult r in results)
        {
            // Brick values should be positive
            r.Open.Should().BeGreaterThan(0);
            r.High.Should().BeGreaterThan(0);
            r.Low.Should().BeGreaterThan(0);
            r.Close.Should().BeGreaterThan(0);

            // High should be >= Low
            r.High.Should().BeGreaterOrEqualTo(r.Low);
        }
    }

    [TestMethod]
    public void NullHistory_WithNullBars_ThrowsArgumentNullException()
        => FluentActions
            .Invoking(static () => ((IReadOnlyList<IBar>)null!).ToRenkoAtr())
            .Should()
            .ThrowExactly<ArgumentNullException>();

    [TestMethod]
    public override void BadBars_DoesNotFail()
    {
        IReadOnlyList<RenkoResult> sut = BadBars
            .ToRenkoAtr();

        sut.Should().NotBeNull();
        sut.Count.Should().NotBe(0);
    }

    [TestMethod]
    public override void NoBars_ReturnsEmpty()
    {
        IReadOnlyList<Bar> emptyBars = [];

        IReadOnlyList<RenkoResult> sut = emptyBars
            .ToRenkoAtr();

        sut.Should().BeEmpty();
    }

    [TestMethod]
    public void InsufficientHistory_WithTooFewBars_ReturnsEmpty()
    {
        IReadOnlyList<RenkoResult> sut = Bars
            .Take(10)
            .ToList()
            .ToRenkoAtr(14);

        // Not enough history for ATR calculation
        sut.Should().BeEmpty();
    }
}
