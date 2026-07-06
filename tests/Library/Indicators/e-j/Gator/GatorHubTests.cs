namespace StreamHubs;

[TestClass]
public class GatorHubTests : StreamHubTestBase, ITestChainObserver
{
    [TestMethod]
    public void BarObserver_WithWarmupLateArrivalAndRemoval_MatchesSeriesExactly()
    {
        // setup bar provider hub
        BarHub barHub = new();

        // prefill bars at provider
        for (int i = 0; i < 20; i++)
        {
            barHub.Add(Bars[i]);
        }

        // initialize observer
        GatorHub observer = barHub
            .ToGatorHub();

        // fetch initial results (early)
        IReadOnlyList<GatorResult> actuals
            = observer.Results;

        // emulate adding bars to provider hub
        for (int i = 20; i < barsCount; i++)
        {
            // skip one (add later)
            if (i == 80)
            {
                continue;
            }

            Bar q = Bars[i];
            barHub.Add(q);

            // resend duplicate bars
            if (i is > 100 and < 105)
            {
                barHub.Add(q);
            }
        }

        // late arrival
        barHub.Add(Bars[80]);

        // delete
        barHub.RemoveAt(removeAtIndex);

        // time-series, for comparison
        IReadOnlyList<GatorResult> expected = RevisedBars.ToGator();

        // assert, should equal series
        actuals.Should().HaveCount(barsCount - 1);
        actuals.IsExactly(expected);

        // cleanup
        observer.Unsubscribe();
        barHub.EndTransmission();
    }

    [TestMethod]
    public void WithCachePruning_MatchesSeriesExactly()
    {
        const int maxCacheSize = 50;
        const int totalBars = 100;

        IReadOnlyList<Bar> bars = Bars.Take(totalBars).ToList();
        IReadOnlyList<GatorResult> expected = bars
            .ToGator()
            .TakeLast(maxCacheSize)
            .ToList();

        // Setup with cache limit
        BarHub barHub = new(maxCacheSize);
        GatorHub observer = barHub.ToGatorHub();

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
    public void ChainObserver_ChainedProvider_MatchesSeriesExactly()
    {
        List<Bar> barsList = Bars.ToList();

        int length = barsList.Count;

        // setup bar provider hub
        BarHub barHub = new();

        // initialize observer
        GatorHub observer = barHub
            .ToSmaHub(10)
            .ToGatorHub();

        // emulate adding bars out of order
        // note: this works when graceful order
        for (int i = 0; i < length; i++)
        {
            // skip one (add later)
            if (i == 80)
            {
                continue;
            }

            Bar q = barsList[i];
            barHub.Add(q);

            // resend duplicate bars
            if (i is > 100 and < 105)
            {
                barHub.Add(q);
            }
        }

        // late arrival
        barHub.Add(barsList[80]);

        // delete
        barHub.RemoveAt(removeAtIndex);

        // final results
        IReadOnlyList<GatorResult> sut
            = observer.Results;

        // time-series, for comparison (revised)
        IReadOnlyList<GatorResult> expected
           = RevisedBars
            .ToSma(10)
            .ToGator();

        // assert, should equal series
        sut.Should().HaveCount(barsCount - 1);
        sut.IsExactly(expected);

        // cleanup
        observer.Unsubscribe();
        barHub.EndTransmission();
    }

    [TestMethod]
    public void Provider_WhenGatorHub_IsAlligatorHub()
    {
        BarHub barHub = new();
        GatorHub observer = barHub.ToGatorHub();

        System.Reflection.PropertyInfo property = typeof(StreamHub<AlligatorResult, GatorResult>)
            .GetProperty("Provider", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;

        object provider = property.GetValue(observer)!;
        provider.Should().BeOfType<AlligatorHub>();

        observer.Unsubscribe();
        barHub.EndTransmission();
    }

    [TestMethod]
    public void Reinitialize_OnSubscribedHub_Throws()
    {
        List<Bar> barsList = Bars.ToList();

        // setup bar provider hub
        BarHub barHub = new();

        // initialize observer
        GatorHub observer = barHub.ToGatorHub();

        // Add ~50 bars to populate state
        for (int i = 0; i < 50; i++)
        {
            barHub.Add(barsList[i]);
        }

        // assert observer.Results has 50 entries and the last result has non-null values
        observer.Results.Should().HaveCount(50);
        observer.Results[^1].Upper.Should().NotBeNull();
        observer.Results[^1].Lower.Should().NotBeNull();

        // reinitializing a subscribed hub is forbidden — it is driven by its provider
        Assert.ThrowsExactly<InvalidOperationException>(observer.Reinitialize);

        // the observer is unchanged and stays in sync via its provider
        observer.Results.Should().HaveCount(50);
        observer.Results[^1].Upper.Should().NotBeNull();
        observer.Results[^1].Lower.Should().NotBeNull();

        // Now test with a completely fresh setup after unsubscribing
        // cleanup
        observer.Unsubscribe();
        barHub.EndTransmission();

        // Create a new barHub with just one bar
        BarHub freshProvider = new();
        GatorHub freshObserver = freshProvider.ToGatorHub();

        // Add one bar and assert observer.Results has count 1 and that values are null (warmup period)
        freshProvider.Add(barsList[0]);

        freshObserver.Results.Should().HaveCount(1);
        freshObserver.Results[^1].Upper.Should().BeNull();
        freshObserver.Results[^1].Lower.Should().BeNull();

        // cleanup
        freshObserver.Unsubscribe();
        freshProvider.EndTransmission();
    }

    [TestMethod]
    public void GatorHub_InvalidParameters_ThrowsExpectedExceptions()
    {
        BarHub barHub = new();
        AlligatorHub alligatorHub = barHub.ToAlligatorHub();

        // test null alligatorHub parameter (throws NullReferenceException from base constructor)
        Action act1 = () => _ = new GatorHub(alligatorHub: null!);
        act1.Should().Throw<NullReferenceException>("AlligatorHub cannot be null");

        // test null item in ToIndicator (via reflection to access protected method)
        GatorHub gatorHub = alligatorHub.ToGatorHub();
        Action act2 = () => {
            System.Reflection.MethodInfo method = typeof(GatorHub).GetMethod(
                "ToIndicator",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            method?.Invoke(gatorHub, [null, null]);
        };
        act2.Should().Throw<System.Reflection.TargetInvocationException>()
            .WithInnerException<ArgumentNullException>("item cannot be null");

        gatorHub.Unsubscribe();
        barHub.EndTransmission();
    }

    [TestMethod]
    public override void ToStringOverride_ReturnsExpectedName()
    {
        GatorHub hub = new(new AlligatorHub(new BarHub(), 13, 8, 8, 5, 5, 3));
        hub.ToString().Should().Be("GATOR()");
    }
}
