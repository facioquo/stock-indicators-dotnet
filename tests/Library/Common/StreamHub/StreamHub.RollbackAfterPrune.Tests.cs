namespace Observables;

/// <summary>
/// Characterizes SR003: a rollback that reaches pruned territory re-seeds a
/// stateful hub's recursive/cumulative state from a truncated history, so the
/// rebuilt result diverges from a continuously-fed oracle.
/// </summary>
/// <remarks>
/// <para>
/// The streaming rollback engine is correct when the provider has NOT pruned
/// the history a rebuild needs — proven by <see cref="AdequateCache_HeadCorrection_MatchesOracleExactly"/>
/// (and by the same-timestamp-correction suite). The defect appears only when
/// <c>maxCacheSize</c> is smaller than the full history AND a correction or
/// late arrival triggers a rebuild that reaches the pruned head: the recursive
/// seed is then rebuilt from retained-but-not-original bars.
/// </para>
/// <para>
/// This is a <b>framework-level</b> property of every stateful hub (EMA and its
/// derivatives, every Wilder hub, and cumulative hubs), not a per-indicator bug.
/// The real fix is a framework state snapshot/restore mechanism tracked as a v3.1
/// architecture item in <c>docs/plans/streaming-indicators.plan.md</c>. Until then
/// the limitation is documented in the streaming guide.
/// </para>
/// <para>
/// <b>When the framework fix lands, invert the divergence assertions below to
/// exact-equality</b> — this class is the acceptance test for that work.
/// </para>
/// </remarks>
[TestClass]
public class RollbackAfterPrune : TestBase
{
    private const int Total = 120;
    private const int SmallCache = 30; // >= each warmup, but << Total so the head prunes

    /// <summary>
    /// Builds subject (stream all, then correct the earliest cached bar) and a
    /// fresh oracle (corrected sequence in order), then counts result mismatches.
    /// </summary>
    private static int CountMismatches<T>(
        int cacheSize,
        Func<BarHub, IReadOnlyList<T>> attach,
        Func<T, double?> selector)
        where T : ISeries
    {
        IReadOnlyList<Bar> originals = Bars.Take(Total).ToList();

        BarHub subjectSource = new(maxCacheSize: cacheSize);
        IReadOnlyList<T> subject = attach(subjectSource);
        subjectSource.Add(originals);

        // correct the earliest still-cached bar (== earliest overall when no prune)
        Bar head = (Bar)subjectSource.Bars[0];
        int headIndex = 0;
        for (int j = 0; j < originals.Count; j++)
        {
            if (originals[j].Timestamp == head.Timestamp) { headIndex = j; break; }
        }

        Bar corrected = head with {
            Open = head.Open * 1.05m,
            High = head.High * 1.05m,
            Low = head.Low * 1.05m,
            Close = head.Close * 1.05m
        };
        subjectSource.Add(corrected);

        List<Bar> correctedSequence = [.. originals];
        correctedSequence[headIndex] = corrected;
        BarHub oracleSource = new(maxCacheSize: cacheSize);
        IReadOnlyList<T> oracle = attach(oracleSource);
        oracleSource.Add(correctedSequence);

        int mismatches = 0;
        int n = Math.Min(subject.Count, oracle.Count);
        for (int k = 0; k < n; k++)
        {
            if (selector(subject[k]) != selector(oracle[k]))
            {
                mismatches++;
            }
        }

        subjectSource.EndTransmission();
        oracleSource.EndTransmission();
        return mismatches;
    }

    [TestMethod]
    public void AdequateCache_HeadCorrection_MatchesOracleExactly()
    {
        // Positive control: when the cache retains the full history (no prune),
        // a correction at the very first bar rebuilds exactly. The rollback
        // engine itself is correct — only pruning breaks it.
        int mismatches = CountMismatches(
            cacheSize: 100_000,
            attach: source => source.ToAtrHub(14).Results,
            selector: r => r.Atr);

        mismatches.Should().Be(0,
            "with the full history retained, a head correction re-seeds from the true warmup");
    }

    [TestMethod]
    public void SmallCachePrune_HeadCorrection_DivergesFromOracle_KnownV31Limitation()
    {
        // KNOWN LIMITATION (SR003): with a pruning cache, a head correction
        // rebuilds the recursive/cumulative seed from truncated history and
        // diverges. Asserted as DIVERGENCE so this flips to a failure (signaling
        // success) when the v3.1 framework snapshot/restore fix lands.
        // Representative across the affected categories: base-recursive (EMA),
        // Wilder (ATR/ADX), cumulative (OBV), and a pure window indicator (SMA).
        CountMismatches(SmallCache, s => s.ToEmaHub(14).Results, r => r.Ema)
            .Should().BeGreaterThan(0, "EMA recursion re-seeds from truncated history after a prune (SR003)");

        CountMismatches(SmallCache, s => s.ToAtrHub(14).Results, r => r.Atr)
            .Should().BeGreaterThan(0, "Wilder ATR re-seeds from truncated history after a prune (SR003)");

        CountMismatches(SmallCache, s => s.ToAdxHub(14).Results, r => r.Adx)
            .Should().BeGreaterThan(0, "Wilder ADX re-seeds from truncated history after a prune (SR003)");

        CountMismatches(SmallCache, s => s.ToObvHub().Results, r => r.Obv)
            .Should().BeGreaterThan(0, "cumulative OBV restarts its running total from truncated history after a prune (SR003)");

        // Pure window indicator: SMA does not drift in value, but its leading
        // results turn null after a post-prune rebuild (the retained cache no
        // longer spans a full lookback) — so it diverges too, contrary to any
        // "window indicators are unaffected" intuition.
        CountMismatches(SmallCache, s => s.ToSmaHub(14).Results, r => r.Sma)
            .Should().BeGreaterThan(0, "SMA loses its leading results to null after a post-prune rebuild (SR003)");
    }
}
