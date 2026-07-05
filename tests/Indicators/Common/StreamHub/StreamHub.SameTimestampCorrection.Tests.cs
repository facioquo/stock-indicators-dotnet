namespace Observables;

/// <summary>
/// Pins the "same-timestamp value correction" invariant for the standalone
/// <see cref="BarHub"/>: re-adding a bar with an existing timestamp but
/// revised values must rebuild dependent hubs to exactly match a fresh hub fed
/// the corrected sequence in order — even when the corrected bar sits below the
/// subscriber's <c>MinCacheSize</c> (inside the warmup window).
/// </summary>
/// <remarks>
/// The existing <c>UpdateBarWithSameTimestamp</c> test only drives a stateless
/// <c>BarPartHub</c> at index 0 (where MinCacheSize is 0). These tests place the
/// correction at an early index that is genuinely below MinCacheSize for each
/// stateful hub, so they exercise the standalone-replacement path rather than the
/// stateless tail case.
/// </remarks>
[TestClass]
public class SameTimestampCorrection : TestBase
{
    private const int TotalBars = 300;

    // Correct an early bar. It is below MinCacheSize for every hub exercised here
    // (Sma 20, Atr 15, AtrStop 22, Stoch 14), so it lands in the warmup window.
    private const int CorrectionIndex = 5;

    private static void AssertSubWarmupCorrectionMatchesFresh<TResult>(
        Func<BarHub, IReadOnlyList<TResult>> attach)
        where TResult : ISeries
    {
        IReadOnlyList<Bar> originals = Bars.Take(TotalBars).ToList();

        // Build the corrected sequence: scale one early bar so its values differ
        // while OHLC ordering stays valid (all components scaled by the same factor).
        Bar original = originals[CorrectionIndex];
        Bar corrected = original with {
            Open = original.Open * 1.05m,
            High = original.High * 1.05m,
            Low = original.Low * 1.05m,
            Close = original.Close * 1.05m
        };

        List<Bar> correctedSequence = [.. originals];
        correctedSequence[CorrectionIndex] = corrected;

        // Streamed root: feed the originals in order, then re-add the correction at
        // its (early, sub-MinCacheSize) timestamp to trigger an in-place rebuild.
        BarHub streamedSource = new();
        IReadOnlyList<TResult> streamed = attach(streamedSource);
        streamedSource.Add(originals);
        streamedSource.Add(corrected);

        // Fresh oracle: feed the corrected sequence in chronological order.
        BarHub freshSource = new();
        IReadOnlyList<TResult> fresh = attach(freshSource);
        freshSource.Add(correctedSequence);

        streamed.Should().HaveCount(fresh.Count);
        streamed.IsExactly(fresh,
            "a same-timestamp correction below MinCacheSize must rebuild like a fresh hub");

        streamedSource.EndTransmission();
        freshSource.EndTransmission();
    }

    [TestMethod]
    public void Sma_SubWarmupCorrection_MatchesFreshStream()
        => AssertSubWarmupCorrectionMatchesFresh(source => source.ToSmaHub(20).Results);

    [TestMethod]
    public void Atr_SubWarmupCorrection_MatchesFreshStream()
        => AssertSubWarmupCorrectionMatchesFresh(source => source.ToAtrHub(14).Results);

    [TestMethod]
    public void AtrStop_SubWarmupCorrection_MatchesFreshStream()
        => AssertSubWarmupCorrectionMatchesFresh(source => source.ToAtrStopHub().Results);

    [TestMethod]
    public void Stoch_SubWarmupCorrection_MatchesFreshStream()
        => AssertSubWarmupCorrectionMatchesFresh(source => source.ToStochHub(14, 3, 3).Results);
}
