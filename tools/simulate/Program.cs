using System.Diagnostics;
using Test.Simulation;

// Parse command line arguments
string mode = args.Length > 0 ? args[0].ToUpperInvariant() : "SSE";

if (mode == "HUB-STRESS")
{
    // Hub stress test mode - tests hubs modified in PR #1927
    string symbol = args.Length > 1 && !string.IsNullOrWhiteSpace(args[1]) ? args[1] : "BTC-USD";
    int count = args.Length > 2 && int.TryParse(args[2], out int c) ? c : 500;
    int maxCache = args.Length > 3 && int.TryParse(args[3], out int m) ? m : 200;

    Console.WriteLine("Starting Hub Stress Test (PR #1927 / Issue #1925)...");
    Console.WriteLine("Symbol: " + symbol + ", Target: " + count + " bars, MaxCache: " + maxCache);
    Console.WriteLine();

    using HubStressTest test = new(symbol, count, maxCache);
    await test.RunAsync().ConfigureAwait(false);
}
else if (mode is "COINBASE" or "COINBASE-KLINES" or "COINBASE-TICKER")
{
    // Coinbase WebSocket mode
    // Determine Coinbase mode type from the mode string
    CoinbaseMode coinbaseMode = mode == "COINBASE-TICKER" ? CoinbaseMode.Ticker : CoinbaseMode.Klines;
    string symbol = args.Length > 1 && !string.IsNullOrWhiteSpace(args[1]) ? args[1] : "BTC-USD";
    int count = args.Length > 2 && int.TryParse(args[2], out int c) ? c : int.MaxValue;

    Console.WriteLine($"Starting Coinbase WebSocket simulation ({coinbaseMode})...");
    Console.WriteLine($"Symbol: {symbol}, Target count: {count}");
    Console.WriteLine();

    using CoinbaseStrategy strategy = new(symbol, count, coinbaseMode);
    await strategy.RunAsync().ConfigureAwait(false);
}
else
{
    // SSE mode (default)
    string dataType = args.Length > 1 ? args[1].ToUpperInvariant() : "BAR";
    int interval = args.Length > 2 && int.TryParse(args[2], out int i) ? i : 100;
    int count = args.Length > 3 && int.TryParse(args[3], out int c) ? c : int.MaxValue;
    string barIntervalCode = args.Length > 4 ? args[4] : "1m";

    // Build default endpoint URL from dataType (normalize to lowercase for URL)
    string defaultEndpoint = dataType switch {
        "BAR" => "http://localhost:5001/bars/random",
        "TRADE" => "http://localhost:5001/trades/random",
        _ => $"http://localhost:5001/{dataType.ToUpperInvariant()}s/random"
            .ToLowerInvariant()
    };

    // Custom endpoint can be passed as 6th argument for SSE mode
    string endpoint = args.Length > 5 && !string.IsNullOrWhiteSpace(args[5]) && args[5].StartsWith("http", StringComparison.Ordinal)
        ? args[5]
        : defaultEndpoint;

    // Start SSE server
    Process? serverProcess = ServerManager.StartServer(5001);

    try
    {
        // Wait for server to start
        Console.WriteLine("[Program] Waiting for SSE server to be ready...");
        await Task.Delay(3000).ConfigureAwait(false);
        Console.WriteLine("[Program] Starting strategy...");
        Console.WriteLine();

        // Run Golden Cross strategy
        using GoldenCrossStrategy strategy = new(endpoint, interval, count, barIntervalCode);
        await strategy.RunAsync().ConfigureAwait(false);
    }
    finally
    {
        // Stop server
        ServerManager.StopServer(serverProcess);
    }
}

// Sentinel line for the VS Code background task problem matcher (see .vscode/tasks.json endsPattern)
Console.WriteLine("Simulation complete.");
