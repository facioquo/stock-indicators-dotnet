namespace StreamHubs;

[TestClass]
public class TtmSqueezeHubTests : StreamHubTestBase, ITestBarObserver, ITestChainProvider
{
    [TestMethod]
    public void BarObserver_WithWarmupLateArrivalAndRemoval_MatchesSeriesExactly()
    {
        // setup bar provider hub
        BarHub barHub = new();

        // prefill bars at provider
        for (int i = 0; i < 25; i++)
        {
            barHub.Add(Bars[i]);
        }

        // initialize observer
        TtmSqueezeHub observer = barHub
            .ToTtmSqueezeHub(20, 2, 1.5);

        // fetch initial results (early)
        IReadOnlyList<TtmSqueezeResult> actuals
            = observer.Results;

        // emulate adding bars to provider hub
        for (int i = 25; i < barsCount; i++)
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
        IReadOnlyList<TtmSqueezeResult> expected = RevisedBars.ToTtmSqueeze(20, 2, 1.5);

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
        IReadOnlyList<TtmSqueezeResult> expected = bars
            .ToTtmSqueeze(20, 2, 1.5)
            .TakeLast(maxCacheSize)
            .ToList();

        // setup with cache limit
        BarHub barHub = new(maxCacheSize);
        TtmSqueezeHub observer = barHub.ToTtmSqueezeHub(20, 2, 1.5);

        // stream more bars than cache can hold
        barHub.Add(bars);

        // verify cache was pruned
        barHub.Bars.Should().HaveCount(maxCacheSize);
        observer.Results.Should().HaveCount(maxCacheSize);

        // streaming results should match last N from full series, not a recomputation on the cached bars
        observer.Results.IsExactly(expected);

        observer.Unsubscribe();
        barHub.EndTransmission();
    }

    [TestMethod]
    public void ChainProvider_MatchesSeriesExactly()
    {
        const int smaPeriods = 8;

        // setup bar provider hub
        BarHub barHub = new();

        // initialize observer
        SmaHub observer = barHub
            .ToTtmSqueezeHub(20, 2, 1.5)
            .ToSmaHub(smaPeriods);

        // emulate bar stream
        for (int i = 0; i < barsCount; i++)
        {
            if (i == 80) { continue; }  // skip for late arrival

            Bar q = Bars[i];
            barHub.Add(q);

            if (i is > 100 and < 105) { barHub.Add(q); }  // duplicate bars
        }

        barHub.Add(Bars[80]);  // late arrival
        barHub.RemoveAt(removeAtIndex);  // remove

        // final results
        IReadOnlyList<SmaResult> sut = observer.Results;

        // time-series, for comparison (revised)
        IReadOnlyList<SmaResult> expected = RevisedBars
            .ToTtmSqueeze(20, 2, 1.5)
            .ToSma(smaPeriods);

        // assert, should equal series
        sut.Should().HaveCount(barsCount - 1);
        sut.IsExactly(expected);

        // cleanup
        observer.Unsubscribe();
        barHub.EndTransmission();
    }

    [TestMethod]
    public override void ToStringOverride_ReturnsExpectedName()
    {
        BarHub barHub = new();
        TtmSqueezeHub observer = barHub.ToTtmSqueezeHub(20, 2, 1.5);

        observer.ToString().Should().Be("TTM-SQUEEZE(20,2,1.5)");

        // cleanup
        observer.Unsubscribe();
        barHub.EndTransmission();
    }
}
