namespace StreamHubs;

[TestClass]
public class BarHubTests : StreamHubTestBase, ITestBarObserver, ITestChainProvider
{
    [TestMethod]
    public void BarObserver_WithWarmupLateArrivalAndRemoval_MatchesSeriesExactly()
    {
        // setup bar provider hub
        BarHub provider = new();

        // prefill bars at provider
        provider.Add(Bars.Take(20));

        // initialize observer
        BarHub observer = provider.ToBarHub();

        // fetch initial results (early)
        IReadOnlyList<IBar> sut = observer.Results;

        // emulate adding bars to provider hub
        for (int i = 20; i < barsCount; i++)
        {
            // skip one (add later)
            if (i == 80) { continue; }

            Bar q = Bars[i];
            provider.Add(q);

            // resend duplicate bars
            if (i is > 100 and < 105) { provider.Add(q); }
        }

        // late arrival, should equal series
        provider.Add(Bars[80]);

        sut.IsExactly(Bars);

        // delete, should equal series (revised)
        provider.RemoveAt(removeAtIndex);

        sut.IsExactly(RevisedBars);
        sut.Should().HaveCount(barsCount - 1);

        // cleanup
        observer.Unsubscribe();
        provider.EndTransmission();
    }

    [TestMethod]
    public void WithCachePruning_MatchesSeriesExactly()
    {
        const int maxCacheSize = 50;
        const int totalBars = 100;

        IReadOnlyList<Bar> bars = Bars.Take(totalBars).ToList();
        IReadOnlyList<IBar> expected = bars
            .Cast<IBar>()
            .TakeLast(maxCacheSize)
            .ToList();

        // Setup with cache limit
        BarHub barHub = new(maxCacheSize);
        BarHub observer = barHub.ToBarHub();

        // Stream more bars than cache can hold
        barHub.Add(bars);

        // Verify cache was pruned
        barHub.Bars.Should().HaveCount(maxCacheSize);
        observer.Results.Should().HaveCount(maxCacheSize);

        // Streaming results should match last N from full series (original series with front chopped off)
        // NOT recomputation on just the cached bars (which would have different warmup)
        observer.Results.IsExactly(expected);

        observer.Unsubscribe();
        barHub.EndTransmission();
    }

    [TestMethod]
    public void ChainProvider_MatchesSeriesExactly()
    {
        const int emaPeriods = 14;

        // setup bar provider hub
        BarHub provider = new();

        // initialize observer
        EmaHub observer = provider
            .ToBarHub()
            .ToEmaHub(emaPeriods);

        // emulate bar stream with comprehensive provider history testing
        for (int i = 0; i < barsCount; i++)
        {
            if (i == 80) { continue; }  // Skip one

            Bar q = Bars[i];
            provider.Add(q);

            if (i is > 100 and < 105) { provider.Add(q); }  // Duplicates
        }

        provider.Add(Bars[80]);  // Late arrival
        provider.RemoveAt(removeAtIndex);  // Delete

        // final results
        IReadOnlyList<EmaResult> sut = observer.Results;

        // time-series, for comparison
        IReadOnlyList<EmaResult> expected = RevisedBars
            .ToEma(emaPeriods);

        // assert, should equal series
        sut.Should().HaveCount(barsCount - 1);
        sut.IsExactly(expected);

        // cleanup
        observer.Unsubscribe();
        provider.EndTransmission();
    }

    [TestMethod]
    public override void ToStringOverride_ReturnsExpectedName()
    {
        BarHub hub = new();

        hub.ToString().Should().Be("BARS<IBar>: 0 items");

        hub.Add(Bars[0]);
        hub.Add(Bars[1]);

        hub.ToString().Should().Be("BARS<IBar>: 2 items");
    }

    [TestMethod]
    public void AddBar()
    {
        // covers both single and batch add

        List<Bar> barsList = Bars.ToList();

        int length = Bars.Count;

        // add base bars (batch)
        BarHub barHub = new();

        barHub.Add(barsList.Take(200));

        // add incremental bars
        for (int i = 200; i < length; i++)
        {
            Bar q = barsList[i];
            barHub.Add(q);
        }

        // assert same as original
        for (int i = 0; i < length; i++)
        {
            Bar o = barsList[i];
            IBar q = barHub.Cache[i];

            q.Should().Be(o);  // same ref
        }

        // confirm public interfaces
        barHub.Bars.Should().HaveCount(barHub.Cache.Count);

        // close observations
        barHub.EndTransmission();
    }

    [TestMethod]
    public void IgnoreBarsInsidePrunedHistory_Standalone()
    {
        // Pruning is what makes a bar unrepresentable: bars [0..49] were
        // discarded, so re-admitting one would leave it adjoining bars[50],
        // a bar it never followed. Dropping is correct *here* — see
        // AcceptBarPrecedingHead_WhenNothingPruned for the case where the
        // same relative ordering carries no such loss.
        const int maxCacheSize = 50;
        const int totalBars = 100;

        IReadOnlyList<Bar> bars = Bars.Take(totalBars).ToList();

        // Setup standalone BarHub with cache limit
        BarHub barHub = new(maxCacheSize);

        // Stream more bars than cache can hold, forcing a prune
        barHub.Add(bars);

        // Verify cache was pruned to maxCacheSize
        barHub.Bars.Should().HaveCount(maxCacheSize);

        // Cache should now contain bars [50..99]
        DateTime firstTimestamp = barHub.Cache[0].Timestamp;

        // the precondition this test turns on: history really was discarded
        firstTimestamp.Should().BeAfter(bars[0].Timestamp,
            "the drop below is only justified because earlier bars were pruned");

        // Try to add a bar that falls inside the pruned range
        Bar oldBar = bars[10]; // This is before bars[50]
        oldBar.Timestamp.Should().BeBefore(firstTimestamp);

        // This should be silently ignored
        barHub.Add(oldBar);

        // Cache size should remain unchanged
        barHub.Bars.Should().HaveCount(maxCacheSize);

        // First bar in cache should still be the same
        barHub.Cache[0].Timestamp.Should().Be(firstTimestamp);

        barHub.EndTransmission();
    }

    [TestMethod]
    public void AddToSubscribedHub_Throws()
    {
        // A subscribed (non-root) hub is driven by its provider; adding to it
        // directly is rejected so a leaf can't be desynchronized from its
        // provider. Feed the root hub instead.

        const int totalBars = 100;
        IReadOnlyList<Bar> bars = Bars.Take(totalBars).ToList();

        // root provider, plus a subscribed observer
        BarHub provider = new();
        BarHub observer = provider.ToBarHub();

        provider.Add(bars.Take(50));

        // a single add to the subscribed observer is forbidden
        Assert.ThrowsExactly<InvalidOperationException>(
            () => observer.Add(bars[50]));

        // a batch add is equally forbidden
        Assert.ThrowsExactly<InvalidOperationException>(
            () => observer.Add(bars.Skip(50)));

        // the observer is unchanged and stays in sync via its provider
        observer.Results.Should().HaveCount(50);

        observer.Unsubscribe();
        provider.EndTransmission();
    }

    [TestMethod]
    public void BarInsidePrunedHistory_ViaProviderNotification_LeavesObserverUnchanged()
    {
        // The rejection on a non-standalone BarHub is only reachable via the
        // provider-notification path (OnAdd), since the public Add is rejected
        // on a subscribed hub. Exercise that branch.
        //
        // Honest limit, stated because over-claiming a test is what let the
        // original defect ship: this pins the OUTCOME, not the branch. A
        // subscribed hub's cache is a function of its provider's, so a bar that
        // slipped past the guard would be rebuilt away against the provider —
        // which no longer holds it — and land on this same cache. The guard is
        // a short-circuit here, not the thing producing the result, and no
        // assertion on the observer's cache can tell the two apart.
        const int maxCacheSize = 50;
        const int totalBars = 100;

        IReadOnlyList<Bar> bars = Bars.Take(totalBars).ToList();

        BarHub provider = new(maxCacheSize);
        BarHub observer = provider.ToBarHub();

        provider.Add(bars);

        // observer head has advanced past the pruned front
        observer.Results.Should().HaveCount(maxCacheSize);
        DateTime headTimestamp = observer.Cache[0].Timestamp;

        Bar oldBar = bars[10];
        oldBar.Timestamp.Should().BeBefore(headTimestamp);

        // simulate a provider notification of a before-head bar
        observer.OnAdd(oldBar, notify: true, indexHint: null);

        // ignored: cache unchanged
        observer.Results.Should().HaveCount(maxCacheSize);
        observer.Cache[0].Timestamp.Should().Be(headTimestamp);

        observer.Unsubscribe();
        provider.EndTransmission();
    }

    [TestMethod]
    public void AcceptBarPrecedingHead_WhenNothingPruned()
    {
        // #2153: a hub seeded from the middle of a series has pruned nothing,
        // so a bar arriving beneath its head is simply one it never received —
        // backfill, a second feed, or plain out-of-order delivery. Discarding
        // it lost data the hub was able to hold.
        BarHub hub = new(); // default cache is 100_000; nothing will prune

        hub.Add(Bars.Skip(50).Take(20));
        DateTime headBefore = hub.Cache[0].Timestamp;

        Bar earlier = Bars[10];
        earlier.Timestamp.Should().BeBefore(headBefore);

        hub.Add(earlier);

        hub.Results.Should().HaveCount(21, "the earlier bar is representable and must be kept");
        hub.Cache[0].Timestamp.Should().Be(earlier.Timestamp, "it sorts ahead of the seeded window");
        hub.Cache[1].Timestamp.Should().Be(headBefore, "the seeded window is otherwise untouched");
        hub.IsFaulted.Should().BeFalse();

        hub.EndTransmission();
    }

    [TestMethod]
    public void AcceptBatchPrecedingHead_WhenNothingPruned()
    {
        // The batch path is where the loss was silent enough to miss: a caller
        // offering 30 bars over a 20-bar cache saw the count stay at 20 — ten
        // dropped, twenty same-timestamp replacements — and no signal either way.
        BarHub hub = new();

        hub.Add(Bars.Skip(50).Take(20));

        IReadOnlyList<Bar> batch = Bars.Skip(40).Take(30).ToList();
        hub.Add(batch);

        // bars [40..69]: ten precede the head, twenty restate what is cached
        hub.Results.Should().HaveCount(30, "the ten leading bars must survive the batch");
        hub.Cache[0].Timestamp.Should().Be(Bars[40].Timestamp);
        hub.Cache.Select(b => b.Timestamp).Should().BeInAscendingOrder();

        hub.EndTransmission();
    }

    [TestMethod]
    public void BarPrecedingHead_ArrivalOrderDoesNotChangeCache()
    {
        // The clearest statement of the rule: the accepted cache is exactly the
        // one the reverse arrival order already produced before this fix, so
        // refusing it was ordering-dependent rather than protective.
        BarHub inOrder = new();
        inOrder.Add(Bars[10]);
        inOrder.Add(Bars.Skip(50).Take(20));

        BarHub outOfOrder = new();
        outOfOrder.Add(Bars.Skip(50).Take(20));
        outOfOrder.Add(Bars[10]);

        outOfOrder.Cache.Select(static b => b.Timestamp)
            .Should().Equal(inOrder.Cache.Select(static b => b.Timestamp));

        inOrder.EndTransmission();
        outOfOrder.EndTransmission();
    }

    [TestMethod]
    public void BarPrecedingHead_CascadesRebuildToObservers()
    {
        // A front insert invalidates every downstream calculation, so the
        // observer must re-derive rather than keep results computed without it.
        BarHub provider = new();
        provider.Add(Bars.Skip(50).Take(20));

        BarHub observer = provider.ToBarHub();
        observer.Results.Should().HaveCount(20);

        provider.Add(Bars[10]);

        observer.Results.Should().HaveCount(21, "the observer rebuilds to include the earlier bar");
        observer.Cache[0].Timestamp.Should().Be(Bars[10].Timestamp);

        observer.Unsubscribe();
        provider.EndTransmission();
    }

    [TestMethod]
    public void BarPrecedingHead_AtCapacity_LeavesCacheUntouched()
    {
        // A full cache has no room for a bar older than everything it holds, so
        // the bar is refused outright. That has to be decided before the insert:
        // the maintenance prune inside InsertWithoutRebuild runs first, so it
        // would evict the retained head to make room and only then find the
        // shifted index negative — dropping the arriving bar AND the head, a net
        // loss of one bar where refusing costs nothing.
        const int maxCacheSize = 50;

        BarHub hub = new(maxCacheSize);

        // exactly at capacity, and nothing has pruned yet
        hub.Add(Bars.Skip(50).Take(maxCacheSize));
        hub.Results.Should().HaveCount(maxCacheSize);

        DateTime headBefore = hub.Cache[0].Timestamp;
        headBefore.Should().Be(Bars[50].Timestamp);

        hub.Add(Bars[10]); // older than every retained bar

        hub.Results.Should().HaveCount(maxCacheSize, "refusing must not shrink the cache");
        hub.Cache[0].Timestamp.Should().Be(headBefore, "the retained head must survive the refusal");
        hub.Cache.Should().NotContain(b => b.Timestamp == Bars[10].Timestamp);

        hub.EndTransmission();
    }

    [TestMethod]
    public void GapBarPrecedingHead_AtCapacity_LeavesCacheUntouched()
    {
        // The variant that matters most, because it strikes a bar this change
        // exists to ACCEPT: one strictly above the prune boundary, sitting in a
        // gap the hub never held, arriving while the cache is full. Left to the
        // insert path it would evict the head, refuse the bar, and — worst —
        // advance the prune boundary to the destroyed head's timestamp,
        // permanently shrinking the acceptance window this change widens.
        BarHub hub = new(21);

        hub.Add(Bars.Take(49));          // prunes; boundary lands at Bars[48]
        hub.Add(Bars.Skip(60).Take(21)); // head is Bars[60]; cache at capacity

        hub.Results.Should().HaveCount(21);
        DateTime headBefore = hub.Cache[0].Timestamp;
        headBefore.Should().Be(Bars[60].Timestamp);

        // above the boundary and below the head: in the never-pruned gap
        Bars[50].Timestamp.Should().BeAfter(Bars[48].Timestamp);
        Bars[50].Timestamp.Should().BeBefore(headBefore);

        hub.Add(Bars[50]);

        hub.Results.Should().HaveCount(21, "refusing must not shrink the cache");
        hub.Cache[0].Timestamp.Should().Be(headBefore, "the retained head must survive");
        hub.Cache.Should().NotContain(b => b.Timestamp == Bars[50].Timestamp);
    }

    [TestMethod]
    public void BarPrecedingHead_AfterPruning_TurnsOnTheBoundaryNotCapacity()
    {
        // Pruning leaves the cache exactly at MaxCacheSize, so in the steady
        // state the capacity rule alone would refuse every before-head bar and
        // the boundary rule would never be observable. Drop below capacity
        // first, so this pins the boundary itself: at it, refuse; above it,
        // accept — the case the suite otherwise never reaches, because every
        // other acceptance test uses a hub that has never pruned.
        BarHub hub = new(50);
        hub.Add(Bars.Take(100)); // prunes [0..49]; boundary at Bars[49]
        hub.RemoveAt(0);         // 49 of 50: capacity rule now inactive

        hub.Results.Should().HaveCount(49);

        // exactly at the boundary — refused, and `<=` is what makes it so
        hub.Add(Bars[49]);
        hub.Results.Should().HaveCount(49, "a bar at the boundary is still inside pruned history");
        hub.Cache.Should().NotContain(b => b.Timestamp == Bars[49].Timestamp);

        // one step above it — accepted, with room to hold it
        hub.Add(Bars[50]);
        hub.Results.Should().HaveCount(50, "a bar above the boundary was never discarded");
        hub.Cache[0].Timestamp.Should().Be(Bars[50].Timestamp);
    }

    [TestMethod]
    public void PrunedBoundary_SurvivesReinitialize()
    {
        // Pruning is irreversible, so the boundary must outlive a reset. A root
        // BarHub deliberately preserves its cache across Reinitialize (see the
        // IsRootHub branch of Rebuild), so the pruned range is still missing
        // afterward and re-admitting into it would still fabricate adjacency.
        const int maxCacheSize = 50;

        BarHub hub = new(maxCacheSize);
        hub.Add(Bars.Take(100)); // forces a prune of bars [0..49]
        DateTime headBefore = hub.Cache[0].Timestamp;

        hub.Reinitialize();

        hub.Add(Bars[10]); // inside the pruned range

        hub.Cache[0].Timestamp.Should().Be(headBefore,
            "a reset does not bring pruned history back, so the boundary still applies");

        hub.EndTransmission();
    }
}
