using System.Reflection;

namespace Observables;

/// <summary>
/// Warm-start behaviour every snapshot-capable bar hub must keep. The snapshot contract itself
/// is covered once, in <see cref="WarmStart"/>; this checks each hub's own state encoding.
/// </summary>
/// <typeparam name="TIn">Hub input type.</typeparam>
/// <typeparam name="TResult">Hub result type.</typeparam>
public abstract class WarmStartCases<TIn, TResult> : TestBase
    where TIn : ISeries
    where TResult : ISeries
{
    private const int Split = 120;

    private protected abstract StreamHub<TIn, TResult> Subscribe(BarHub barHub);

    private protected abstract IReadOnlyList<TResult> Series(IReadOnlyList<Bar> bars);

    /// <summary>
    /// Gets restore points inside the hub's warmup, where its state is partly unset.
    /// </summary>
    private protected abstract int[] WarmupSplits { get; }

    /// <summary>
    /// Gets the hub's state version, the first byte of its encoded state.
    /// </summary>
    private protected abstract byte StateVersion { get; }

    /// <summary>
    /// Gets the hub's private state members in their encoded order, which pins the byte layout.
    /// </summary>
    private protected abstract string[] StateMembers { get; }

    /// <summary>
    /// Gets a provider cache limit short enough that a replay from the pruned provider
    /// reaches a different state than the restored rollback ring.
    /// </summary>
    private protected virtual int PrunedCacheSize => 150;

    private (BarHub Bars, StreamHub<TIn, TResult> Hub) Chain(IEnumerable<Bar> bars, int? maxCacheSize = null)
    {
        BarHub barHub = new(maxCacheSize);
        StreamHub<TIn, TResult> hub = Subscribe(barHub);
        barHub.Add(bars.ToList());
        return (barHub, hub);
    }

    private (BarHub Bars, StreamHub<TIn, TResult> Hub, HubSnapshot<TResult> Snapshot) StartFromSnapshot(
        IReadOnlyList<Bar> bars, int split = Split, int? maxCacheSize = null)
    {
        (BarHub source, StreamHub<TIn, TResult> sourceHub) = Chain(bars.Take(split), maxCacheSize);
        HubSnapshot<IBar> barSnapshot = source.CaptureSnapshot();
        HubSnapshot<TResult> snapshot = sourceHub.CaptureSnapshot();

        BarHub barHub = new(maxCacheSize);
        StreamHub<TIn, TResult> hub = Subscribe(barHub);
        barHub.RestoreSnapshot(barSnapshot);
        hub.RestoreSnapshot(snapshot);

        return (barHub, hub, snapshot);
    }

    private static Bar Revise(Bar bar, decimal by = 1m)
        => new(bar.Timestamp, bar.Open, bar.High + by, bar.Low, bar.Close + (by / 2), bar.Volume);

    [TestMethod]
    public void Restore_ResumesBitExact()
    {
        IReadOnlyList<Bar> bars = Bars.Take(200).ToList();

        (BarHub barHub, StreamHub<TIn, TResult> hub, _) = StartFromSnapshot(bars);
        barHub.Add(bars.Skip(Split).ToList());

        hub.Results.IsExactly(Series(bars));
    }

    [TestMethod]
    public void Restore_DuringWarmup_ContinuesBitExact()
    {
        IReadOnlyList<Bar> bars = Bars.Take(100).ToList();

        foreach (int split in WarmupSplits)
        {
            (BarHub barHub, StreamHub<TIn, TResult> hub, HubSnapshot<TResult> snapshot) = StartFromSnapshot(bars, split);

            hub.CaptureSnapshot().State.Should().Equal(snapshot.State, "split {0} re-encodes unchanged", split);

            barHub.Add(bars.Skip(split).ToList());
            hub.Results.IsExactly(Series(bars), "split {0}", split);
        }
    }

    [TestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public void Restore_ThenCorrectNewest_MatchesAnUninterruptedChain(bool pruned)
    {
        // a pruned provider is where a replay from the provider differs from the restored ring
        int? maxCacheSize = pruned ? PrunedCacheSize : null;
        List<Bar> bars = [.. Bars.Take(300)];
        const int split = 220;

        (BarHub uninterruptedBars, StreamHub<TIn, TResult> uninterrupted) = Chain(bars.Take(split), maxCacheSize);
        (BarHub restoredBars, StreamHub<TIn, TResult> restored, _) = StartFromSnapshot(bars, split, maxCacheSize);

        Bar revised = Revise(bars[split - 1]);
        uninterruptedBars.Add(revised);
        restoredBars.Add(revised);

        // compared before more bars arrive: a hub whose state resets on reversal can
        // converge again and prune the difference away
        restored.Results.IsExactly(uninterrupted.Results);

        uninterruptedBars.Add(bars.Skip(split).ToList());
        restoredBars.Add(bars.Skip(split).ToList());

        restored.Results.IsExactly(uninterrupted.Results);
        restoredBars.Cache.Should().Equal(uninterruptedBars.Cache);
        restored.CaptureSnapshot().State.Should().Equal(uninterrupted.CaptureSnapshot().State);
    }

    [TestMethod]
    public void Capture_AfterMidTailCorrection_RestoresAndContinues()
    {
        // a correction behind the newest bar replays later bars, so the ring is no longer ascending
        List<Bar> bars = [.. Bars.Take(200)];

        (BarHub source, StreamHub<TIn, TResult> sourceHub) = Chain(bars.Take(Split));
        bars[Split - 5] = Revise(bars[Split - 5]);
        source.Add(bars[Split - 5]);

        HubSnapshot<TResult> snapshot = sourceHub.CaptureSnapshot();

        BarHub barHub = new();
        StreamHub<TIn, TResult> hub = Subscribe(barHub);
        barHub.RestoreSnapshot(source.CaptureSnapshot());
        hub.RestoreSnapshot(snapshot);

        hub.CaptureSnapshot().State.Should().Equal(snapshot.State);

        // a newest-bar correction now resolves from the restored, non-ascending ring
        bars[Split - 1] = Revise(bars[Split - 1]);
        barHub.Add(bars[Split - 1]);
        barHub.Add(bars.Skip(Split).ToList());

        hub.Results.IsExactly(Series(bars));
    }

    [TestMethod]
    public void Restore_RepeatedNewestUpdates_KeepRollingBack()
    {
        List<Bar> bars = [.. Bars.Take(200)];

        (BarHub barHub, StreamHub<TIn, TResult> hub, _) = StartFromSnapshot(bars);

        for (int tick = 1; tick <= 5; tick++)
        {
            bars[Split - 1] = Revise(bars[Split - 1], tick);
            barHub.Add(bars[Split - 1]);
        }

        barHub.Add(bars.Skip(Split).ToList());

        hub.Results.IsExactly(Series(bars));
    }

    [TestMethod]
    public void Restore_DeepCorrection_FallsBackToReplay()
    {
        List<Bar> bars = [.. Bars.Take(200)];

        (BarHub barHub, StreamHub<TIn, TResult> hub, _) = StartFromSnapshot(bars);
        barHub.Add(bars.Skip(Split).ToList());

        bars[Split - 30] = Revise(bars[Split - 30], 2m);
        barHub.Add(bars[Split - 30]);

        hub.Results.IsExactly(Series(bars));
    }

    [TestMethod]
    public void Restore_IntoUsedHub_ReplacesItsRollbackState()
    {
        // same timestamps, different prices: a stale rollback entry would restore the wrong state
        List<Bar> bars = [.. Bars.Take(200)];
        List<Bar> other = bars.Take(Split)
            .Select(static b => new Bar(b.Timestamp, b.Open + 7m, b.High + 7m, b.Low + 7m, b.Close * 1.3m, b.Volume * 2))
            .ToList();
        (BarHub barHub, StreamHub<TIn, TResult> hub) = Chain(other);

        (BarHub source, StreamHub<TIn, TResult> sourceHub) = Chain(bars.Take(Split));
        barHub.RestoreSnapshot(source.CaptureSnapshot());
        hub.RestoreSnapshot(sourceHub.CaptureSnapshot());

        bars[Split - 1] = Revise(bars[Split - 1]);
        barHub.Add(bars[Split - 1]);
        barHub.Add(bars.Skip(Split).ToList());

        hub.Results.IsExactly(Series(bars));
    }

    [TestMethod]
    public void Restore_TruncatedOrOtherVersionState_Throws()
    {
        (_, StreamHub<TIn, TResult> source) = Chain(Bars.Take(Split));
        HubSnapshot<TResult> snapshot = source.CaptureSnapshot();
        byte[] state = [.. snapshot.State];

        byte[] otherVersion = [.. state];
        otherVersion[0]++;

        // the target holds other prices, so a partly applied state would show
        List<Bar> other = [.. Bars.Take(200)
            .Select(static b => new Bar(b.Timestamp, b.Open + 7m, b.High + 7m, b.Low + 7m, b.Close * 1.3m, b.Volume * 2))];

        foreach (byte[] bad in new[] { state[..^1], [.. state, 0], otherVersion })
        {
            (BarHub targetBars, StreamHub<TIn, TResult> target) = Chain(other.Take(Split));
            byte[] before = target.CaptureSnapshot().State;
            TResult[] cacheBefore = [.. target.Cache];

            Action act = () => target.RestoreSnapshot(snapshot with { State = bad });
            act.Should().ThrowExactly<InvalidDataException>();

            target.CaptureSnapshot().State.Should().Equal(before);
            target.Cache.Should().Equal(cacheBefore);

            targetBars.Add(other.Skip(Split).ToList());
            target.Results.IsExactly(Series(other));
        }
    }

    [TestMethod]
    public void Capture_State_FollowsTheDocumentedLayout()
    {
        (_, StreamHub<TIn, TResult> hub) = Chain(Bars.Take(Split));

        // distinct doubles, and one capture per flag set alone, so swapping any two members shows
        string[] flags = [.. StateMembers.Where(n => ReadMember(hub, n) is bool)];
        foreach (string setFlag in flags.Length == 0 ? [string.Empty] : flags)
        {
            using MemoryStream stream = new();
            using (BinaryWriter w = new(stream))
            {
                w.Write(StateVersion);

                for (int i = 0; i < StateMembers.Length; i++)
                {
                    string name = StateMembers[i];
                    object value = ReadMember(hub, name) switch {
                        bool => name == setFlag,
                        double => 0.5 + i,
                        object other => throw new InvalidOperationException($"{name} is a {other.GetType().Name}.")
                    };

                    WriteMember(hub, name, value);

                    if (value is bool flag)
                    {
                        w.Write(flag);
                    }
                    else
                    {
                        w.Write((double)value);
                    }
                }
            }

            byte[] expected = stream.ToArray();
            byte[] state = hub.CaptureSnapshot().State;

            state[..expected.Length].Should().Equal(expected, "flag {0} set alone", setFlag);

            // the members end where the next count begins, so none is missing from the list
            BitConverter.ToInt32(state, expected.Length).Should().Be(CountOf(ReadMember(hub, CountMemberAfterState)));
        }
    }

    /// <summary>
    /// Gets the member whose entry count the encoding writes right after the state members.
    /// </summary>
    private protected virtual string CountMemberAfterState => "_rollback";

    private static int CountOf(object collection) => collection switch {
        System.Collections.ICollection items => items.Count,
        _ => ((System.Collections.IEnumerable)collection.GetType()
            .GetMethod("Entries", BindingFlags.Instance | BindingFlags.NonPublic)
            .Invoke(collection, null)).Cast<object>().Count()
    };

    private static void WriteMember(object hub, string name, object value)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        Type type = hub.GetType();

        if (type.GetField(name, flags) is FieldInfo field)
        {
            field.SetValue(hub, value);
        }
        else
        {
            type.GetProperty(name, flags).SetValue(hub, value);
        }
    }

    private static object ReadMember(object hub, string name)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        Type type = hub.GetType();

        return type.GetField(name, flags)?.GetValue(hub)
            ?? type.GetProperty(name, flags)?.GetValue(hub)
            ?? throw new InvalidOperationException($"{type.Name} has no member {name}.");
    }
}

[TestClass]
public class RsiWarmStart : WarmStartCases<IReusable, RsiResult>
{
    private const int Lookback = 14;

    private protected override StreamHub<IReusable, RsiResult> Subscribe(BarHub barHub) => barHub.ToRsiHub(Lookback);

    private protected override IReadOnlyList<RsiResult> Series(IReadOnlyList<Bar> bars) => bars.ToRsi(Lookback);

    // empty hub, mid-warmup, the last bar before the seed, the seed
    private protected override int[] WarmupSplits => [0, 10, Lookback - 1, Lookback];

    private protected override byte StateVersion => 2;

    private protected override string[] StateMembers => ["_avgGain", "_avgLoss"];
}

[TestClass]
public class AdxWarmStart : WarmStartCases<IBar, AdxResult>
{
    private const int Lookback = 14;

    private protected override StreamHub<IBar, AdxResult> Subscribe(BarHub barHub) => barHub.ToAdxHub(Lookback);

    private protected override IReadOnlyList<AdxResult> Series(IReadOnlyList<Bar> bars) => bars.ToAdx(Lookback);

    private protected override byte StateVersion => 1;

    private protected override string[] StateMembers => [
        "_isFirstPeriod", "_prevHigh", "_prevLow", "_prevClose", "_prevTrs", "_prevPdm", "_prevMdm",
        "_prevAdx", "_sumTr", "_sumPdm", "_sumMdm", "_sumDx"];

    // first bar, smoothing seed, first ADX, first ADXR
    private protected override int[] WarmupSplits => [0, 1, Lookback, (2 * Lookback) - 1, (3 * Lookback) - 1];
}

[TestClass]
public class KvoWarmStart : WarmStartCases<IBar, KvoResult>
{
    private const int Fast = 34;
    private const int Slow = 55;
    private const int Signal = 13;

    private protected override StreamHub<IBar, KvoResult> Subscribe(BarHub barHub) => barHub.ToKvoHub(Fast, Slow, Signal);

    private protected override IReadOnlyList<KvoResult> Series(IReadOnlyList<Bar> bars) => bars.ToKvo(Fast, Slow, Signal);

    private protected override byte StateVersion => 1;

    private protected override string[] StateMembers => [
        "_prevHlc", "_prevTrend", "_prevDm", "_prevCm", "_prevVfFastEma", "_prevVfSlowEma", "_sumVf"];

    // empty hub, HLC basis, trend seed, first volume force, then just after the fast, slow and signal seeds
    private protected override int[] WarmupSplits => [0, 1, 2, 3, Fast + 2, Slow + 2, Slow + Signal + 1];
}

[TestClass]
public class ParabolicSarWarmStart : WarmStartCases<IBar, ParabolicSarResult>
{
    private protected override StreamHub<IBar, ParabolicSarResult> Subscribe(BarHub barHub) => barHub.ToParabolicSarHub();

    private protected override IReadOnlyList<ParabolicSarResult> Series(IReadOnlyList<Bar> bars) => bars.ToParabolicSar();

    private protected override byte StateVersion => 1;

    private protected override string[] StateMembers => [
        "_accelerationFactor", "_extremePoint", "_priorSar", "_isRising", "_isInitialized", "_firstReversalFound"];

    // the prior-bar buffer count follows the members, ahead of the ring
    private protected override string CountMemberAfterState => "_buffer";

    // empty hub, first bar, two bars buffered, the buffer rolling, the last bar before the first
    // reversal (AF at its maximum in this data), then the first falling trend, which a lost
    // direction flag would continue as rising
    private protected override int[] WarmupSplits => [0, 1, 2, 3, FirstFallingSplit() - 1, FirstFallingSplit()];

    // a reversal resets the state, so the replay window must not reach one
    private protected override int PrunedCacheSize => 5;

    private static int FirstFallingSplit()
    {
        IReadOnlyList<Bar> bars = Bars.Take(100).ToList();
        IReadOnlyList<ParabolicSarResult> series = bars.ToParabolicSar();

        for (int i = 2; i < series.Count; i++)
        {
            if (series[i].Sar > (double)bars[i].High)
            {
                return i + 1;
            }
        }

        throw new InvalidOperationException("Test data has no falling trend.");
    }

    [TestMethod]
    [DataRow(-1)]
    [DataRow(3)]
    public void Restore_PriorBarCountOutOfRange_Throws(int count)
    {
        BarHub barHub = new();
        ParabolicSarHub hub = barHub.ToParabolicSarHub();
        barHub.Add(Bars.Take(50).ToList());
        HubSnapshot<ParabolicSarResult> snapshot = hub.CaptureSnapshot();

        // version, three doubles and three flags precede the prior-bar count
        const int countOffset = 1 + (3 * sizeof(double)) + 3;
        byte[] state = [.. snapshot.State];
        BitConverter.ToInt32(state, countOffset).Should().Be(2);
        BitConverter.GetBytes(count).CopyTo(state, countOffset);

        // one more well-formed pair, so only the count bound can reject it
        const int ringOffset = countOffset + sizeof(int) + (2 * 2 * sizeof(double));
        byte[] bad = [.. state[..ringOffset], .. new byte[2 * sizeof(double)], .. state[ringOffset..]];

        Action act = () => hub.RestoreSnapshot(snapshot with { State = bad });
        act.Should().ThrowExactly<InvalidDataException>();
    }
}

[TestClass]
public class SuperTrendWarmStart : WarmStartCases<IBar, SuperTrendResult>
{
    private const int Lookback = 10;

    private protected override StreamHub<IBar, SuperTrendResult> Subscribe(BarHub barHub) => barHub.ToSuperTrendHub(Lookback);

    private protected override IReadOnlyList<SuperTrendResult> Series(IReadOnlyList<Bar> bars) => bars.ToSuperTrend(Lookback);

    private protected override byte StateVersion => 1;

    private protected override string[] StateMembers => ["IsBullish", "UpperBand", "LowerBand", "PrevAtr"];

    // warmup with no band yet, the ATR seed, the first smoothed ATR, the last bullish bar before the
    // first flip, then the first bearish trend, which a lost direction flag would continue as bullish
    private protected override int[] WarmupSplits
        => [Lookback, Lookback + 1, Lookback + 2, FirstBearishSplit() - 1, FirstBearishSplit()];

    private static int FirstBearishSplit()
    {
        IReadOnlyList<SuperTrendResult> series = Bars.Take(100).ToList().ToSuperTrend(Lookback);

        for (int i = Lookback; i < series.Count; i++)
        {
            if (series[i].UpperBand is not null)
            {
                return i + 1;
            }
        }

        throw new InvalidOperationException("Test data has no bearish trend.");
    }
}
