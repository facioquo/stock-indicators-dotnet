using System.Globalization;
using Coinbase.Net.Clients;
using Coinbase.Net.Objects.Models;
using CryptoExchange.Net.Objects;
using CryptoExchange.Net.Objects.Sockets;
using FacioQuo.Stock.Indicators;

namespace Test.Simulation;

/// <summary>
/// Stress test for hubs that were modified in PR #1927 to address issue #1925.
/// Tests STC, Slope, EPMA and other hubs with pruning scenarios using live Coinbase data.
/// </summary>
internal sealed class HubStressTest : IDisposable
{
    private readonly string _symbol;
    private readonly int _targetCount;
    private readonly int _maxCacheSize;
    private readonly CoinbaseSocketClient _socketClient;

    // Lock object for thread-safe access - required per documented threading model
    private readonly object _hubLock = new();

    // Primary bar hub with constrained cache size to force pruning
    private readonly BarHub _barHub;

    // Hubs modified in PR #1927 - these need special attention:
    private readonly StcHub _stcHub;           // Refactored to chain MacdHub
    private readonly SlopeHub _slopeHub;       // Removed lock wrapper from OnAdd
    private readonly EpmaHub _epmaHub;         // Removed _globalIndexOffset

    // Additional hubs with internal state arrays that have PruneState overrides:
    private readonly ConnorsRsiHub _connorsRsiHub;
    private readonly FisherTransformHub _fisherTransformHub;
    private readonly HtTrendlineHub _htTrendlineHub;
    private readonly MamaHub _mamaHub;

    // Other commonly used hubs for comparison:
    private readonly EmaHub _emaHub;
    private readonly SmaHub _smaHub;
    private readonly RsiHub _rsiHub;
    private readonly MacdHub _macdHub;
    private readonly BollingerBandsHub _bollingerHub;
    private readonly AtrHub _atrHub;

    private int _barsProcessed;
    private int _pruneEvents;
    private readonly List<string> _errors = [];

    public HubStressTest(string symbol, int targetCount, int maxCacheSize = 500)
    {
        _symbol = symbol;
        _targetCount = targetCount;
        _maxCacheSize = maxCacheSize;
        _socketClient = new CoinbaseSocketClient();

        // Create BarHub with constrained cache size to force pruning
        _barHub = new BarHub(maxCacheSize);

        // Subscribe all hubs modified in PR #1927
        _stcHub = _barHub.ToStcHub();                // STC: ChainHub<MacdResult, StcResult>
        _slopeHub = _barHub.ToSlopeHub(20);          // Slope: removed lock wrapper
        _epmaHub = _barHub.ToEpmaHub(20);            // EPMA: removed _globalIndexOffset

        // Hubs with PruneState overrides (modified to use DateTime signature)
        _connorsRsiHub = _barHub.ToConnorsRsiHub();
        _fisherTransformHub = _barHub.ToFisherTransformHub(10);
        _htTrendlineHub = _barHub.ToHtTrendlineHub();
        _mamaHub = _barHub.ToMamaHub();

        // Reference hubs for baseline comparison
        _emaHub = _barHub.ToEmaHub(20);
        _smaHub = _barHub.ToSmaHub(20);
        _rsiHub = _barHub.ToRsiHub(14);
        _macdHub = _barHub.ToMacdHub();
        _bollingerHub = _barHub.ToBollingerBandsHub(20, 2);
        _atrHub = _barHub.ToAtrHub(14);
    }

    public async Task RunAsync()
    {
        try
        {
            Console.WriteLine("==========================================");
            Console.WriteLine("  HUB STRESS TEST ");
            Console.WriteLine("==========================================");
            Console.WriteLine($"Symbol: {_symbol}");
            Console.WriteLine($"Target bars: {_targetCount}");
            Console.WriteLine($"Max cache size: {_maxCacheSize}");
            Console.WriteLine();
            Console.WriteLine("Testing hubs modified in PR #1927:");
            Console.WriteLine("  - StcHub (refactored to chain MacdHub)");
            Console.WriteLine("  - SlopeHub (removed lock wrapper from OnAdd)");
            Console.WriteLine("  - EpmaHub (removed _globalIndexOffset)");
            Console.WriteLine("  - ConnorsRsiHub, FisherTransformHub, HtTrendlineHub, MamaHub (PruneState changes)");
            Console.WriteLine();
            Console.WriteLine($"Cache will prune when bars exceed {_maxCacheSize}");
            Console.WriteLine("==========================================");
            Console.WriteLine();

            TaskCompletionSource<bool> completionSource = new();

            // Use ticker mode for faster data arrival (trades come every few seconds)
            Console.WriteLine("[HubStressTest] Subscribing to trade feed for rapid updates...");
            Console.WriteLine("[HubStressTest] ⚠️  Trades arrive every few seconds - this tests real async concurrency");
            Console.WriteLine();

            WebSocketResult<UpdateSubscription> subscription =
                await _socketClient.AdvancedTradeApi
                    .SubscribeToTradeUpdatesAsync(
                        _symbol,
                        onData => ProcessTradeUpdate(onData.Data, completionSource),
                        ct: default)
                    .ConfigureAwait(false);

            if (!subscription.Success || subscription.Data is null)
            {
                string errorMsg = subscription.Error?.Message ?? "Unknown error";
                Console.WriteLine($"[HubStressTest] Failed to subscribe: {errorMsg}");
                return;
            }

            Console.WriteLine($"[HubStressTest] Successfully subscribed to {_symbol} trades");
            Console.WriteLine("[HubStressTest] Processing bars and monitoring for errors...");
            Console.WriteLine();

            await completionSource.Task.ConfigureAwait(false);
            await subscription.Data.CloseAsync().ConfigureAwait(false);

            Console.WriteLine();
            PrintSummary();
        }
        catch (TaskCanceledException)
        {
            Console.WriteLine("[HubStressTest] Operation was cancelled");
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("[HubStressTest] Operation was cancelled");
        }
        catch (ArgumentOutOfRangeException ex)
        {
            Console.WriteLine($"[HubStressTest] Error: {ex.GetType().Name} - {ex.Message}");
            _errors.Add($"Fatal: {ex.GetType().Name} - {ex.Message}");
        }
        catch (InvalidOperationException ex)
        {
            Console.WriteLine($"[HubStressTest] Error: {ex.GetType().Name} - {ex.Message}");
            _errors.Add($"Fatal: {ex.GetType().Name} - {ex.Message}");
        }
    }

    public void Dispose() => _socketClient.Dispose();

    private void ProcessTradeUpdate(CoinbaseTrade[] trades, TaskCompletionSource<bool> completionSource)
    {
        // Lock the entire callback - WebSocket library may invoke this concurrently
        lock (_hubLock)
        {
            foreach (CoinbaseTrade trade in trades)
            {
                try
                {
                    // Convert trade to Bar
                    Bar bar = new(
                        Timestamp: trade.Timestamp,
                        Open: trade.Price,
                        High: trade.Price,
                        Low: trade.Price,
                        Close: trade.Price,
                        Volume: trade.Quantity);

                    int cacheSizeBefore = _barHub.Results.Count;

                    // This is the critical operation that can fail with threading issues
                    _barHub.Add(bar);

                    int cacheSizeAfter = _barHub.Results.Count;

                    // Detect pruning
                    if (cacheSizeAfter < cacheSizeBefore + 1)
                    {
                        _pruneEvents++;
                    }

                    _barsProcessed++;

                    // Verify hub state consistency after each add
                    VerifyHubConsistency();

                    // Progress output
                    if (_barsProcessed % 50 == 0 || _barsProcessed >= _targetCount)
                    {
                        string countDisplay = _targetCount == int.MaxValue ? "∞" : _targetCount.ToString(CultureInfo.InvariantCulture);
                        Console.WriteLine($"[{DateTime.UtcNow:HH:mm:ss}] Processed: {_barsProcessed}/{countDisplay} | Cache: {cacheSizeAfter} | Prunes: {_pruneEvents} | Errors: {_errors.Count}");
                    }

                    if (_barsProcessed >= _targetCount)
                    {
                        completionSource.TrySetResult(true);
                        return;
                    }
                }
                catch (ArgumentOutOfRangeException ex)
                {
                    string error = $"ArgumentOutOfRangeException at bar {_barsProcessed}: {ex.Message}";
                    Console.WriteLine($"[HubStressTest] ❌ {error}");
                    _errors.Add(error);
                    completionSource.TrySetException(ex);
                    return;
                }
                catch (IndexOutOfRangeException ex)
                {
                    string error = $"IndexOutOfRangeException at bar {_barsProcessed}: {ex.Message}";
                    Console.WriteLine($"[HubStressTest] ❌ {error}");
                    _errors.Add(error);
                    completionSource.TrySetException(ex);
                    return;
                }
                catch (InvalidOperationException ex)
                {
                    string error = $"InvalidOperationException at bar {_barsProcessed}: {ex.Message}";
                    Console.WriteLine($"[HubStressTest] ❌ {error}");
                    _errors.Add(error);
                    completionSource.TrySetException(ex);
                    return;
                }
            }
        }
    }

    private void VerifyHubConsistency()
    {
        // Verify all hub result counts match or are within expected bounds
        int barCount = _barHub.Results.Count;

        // All hubs should have the same count as bars (1:1 mapping)
        VerifyHubCount("STC", _stcHub.Results.Count, barCount);
        VerifyHubCount("Slope", _slopeHub.Results.Count, barCount);
        VerifyHubCount("EPMA", _epmaHub.Results.Count, barCount);
        VerifyHubCount("ConnorsRsi", _connorsRsiHub.Results.Count, barCount);
        VerifyHubCount("FisherTransform", _fisherTransformHub.Results.Count, barCount);
        VerifyHubCount("HtTrendline", _htTrendlineHub.Results.Count, barCount);
        VerifyHubCount("Mama", _mamaHub.Results.Count, barCount);
        VerifyHubCount("EMA", _emaHub.Results.Count, barCount);
        VerifyHubCount("SMA", _smaHub.Results.Count, barCount);
        VerifyHubCount("RSI", _rsiHub.Results.Count, barCount);
        VerifyHubCount("MACD", _macdHub.Results.Count, barCount);
        VerifyHubCount("Bollinger", _bollingerHub.Results.Count, barCount);
        VerifyHubCount("ATR", _atrHub.Results.Count, barCount);

        // Verify STC's chained MacdHub is in sync
        // STC chains from MacdHub, so we verify the chain relationship
        if (_stcHub.Results.Count > 0 && _macdHub.Results.Count > 0)
        {
            DateTime stcLast = _stcHub.Results[^1].Timestamp;
            DateTime macdLast = _macdHub.Results[^1].Timestamp;
            if (stcLast != macdLast)
            {
                _errors.Add($"STC/MACD timestamp mismatch: STC={stcLast}, MACD={macdLast}");
            }
        }
    }

    private void VerifyHubCount(string hubName, int hubCount, int expectedCount)
    {
        if (hubCount != expectedCount)
        {
            string error = $"{hubName} count mismatch: expected {expectedCount}, got {hubCount}";
            _errors.Add(error);
        }
    }

    private void PrintSummary()
    {
        Console.WriteLine("==========================================");
        Console.WriteLine("  HUB STRESS TEST SUMMARY");
        Console.WriteLine("==========================================");
        Console.WriteLine($"Total bars processed: {_barsProcessed}");
        Console.WriteLine($"Final cache size: {_barHub.Results.Count}");
        Console.WriteLine($"Prune events: {_pruneEvents}");
        Console.WriteLine();

        Console.WriteLine("Hub Result Counts:");
        Console.WriteLine($"  BarHub:        {_barHub.Results.Count}");
        Console.WriteLine($"  StcHub:          {_stcHub.Results.Count}");
        Console.WriteLine($"  SlopeHub:        {_slopeHub.Results.Count}");
        Console.WriteLine($"  EpmaHub:         {_epmaHub.Results.Count}");
        Console.WriteLine($"  ConnorsRsiHub:   {_connorsRsiHub.Results.Count}");
        Console.WriteLine($"  FisherTransform: {_fisherTransformHub.Results.Count}");
        Console.WriteLine($"  HtTrendlineHub:  {_htTrendlineHub.Results.Count}");
        Console.WriteLine($"  MamaHub:         {_mamaHub.Results.Count}");
        Console.WriteLine($"  EmaHub:          {_emaHub.Results.Count}");
        Console.WriteLine($"  SmaHub:          {_smaHub.Results.Count}");
        Console.WriteLine($"  RsiHub:          {_rsiHub.Results.Count}");
        Console.WriteLine($"  MacdHub:         {_macdHub.Results.Count}");
        Console.WriteLine($"  BollingerHub:    {_bollingerHub.Results.Count}");
        Console.WriteLine($"  AtrHub:          {_atrHub.Results.Count}");
        Console.WriteLine();

        if (_errors.Count == 0)
        {
            Console.WriteLine("✅ TEST PASSED - No errors detected!");
            Console.WriteLine("   All hubs maintained consistency through pruning.");
        }
        else
        {
            Console.WriteLine($"❌ TEST FAILED - {_errors.Count} errors detected:");
            foreach (string error in _errors.Take(20))
            {
                Console.WriteLine($"   - {error}");
            }

            if (_errors.Count > 20)
            {
                Console.WriteLine($"   ... and {_errors.Count - 20} more errors");
            }
        }

        Console.WriteLine("==========================================");
    }
}
