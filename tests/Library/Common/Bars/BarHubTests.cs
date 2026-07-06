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
    public void IgnoreBarsPrecedingTimeline_Standalone()
    {
        const int maxCacheSize = 50;
        const int totalBars = 100;

        IReadOnlyList<Bar> bars = Bars.Take(totalBars).ToList();

        // Setup standalone BarHub with cache limit
        BarHub barHub = new(maxCacheSize);

        // Stream more bars than cache can hold
        barHub.Add(bars);

        // Verify cache was pruned to maxCacheSize
        barHub.Bars.Should().HaveCount(maxCacheSize);

        // Cache should now contain bars [50..99]
        DateTime firstTimestamp = barHub.Cache[0].Timestamp;

        // Try to add a bar that precedes the current timeline
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
    public void BarBeforeHead_ViaProviderNotification_IsIgnored()
    {
        // The before-head drop on a non-standalone BarHub is now only
        // reachable via the provider-notification path (OnAdd), since the
        // public Add is rejected on a subscribed hub. Pin that branch directly.
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
}
