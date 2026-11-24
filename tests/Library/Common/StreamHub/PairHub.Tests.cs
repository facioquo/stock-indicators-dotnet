namespace StreamHubs;

/// <summary>
/// Pins <see cref="PairHub"/> against a Series oracle: the hub's results must equal
/// the two Series results joined on the timestamps both hold, whatever order the
/// two sides arrive in.
/// </summary>
[TestClass]
public class PairHubTests : TestBase
{
    private const int TotalBars = 200;
    private const int SkipIndex = 50;

    private static readonly IReadOnlyList<Bar> Feed = Bars.Take(TotalBars).ToList();

    [TestMethod]
    public void SingleRoot_Diamond_MatchesSeriesExactly()
    {
        BarHub barHub = new();
        PairHub pair = new(barHub.ToSmaHub(5), barHub.ToEmaHub(10));

        barHub.Add(Feed);

        pair.Results.Should().HaveCount(TotalBars);
        pair.Results.IsExactly(Join(Feed.ToSma(5), Feed.ToEma(10)));

        barHub.EndTransmission();
    }

    [TestMethod]
    public void SingleRoot_WarmStart_MatchesSeriesExactly()
    {
        BarHub barHub = new();
        SmaHub sma = barHub.ToSmaHub(5);
        EmaHub ema = barHub.ToEmaHub(10);
        barHub.Add(Feed.Take(120));

        PairHub pair = new(sma, ema);
        barHub.Add(Feed.Skip(120));

        pair.Results.IsExactly(Join(Feed.ToSma(5), Feed.ToEma(10)));

        barHub.EndTransmission();
    }

    [TestMethod]
    public void TwoRoots_SourceFirst_MatchesSeriesExactly()
        => AssertTwoRoots(static (source, other, bar) => {
            source.Add(bar);
            other.Add(bar);
        });

    [TestMethod]
    public void TwoRoots_OtherFirst_MatchesSeriesExactly()
        => AssertTwoRoots(static (source, other, bar) => {
            other.Add(bar);
            source.Add(bar);
        });

    [TestMethod]
    public void TwoRoots_SourceFarAhead_MatchesSeriesExactly()
    {
        BarHub source = new();
        BarHub other = new();
        PairHub pair = new(source, other);

        source.Add(Feed);
        pair.Results.Should().BeEmpty();

        other.Add(Feed);
        pair.Results.IsExactly(Join(Feed, Feed));

        source.EndTransmission();
        other.EndTransmission();
    }

    [TestMethod]
    public void OtherSkipsTimestamp_NoPairUntilLateArrival()
    {
        List<Bar> withGap = WithoutIndex(SkipIndex);

        BarHub source = new();
        BarHub other = new();
        PairHub pair = new(source.ToSmaHub(5), other.ToEmaHub(10));

        source.Add(Feed);
        other.Add(withGap);

        // the unmatched timestamp is skipped, not paired with a neighbor
        pair.Results.Should().HaveCount(TotalBars - 1);
        pair.Results.Should().NotContain(x => x.Timestamp == Feed[SkipIndex].Timestamp);
        pair.Results.IsExactly(Join(Feed.ToSma(5), withGap.ToEma(10)));

        // the late value is paired through the usual rebuild
        other.Add(Feed[SkipIndex]);
        pair.Results.IsExactly(Join(Feed.ToSma(5), Feed.ToEma(10)));

        source.EndTransmission();
        other.EndTransmission();
    }

    [TestMethod]
    public void SourceSkipsTimestamp_NoPairUntilLateArrival()
    {
        List<Bar> withGap = WithoutIndex(SkipIndex);

        BarHub source = new();
        BarHub other = new();
        PairHub pair = new(source.ToSmaHub(5), other.ToEmaHub(10));

        other.Add(Feed);
        source.Add(withGap);

        pair.Results.IsExactly(Join(withGap.ToSma(5), Feed.ToEma(10)));

        source.Add(Feed[SkipIndex]);
        pair.Results.IsExactly(Join(Feed.ToSma(5), Feed.ToEma(10)));

        source.EndTransmission();
        other.EndTransmission();
    }

    [TestMethod]
    public void Removal_EitherSide_RemovesPair()
    {
        List<Bar> withGap = WithoutIndex(SkipIndex);

        BarHub source = new();
        BarHub other = new();
        PairHub pair = new(source, other);
        source.Add(Feed);
        other.Add(Feed);

        source.RemoveAt(SkipIndex);
        pair.Results.IsExactly(Join(withGap, Feed));

        other.RemoveAt(SkipIndex + 1);
        pair.Results.IsExactly(Join(withGap, WithoutIndex(SkipIndex + 1)));

        source.EndTransmission();
        other.EndTransmission();
    }

    [TestMethod]
    public void SameTimestampCorrection_EitherSide_UpdatesPair()
    {
        Bar original = Feed[SkipIndex];
        Bar corrected = original with { Close = original.Close * 1.05m };
        List<Bar> correctedFeed = [.. Feed];
        correctedFeed[SkipIndex] = corrected;

        BarHub source = new();
        BarHub other = new();
        PairHub pair = new(source.ToSmaHub(5), other.ToEmaHub(10));
        source.Add(Feed);
        other.Add(Feed);

        other.Add(corrected);
        pair.Results.IsExactly(Join(Feed.ToSma(5), correctedFeed.ToEma(10)));

        source.Add(corrected);
        pair.Results.IsExactly(Join(correctedFeed.ToSma(5), correctedFeed.ToEma(10)));

        // the corrections changed the output, so the assertions above are not vacuous
        Join(correctedFeed.ToSma(5), correctedFeed.ToEma(10))
            .Should().NotEqual(Join(Feed.ToSma(5), Feed.ToEma(10)));

        source.EndTransmission();
        other.EndTransmission();
    }

    [TestMethod]
    public void ChainedObserver_MatchesSeriesExactly()
    {
        BarHub barHub = new();
        PairHub pair = new(barHub.ToSmaHub(5), barHub.ToEmaHub(10));
        SmaHub downstream = pair.ToSmaHub(3);

        barHub.Add(Feed);

        downstream.Results.IsExactly(Join(Feed.ToSma(5), Feed.ToEma(10)).ToSma(3));

        barHub.EndTransmission();
    }

    [TestMethod]
    public void Pruning_UnequalCacheSizes_KeepsNewestPairs()
    {
        BarHub source = new(50);
        BarHub other = new(120);
        PairHub pair = new(source, other);

        for (int i = 0; i < TotalBars; i++)
        {
            other.Add(Feed[i]);
            source.Add(Feed[i]);
        }

        // the prune rule keeps MaxCacheSize - 1 after each trim
        pair.MaxCacheSize.Should().Be(50);
        pair.Results.Count.Should().BeInRange(49, 50);
        pair.Results[^1].Timestamp.Should().Be(Feed[^1].Timestamp);
        pair.Results.IsExactly(Join(Feed, Feed).TakeLast(pair.Results.Count));

        source.EndTransmission();
        other.EndTransmission();
    }

    [TestMethod]
    public void Pruning_OtherCacheSmaller_LateArrivalRepairsBeyondOtherCache()
    {
        // the other provider prunes to 50, but the hub stages up to the source's 120,
        // so a source rebuild can still pair timestamps the other provider no longer holds
        const int lateIndex = 120;

        BarHub source = new(120);
        BarHub other = new(50);
        PairHub pair = new(source, other);

        for (int i = 0; i < TotalBars; i++)
        {
            other.Add(Feed[i]);

            if (i != lateIndex)
            {
                source.Add(Feed[i]);
            }
        }

        other.Results[0].Timestamp.Should().BeAfter(Feed[lateIndex].Timestamp);
        pair.Results.Should().NotContain(x => x.Timestamp == Feed[lateIndex].Timestamp);

        source.Add(Feed[lateIndex]);

        pair.Results.Should().Contain(x => x.Timestamp == Feed[lateIndex].Timestamp);
        pair.Results.Count.Should().BeInRange(pair.MaxCacheSize - 1, pair.MaxCacheSize);
        pair.Results[^1].Timestamp.Should().Be(Feed[^1].Timestamp);
        pair.Results.IsExactly(Join(Feed, Feed).TakeLast(pair.Results.Count));

        source.EndTransmission();
        other.EndTransmission();
    }

    [TestMethod]
    public void Diamond_RootLateArrival_MatchesSeriesExactly()
        => AssertDiamondMutation(static (root, feed) =>             // the skipped bar arrives last
            root.Add(feed[SkipIndex]), skipOnFeed: true);

    [TestMethod]
    public void Diamond_RootRemoval_MatchesSeriesExactly()
        => AssertDiamondMutation(static (root, feed) => {
            root.RemoveAt(SkipIndex);
            feed.RemoveAt(SkipIndex);
        });

    [TestMethod]
    public void Diamond_RootCorrection_MatchesSeriesExactly()
        => AssertDiamondMutation(static (root, feed) => {
            Bar corrected = feed[SkipIndex] with { Close = feed[SkipIndex].Close * 1.05m };
            root.Add(corrected);
            feed[SkipIndex] = corrected;
        });

    [TestMethod]
    public void OtherRemovesNewest_RemovesPair()
    {
        BarHub source = new();
        BarHub other = new();
        PairHub pair = new(source, other);
        source.Add(Feed);
        other.Add(Feed);

        // nothing is left to re-stage after the removed timestamp
        other.RemoveAt(TotalBars - 1);

        pair.Results.Should().HaveCount(TotalBars - 1);
        pair.Results.IsExactly(Join(Feed, WithoutIndex(TotalBars - 1)));

        source.EndTransmission();
        other.EndTransmission();
    }

    [TestMethod]
    public void Unsubscribe_ReleasesBothProviders()
    {
        BarHub source = new();
        BarHub other = new();
        PairHub pair = new(source, other);
        source.Add(Feed.Take(10));
        other.Add(Feed.Take(10));

        pair.Unsubscribe();

        pair.IsSubscribed.Should().BeFalse();
        source.HasObservers.Should().BeFalse();
        other.HasObservers.Should().BeFalse();

        // a second release is harmless
        pair.Unsubscribe();

        other.Add(Feed[10]);
        source.Add(Feed[10]);
        pair.Results.Should().HaveCount(10);
    }

    [TestMethod]
    public void OtherCompletes_EndsPair()
    {
        BarHub source = new();
        BarHub other = new();
        PairHub pair = new(source, other);

        other.EndTransmission();

        pair.IsSubscribed.Should().BeFalse();
        source.HasObservers.Should().BeFalse();
    }

    [TestMethod]
    public void OtherFaults_CascadesError()
    {
        BarHub source = new();
        BarHub other = new();
        PairHub pair = new(source, other);
        ErrorSpy spy = new();
        pair.Subscribe(spy);

        Bar dup = Feed[0];

        FluentActions.Invoking(() => {
            for (int i = 0; i <= 101; i++)
            {
                other.Add(dup);
            }
        }).Should().ThrowExactly<OverflowException>();

        spy.Error.Should().BeOfType<OverflowException>();

        source.EndTransmission();
    }

    [TestMethod]
    public void NullOther_Throws()
    {
        BarHub source = new();

        FluentActions.Invoking(() => new PairHub(source, null!))
            .Should().ThrowExactly<ArgumentNullException>()
            .WithParameterName("other");
    }

    [TestMethod]
    public void ToString_ReturnsName()
    {
        BarHub barHub = new();
        PairHub pair = new(barHub, barHub);

        pair.ToString().Should().Be("PAIR");

        barHub.EndTransmission();
    }

    /// <summary>
    /// Applies one root mutation to a diamond, with the source side subscribed
    /// first and then the other side first, and compares each with the oracle.
    /// One root change reaches the hub twice, once per side, so the end state
    /// must not depend on which side rebuilds first.
    /// </summary>
    private static void AssertDiamondMutation(Action<BarHub, List<Bar>> mutate, bool skipOnFeed = false)
    {
        foreach (bool otherFirst in new[] { false, true })
        {
            BarHub root = new();
            SmaHub sma;
            EmaHub ema;

            if (otherFirst)
            {
                ema = root.ToEmaHub(10);
                sma = root.ToSmaHub(5);
            }
            else
            {
                sma = root.ToSmaHub(5);
                ema = root.ToEmaHub(10);
            }

            PairHub pair = new(sma, ema);
            List<Bar> feed = [.. Feed];

            for (int i = 0; i < TotalBars; i++)
            {
                if (skipOnFeed && i == SkipIndex)
                {
                    continue;
                }

                root.Add(feed[i]);
            }

            IReadOnlyList<PairedValue> before = pair.Snapshot();
            mutate(root, feed);

            // the mutation must change the output, or the replay goes unchecked
            pair.Results.Should().NotEqual(before);
            pair.Results.IsExactly(Join(feed.ToSma(5), feed.ToEma(10)), $"otherFirst={otherFirst}");

            root.EndTransmission();
        }
    }

    private static void AssertTwoRoots(Action<BarHub, BarHub, Bar> feedOne)
    {
        BarHub source = new();
        BarHub other = new();
        PairHub pair = new(source.ToSmaHub(5), other.ToEmaHub(10));

        foreach (Bar bar in Feed)
        {
            feedOne(source, other, bar);
        }

        pair.Results.Should().HaveCount(TotalBars);
        pair.Results.IsExactly(Join(Feed.ToSma(5), Feed.ToEma(10)));

        source.EndTransmission();
        other.EndTransmission();
    }

    private static List<Bar> WithoutIndex(int index)
    {
        List<Bar> bars = [.. Feed];
        bars.RemoveAt(index);
        return bars;
    }

    /// <summary>
    /// Joins two Series results on the timestamps both hold, in source order.
    /// </summary>
    private static List<PairedValue> Join(
        IReadOnlyList<IReusable> source,
        IReadOnlyList<IReusable> other)
    {
        Dictionary<DateTime, double> others = other.ToDictionary(static x => x.Timestamp, static x => x.Value);

        return source
            .Where(x => others.ContainsKey(x.Timestamp))
            .Select(x => new PairedValue(x.Timestamp, x.Value, others[x.Timestamp]))
            .ToList();
    }

    private sealed class ErrorSpy : IStreamObserver<PairedValue>
    {
        internal Exception Error { get; private set; }

        public bool IsSubscribed => true;

        public void Unsubscribe() { }

        public void OnAdd(PairedValue item, bool notify, int? indexHint) { }

        public void OnRebuild(DateTime fromTimestamp) { }

        public void OnPrune(DateTime toTimestamp) { }

        public void OnError(Exception exception) => Error = exception;

        public void OnCompleted() { }
    }
}
