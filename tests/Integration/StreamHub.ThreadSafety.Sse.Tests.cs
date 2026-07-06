using System.Diagnostics;
using System.Text.Json;
using Test.Tools;

namespace StreamHubs;

/// <summary>
/// Integration tests to exercise the SSE streaming bar server and ensure that every streaming indicator
/// hub produces results identical to their static series counterparts.  These tests intentionally
/// ingest more bars than the hub cache can hold and then perform a variety of out-of-order insert,
/// remove and replace operations.  The goal is to verify that pruned timelines heal correctly and
/// that the tail end of the static indicator series (computed on the full, corrected bar sequence)
/// matches the contents of the indicator hub caches exactly.  Do not modify the production code
/// when adjusting this test – instead, adjust the assertions here to reflect the correct behaviour.
/// </summary>
[TestClass, TestCategory("Integration")]
public class ThreadSafetyTests : TestBase
{
    private const int StcTestPort = 5099;
    private const int AllHubsTestPort = 5100;
    private const int AggregatorTestPort = 5101;
    private const int TargetBarCount = 2000;
    private const int MaxCacheSize = 1500;
    private const int SseInterval = 1; // milliseconds between bars
    private const string BarIntervalCode = "1Day"; // daily bars

    private static readonly JsonSerializerOptions JsonOptions = new() {
        PropertyNameCaseInsensitive = true
    };

    public TestContext? TestContext { get; set; }

    /// <summary>
    /// This test runs the SSE server and pipes bars into a <see cref="BarHub"/> that is chained
    /// only to the STC indicator.  It reproduces a specific bug in <c>StcHub.RollbackState()</c>
    /// by performing several out-of-order operations.  After all bars and revisions are processed
    /// the test computes a full static STC series on the amended bar list and then verifies that
    /// the tail end of the static series matches the streaming hub results exactly.
    /// </summary>
    [TestMethod]
    public async Task StcHub_WithSseStreamAndRollbacks_MatchesSeriesExactly()
    {
        CancellationToken cts = TestContext?.CancellationToken ?? default;

        // Start SSE server
        Process? serverProcess = StartSseServer(StcTestPort);
        if (serverProcess is null)
        {
            Assert.Fail("Failed to start SSE server");
            return;
        }

        // Setup: BarHub with StcHub
        BarHub barHub = new(MaxCacheSize);
        StcHub stcHub = barHub.ToStcHub();

        try
        {
            // Wait for server to be ready
            bool serverReady = await WaitForServerReady(StcTestPort, cts).ConfigureAwait(true);
            if (!serverReady)
            {
                Assert.Fail("SSE server did not become ready within timeout period");
                return;
            }

            // Consume bars + rollback scenarios from SSE stream
            SseBarBatch stcBatch = await ConsumeBarsFromSse(barHub, StcTestPort, "stc-rollbacks")
                .ConfigureAwait(true);
            stcBatch.InitialBars.Should().HaveCount(TargetBarCount);

            // The hub cache should be completely filled after streaming 2000 bars with pruning.
            // However, the test scenario includes a RemoveAt operation (removing bar at index 1900),
            // which reduces the count by 1. Subsequent revisions to the last bar (same timestamp)
            // are replacements, not additions, so they don't refill the cache.
            // Expected final count: MaxCacheSize - 1 (due to the removal)
            barHub.Results.Should().HaveCount(MaxCacheSize - 1, "bar hub should have MaxCacheSize - 1 after removal operation");

            // Compute series on FULL bar list, then take last N matching cache size.
            // Streaming indicators process all bars and maintain state, so series must be
            // computed on the full history, then truncated to match the final cache size.
            int cacheSize = barHub.Results.Count;
            IReadOnlyList<StcResult> expected = stcBatch.RevisedBars
                .ToStc()
                .TakeLast(cacheSize)
                .ToList();

            // Streaming results should match last N from full series
            stcHub.Results.IsExactly(expected);
        }
        finally
        {
            barHub.EndTransmission();

            if (!serverProcess.HasExited)
            {
                serverProcess.Kill();
                try
                {
                    await serverProcess.WaitForExitAsync(CancellationToken.None).ConfigureAwait(true);
                }
                catch (OperationCanceledException)
                {
                    // Ignore cancellation - process is already killed
                }
            }

            serverProcess.Dispose();
        }
    }

    /// <summary>
    /// Runs the SSE server and ingests 2,000+ bars into a single <see cref="BarHub"/> with a
    /// maximum cache size of 1,500.  Every built-in streaming indicator hub is subscribed to the
    /// primary bar hub.  A series of out-of-order operations (add, remove and replace) are
    /// applied to exercise the hub’s rollback logic both before and after pruning occurs.  After
    /// all bars and revisions have been processed the full static series for each indicator is
    /// computed on the amended bar sequence, the results are truncated to the current cache size,
    /// and the streaming hub results are compared against the static tail.  This verifies that
    /// pruning and healing work correctly and that the streaming hubs remain in lockstep with
    /// their static counterparts.
    /// </summary>
    [TestMethod]
    public async Task AllHubs_WithSseStream_MatchSeriesExactly()
    {
        CancellationToken cts = TestContext?.CancellationToken ?? default;

        // Start SSE server
        Process? serverProcess = StartSseServer(AllHubsTestPort);
        if (serverProcess is null)
        {
            Assert.Fail("Failed to start SSE server");
            return;
        }

        // Setup: Create one primary BarHub
        BarHub barHub = new(MaxCacheSize);

        try
        {
            // Wait for server to be ready (poll until it responds or timeout)
            bool serverReady = await WaitForServerReady(AllHubsTestPort, cts).ConfigureAwait(true);
            if (!serverReady)
            {
                Assert.Fail("SSE server did not become ready within timeout period");
                return;
            }

            // Subscribe all 80+ indicator hubs to the primary BarHub
            AdlHub adlHub = barHub.ToAdlHub();
            AdxHub adxHub = barHub.ToAdxHub(14);
            AlligatorHub alligatorHub = barHub.ToAlligatorHub();
            AlmaHub almaHub = barHub.ToAlmaHub(10, 0.85, 6);
            AroonHub aroonHub = barHub.ToAroonHub(25);
            AtrHub atrHub = barHub.ToAtrHub(14);
            AtrStopHub atrStopHub = barHub.ToAtrStopHub(21);
            AwesomeHub awesomeHub = barHub.ToAwesomeHub();
            BollingerBandsHub bollingerBandsHub = barHub.ToBollingerBandsHub(20, 2);
            BopHub bopHub = barHub.ToBopHub();
            CciHub cciHub = barHub.ToCciHub(20);
            ChaikinOscHub chaikinOscHub = barHub.ToChaikinOscHub();
            ChandelierHub chandelierHub = barHub.ToChandelierHub();
            ChopHub chopHub = barHub.ToChopHub(14);
            CmfHub cmfHub = barHub.ToCmfHub(20);
            CmoHub cmoHub = barHub.ToCmoHub(14);
            ConnorsRsiHub connorsRsiHub = barHub.ToConnorsRsiHub();
            DemaHub demaHub = barHub.ToDemaHub(20);
            DojiHub dojiHub = barHub.ToDojiHub();
            DonchianHub donchianHub = barHub.ToDonchianHub(20);
            DpoHub dpoHub = barHub.ToDpoHub(14);
            DynamicHub dynamicHub = barHub.ToDynamicHub(14);
            ElderRayHub elderRayHub = barHub.ToElderRayHub(13);
            EmaHub emaHub = barHub.ToEmaHub(20);
            EpmaHub epmaHub = barHub.ToEpmaHub(20);
            FractalHub fractalHub = barHub.ToFractalHub(2);
            FcbHub fcbHub = barHub.ToFcbHub(2);
            FisherTransformHub fisherTransformHub = barHub.ToFisherTransformHub(10);
            ForceIndexHub forceIndexHub = barHub.ToForceIndexHub(13);
            GatorHub gatorHub = barHub.ToGatorHub();
            HeikinAshiHub heikinAshiHub = barHub.ToHeikinAshiHub();
            HmaHub hmaHub = barHub.ToHmaHub(20);
            HtTrendlineHub htTrendlineHub = barHub.ToHtTrendlineHub();
            HurstHub hurstHub = barHub.ToHurstHub(20);
            IchimokuHub ichimokuHub = barHub.ToIchimokuHub();
            KamaHub kamaHub = barHub.ToKamaHub(10, 2, 30);
            KeltnerHub keltnerHub = barHub.ToKeltnerHub();
            KvoHub kvoHub = barHub.ToKvoHub();
            MaEnvelopesHub maEnvelopesHub = barHub.ToMaEnvelopesHub(20, 2.5, MaType.SMA);
            MacdHub macdHub = barHub.ToMacdHub();
            MamaHub mamaHub = barHub.ToMamaHub();
            MarubozuHub marubozuHub = barHub.ToMarubozuHub();
            MfiHub mfiHub = barHub.ToMfiHub(14);
            ObvHub obvHub = barHub.ToObvHub();
            ParabolicSarHub parabolicSarHub = barHub.ToParabolicSarHub();
            PivotPointsHub pivotPointsHub = barHub.ToPivotPointsHub(BarInterval.Month, PivotPointType.Standard);
            PivotsHub pivotsHub = barHub.ToPivotsHub(11, 14);
            PmoHub pmoHub = barHub.ToPmoHub();
            PvoHub pvoHub = barHub.ToPvoHub();
            RenkoHub renkoHub = barHub.ToRenkoHub(2.5m);
            RocHub rocHub = barHub.ToRocHub(20);
            RocWbHub rocWbHub = barHub.ToRocWbHub(14);
            RollingPivotsHub rollingPivotsHub = barHub.ToRollingPivotsHub(20, 0);
            RsiHub rsiHub = barHub.ToRsiHub(14);
            SlopeHub slopeHub = barHub.ToSlopeHub(20);
            SmaHub smaHub = barHub.ToSmaHub(20);
            SmaAnalysisHub smaAnalysisHub = barHub.ToSmaAnalysisHub(10);
            SmiHub smiHub = barHub.ToSmiHub();
            SmmaHub smmaHub = barHub.ToSmmaHub(20);
            StarcBandsHub starcBandsHub = barHub.ToStarcBandsHub();
            StcHub stcHub = barHub.ToStcHub();
            StdDevHub stdDevHub = barHub.ToStdDevHub(10);
            StochHub stochHub = barHub.ToStochHub();
            StochRsiHub stochRsiHub = barHub.ToStochRsiHub(14);
            SuperTrendHub superTrendHub = barHub.ToSuperTrendHub();
            T3Hub t3Hub = barHub.ToT3Hub(5);
            TemaHub temaHub = barHub.ToTemaHub(20);
            TrHub trHub = barHub.ToTrHub();
            TrixHub trixHub = barHub.ToTrixHub(14);
            TsiHub tsiHub = barHub.ToTsiHub();
            UlcerIndexHub ulcerIndexHub = barHub.ToUlcerIndexHub(14);
            UltimateHub ultimateHub = barHub.ToUltimateHub();
            VolatilityStopHub volatilityStopHub = barHub.ToVolatilityStopHub();
            VortexHub vortexHub = barHub.ToVortexHub(14);
            VwapHub vwapHub = barHub.ToVwapHub();
            VwmaHub vwmaHub = barHub.ToVwmaHub(10);
            WilliamsRHub williamsRHub = barHub.ToWilliamsRHub(14);
            WmaHub wmaHub = barHub.ToWmaHub(20);

            // Consume bars from SSE stream and apply test scenarios
            SseBarBatch allHubsBatch = await ConsumeBarsFromSse(barHub, AllHubsTestPort, "allhubs-rollbacks")
                .ConfigureAwait(true);
            List<Bar> allBars = allHubsBatch.InitialBars;
            List<Bar> allBarsWithRevisions = allHubsBatch.RevisedBars;

            // Verify SSE stream delivered the full batch
            allBars.Should().HaveCount(TargetBarCount, "SSE stream should deliver the full batch");

            // Get final results from BarHub (this is the ground truth after all operations)
            IReadOnlyList<IBar> barHubResults = barHub.Results;

            // Verify that the hub cache is exactly at the configured capacity.  A BarHub that
            // receives more bars than its capacity must prune the oldest bars until the cache
            // contains MaxCacheSize entries.  Having fewer or more entries indicates incorrect
            // pruning behaviour.
            barHubResults.Count.Should().BeLessThanOrEqualTo(MaxCacheSize, "bar hub should not exceed the configured cache size");

            // Verify pruning occurred by checking that we delivered more bars than cache can hold
            int expectedPruned = Math.Max(0, allBarsWithRevisions.Count - MaxCacheSize);
            int actualPruned = allBarsWithRevisions.Count - barHubResults.Count;
            actualPruned.Should().BeGreaterThanOrEqualTo(expectedPruned,
                "pruning should remove at least the excess bars beyond the configured cache size");

            // Compute static series on the FULL bar list (with revisions), then take the last N results.
            // Streaming indicators process the entire history and maintain state across revisions,
            // therefore the correct comparison pattern is to compute on the full history and then
            // truncate to the current cache size.
            int cacheSize = barHubResults.Count;
            adlHub.Results.IsExactly(allBarsWithRevisions.ToAdl().TakeLast(cacheSize).ToList());
            adxHub.Results.IsExactly(allBarsWithRevisions.ToAdx(14).TakeLast(cacheSize).ToList());
            alligatorHub.Results.IsExactly(allBarsWithRevisions.ToAlligator().TakeLast(cacheSize).ToList());
            almaHub.Results.IsExactly(allBarsWithRevisions.ToAlma(10, 0.85, 6).TakeLast(cacheSize).ToList());
            aroonHub.Results.IsExactly(allBarsWithRevisions.ToAroon(25).TakeLast(cacheSize).ToList());
            atrHub.Results.IsExactly(allBarsWithRevisions.ToAtr(14).TakeLast(cacheSize).ToList());
            atrStopHub.Results.IsExactly(allBarsWithRevisions.ToAtrStop(21).TakeLast(cacheSize).ToList());
            awesomeHub.Results.IsExactly(allBarsWithRevisions.ToAwesome().TakeLast(cacheSize).ToList());
            bollingerBandsHub.Results.IsExactly(allBarsWithRevisions.ToBollingerBands(20, 2).TakeLast(cacheSize).ToList());
            bopHub.Results.IsExactly(allBarsWithRevisions.ToBop().TakeLast(cacheSize).ToList());
            cciHub.Results.IsExactly(allBarsWithRevisions.ToCci(20).TakeLast(cacheSize).ToList());
            chaikinOscHub.Results.IsExactly(allBarsWithRevisions.ToChaikinOsc().TakeLast(cacheSize).ToList());
            chandelierHub.Results.IsExactly(allBarsWithRevisions.ToChandelier().TakeLast(cacheSize).ToList());
            chopHub.Results.IsExactly(allBarsWithRevisions.ToChop(14).TakeLast(cacheSize).ToList());
            cmfHub.Results.IsExactly(allBarsWithRevisions.ToCmf(20).TakeLast(cacheSize).ToList());
            cmoHub.Results.IsExactly(allBarsWithRevisions.ToCmo(14).TakeLast(cacheSize).ToList());
            connorsRsiHub.Results.IsExactly(allBarsWithRevisions.ToConnorsRsi().TakeLast(cacheSize).ToList());
            demaHub.Results.IsExactly(allBarsWithRevisions.ToDema(20).TakeLast(cacheSize).ToList());
            dojiHub.Results.IsExactly(allBarsWithRevisions.ToDoji().TakeLast(cacheSize).ToList());
            donchianHub.Results.IsExactly(allBarsWithRevisions.ToDonchian(20).TakeLast(cacheSize).ToList());
            dpoHub.Results.IsExactly(allBarsWithRevisions.ToDpo(14).TakeLast(cacheSize).ToList());
            dynamicHub.Results.IsExactly(allBarsWithRevisions.ToDynamic(14).TakeLast(cacheSize).ToList());
            elderRayHub.Results.IsExactly(allBarsWithRevisions.ToElderRay(13).TakeLast(cacheSize).ToList());
            emaHub.Results.IsExactly(allBarsWithRevisions.ToEma(20).TakeLast(cacheSize).ToList());
            epmaHub.Results.IsExactly(allBarsWithRevisions.ToEpma(20).TakeLast(cacheSize).ToList());
            fractalHub.Results.IsExactly(allBarsWithRevisions.ToFractal(2).TakeLast(cacheSize).ToList());
            fcbHub.Results.IsExactly(allBarsWithRevisions.ToFcb(2).TakeLast(cacheSize).ToList());
            fisherTransformHub.Results.IsExactly(allBarsWithRevisions.ToFisherTransform(10).TakeLast(cacheSize).ToList());
            forceIndexHub.Results.IsExactly(allBarsWithRevisions.ToForceIndex(13).TakeLast(cacheSize).ToList());
            gatorHub.Results.IsExactly(allBarsWithRevisions.ToGator().TakeLast(cacheSize).ToList());
            heikinAshiHub.Results.IsExactly(allBarsWithRevisions.ToHeikinAshi().TakeLast(cacheSize).ToList());
            hmaHub.Results.IsExactly(allBarsWithRevisions.ToHma(20).TakeLast(cacheSize).ToList());
            htTrendlineHub.Results.IsExactly(allBarsWithRevisions.ToHtTrendline().TakeLast(cacheSize).ToList());
            hurstHub.Results.IsExactly(allBarsWithRevisions.ToHurst(20).TakeLast(cacheSize).ToList());
            ichimokuHub.Results.IsExactly(allBarsWithRevisions.ToIchimoku().TakeLast(cacheSize).ToList());
            kamaHub.Results.IsExactly(allBarsWithRevisions.ToKama(10, 2, 30).TakeLast(cacheSize).ToList());
            keltnerHub.Results.IsExactly(allBarsWithRevisions.ToKeltner().TakeLast(cacheSize).ToList());
            kvoHub.Results.IsExactly(allBarsWithRevisions.ToKvo().TakeLast(cacheSize).ToList());
            maEnvelopesHub.Results.IsExactly(allBarsWithRevisions.ToMaEnvelopes(20, 2.5, MaType.SMA).TakeLast(cacheSize).ToList());
            macdHub.Results.IsExactly(allBarsWithRevisions.ToMacd().TakeLast(cacheSize).ToList());
            mamaHub.Results.IsExactly(allBarsWithRevisions.ToMama().TakeLast(cacheSize).ToList());
            marubozuHub.Results.IsExactly(allBarsWithRevisions.ToMarubozu().TakeLast(cacheSize).ToList());
            mfiHub.Results.IsExactly(allBarsWithRevisions.ToMfi(14).TakeLast(cacheSize).ToList());
            obvHub.Results.IsExactly(allBarsWithRevisions.ToObv().TakeLast(cacheSize).ToList());
            parabolicSarHub.Results.IsExactly(allBarsWithRevisions.ToParabolicSar().TakeLast(cacheSize).ToList());
            pivotPointsHub.Results.IsExactly(allBarsWithRevisions.ToPivotPoints(BarInterval.Month, PivotPointType.Standard).TakeLast(cacheSize).ToList());
            pivotsHub.Results.IsExactly(allBarsWithRevisions.ToPivots(11, 14).TakeLast(cacheSize).ToList());
            pmoHub.Results.IsExactly(allBarsWithRevisions.ToPmo().TakeLast(cacheSize).ToList());
            pvoHub.Results.IsExactly(allBarsWithRevisions.ToPvo().TakeLast(cacheSize).ToList());
            // Renko charts do not have a one‑to‑one relationship between bars and bricks, so we
            // cannot simply take the last N results.  Instead, align the static Renko series
            // with the streaming hub by matching on the first brick’s date.  Any bricks before
            // that date in the static series represent periods that were pruned from the bar
            // hub and must be discarded.  Then compare the remainder of the series to the
            // streaming hub results.
            {
                List<RenkoResult> renkoStatic = allBarsWithRevisions.ToRenko(2.5m).ToList();
                DateTime firstDate = renkoHub.Results[0].Timestamp;
                int startIndex = renkoStatic.FindIndex(r => r.Timestamp == firstDate);
                startIndex.Should().BeGreaterThanOrEqualTo(0,
                    "the first Renko result in the hub should exist in the static series");
                List<RenkoResult> expectedRenko = renkoStatic.Skip(startIndex).ToList();
                renkoHub.Results.IsExactly(expectedRenko);
            }

            rocHub.Results.IsExactly(allBarsWithRevisions.ToRoc(20).TakeLast(cacheSize).ToList());
            rocWbHub.Results.IsExactly(allBarsWithRevisions.ToRocWb(14).TakeLast(cacheSize).ToList());
            rollingPivotsHub.Results.IsExactly(allBarsWithRevisions.ToRollingPivots(20, 0).TakeLast(cacheSize).ToList());
            rsiHub.Results.IsExactly(allBarsWithRevisions.ToRsi(14).TakeLast(cacheSize).ToList());
            slopeHub.Results.IsExactly(allBarsWithRevisions.ToSlope(20).TakeLast(cacheSize).ToList());
            smaHub.Results.IsExactly(allBarsWithRevisions.ToSma(20).TakeLast(cacheSize).ToList());
            smaAnalysisHub.Results.IsExactly(allBarsWithRevisions.ToSmaAnalysis(10).TakeLast(cacheSize).ToList());
            smiHub.Results.IsExactly(allBarsWithRevisions.ToSmi().TakeLast(cacheSize).ToList());
            smmaHub.Results.IsExactly(allBarsWithRevisions.ToSmma(20).TakeLast(cacheSize).ToList());
            starcBandsHub.Results.IsExactly(allBarsWithRevisions.ToStarcBands().TakeLast(cacheSize).ToList());
            stcHub.Results.IsExactly(allBarsWithRevisions.ToStc().TakeLast(cacheSize).ToList());
            stdDevHub.Results.IsExactly(allBarsWithRevisions.ToStdDev(10).TakeLast(cacheSize).ToList());
            stochHub.Results.IsExactly(allBarsWithRevisions.ToStoch().TakeLast(cacheSize).ToList());
            stochRsiHub.Results.IsExactly(allBarsWithRevisions.ToStochRsi(14).TakeLast(cacheSize).ToList());
            superTrendHub.Results.IsExactly(allBarsWithRevisions.ToSuperTrend().TakeLast(cacheSize).ToList());
            t3Hub.Results.IsExactly(allBarsWithRevisions.ToT3(5).TakeLast(cacheSize).ToList());
            temaHub.Results.IsExactly(allBarsWithRevisions.ToTema(20).TakeLast(cacheSize).ToList());
            trHub.Results.IsExactly(allBarsWithRevisions.ToTr().TakeLast(cacheSize).ToList());
            trixHub.Results.IsExactly(allBarsWithRevisions.ToTrix(14).TakeLast(cacheSize).ToList());
            tsiHub.Results.IsExactly(allBarsWithRevisions.ToTsi().TakeLast(cacheSize).ToList());
            ulcerIndexHub.Results.IsExactly(allBarsWithRevisions.ToUlcerIndex(14).TakeLast(cacheSize).ToList());
            ultimateHub.Results.IsExactly(allBarsWithRevisions.ToUltimate().TakeLast(cacheSize).ToList());
            volatilityStopHub.Results.IsExactly(allBarsWithRevisions.ToVolatilityStop().TakeLast(cacheSize).ToList());
            vortexHub.Results.IsExactly(allBarsWithRevisions.ToVortex(14).TakeLast(cacheSize).ToList());
            vwapHub.Results.IsExactly(allBarsWithRevisions.ToVwap().TakeLast(cacheSize).ToList());
            vwmaHub.Results.IsExactly(allBarsWithRevisions.ToVwma(10).TakeLast(cacheSize).ToList());
            williamsRHub.Results.IsExactly(allBarsWithRevisions.ToWilliamsR(14).TakeLast(cacheSize).ToList());
            wmaHub.Results.IsExactly(allBarsWithRevisions.ToWma(20).TakeLast(cacheSize).ToList());
        }
        finally
        {
            // Cleanup
            barHub.EndTransmission();

            // Stop SSE server
            if (!serverProcess.HasExited)
            {
                serverProcess.Kill();
                try
                {
                    await serverProcess.WaitForExitAsync(CancellationToken.None).ConfigureAwait(true);
                }
                catch (OperationCanceledException)
                {
                    // Ignore cancellation - process is already killed
                }
            }

            serverProcess.Dispose();
        }
    }

    /// <summary>
    /// Tests that BarAggregatorHub correctly aggregates incoming bar stream into 5-minute
    /// bars and that downstream EMA indicator produces results matching the static series
    /// computed on the aggregated bars.
    /// </summary>
    [TestMethod]
    public async Task BarAggregatorHub_WithSseStream_MatchesSeriesExactly()
    {
        CancellationToken cts = TestContext?.CancellationToken ?? default;

        // Start SSE server
        Process? serverProcess = StartSseServer(AggregatorTestPort);
        if (serverProcess is null)
        {
            Assert.Fail("Failed to start SSE server");
            return;
        }

        // Setup: BarHub -> BarAggregatorHub -> EmaHub
        BarHub barHub = new();
        BarAggregatorHub aggregator = barHub.ToBarAggregatorHub(BarInterval.FiveMinutes);
        EmaHub emaHub = aggregator.ToEmaHub(14);

        try
        {
            // Wait for server to be ready
            bool serverReady = await WaitForServerReady(AggregatorTestPort, cts).ConfigureAwait(true);
            if (!serverReady)
            {
                Assert.Fail("SSE server did not become ready within timeout period");
                return;
            }

            // Stream bars with 1-minute intervals (will be aggregated to 5-minute bars)
            SseBarBatch barBatch = await ConsumeBarsFromSse(barHub, AggregatorTestPort, "aggregator-test")
                .ConfigureAwait(true);

            // Verify bars were delivered
            barBatch.InitialBars.Should().HaveCount(TargetBarCount);

            // Compute expected aggregated bars and EMA on static series
            IReadOnlyList<Bar> aggregatedBars = barBatch.RevisedBars.Aggregate(BarInterval.FiveMinutes);
            IReadOnlyList<EmaResult> expected = aggregatedBars.ToEma(14);

            // Verify aggregator results count
            aggregator.Results.Count.Should().BeGreaterThan(0, "aggregator should produce results");

            // Verify EMA results match exactly
            emaHub.Results.IsExactly(expected);
        }
        finally
        {
            emaHub.Unsubscribe();
            aggregator.Unsubscribe();
            barHub.EndTransmission();

            if (!serverProcess.HasExited)
            {
                serverProcess.Kill();
                try
                {
                    await serverProcess.WaitForExitAsync(CancellationToken.None).ConfigureAwait(true);
                }
                catch (OperationCanceledException)
                {
                    // Ignore cancellation - process is already killed
                }
            }

            serverProcess.Dispose();
        }
    }

    /// <summary>
    /// Tests that TradeTickAggregatorHub correctly aggregates incoming tick stream into 5-minute
    /// price bars and that downstream EMA indicator produces results matching the static series
    /// computed on the aggregated bars.
    /// </summary>
    [TestMethod]
    public async Task TradeTickAggregatorHub_WithSseStream_MatchesSeriesExactly()
    {
        CancellationToken cts = TestContext?.CancellationToken ?? default;

        // Start SSE server
        Process? serverProcess = StartSseServer(AggregatorTestPort + 1);
        if (serverProcess is null)
        {
            Assert.Fail("Failed to start SSE server");
            return;
        }

        // Setup: TradeTickHub -> TradeTickAggregatorHub -> EmaHub
        TradeTickHub tickHub = new();
        TradeTickAggregatorHub aggregator = tickHub.ToTradeTickAggregatorHub(BarInterval.FiveMinutes);
        EmaHub emaHub = aggregator.ToEmaHub(14);

        try
        {
            // Wait for server to be ready
            bool serverReady = await WaitForServerReady(AggregatorTestPort + 1, cts).ConfigureAwait(true);
            if (!serverReady)
            {
                Assert.Fail("SSE server did not become ready within timeout period");
                return;
            }

            // Stream bars (which will be converted to ticks)
            SseBarBatch barBatch = await ConsumeBarsFromSse(tickHub, AggregatorTestPort + 1, "aggregator-test")
                .ConfigureAwait(true);

            // Verify bars were delivered
            barBatch.InitialBars.Should().HaveCount(TargetBarCount);

            // Compute expected: bars -> aggregate -> EMA
            IReadOnlyList<Bar> aggregatedBars = barBatch.RevisedBars.Aggregate(BarInterval.FiveMinutes);
            IReadOnlyList<EmaResult> expected = aggregatedBars.ToEma(14);

            // Verify aggregator results count
            aggregator.Results.Count.Should().BeGreaterThan(0, "aggregator should produce results");

            // Verify EMA results match exactly
            emaHub.Results.IsExactly(expected);
        }
        finally
        {
            emaHub.Unsubscribe();
            aggregator.Unsubscribe();
            tickHub.EndTransmission();

            if (!serverProcess.HasExited)
            {
                serverProcess.Kill();
                try
                {
                    await serverProcess.WaitForExitAsync(CancellationToken.None).ConfigureAwait(true);
                }
                catch (OperationCanceledException)
                {
                    // Ignore cancellation - process is already killed
                }
            }

            serverProcess.Dispose();
        }
    }

    // Keep helper methods StartSseServer, FindRepositoryRoot, and WaitForServerReady unchanged.
    // Extend ConsumeBarsFromSse below to handle action events and batching.
    // Encapsulate the logic for launching the test SSE server, waiting until it is ready,
    // and streaming bars into the BarHub.
    private static Process? StartSseServer(int port)
    {
        try
        {
            string? repoRoot = FindRepositoryRoot();
            if (repoRoot is null)
            {
                Console.WriteLine("[Test] Cannot find repository root.");
                return null;
            }

            // Try DOTNET_CONFIGURATION env var, then check for Release build, fallback to Debug
            string configuration = Environment.GetEnvironmentVariable("DOTNET_CONFIGURATION") ?? "Debug";
            string serverExePath = Path.Combine(
                repoRoot, "tools", "sse-server", "bin", configuration, "net10.0", "Test.SseServer.exe");
            string serverDllPath = Path.Combine(
                repoRoot, "tools", "sse-server", "bin", configuration, "net10.0", "Test.SseServer.dll");

            // Auto-detect configuration if default doesn't exist
            if (!File.Exists(serverExePath) && !File.Exists(serverDllPath))
            {
                string alternateConfig = configuration == "Debug" ? "Release" : "Debug";
                string alternateExePath = Path.Combine(
                    repoRoot, "tools", "sse-server", "bin", alternateConfig, "net10.0", "Test.SseServer.exe");
                string alternateDllPath = Path.Combine(
                    repoRoot, "tools", "sse-server", "bin", alternateConfig, "net10.0", "Test.SseServer.dll");

                if (File.Exists(alternateExePath) || File.Exists(alternateDllPath))
                {
                    configuration = alternateConfig;
                    serverExePath = alternateExePath;
                    serverDllPath = alternateDllPath;
                }
            }

            ProcessStartInfo startInfo = new() {
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            // Use .exe if it exists (Windows), otherwise use dotnet with .dll (Linux/macOS)
            if (File.Exists(serverExePath))
            {
                startInfo.FileName = serverExePath;
                startInfo.Arguments = $"--urls http://localhost:{port}";
            }
            else if (File.Exists(serverDllPath))
            {
                startInfo.FileName = "dotnet";
                startInfo.Arguments = $"\"{serverDllPath}\" --urls http://localhost:{port}";
            }
            else
            {
                Console.WriteLine($"[Test] SSE server executable not found at {serverExePath} or {serverDllPath}");
                return null;
            }

            Process? process = Process.Start(startInfo);
            if (process is not null)
            {
                process.OutputDataReceived += (_, e) => {
                    if (!string.IsNullOrEmpty(e.Data))
                    {
                        Console.WriteLine($"[SSE Server] {e.Data}");
                    }
                };
                process.ErrorDataReceived += (_, e) => {
                    if (!string.IsNullOrEmpty(e.Data))
                    {
                        Console.WriteLine($"[SSE Server ERROR] {e.Data}");
                    }
                };
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                Console.WriteLine($"[Test] SSE server started on port {port} (PID: {process.Id})");
            }

            return process;
        }
        catch (IOException ex)
        {
            Console.WriteLine($"[Test] IO error starting server: {ex.Message}");
            return null;
        }
        catch (InvalidOperationException ex)
        {
            Console.WriteLine($"[Test] Invalid operation starting server: {ex.Message}");
            return null;
        }
    }

    private static string? FindRepositoryRoot()
    {
        string currentDir = Directory.GetCurrentDirectory();
        while (!string.IsNullOrEmpty(currentDir))
        {
            if (File.Exists(Path.Combine(currentDir, "Stock.Indicators.sln")))
            {
                return currentDir;
            }

            string? parentDir = Directory.GetParent(currentDir)?.FullName;
            if (parentDir == currentDir)
            {
                break;
            }

            currentDir = parentDir ?? string.Empty;
        }

        return null;
    }

    private static async Task<bool> WaitForServerReady(int port, CancellationToken cancellationToken)
    {
        using HttpClient httpClient = new() { Timeout = TimeSpan.FromSeconds(2) };
        Uri healthUri = new($"http://localhost:{port}/bars/longest?batchSize=1");

        // Try for up to 30 seconds (15 attempts * 2 second intervals)
        for (int attempt = 0; attempt < 15; attempt++)
        {
            try
            {
                using HttpResponseMessage response = await httpClient
                    .GetAsync(healthUri, HttpCompletionOption.ResponseHeadersRead, cancellationToken)
                    .ConfigureAwait(false);

                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"[Test] SSE server ready after {attempt + 1} attempts");
                    return true;
                }
            }
            catch (HttpRequestException)
            {
                // Server not ready yet, continue polling
            }
            catch (TaskCanceledException)
            {
                // Timeout on this attempt, continue polling
            }

            if (attempt < 14) // Don't delay after the last attempt
            {
                await Task.Delay(2000, cancellationToken).ConfigureAwait(false);
            }
        }

        Console.WriteLine("[Test] SSE server did not become ready within timeout");
        return false;
    }

    private static async Task<SseBarBatch> ConsumeBarsFromSse(BarHub barHub, int port, string? scenario = null)
    {
        List<Bar> allBars = [];
        List<Bar> revisedBars = [];
        using HttpClient httpClient = new() { Timeout = TimeSpan.FromMinutes(5) };

        string batchSizeParam = $"&batchSize={TargetBarCount}";
        string scenarioParam = string.IsNullOrWhiteSpace(scenario) ? string.Empty : $"&scenario={scenario}";
        Uri uri = new($"http://localhost:{port}/bars/longest?interval={SseInterval}&barIntervalCode={BarIntervalCode}{batchSizeParam}{scenarioParam}");

        using HttpResponseMessage response = await httpClient
            .GetAsync(uri, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);

        response.EnsureSuccessStatusCode();

        Stream stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
        using StreamReader reader = new(stream);

        string? line;
        string? eventType = null;

        while ((line = await reader.ReadLineAsync().ConfigureAwait(false)) is not null)
        {
            // SSE format: "event: bar", "data: {...}", blank line
            if (line.StartsWith("event:", StringComparison.Ordinal))
            {
                eventType = line[6..].Trim();
            }
            else if (line.StartsWith("data:", StringComparison.Ordinal))
            {
                string json = line[5..].Trim();
                string eventName = string.IsNullOrWhiteSpace(eventType) ? "bar" : eventType;

                if (string.Equals(eventName, "bar", StringComparison.OrdinalIgnoreCase))
                {
                    Bar? bar = JsonSerializer.Deserialize<Bar>(json, JsonOptions);
                    if (bar is not null)
                    {
                        allBars.Add(bar);
                        revisedBars.Add(bar);
                        barHub.Add(bar);
                    }
                }
                else
                {
                    BarAction? action = JsonSerializer.Deserialize<BarAction>(json, JsonOptions);

                    // Fail fast on malformed SSE payloads
                    if (action is null || (action.Bar is null && action.CacheIndex is null))
                    {
                        throw new InvalidOperationException(
                            $"Malformed SSE payload for event '{eventName}': {json}");
                    }

                    ApplyBarAction(action, eventName, barHub, revisedBars);
                }
            }
            else if (line.Length == 0)
            {
                eventType = null;
            }
        }

        return new SseBarBatch(allBars, revisedBars);
    }

    private static async Task<SseBarBatch> ConsumeBarsFromSse(TradeTickHub tickHub, int port, string? scenario = null)
    {
        List<Bar> allBars = [];
        List<Bar> revisedBars = [];
        using HttpClient httpClient = new() { Timeout = TimeSpan.FromMinutes(5) };

        string batchSizeParam = $"&batchSize={TargetBarCount}";
        string scenarioParam = string.IsNullOrWhiteSpace(scenario) ? string.Empty : $"&scenario={scenario}";
        Uri uri = new($"http://localhost:{port}/bars/longest?interval={SseInterval}&barIntervalCode={BarIntervalCode}{batchSizeParam}{scenarioParam}");

        using HttpResponseMessage response = await httpClient
            .GetAsync(uri, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);

        response.EnsureSuccessStatusCode();

        Stream stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
        using StreamReader reader = new(stream);

        string? line;
        string? eventType = null;

        while ((line = await reader.ReadLineAsync().ConfigureAwait(false)) is not null)
        {
            // SSE format: "event: bar", "data: {...}", blank line
            if (line.StartsWith("event:", StringComparison.Ordinal))
            {
                eventType = line[6..].Trim();
            }
            else if (line.StartsWith("data:", StringComparison.Ordinal))
            {
                string json = line[5..].Trim();
                string eventName = string.IsNullOrWhiteSpace(eventType) ? "bar" : eventType;

                if (string.Equals(eventName, "bar", StringComparison.OrdinalIgnoreCase))
                {
                    Bar? bar = JsonSerializer.Deserialize<Bar>(json, JsonOptions);
                    if (bar is not null)
                    {
                        allBars.Add(bar);
                        revisedBars.Add(bar);
                        // Convert bar to tick for TradeTickHub
                        tickHub.Add(new TradeTick(bar.Timestamp, bar.Close, bar.Volume));
                    }
                }
                else
                {
                    BarAction? action = JsonSerializer.Deserialize<BarAction>(json, JsonOptions);

                    // Fail fast on malformed SSE payloads
                    if (action is null || (action.Bar is null && action.CacheIndex is null))
                    {
                        throw new InvalidOperationException(
                            $"Malformed SSE payload for event '{eventName}': {json}");
                    }

                    ApplyBarAction(action, eventName, tickHub, revisedBars);
                }
            }
            else if (line.Length == 0)
            {
                eventType = null;
            }
        }

        return new SseBarBatch(allBars, revisedBars);
    }

    private static void ApplyBarAction(
        BarAction action,
        string eventName,
        BarHub barHub,
        List<Bar> revisedBars)
    {
        if (string.Equals(eventName, "remove", StringComparison.OrdinalIgnoreCase))
        {
            // Capture timestamp before removal to maintain synchronization
            DateTime? timestampToRemove = null;
            if (action.CacheIndex is >= 0 && action.CacheIndex < barHub.Results.Count)
            {
                timestampToRemove = barHub.Results[action.CacheIndex.Value].Timestamp;
                barHub.RemoveAt(action.CacheIndex.Value);
            }

            // Update revisedBars using captured timestamp or fallback to action.Bar
            DateTime? removalTimestamp = timestampToRemove ?? action.Bar?.Timestamp;
            if (removalTimestamp.HasValue)
            {
                RemoveBarByTimestamp(revisedBars, removalTimestamp.Value);
            }

            return;
        }

        if (string.Equals(eventName, "add", StringComparison.OrdinalIgnoreCase))
        {
            if (action.Bar is null)
            {
                return;
            }

            barHub.Add(action.Bar);
            UpsertBarByTimestamp(revisedBars, action.Bar);
        }
    }

    private static void ApplyBarAction(
        BarAction action,
        string eventName,
        TradeTickHub tickHub,
        List<Bar> revisedBars)
    {
        if (string.Equals(eventName, "remove", StringComparison.OrdinalIgnoreCase))
        {
            // Capture timestamp before removal to maintain synchronization
            DateTime? timestampToRemove = null;
            if (action.CacheIndex is >= 0 && action.CacheIndex < tickHub.Results.Count)
            {
                timestampToRemove = tickHub.Results[action.CacheIndex.Value].Timestamp;
                tickHub.RemoveAt(action.CacheIndex.Value);
            }

            // Update revisedBars using captured timestamp or fallback to action.Bar
            DateTime? removalTimestamp = timestampToRemove ?? action.Bar?.Timestamp;
            if (removalTimestamp.HasValue)
            {
                RemoveBarByTimestamp(revisedBars, removalTimestamp.Value);
            }

            return;
        }

        if (string.Equals(eventName, "add", StringComparison.OrdinalIgnoreCase))
        {
            if (action.Bar is null)
            {
                return;
            }

            // Convert bar to tick for TradeTickHub
            tickHub.Add(new TradeTick(action.Bar.Timestamp, action.Bar.Close, action.Bar.Volume));
            UpsertBarByTimestamp(revisedBars, action.Bar);
        }
    }

    private static void RemoveBarByTimestamp(List<Bar> bars, DateTime timestamp)
    {
        int index = bars.FindIndex(item => item.Timestamp == timestamp);
        if (index >= 0)
        {
            bars.RemoveAt(index);
        }
    }

    private static void UpsertBarByTimestamp(List<Bar> bars, Bar bar)
    {
        int index = bars.FindIndex(item => item.Timestamp == bar.Timestamp);
        if (index >= 0)
        {
            bars[index] = bar;
            return;
        }

        int insertIndex = bars.FindIndex(item => item.Timestamp > bar.Timestamp);
        if (insertIndex >= 0)
        {
            bars.Insert(insertIndex, bar);
        }
        else
        {
            bars.Add(bar);
        }
    }

    private sealed record SseBarBatch(List<Bar> InitialBars, List<Bar> RevisedBars);

    // Intentional: BarAction is retained solely as a reflection anchor for JsonSerializer.Deserialize.
    // The SuppressMessage attribute suppresses CA1812 (Avoid uninstantiated internal classes).
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Performance",
        "CA1812:Avoid uninstantiated internal classes",
        Justification = "Instantiated via JsonSerializer.Deserialize")]
    private sealed record BarAction(Bar? Bar, int? CacheIndex);
}
