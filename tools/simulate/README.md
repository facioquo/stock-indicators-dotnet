# Simulation tool

`Test.Simulation` feeds live-style market data into StreamHubs to reproduce thread-safety and pruning problems that only show up under asynchronous delivery. It has three modes:

| Mode | Data source | Runs |
| ---- | ----------- | ---- |
| `sse` (default) | The local [SSE server](../sse-server/README.md), which the tool starts on port 5001 | Golden Cross strategy |
| `coinbase`, `coinbase-klines`, `coinbase-ticker` | Live Coinbase WebSocket feed via `JKorf.Coinbase.Net` | Golden Cross strategy |
| `hub-stress` | Live Coinbase trade feed | Hub stress test |

Run the commands below from `tools/simulate`, or add `--project tools/simulate` to run them from the repository root. The Coinbase modes need internet access.

## SSE mode

```bash
dotnet run -- sse [dataType] [interval] [count] [barInterval] [endpoint]
```

| Argument | Type | Default | Description |
| -------- | ---- | ------- | ----------- |
| `dataType` | _`string`_ | `bar` | Must be `bar`, the only data type the SSE server streams; any other value exits with an error |
| `interval` | _`int`_ | `100` | Delay between bars, in milliseconds |
| `count` | _`int`_ | unlimited | Number of bars to process; pass `""` to keep the default and set later arguments |
| `barInterval` | _`string`_ | `1m` | Timestamp spacing between bars, such as `1s`, `5m`, `1h`, or `1d` |
| `endpoint` | _`string`_ | `http://localhost:5001/bars/random` | SSE endpoint URL; must start with `http` |

```bash
dotnet run -- sse                     # 1m bars every 100 ms, indefinitely
dotnet run -- sse bar 50 500          # 1m bars every 50 ms, stops after 500
dotnet run -- sse bar 100 1000 1h     # hourly bars every 100 ms, stops after 1000
dotnet run -- sse bar 100 "" 1d       # daily bars every 100 ms, indefinitely
```

`barInterval` sets timestamp spacing independently of `interval`, so `1h` bars at `100` ms cover 24 hours in 2.4 seconds.

## Coinbase modes

```bash
dotnet run -- coinbase [symbol] [count]
```

| Mode | Feed |
| ---- | ---- |
| `coinbase`, `coinbase-klines` | 5-minute klines; an update arrives about every 5 minutes, when a candle closes |
| `coinbase-ticker` | Individual trades, as they happen |

| Argument | Type | Default | Description |
| -------- | ---- | ------- | ----------- |
| `symbol` | _`string`_ | `BTC-USD` | Coinbase trading pair, such as `ETH-USD` |
| `count` | _`int`_ | unlimited | Number of updates to process |

```bash
dotnet run -- coinbase                      # BTC-USD klines, indefinitely
dotnet run -- coinbase-ticker ETH-USD 1000  # ETH-USD trades, stops after 1000
```

## Hub stress mode

```bash
dotnet run -- hub-stress [symbol] [count] [maxCache]
```

Defaults: `BTC-USD`, `500` bars, and a `BarHub` cache of `200`. The small cache forces pruning while live trades drive STC, Slope, EPMA, ConnorsRSI, Fisher Transform, HT Trendline, MAMA, and common hubs (EMA, SMA, RSI, MACD, Bollinger Bands, ATR) from one `BarHub`.

## Golden Cross strategy

The `sse` and `coinbase` modes feed one `BarHub` into a 50-period and a 200-period `EmaHub`. The strategy buys with its full balance when the fast EMA crosses above the slow EMA, sells when it crosses below, and starts with a $10,000 balance.

To hunt for race conditions, run `coinbase-ticker` or `hub-stress` with a high count and watch for exceptions such as `ArgumentOutOfRangeException`.

## VS Code tasks

- `Run: Simulation (default SSE)` — SSE mode, 50 ms delivery, `1m` bars, unlimited count
- `Run: SSE simulation (with inputs)` — SSE mode with `bar` data, prompting for delivery interval, count, and bar interval
- `Run: Coinbase simulation (with inputs)` — a Coinbase mode, prompting for symbol and count
- `Run: Hub stress test (with inputs)` — hub stress mode, prompting for symbol, count, and cache size

## Stopping

Press `Ctrl+C` in the terminal running the tool. To stop hosts that do not exit, run one of these VS Code tasks or its script in [`tools/scripts`](../scripts/README.md):

- `Stop: Simulation hosts` — `bash tools/scripts/stop-simulation.sh`
- `Stop: SseServer hosts` — `bash tools/scripts/stop-sseserver.sh`
- `Stop: All hosted servers` — both of the above, plus the VitePress dev and preview servers
