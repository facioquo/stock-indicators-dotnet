using System.Text.Json;
using FacioQuo.Stock.Indicators;

namespace Test.Simulation;

internal sealed class GoldenCrossStrategy : IDisposable
{
    private readonly string _endpoint;
    private readonly int _interval;
    private readonly int _targetCount;
    private readonly string _barIntervalCode;
    private readonly HttpClient _httpClient;

    private readonly BarHub _barHub;
    private readonly EmaHub _fastEma;
    private readonly EmaHub _slowEma;

    private static readonly JsonSerializerOptions JsonOptions = new() {
        PropertyNameCaseInsensitive = true
    };

    private double _balance = 10000.0;
    private double _units;
    private double _entryValue; // Track the cost basis when entering a position
    private int _totalTrades;
    private int _winningTrades;
    private int _losingTrades;
    private double _largestWin;
    private double _largestLoss;
    private readonly List<double> _tradeResults = [];

    private const int FastPeriod = 50;
    private const int SlowPeriod = 200;

    public GoldenCrossStrategy(string endpoint, int interval, int targetCount, string barIntervalCode)
    {
        _endpoint = endpoint;
        _interval = interval;
        _targetCount = targetCount;
        _barIntervalCode = barIntervalCode;
        _httpClient = new HttpClient { Timeout = TimeSpan.FromMinutes(30) };

        // Use default cache size (100,000 items)
        _barHub = new BarHub();
        _fastEma = _barHub.ToEmaHub(FastPeriod);
        _slowEma = _barHub.ToEmaHub(SlowPeriod);
    }

    public async Task RunAsync()
    {
        PrintHeader();

        try
        {
            string batchSizeParam = _targetCount == int.MaxValue
                ? string.Empty
                : $"&batchSize={_targetCount}";
            Uri uri = new($"{_endpoint}?interval={_interval}&barIntervalCode={_barIntervalCode}{batchSizeParam}");
            Console.WriteLine($"[Strategy] Connecting to {uri}");
            Console.WriteLine();

            using HttpResponseMessage response = await _httpClient
                .GetAsync(uri, HttpCompletionOption.ResponseHeadersRead)
                .ConfigureAwait(false);

            response.EnsureSuccessStatusCode();

            Stream stream = await response.Content.ReadAsStreamAsync()
                .ConfigureAwait(false);
            try
            {
                using StreamReader reader = new(stream);

                int barsProcessed = 0;
                string? line;

                while ((line = await reader.ReadLineAsync().ConfigureAwait(false)) is not null)
                {
                    // SSE format: "event: bar", "data: {...}", blank line
                    if (line.StartsWith("data:", StringComparison.Ordinal))
                    {
                        string json = line[5..].Trim();
                        Bar? bar = JsonSerializer.Deserialize<Bar>(json, JsonOptions);

                        if (bar is not null)
                        {
                            ProcessBar(bar);
                            barsProcessed++;

                            if (barsProcessed >= _targetCount)
                            {
                                break;
                            }
                        }
                    }
                }
            }
            finally
            {
                await stream.DisposeAsync().ConfigureAwait(false);
            }

            Console.WriteLine();
            PrintSummary();
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"[Strategy] HTTP Error: {ex.Message}");
        }
        catch (IOException ex)
        {
            Console.WriteLine($"[Strategy] I/O Error: {ex.Message}");
        }
        catch (JsonException ex)
        {
            Console.WriteLine($"[Strategy] JSON Error: {ex.Message}");
        }
    }

    public void Dispose() => _httpClient.Dispose();

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

        // Verify timestamps are aligned
        if (fastCurrent.Timestamp != slowCurrent.Timestamp
            || fastPrevious.Timestamp != slowPrevious.Timestamp)
        {
            return;
        }

        double currentPrice = (double)bar.Close;

        // Golden Cross: Fast EMA crosses above Slow EMA (Buy signal)
        bool goldenCross = fastPrevious.Ema <= slowPrevious.Ema
            && fastCurrent.Ema > slowCurrent.Ema;

        // Death Cross: Fast EMA crosses below Slow EMA (Sell signal)
        bool deathCross = fastPrevious.Ema >= slowPrevious.Ema
            && fastCurrent.Ema < slowCurrent.Ema;

        if (goldenCross && _units == 0)
        {
            // Buy: spend full balance on units at current price
            _entryValue = _balance;
            _units = _balance / currentPrice;
            _balance = 0;
            Console.WriteLine(
                $"{bar.Timestamp:yyyy-MM-dd HH:mm}  BUY   {_units,10:N2}   ${currentPrice,10:N2}   ${_entryValue,12:N2}   ${_balance,12:N2}");
            _totalTrades++;
        }
        else if (deathCross && _units > 0)
        {
            // Sell: convert units to cash at current price
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
        Console.WriteLine("  GOLDEN CROSS TRADING STRATEGY");
        Console.WriteLine("==========================================");
        Console.WriteLine($"Initial Balance: ${_balance:N2}");
        Console.WriteLine($"Fast EMA: {FastPeriod} periods");
        Console.WriteLine($"Slow EMA: {SlowPeriod} periods");
        Console.WriteLine("==========================================");
        Console.WriteLine();
        Console.WriteLine(
            "Date/Time         Action  Quantity         Price        P&L/Cost         Balance");
        Console.WriteLine(
            "-------------------------------------------------------------------------------------");
    }

    private void PrintSummary()
    {
        double finalValue = _units > 0 && _barHub.Results.Count > 0
            ? _units * (double)_barHub.Results[^1].Close
            : _balance;
        double totalPnL = finalValue - 10000.0;

        Console.WriteLine("==========================================");
        Console.WriteLine("  STRATEGY SUMMARY");
        Console.WriteLine("==========================================");
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
