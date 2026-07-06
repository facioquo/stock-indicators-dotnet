namespace StaticSeries;

[TestClass]
public class StochSeriesTests : StaticSeriesTestBase
{
    /// <summary>
    /// Slow
    /// </summary>
    [TestMethod]
    public override void DefaultParameters_ReturnsExpectedResults()
    {
        const int lookbackPeriods = 14;
        const int signalPeriods = 3;
        const int smoothPeriods = 3;

        IReadOnlyList<StochResult> sut = Bars
            .ToStoch(lookbackPeriods, signalPeriods, smoothPeriods);

        // proper quantities
        sut.Should().HaveCount(502);
        sut.Where(static x => x.Oscillator != null).Should().HaveCount(487);
        sut.Where(static x => x.Signal != null).Should().HaveCount(485);

        // sample values
        StochResult r15 = sut[15];
        r15.Oscillator.Should().BeApproximately(81.1253, Money4);
        r15.Signal.Should().BeNull();
        r15.PercentJ.Should().BeNull();

        StochResult r17 = sut[17];
        r17.Oscillator.Should().BeApproximately(92.1307, Money4);
        r17.Signal.Should().BeApproximately(88.4995, Money4);
        r17.PercentJ.Should().BeApproximately(99.3929, Money4);

        StochResult r149 = sut[149];
        r149.Oscillator.Should().BeApproximately(81.6870, Money4);
        r149.Signal.Should().BeApproximately(79.7935, Money4);
        r149.PercentJ.Should().BeApproximately(85.4741, Money4);

        StochResult r249 = sut[249];  // also testing aliases here
        r249.K.Should().BeApproximately(83.2020, Money4);
        r249.D.Should().BeApproximately(83.0813, Money4);
        r249.J.Should().BeApproximately(83.4435, Money4);

        StochResult r501 = sut[501];
        r501.Oscillator.Should().BeApproximately(43.1353, Money4);
        r501.Signal.Should().BeApproximately(35.5674, Money4);
        r501.PercentJ.Should().BeApproximately(58.2712, Money4);
    }

    [TestMethod]
    public void Results_AreAlwaysBounded()
    {
        IReadOnlyList<StochResult> sut = Bars.ToStoch(14, 3, 3);
        sut.IsBetween(static x => x.Oscillator, 0, 100);
        sut.IsBetween(static x => x.Signal, 0, 100);
    }

    /// <summary>
    /// with extra parameters
    /// </summary>
    [TestMethod]
    public void Extended_WithExtraParameters_ReturnsExpectedResult()
    {
        IReadOnlyList<StochResult> sut =
            Bars.ToStoch(9, 3, 3, 5, 4, MaType.SMMA);

        // proper quantities
        sut.Should().HaveCount(502);
        sut.Where(static x => x.K != null).Should().HaveCount(492);
        sut.Where(static x => x.D != null).Should().HaveCount(490);

        // sample values
        StochResult r9 = sut[9];
        r9.K.Should().BeNull();
        r9.D.Should().BeNull();
        r9.J.Should().BeNull();

        StochResult r12 = sut[12];
        r12.K.Should().BeApproximately(59.7656, Money4);
        r12.D.Should().BeApproximately(59.4459, Money4);
        r12.J.Should().BeApproximately(61.0445, Money4);

        StochResult r17 = sut[17];
        r17.K.Should().BeApproximately(82.2852, Money4);
        r17.D.Should().BeApproximately(74.9715, Money4);
        r17.J.Should().BeApproximately(111.5401, Money4);

        StochResult r149 = sut[149];
        r149.K.Should().BeApproximately(77.1571, Money4);
        r149.D.Should().BeApproximately(72.8206, Money4);
        r149.J.Should().BeApproximately(94.5030, Money4);

        StochResult r249 = sut[249];  // also testing aliases here
        r249.K.Should().BeApproximately(74.3652, Money4);
        r249.D.Should().BeApproximately(75.5660, Money4);
        r249.J.Should().BeApproximately(69.5621, Money4);

        StochResult r501 = sut[501];
        r501.K.Should().BeApproximately(46.9807, Money4);
        r501.D.Should().BeApproximately(32.0413, Money4);
        r501.J.Should().BeApproximately(106.7382, Money4);
    }

    [TestMethod]
    public void ChainFromResults_ToSma_ReturnsExpectedResult()
    {
        IReadOnlyList<SmaResult> sut = Bars
            .ToStoch()
            .ToSma(10);

        sut.Should().HaveCount(502);
        sut.Where(static x => x.Sma != null).Should().HaveCount(478);
    }

    [TestMethod]
    public void NoSignal_WithSignalPeriodOne_EqualsOscillator()
    {
        const int lookbackPeriods = 5;
        const int signalPeriods = 1;
        const int smoothPeriods = 3;

        IReadOnlyList<StochResult> sut = Bars
            .ToStoch(lookbackPeriods, signalPeriods, smoothPeriods);

        // signal equals oscillator
        StochResult r1 = sut[487];
        r1.Signal.Should().Be(r1.Oscillator);

        StochResult r2 = sut[501];
        r2.Signal.Should().Be(r2.Oscillator);
    }

    [TestMethod]
    public void Fast_WithSmoothPeriodOne_ReturnsFastStochastic()
    {
        const int lookbackPeriods = 5;
        const int signalPeriods = 10;
        const int smoothPeriods = 1;

        IReadOnlyList<StochResult> sut = Bars
            .ToStoch(lookbackPeriods, signalPeriods, smoothPeriods);

        // sample values
        StochResult r1 = sut[487];
        r1.Oscillator.Should().BeApproximately(25.0353, Money4);
        r1.Signal.Should().BeApproximately(60.5706, Money4);

        StochResult r2 = sut[501];
        r2.Oscillator.Should().BeApproximately(91.6233, Money4);
        r2.Signal.Should().BeApproximately(36.0608, Money4);
    }

    [TestMethod]
    public void FastSmall_WithMinimalLookback_ReturnsExpectedResult()
    {
        const int lookbackPeriods = 1;
        const int signalPeriods = 10;
        const int smoothPeriods = 1;

        IReadOnlyList<StochResult> sut = Bars
            .ToStoch(lookbackPeriods, signalPeriods, smoothPeriods);

        // sample values
        StochResult r1 = sut[70];
        r1.Oscillator.Should().Be(0);

        StochResult r2 = sut[71];
        r2.Oscillator.Should().Be(100);
    }

    [TestMethod]
    public override void BadBars_DoesNotFail()
    {
        IReadOnlyList<StochResult> r = BadBars
            .ToStoch(15);

        r.Should().HaveCount(502);
        r.Where(static x => x.Oscillator is double v && double.IsNaN(v)).Should().BeEmpty();

    }

    [TestMethod]
    public override void NoBars_ReturnsEmpty()
    {
        IReadOnlyList<StochResult> r0 = Nobars
            .ToStoch();

        r0.Should().BeEmpty();

        IReadOnlyList<StochResult> r1 = Onebar
            .ToStoch();

        r1.Should().HaveCount(1);
    }

    [TestMethod]
    public void Removed_WithWarmupPeriods_TruncatesResults()
    {
        const int lookbackPeriods = 14;
        const int signalPeriods = 3;
        const int smoothPeriods = 3;

        IReadOnlyList<StochResult> sut = Bars
            .ToStoch(lookbackPeriods, signalPeriods, smoothPeriods)
            .RemoveWarmupPeriods();

        // assertions
        sut.Should().HaveCount(502 - (lookbackPeriods + smoothPeriods - 2));

        StochResult last = sut[^1];
        last.Oscillator.Should().BeApproximately(43.1353, Money4);
        last.Signal.Should().BeApproximately(35.5674, Money4);
        last.PercentJ.Should().BeApproximately(58.2712, Money4);
    }

    [TestMethod]
    public void Boundary_WithRandomBars_StaysWithinBounds()
    {
        const int lookbackPeriods = 14;
        const int signalPeriods = 3;
        const int smoothPeriods = 3;

        IReadOnlyList<StochResult> sut = Data
            .GetRandom(2500)
            .ToStoch(lookbackPeriods, signalPeriods, smoothPeriods);

        // test boundary condition

        sut.IsBetween(static x => x.Oscillator, 0d, 100d);
        sut.IsBetween(static x => x.Signal, 0d, 100d);
    }

    [TestMethod]
    public void Issue1127_BoundaryThreshold_Maintained()
    {
        // initialize
        IReadOnlyList<Bar> bars = Data.BarsFromCsv("_issue1127.williamr.revisit.csv");

        // get indicators (using Fast Stochastic parameters to match Williams %R)
        IReadOnlyList<StochResult> sut = bars
            .ToStoch(14, 1, 1);  // Fast Stochastic matches Williams %R formula

        // analyze boundary
        sut.Should().HaveCountGreaterThan(0);
        sut.IsBetween(static x => x.Oscillator, 0d, 100d);
        sut.IsBetween(static x => x.Signal, 0d, 100d);
    }

    [TestMethod]
    public void SmmaReinitialization_WithNanValues()
    {
        // Test SMMA re-initialization logic when NaN values are encountered
        // This verifies that SMMA correctly initializes with SMA (not just current value)
        // when prevK or prevD becomes NaN during processing

        IReadOnlyList<StochResult> sut = BadBars
            .ToStoch(14, 3, 3, 3, 2, MaType.SMMA);

        // Should produce valid results without NaN propagation
        sut.Should().HaveCount(502);
        sut.Where(static x => x.Oscillator is double v && double.IsNaN(v)).Should().BeEmpty();
        sut.Where(static x => x.Signal is double v && double.IsNaN(v)).Should().BeEmpty();

        // Verify some results are calculated (not all null)
        sut.Where(static x => x.Oscillator != null).Should().NotBeEmpty();
        sut.Where(static x => x.Signal != null).Should().NotBeEmpty();
    }

    [TestMethod]
    public void Exceptions_InvalidParameters_ThrowsArgumentOutOfRangeException()
    {
        // bad lookback period
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(
            static () => Bars.ToStoch(0));

        // bad signal period
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(
            static () => Bars.ToStoch(14, 0));

        // bad smoothing period
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(
            static () => Bars.ToStoch(14, 3, 0));

        // bad kFactor
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(
            static () => Bars.ToStoch(9, 3, 1, 0, 2, MaType.SMA));

        // bad dFactor
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(
            static () => Bars.ToStoch(9, 3, 1, 3, 0, MaType.SMA));

        // bad MA type
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(
            static () => Bars.ToStoch(9, 3, 3, 3, 2, MaType.ALMA));
    }
}
