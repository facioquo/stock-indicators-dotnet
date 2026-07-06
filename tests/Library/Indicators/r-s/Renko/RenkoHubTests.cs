namespace StreamHubs;

[TestClass]
public class RenkoHubTests : StreamHubTestBase, ITestBarObserver, ITestChainProvider
{
    // NOTE: Renko transforms bars to variable brick counts (non-1:1 timestamps).
    // Intentionally excluded from comprehensive provider history testing (Add/Remove)
    // as bar transformations don't preserve timestamp mappings.
    // TODO: Revisit to explore alternative testing approach for bar transformations.

    [TestMethod]
    public void BarObserver_WithWarmupLateArrivalAndRemoval_MatchesSeriesExactly()
    {
        const decimal brickSize = 2.5m;
        const EndType endType = EndType.HighLow;

        List<Bar> bars = Bars.ToList();

        int length = bars.Count;

        // setup bar provider hub
        BarHub barHub = new();

        // prefill bars at provider
        for (int i = 0; i < 50; i++)
        {
            barHub.Add(bars[i]);
        }

        // initialize observer
        RenkoHub observer = barHub
            .ToRenkoHub(brickSize, endType);

        // fetch initial results (early)
        IReadOnlyList<RenkoResult> streamList
            = observer.Results;

        // emulate adding bars to provider hub
        for (int i = 50; i < length; i++)
        {
            // skip one (add later)
            if (i == 80)
            {
                continue;
            }

            Bar q = bars[i];
            barHub.Add(q);

            // resend duplicate bars
            if (i is > 100 and < 105)
            {
                barHub.Add(q);
            }
        }

        // late arrival
        barHub.Add(bars[80]);

        // delete
        barHub.RemoveAt(350);
        bars.RemoveAt(350);

        // time-series, for comparison
        IReadOnlyList<RenkoResult> seriesList = bars
            .ToRenko(brickSize, endType);

        // assert, should equal series
        streamList.IsExactly(seriesList);
        streamList.Should().HaveCount(159);

        // cleanup
        observer.Unsubscribe();
        barHub.EndTransmission();
    }

    [TestMethod]
    public void WithCachePruning_MatchesSeriesExactly()
    {
        // NOTE: Renko transforms bars to bricks (non-1:1 mapping).
        // Bar pruning removes oldest bars, but brick count != bar count.
        // Use timestamp-based alignment to match streaming results against
        // the corresponding tail of the full static series.

        const int maxCacheSize = 100;  // Sufficient for bar retention
        const int totalBars = 200;  // ~2x cache size
        const decimal brickSize = 2.5m;
        const EndType endType = EndType.HighLow;

        IReadOnlyList<Bar> bars = Bars.Take(totalBars).ToList();

        // Get full series results
        List<RenkoResult> fullSeries = bars
            .ToRenko(brickSize, endType)
            .ToList();

        // Setup with cache limit
        BarHub barHub = new(maxCacheSize);
        RenkoHub observer = barHub.ToRenkoHub(brickSize, endType);

        // Stream more bars than cache can hold
        barHub.Add(bars);

        // Verify cache was pruned (bars, not results)
        barHub.Bars.Should().HaveCount(maxCacheSize);

        // Renko bricks don't map 1:1 with bars. Align the static series
        // with the streaming hub by matching on the first brick's timestamp.
        // Any bricks before that date in the static series represent periods
        // that were pruned from the bar hub and must be discarded.
        DateTime firstDate = observer.Results[0].Timestamp;
        int startIndex = fullSeries.FindIndex(r => r.Timestamp == firstDate);
        startIndex.Should().BeGreaterThanOrEqualTo(0,
            "the first Renko result in the hub should exist in the static series");

        List<RenkoResult> expected = fullSeries.Skip(startIndex).ToList();
        observer.Results.IsExactly(expected);

        observer.Unsubscribe();
        barHub.EndTransmission();
    }

    [TestMethod]
    public void ChainProvider_MatchesSeriesExactly()
    {
        const decimal brickSize = 2.5m;
        const EndType endType = EndType.Close;
        const int smaPeriods = 50;

        // setup bar provider hub
        BarHub barHub = new();

        // initialize observer
        SmaHub observer = barHub
            .ToRenkoHub(brickSize, endType)
            .ToSmaHub(smaPeriods);

        // emulate bar stream (Renko transforms to bricks, no Add/Remove)
        for (int i = 0; i < barsCount; i++)
        {
            barHub.Add(Bars[i]);
        }

        // final results
        IReadOnlyList<SmaResult> sut = observer.Results;

        // time-series, for comparison
        IReadOnlyList<SmaResult> expected = Bars
            .ToRenko(brickSize, endType)
            .ToSma(smaPeriods);

        // assert, should equal series
        sut.IsExactly(expected);
        sut.Should().HaveCount(112);

        // cleanup
        observer.Unsubscribe();
        barHub.EndTransmission();
    }

    [TestMethod]
    public void LateArrival_MidStream_MatchesFreshStream()
    {
        // Renko is bricks-from-price, not 1:1 with bars — the late-arrival
        // path must reproduce the same brick sequence as the fresh stream.
        const decimal brickSize = 2.5m;
        const EndType endType = EndType.HighLow;
        const int totalBars = 300;
        const int lateIndex = 150;

        IReadOnlyList<Bar> bars = Bars.Take(totalBars).ToList();

        BarHub lateSource = new();
        RenkoHub lateHub = lateSource.ToRenkoHub(brickSize, endType);
        for (int i = 0; i < totalBars; i++)
        {
            if (i == lateIndex) { continue; }

            lateSource.Add(bars[i]);
        }

        lateSource.Add(bars[lateIndex]);

        BarHub freshSource = new();
        RenkoHub freshHub = freshSource.ToRenkoHub(brickSize, endType);
        freshSource.Add(bars);

        // Renko brick count is data-dependent (not 1:1 with bars), so
        // pin the oracle's non-emptiness before equality to guard against
        // trivial empty-vs-empty pass if the hub ever falls silent.
        freshHub.Results.Should().NotBeEmpty();
        lateHub.Results.Should().HaveCount(freshHub.Results.Count);
        lateHub.Results.IsExactly(freshHub.Results);

        lateHub.Unsubscribe();
        freshHub.Unsubscribe();
        lateSource.EndTransmission();
        freshSource.EndTransmission();
    }

    [TestMethod]
    public void LateArrival_NearFirstBrickFormation_MatchesFreshStream()
    {
        // Renko's first brick is data-dependent (requires sufficient price
        // move from anchor); index 60 is chosen to cover the early-brick
        // formation window in the default fixture.
        const decimal brickSize = 2.5m;
        const EndType endType = EndType.HighLow;
        const int totalBars = 300;
        const int lateIndex = 60;

        IReadOnlyList<Bar> bars = Bars.Take(totalBars).ToList();

        BarHub lateSource = new();
        RenkoHub lateHub = lateSource.ToRenkoHub(brickSize, endType);
        for (int i = 0; i < totalBars; i++)
        {
            if (i == lateIndex) { continue; }

            lateSource.Add(bars[i]);
        }

        lateSource.Add(bars[lateIndex]);

        BarHub freshSource = new();
        RenkoHub freshHub = freshSource.ToRenkoHub(brickSize, endType);
        freshSource.Add(bars);

        // Renko brick count is data-dependent (not 1:1 with bars), so
        // pin the oracle's non-emptiness before equality to guard against
        // trivial empty-vs-empty pass if the hub ever falls silent.
        freshHub.Results.Should().NotBeEmpty();
        lateHub.Results.Should().HaveCount(freshHub.Results.Count);
        lateHub.Results.IsExactly(freshHub.Results);

        lateHub.Unsubscribe();
        freshHub.Unsubscribe();
        lateSource.EndTransmission();
        freshSource.EndTransmission();
    }

    [TestMethod]
    public override void ToStringOverride_ReturnsExpectedName()
    {
        RenkoHub hub = new(new BarHub(), 2.5m, EndType.Close);
        hub.ToString().Should().Be("RENKO(2.5,CLOSE)");
    }

    [TestMethod]
    public void SettingsInheritance_InHubChain_PropagatesProperties()
    {
        // setup bar hub (1st level)
        BarHub barHub = new();

        // setup renko hub (2nd level)
        RenkoHub renkoHub = barHub
            .ToRenkoHub(brickSize: 2.5m, endType: EndType.Close);

        // setup child hub (3rd level)
        SmaHub childHub = renkoHub
            .ToSmaHub(lookbackPeriods: 5);

        // note: dispite `barHub` being parentless,
        // it has default properties; it should not
        // inherit its own empty barHub settings

        // assert
        barHub.Properties.Settings.Should().Be(0b00000000, "is has default settings, not inherited");
        renkoHub.Properties.Settings.Should().Be(0b00000010, "it has custom Renko properties");
        childHub.Properties.Settings.Should().Be(0b00000010, "it inherits Renko properties");
    }
}
