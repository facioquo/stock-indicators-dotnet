using System.Globalization;
using Coinbase.Net.Clients;
using Coinbase.Net.Objects.Models;
using CryptoExchange.Net.Objects;
using CryptoExchange.Net.Objects.Sockets;
using FacioQuo.Stock.Indicators;

namespace Test.Simulation;

internal enum CoinbaseMode
{
    Ticker,
    Klines
}

internal sealed class CoinbaseStrategy : IDisposable
{
    private readonly string _symbol;
    private readonly int _targetCount;
    private readonly CoinbaseMode _mode;
    private readonly CoinbaseSocketClient _socketClient;

    private readonly BarHub _barHub;
    private readonly EmaHub _fastEma;
    private readonly EmaHub _slowEma;

    // Lock object for thread-safe access to indicators
    private readonly object _hubLock = new();

    private double _balance = 10000.0;
    private double _units;
    private double _entryValue;
    private int _totalTrades;
    private int _winningTrades;
    private int _losingTrades;
    private double _largestWin;
    private double _largestLoss;
    private readonly List<double> _tradeResults = [];

    private int _barsProcessed;

    private const int FastPeriod = 50;
    private const int SlowPeriod = 200;

    public CoinbaseStrategy(string symbol, int targetCount, CoinbaseMode mode = CoinbaseMode.Klines)
    {
        _symbol = symbol;
        _targetCount = targetCount;
        _mode = mode;
        _socketClient = new CoinbaseSocketClient();

        // Use default cache size (100,000 items)
        _barHub = new BarHub();
        _fastEma = _barHub.ToEmaHub(FastPeriod);
        _slowEma = _barHub.ToEmaHub(SlowPeriod);
    }

    public async Task RunAsync()
    {
        try
        {
            Console.WriteLine($"[CoinbaseStrategy] Connecting to Coinbase WebSocket for {_symbol}");

            TaskCompletionSource<bool> completionSource = new();
            WebSocketResult<UpdateSubscription> subscription;

            if (_mode == CoinbaseMode.Ticker)
            {
                Console.WriteLine("[CoinbaseStrategy] Subscribing to trade feed (individual trades in real-time)");
                Console.WriteLine("[CoinbaseStrategy] ℹ️  Trade updates arrive every few seconds with actual market trades");
                Console.WriteLine();

                subscription = await _socketClient.AdvancedTradeApi
                    .SubscribeToTradeUpdatesAsync(
                        _symbol,
                        onData => ProcessTradeUpdate(onData.Data, completionSource),
                        ct: default)
                    .ConfigureAwait(false);
            }
            else
            {
                Console.WriteLine("[CoinbaseStrategy] Subscribing to 5-minute klines (candles) feed");
                Console.WriteLine("[CoinbaseStrategy] ⚠️  NOTE: Kline updates arrive every ~5 minutes when candles close");
                if (_targetCount != int.MaxValue)
                {
                    Console.WriteLine($"[CoinbaseStrategy] ⚠️  Receiving {_targetCount} updates will take approximately {_targetCount * 5} minutes ({_targetCount * 5 / 60.0:F1} hours)");
                }
                else
                {
                    Console.WriteLine("[CoinbaseStrategy] ⚠️  Running indefinitely (no count limit)");
                }

                Console.WriteLine();

                subscription = await _socketClient.AdvancedTradeApi
                    .SubscribeToKlineUpdatesAsync(
                        _symbol,
                        onData => ProcessKlineUpdate(onData.Data, completionSource),
                        ct: default)
                    .ConfigureAwait(false);
            }

            if (!subscription.Success || subscription.Data is null)
            {
                string errorMsg = subscription.Error?.Message ?? "Unknown error";
                Console.WriteLine($"[CoinbaseStrategy] Failed to subscribe: {errorMsg}");
                Console.WriteLine($"[CoinbaseStrategy] Error code: {subscription.Error?.Code}");
                Console.WriteLine($"[CoinbaseStrategy] Error details: {subscription.Error}");
                return;
            }

            string feedType = _mode == CoinbaseMode.Ticker ? "trades" : "klines";
            Console.WriteLine($"[CoinbaseStrategy] Successfully subscribed to {_symbol} {feedType}");
            Console.WriteLine("[CoinbaseStrategy] Processing bars...");
            Console.WriteLine();

            PrintHeader();
            await completionSource.Task.ConfigureAwait(false);

            await subscription.Data.CloseAsync().ConfigureAwait(false);

            Console.WriteLine();
            PrintSummary();
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("[CoinbaseStrategy] Operation was cancelled");
        }
        catch (InvalidOperationException ex)
        {
            Console.WriteLine($"[CoinbaseStrategy] Invalid operation: {ex.Message}");
        }
        catch (IOException ex)
        {
            Console.WriteLine($"[CoinbaseStrategy] I/O Error: {ex.Message}");
        }
    }

    public void Dispose() => _socketClient.Dispose();

    private void ProcessTradeUpdate(CoinbaseTrade[] trades, TaskCompletionSource<bool> completionSource)
    {
        // Lock the entire callback - WebSocket library may invoke this concurrently
        lock (_hubLock)
        {
            try
            {
                // Process trades in order received - no sorting!
                // Out-of-order bars are handled by the hub's healing/rebuild feature
                foreach (CoinbaseTrade trade in trades)
                {
                    // Convert trade to Bar - use trade price for all OHLC values
                    Bar bar = new(
                        Timestamp: trade.Timestamp,
                        Open: trade.Price,
                        High: trade.Price,
                        Low: trade.Price,
                        Close: trade.Price,
                        Volume: trade.Quantity);

                    ProcessBar(bar);
                    _barsProcessed++;

                    // Show progress every 10 bars or at target
                    if (_barsProcessed % 10 == 0 || _barsProcessed >= _targetCount)
                    {
                        string countDisplay = _targetCount == int.MaxValue ? "∞" : _targetCount.ToString(CultureInfo.InvariantCulture);
                        Console.WriteLine($"... processed {_barsProcessed}/{countDisplay} bars (latest: {trade.Timestamp:yyyy-MM-dd HH:mm:ss} UTC @ ${trade.Price:N2})");
                    }

                    if (_barsProcessed >= _targetCount)
                    {
                        completionSource.TrySetResult(true);
                        return;
                    }
                }
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine($"[CoinbaseStrategy] Invalid operation processing trade: {ex.Message}");
                completionSource.TrySetException(ex);
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine($"[CoinbaseStrategy] Argument error processing trade: {ex.Message}");
                completionSource.TrySetException(ex);
            }
        }
    }

    private void ProcessKlineUpdate(CoinbaseStreamKline[] klines, TaskCompletionSource<bool> completionSource)
    {
        // Lock the entire callback - WebSocket library may invoke this concurrently
        lock (_hubLock)
        {
            try
            {
                foreach (CoinbaseStreamKline kline in klines)
                {
                    // Convert kline (candle) data to Bar
                    Bar bar = new(
                        Timestamp: kline.OpenTime,
                        Open: kline.OpenPrice,
                        High: kline.HighPrice,
                        Low: kline.LowPrice,
                        Close: kline.ClosePrice,
                        Volume: kline.Volume);

                    ProcessBar(bar);
                    _barsProcessed++;

                    // Show progress every 10 bars or at target
                    if (_barsProcessed % 10 == 0 || _barsProcessed >= _targetCount)
                    {
                        string countDisplay = _targetCount == int.MaxValue ? "∞" : _targetCount.ToString(CultureInfo.InvariantCulture);
                        Console.WriteLine($"... processed {_barsProcessed}/{countDisplay} bars (latest: {kline.OpenTime:yyyy-MM-dd HH:mm} UTC @ ${kline.ClosePrice:N2})");
                    }

                    if (_barsProcessed >= _targetCount)
                    {
                        completionSource.TrySetResult(true);
                        return;
                    }
                }
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine($"[CoinbaseStrategy] Invalid operation processing kline: {ex.Message}");
                completionSource.TrySetException(ex);
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine($"[CoinbaseStrategy] Argument error processing kline: {ex.Message}");
                completionSource.TrySetException(ex);
            }
        }
    }

    private void ProcessBar(Bar bar)
    {
        _barHub.Add(bar);

        // Need at least 2 results from each EMA to compare current vs previous
        if (_fastEma.Results.Count < 2 || _slowEma.Results.Count < 2)
        {
            return;
        }

        // Get current and previous results directly from each hub
        EmaResult fastCurrent = _fastEma.Results[^1];
        EmaResult fastPrevious = _fastEma.Results[^2];
        EmaResult slowCurrent = _slowEma.Results[^1];
        EmaResult slowPrevious = _slowEma.Results[^2];

        // Check if we have valid EMA values
        if (fastCurrent.Ema is null || fastPrevious.Ema is null
            || slowCurrent.Ema is null || slowPrevious.Ema is null)
        {
            return;
        }

        // Verify timestamps are aligned (should always be true with coordinated hub)
        if (fastCurrent.Timestamp != slowCurrent.Timestamp
            || fastPrevious.Timestamp != slowPrevious.Timestamp)
        {
            return;
        }

        double currentPrice = (double)bar.Close;

        // Golden Cross: fast EMA crosses above slow EMA
        bool goldenCross = fastPrevious.Ema <= slowPrevious.Ema
            && fastCurrent.Ema > slowCurrent.Ema;

        // Death Cross: fast EMA crosses below slow EMA
        bool deathCross = fastPrevious.Ema >= slowPrevious.Ema
            && fastCurrent.Ema < slowCurrent.Ema;

        if (goldenCross && _units == 0)
        {
            _entryValue = _balance;
            _units = _balance / currentPrice;
            Console.WriteLine(
                $"{bar.Timestamp:yyyy-MM-dd HH:mm}  BUY   {_units,10:N2}   ${currentPrice,10:N2}   ${_entryValue,12:N2}   ${0.00,12:N2}");
            _balance = 0;
            _totalTrades++;
        }
        else if (deathCross && _units > 0)
        {
            double unitsSold = _units;
            double proceeds = _units * currentPrice;
            double profit = proceeds - _entryValue;
            _balance = proceeds;
            _units = 0;

            _tradeResults.Add(profit);

            if (profit > 0)
            {
                _winningTrades++;
                _largestWin = Math.Max(_largestWin, profit);
            }
            else
            {
                _losingTrades++;
                _largestLoss = Math.Min(_largestLoss, profit);
            }

            Console.WriteLine(
                $"{bar.Timestamp:yyyy-MM-dd HH:mm}  SELL  {unitsSold,10:N2}   ${currentPrice,10:N2}   ${profit,12:N2}   ${_balance,12:N2}");
        }
    }

    private void PrintHeader()
    {
        Console.WriteLine("==========================================");
        Console.WriteLine("  COINBASE WEBSOCKET TRADING STRATEGY");
        Console.WriteLine("==========================================");
        Console.WriteLine($"Symbol: {_symbol}");
        Console.WriteLine($"Initial Balance: ${_balance:N2}");
        Console.WriteLine($"Fast EMA: {FastPeriod} periods");
        Console.WriteLine($"Slow EMA: {SlowPeriod} periods");
        Console.WriteLine("==========================================");
        Console.WriteLine();
        Console.WriteLine(
            "Date/Time           Action    Quantity       Price        P&L/Cost         Balance");
        Console.WriteLine(
            "-------------------------------------------------------------------------------------");
    }

    private void PrintSummary()
    {
        double finalValue;
        int resultCount = _barHub.Results.Count;

        if (_units > 0 && resultCount > 0)
        {
            decimal lastClose = _barHub.Results[resultCount - 1].Close;
            finalValue = _units * (double)lastClose;
        }
        else
        {
            finalValue = _balance;
        }

        double totalPnL = finalValue - 10000.0;

        Console.WriteLine("==========================================");
        Console.WriteLine("  STRATEGY SUMMARY");
        Console.WriteLine("==========================================");
        Console.WriteLine($"Bars Processed:  {_barsProcessed}");
        Console.WriteLine($"Initial Balance:   ${10000.0:N2}");
        Console.WriteLine($"Final Value:       ${finalValue:N2}");
        Console.WriteLine($"Total P&L:         ${totalPnL:N2} ({totalPnL / 10000.0:P2})");
        Console.WriteLine($"Total Trades:      {_totalTrades}");
        Console.WriteLine($"Winning Trades:    {_winningTrades}");
        Console.WriteLine($"Losing Trades:     {_losingTrades}");
        Console.WriteLine($"Largest Win:       ${_largestWin:N2}");
        Console.WriteLine($"Largest Loss:      ${_largestLoss:N2}");

        if (_tradeResults.Count > 0)
        {
            double avgPnL = _tradeResults.Average();
            Console.WriteLine($"Average P&L/Trade: ${avgPnL:N2}");
        }

        Console.WriteLine("==========================================");
    }
}
