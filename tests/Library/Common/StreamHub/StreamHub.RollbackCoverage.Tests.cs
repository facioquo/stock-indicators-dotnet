namespace Observables;

/// <summary>
/// Rollback-engine coverage for the aggregator hubs and through-aggregator
/// chains that the generic catalog-driven rollback contract does not reach.
/// <c>BarAggregatorHub</c>/<c>TradeTickAggregatorHub</c> override
/// <c>RollbackState</c>/<c>Rebuild</c> but are not catalog-registered, so they
/// are exercised here directly; and a late arrival routed through an aggregator
/// into a downstream indicator must converge to the same results as a fresh
/// chain.
/// </summary>
[TestClass]
public class RollbackCoverage : TestBase
{
    private const int TradeTickCount = 500;

    [TestMethod]
    public void BarAggregator_AfterRebuild_MatchesFreshStreamAndBatch()
    {
        IReadOnlyList<Bar> bars = Bars.Take(500).ToList();
        DateTime rollback = bars[300].Timestamp;

        // a hub that was rolled back mid-stream and replayed
        BarHub providerA = new();
        BarAggregatorHub aggA = providerA.ToBarAggregatorHub(BarInterval.Week);
        providerA.Add(bars);
        aggA.Rebuild(rollback);

        // a fresh hub fed the same bars once
        BarHub providerB = new();
        BarAggregatorHub aggB = providerB.ToBarAggregatorHub(BarInterval.Week);
        providerB.Add(bars);

        // independent batch oracle: the streamed aggregation (fresh or rebuilt)
        // must equal the batch Aggregate — this upgrades the rollback-equivalence
        // check to rollback-and-correctness
        IReadOnlyList<Bar> batch = bars.Aggregate(BarInterval.Week);

        aggA.Results.Should().NotBeEmpty();
        aggB.Results.IsExactly(batch);
        aggA.Results.IsExactly(aggB.Results);

        aggA.Unsubscribe();
        aggB.Unsubscribe();
        providerA.EndTransmission();
        providerB.EndTransmission();
    }

    [TestMethod]
    public void TradeTickAggregator_AfterRebuild_MatchesFreshStream()
    {
        List<TradeTick> ticks = BuildTradeTicks(TradeTickCount);
        DateTime rollback = ticks[300].Timestamp;

        TradeTickHub providerA = new();
        TradeTickAggregatorHub aggA = providerA.ToTradeTickAggregatorHub(BarInterval.FifteenMinutes);
        providerA.Add(ticks);
        aggA.Rebuild(rollback);

        TradeTickHub providerB = new();
        TradeTickAggregatorHub aggB = providerB.ToTradeTickAggregatorHub(BarInterval.FifteenMinutes);
        providerB.Add(ticks);

        aggA.Results.Should().NotBeEmpty();
        aggB.Results.Should().NotBeEmpty();
        aggA.Results.IsExactly(aggB.Results);

        aggA.Unsubscribe();
        aggB.Unsubscribe();
        providerA.EndTransmission();
        providerB.EndTransmission();
    }

    [TestMethod]
    public void BarHub_Aggregator_Ema_LateArrival_MatchesFreshChain()
    {
        const int total = 500;
        const int skipIndex = 200;
        const int emaPeriods = 5;

        IReadOnlyList<Bar> bars = Bars.Take(total).ToList();

        // late chain: feed all but one bar, then add it late so the correction
        // rolls back through the aggregator and replays into the EMA
        BarHub lateProvider = new();
        BarAggregatorHub lateAgg = lateProvider.ToBarAggregatorHub(BarInterval.Week);
        EmaHub lateEma = lateAgg.ToEmaHub(emaPeriods);

        for (int i = 0; i < total; i++)
        {
            if (i == skipIndex) { continue; }

            lateProvider.Add(bars[i]);
        }

        lateProvider.Add(bars[skipIndex]);  // late arrival → rollback through the aggregator

        // fresh chain: in-order oracle
        BarHub freshProvider = new();
        BarAggregatorHub freshAgg = freshProvider.ToBarAggregatorHub(BarInterval.Week);
        EmaHub freshEma = freshAgg.ToEmaHub(emaPeriods);

        freshProvider.Add(bars);

        // Aggregator-level equality is the sensitive pin: a bucket's Volume is a
        // sum, so a dropped or duplicated bar in the rollback replay diverges
        // here even when the EMA (which sees only the bucket close) would not.
        // The downstream EMA equality then confirms the correction propagates
        // through the aggregator into the chained indicator.
        lateAgg.Results.IsExactly(freshAgg.Results);

        lateEma.Results.Should().NotBeEmpty();
        freshEma.Results.Should().NotBeEmpty();
        lateEma.Results.IsExactly(freshEma.Results);

        lateEma.Unsubscribe();
        lateAgg.Unsubscribe();
        lateProvider.EndTransmission();
        freshEma.Unsubscribe();
        freshAgg.Unsubscribe();
        freshProvider.EndTransmission();
    }

    [TestMethod]
    public void TradeTickHub_Aggregator_Ema_LateArrival_MatchesFreshChain()
    {
        const int skipIndex = 200;
        const int emaPeriods = 5;

        List<TradeTick> ticks = BuildTradeTicks(TradeTickCount);

        TradeTickHub lateProvider = new();
        TradeTickAggregatorHub lateAgg = lateProvider.ToTradeTickAggregatorHub(BarInterval.FifteenMinutes);
        EmaHub lateEma = lateAgg.ToEmaHub(emaPeriods);

        for (int i = 0; i < ticks.Count; i++)
        {
            if (i == skipIndex) { continue; }

            lateProvider.Add(ticks[i]);
        }

        lateProvider.Add(ticks[skipIndex]);  // late arrival

        TradeTickHub freshProvider = new();
        TradeTickAggregatorHub freshAgg = freshProvider.ToTradeTickAggregatorHub(BarInterval.FifteenMinutes);
        EmaHub freshEma = freshAgg.ToEmaHub(emaPeriods);

        freshProvider.Add(ticks);

        // aggregator-level equality is the sensitive pin (see the bar analog)
        lateAgg.Results.IsExactly(freshAgg.Results);

        lateEma.Results.Should().NotBeEmpty();
        freshEma.Results.Should().NotBeEmpty();
        lateEma.Results.IsExactly(freshEma.Results);

        lateEma.Unsubscribe();
        lateAgg.Unsubscribe();
        lateProvider.EndTransmission();
        freshEma.Unsubscribe();
        freshAgg.Unsubscribe();
        freshProvider.EndTransmission();
    }

    /// <summary>
    /// Synthetic one-minute ticks with strictly increasing timestamps and a mild
    /// deterministic wave so OHLC aggregation is non-trivial.
    /// </summary>
    private static List<TradeTick> BuildTradeTicks(int count)
    {
        DateTime start = new(2023, 11, 9, 9, 30, 0, DateTimeKind.Utc);
        List<TradeTick> ticks = new(count);

        for (int i = 0; i < count; i++)
        {
            decimal price = 100m + (i % 37) - (i % 13);
            ticks.Add(new TradeTick(start.AddMinutes(i), price, 10m + (i % 5)));
        }

        return ticks;
    }
}
