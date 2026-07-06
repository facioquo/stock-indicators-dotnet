namespace Precision;

/// <summary>
/// Precision analysis tests using synthetic boundary data designed to expose
/// floating-point precision issues at mathematical limits.
/// These tests complement Results_AreAlwaysBounded tests by using pathological
/// edge cases (monotonic sequences, exact boundary conditions) rather than
/// normal market data. Both test types serve distinct purposes:
/// - BoundaryTests: Exposes precision vulnerabilities using extreme synthetic data
/// - Results_AreAlwaysBounded: Validates bounds using realistic market data
/// </summary>
[TestClass]
public class BoundaryTests : TestBase
{
    /// <summary>
    /// Tests RSI with monotonically increasing prices.
    /// After warmup, RSI should approach but not exceed 100.
    /// </summary>
    [TestMethod]
    public void RSI_WithMonotonicallyIncreasingPrices_StaysBelow100()
    {
        // Arrange
        IReadOnlyList<Bar> bars = BoundaryBars.GetMonotonicallyIncreasing(100);

        // Act
        IReadOnlyList<RsiResult> seriesResults = bars.ToRsi(14);
        IReadOnlyList<RsiResult> bufferResults = new RsiList(14, bars);
        IReadOnlyList<RsiResult> streamResults = bars.ToRsiHub(14).Results;

        // Analyze
        AnalyzeBounds("RSI Series (increasing)", seriesResults, static x => x.Rsi, 0, 100);
        AnalyzeBounds("RSI Buffer (increasing)", bufferResults, static x => x.Rsi, 0, 100);
        AnalyzeBounds("RSI Stream (increasing)", streamResults, static x => x.Rsi, 0, 100);
    }

    /// <summary>
    /// Tests RSI with monotonically decreasing prices.
    /// After warmup, RSI should approach but not go below 0.
    /// </summary>
    [TestMethod]
    public void RSI_WithMonotonicallyDecreasingPrices_StaysAbove0()
    {
        // Arrange
        IReadOnlyList<Bar> bars = BoundaryBars.GetMonotonicallyDecreasing(100);

        // Act
        IReadOnlyList<RsiResult> seriesResults = bars.ToRsi(14);
        IReadOnlyList<RsiResult> bufferResults = new RsiList(14, bars);
        IReadOnlyList<RsiResult> streamResults = bars.ToRsiHub(14).Results;

        // Analyze
        AnalyzeBounds("RSI Series (decreasing)", seriesResults, static x => x.Rsi, 0, 100);
        AnalyzeBounds("RSI Buffer (decreasing)", bufferResults, static x => x.Rsi, 0, 100);
        AnalyzeBounds("RSI Stream (decreasing)", streamResults, static x => x.Rsi, 0, 100);
    }

    /// <summary>
    /// Tests Stoch with Close = High scenario.
    /// Stochastic should be exactly 100 when close equals highest high.
    /// </summary>
    [TestMethod]
    public void Stoch_WithCloseEqualsHigh_StaysBelow100()
    {
        // Arrange
        IReadOnlyList<Bar> bars = BoundaryBars.GetCloseEqualsHigh(100);

        // Using minimal smoothing/signal periods to test raw oscillator boundary
        const int lookbackPeriods = 14;
        const int signalPeriods = 1;
        const int smoothPeriods = 1;

        // Act
        IReadOnlyList<StochResult> seriesResults = bars.ToStoch(lookbackPeriods, signalPeriods, smoothPeriods);
        IReadOnlyList<StochResult> bufferResults = new StochList(
            lookbackPeriods, signalPeriods, smoothPeriods,
            kFactor: 1, dFactor: 1, movingAverageType: MaType.SMA, bars);
        IReadOnlyList<StochResult> streamResults = bars.ToStochHub(lookbackPeriods, signalPeriods, smoothPeriods).Results;

        // Analyze
        AnalyzeBounds("Stoch Series (close=high)", seriesResults, static x => x.Oscillator, 0, 100);
        AnalyzeBounds("Stoch Buffer (close=high)", bufferResults, static x => x.Oscillator, 0, 100);
        AnalyzeBounds("Stoch Stream (close=high)", streamResults, static x => x.Oscillator, 0, 100);
    }

    /// <summary>
    /// Tests Stoch with Close = Low scenario.
    /// Stochastic should be exactly 0 when close equals lowest low.
    /// </summary>
    [TestMethod]
    public void Stoch_WithCloseEqualsLow_StaysAbove0()
    {
        // Arrange
        IReadOnlyList<Bar> bars = BoundaryBars.GetCloseEqualsLow(100);

        // Using minimal smoothing/signal periods to test raw oscillator boundary
        const int lookbackPeriods = 14;
        const int signalPeriods = 1;
        const int smoothPeriods = 1;

        // Act
        IReadOnlyList<StochResult> seriesResults = bars.ToStoch(lookbackPeriods, signalPeriods, smoothPeriods);
        IReadOnlyList<StochResult> bufferResults = new StochList(
            lookbackPeriods, signalPeriods, smoothPeriods,
            kFactor: 1, dFactor: 1, movingAverageType: MaType.SMA, bars);
        IReadOnlyList<StochResult> streamResults = bars.ToStochHub(lookbackPeriods, signalPeriods, smoothPeriods).Results;

        // Analyze
        AnalyzeBounds("Stoch Series (close=low)", seriesResults, static x => x.Oscillator, 0, 100);
        AnalyzeBounds("Stoch Buffer (close=low)", bufferResults, static x => x.Oscillator, 0, 100);
        AnalyzeBounds("Stoch Stream (close=low)", streamResults, static x => x.Oscillator, 0, 100);
    }

    /// <summary>
    /// Tests StochRSI which compounds precision issues from both RSI and Stoch.
    /// This is currently failing in the main test suite.
    /// </summary>
    [TestMethod]
    public void StochRSI_WithMonotonicallyIncreasingPrices_StaysWithinBounds()
    {
        // Arrange
        IReadOnlyList<Bar> bars = BoundaryBars.GetMonotonicallyIncreasing(100);

        // Act
        IReadOnlyList<StochRsiResult> seriesResults = bars.ToStochRsi(14, 14, 3, 1);
        IReadOnlyList<StochRsiResult> bufferResults = new StochRsiList(14, 14, 3, 1, bars);
        IReadOnlyList<StochRsiResult> streamResults = bars.ToStochRsiHub(14, 14, 3, 1).Results;

        // Analyze
        AnalyzeBounds("StochRSI Series (increasing)", seriesResults, static x => x.StochRsi, 0, 100);
        AnalyzeBounds("StochRSI Buffer (increasing)", bufferResults, static x => x.StochRsi, 0, 100);
        AnalyzeBounds("StochRSI Stream (increasing)", streamResults, static x => x.StochRsi, 0, 100);
    }

    /// <summary>
    /// Tests CMO with alternating prices.
    /// </summary>
    [TestMethod]
    public void CMO_WithAlternatingPrices_StaysWithinBounds()
    {
        // Arrange
        IReadOnlyList<Bar> bars = BoundaryBars.GetAlternating(100);

        // Act
        IReadOnlyList<CmoResult> seriesResults = bars.ToCmo(14);
        IReadOnlyList<CmoResult> bufferResults = new CmoList(14, bars);
        IReadOnlyList<CmoResult> streamResults = bars.ToCmoHub(14).Results;

        // Analyze
        AnalyzeBounds("CMO Series (alternating)", seriesResults, static x => x.Cmo, -100, 100);
        AnalyzeBounds("CMO Buffer (alternating)", bufferResults, static x => x.Cmo, -100, 100);
        AnalyzeBounds("CMO Stream (alternating)", streamResults, static x => x.Cmo, -100, 100);
    }

    /// <summary>
    /// Tests MFI with monotonically increasing prices.
    /// </summary>
    [TestMethod]
    public void MFI_WithMonotonicallyIncreasingPrices_StaysWithinBounds()
    {
        // Arrange
        IReadOnlyList<Bar> bars = BoundaryBars.GetMonotonicallyIncreasing(100);

        // Act
        IReadOnlyList<MfiResult> seriesResults = bars.ToMfi(14);
        IReadOnlyList<MfiResult> bufferResults = new MfiList(14, bars);
        IReadOnlyList<MfiResult> streamResults = bars.ToMfiHub(14).Results;

        // Analyze
        AnalyzeBounds("MFI Series (increasing)", seriesResults, static x => x.Mfi, 0, 100);
        AnalyzeBounds("MFI Buffer (increasing)", bufferResults, static x => x.Mfi, 0, 100);
        AnalyzeBounds("MFI Stream (increasing)", streamResults, static x => x.Mfi, 0, 100);
    }

    /// <summary>
    /// Tests WilliamsR with Close = High scenario.
    /// </summary>
    [TestMethod]
    public void WilliamsR_WithCloseEqualsHigh_StaysWithinBounds()
    {
        // Arrange
        IReadOnlyList<Bar> bars = BoundaryBars.GetCloseEqualsHigh(100);

        // Act
        IReadOnlyList<WilliamsResult> seriesResults = bars.ToWilliamsR(14);
        IReadOnlyList<WilliamsResult> bufferResults = new WilliamsRList(14, bars);
        IReadOnlyList<WilliamsResult> streamResults = bars.ToWilliamsRHub(14).Results;

        // Analyze
        AnalyzeBounds("WilliamsR Series (close=high)", seriesResults, static x => x.WilliamsR, -100, 0);
        AnalyzeBounds("WilliamsR Buffer (close=high)", bufferResults, static x => x.WilliamsR, -100, 0);
        AnalyzeBounds("WilliamsR Stream (close=high)", streamResults, static x => x.WilliamsR, -100, 0);
    }

    /// <summary>
    /// Tests TSI with monotonically increasing prices.
    /// </summary>
    [TestMethod]
    public void TSI_WithMonotonicallyIncreasingPrices_StaysWithinBounds()
    {
        // Arrange
        IReadOnlyList<Bar> bars = BoundaryBars.GetMonotonicallyIncreasing(100);

        // Act
        IReadOnlyList<TsiResult> seriesResults = bars.ToTsi(25, 13, 7);
        IReadOnlyList<TsiResult> bufferResults = new TsiList(25, 13, 7, bars);
        IReadOnlyList<TsiResult> streamResults = bars.ToTsiHub(25, 13, 7).Results;

        // Analyze
        AnalyzeBounds("TSI Series (increasing)", seriesResults, static x => x.Tsi, -100, 100);
        AnalyzeBounds("TSI Buffer (increasing)", bufferResults, static x => x.Tsi, -100, 100);
        AnalyzeBounds("TSI Stream (increasing)", streamResults, static x => x.Tsi, -100, 100);
    }

    /// <summary>
    /// Analyzes bounds violations and outputs diagnostic information.
    /// </summary>
    private static void AnalyzeBounds<T>(
        string name,
        IEnumerable<T> results,
        Func<T, double?> selector,
        double minBound,
        double maxBound)
    {
        double? minValue = null;
        double? maxValue = null;
        int belowMinCount = 0;
        int aboveMaxCount = 0;
        double worstBelowMin = 0;
        double worstAboveMax = 0;

        foreach (T result in results)
        {
            double? value = selector(result);

            if (!value.HasValue || double.IsNaN(value.Value))
            {
                continue;
            }

            double v = value.Value;

            if (!minValue.HasValue || v < minValue.Value)
            {
                minValue = v;
            }

            if (!maxValue.HasValue || v > maxValue.Value)
            {
                maxValue = v;
            }

            if (v < minBound)
            {
                belowMinCount++;
                double diff = minBound - v;
                if (diff > worstBelowMin)
                {
                    worstBelowMin = diff;
                }
            }

            if (v > maxBound)
            {
                aboveMaxCount++;
                double diff = v - maxBound;
                if (diff > worstAboveMax)
                {
                    worstAboveMax = diff;
                }
            }
        }

        // Output diagnostic information
        Console.WriteLine($"\n=== {name} ===");
        Console.WriteLine($"  Range: [{minValue:G17} to {maxValue:G17}]");
        Console.WriteLine($"  Expected bounds: [{minBound} to {maxBound}]");

        if (belowMinCount > 0)
        {
            Console.WriteLine($"  VIOLATION: {belowMinCount} values below min by up to {worstBelowMin:E}");
        }

        if (aboveMaxCount > 0)
        {
            Console.WriteLine($"  VIOLATION: {aboveMaxCount} values above max by up to {worstAboveMax:E}");
        }

        if (belowMinCount == 0 && aboveMaxCount == 0)
        {
            Console.WriteLine("  PASS: All values within bounds");
        }

        // Assert bounds - these assertions enforce that indicators stay within mathematical limits.
        // If an indicator has precision issues, these will fail and document the exact violation.
        if (minValue.HasValue)
        {
            minValue.Value.Should().BeGreaterThanOrEqualTo(minBound,
                $"{name} minimum value {minValue.Value:G17} is below bound {minBound}");
        }

        if (maxValue.HasValue)
        {
            maxValue.Value.Should().BeLessThanOrEqualTo(maxBound,
                $"{name} maximum value {maxValue.Value:G17} is above bound {maxBound}");
        }
    }
}
