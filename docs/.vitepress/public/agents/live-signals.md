# Playbook: live signals on a symbol

Read [start.md](https://dotnet.stockindicators.dev/agents/start.md) first; its ground rules apply here.

## Finish line

The developer runs one command and a browser tab shows:

- real candles for their symbol, with the forming candle updating as trades print
- at least two indicator overlays, warmed up from the first visible bar
- a signal panel where each buy or sell signal states its reason in numbers, for example `EMA21 crossed above EMA55 · ADX 27 · stop 84,091.74 (1.5×ATR)`
- markers on the chart where past signals fired

Then tell them how to change the symbol and the recipe, and offer the next steps at the end of this page.

## Start from the reference app

[`docs/examples/LiveSignals`](https://github.com/facioquo/stock-indicators-dotnet/tree/main/docs/examples/LiveSignals) is a complete, CI-built ASP.NET Core app that already meets the finish line, for crypto pairs (Kraken, no key) and US stocks (Alpaca). Fetch its files from `https://raw.githubusercontent.com/facioquo/stock-indicators-dotnet/main/docs/examples/LiveSignals/`:

- `LiveSignals.csproj` and `Program.cs`: the host, and the symbol-based choice of feed
- `Desk.cs`: the hubs, the signal recipe, and the snapshots sent to the page
- `KrakenFeed.cs` and `AlpacaFeed.cs`: history seeding and the live WebSocket loop for each provider
- `wwwroot/index.html`: the chart and signal panel

Copy them into a new project, then run `dotnet run -- --symbol ETH/USD` or `dotnet run -- --symbol MSFT` and open the URL it prints. The stock feed reads `ALPACA_API_KEY` and `ALPACA_SECRET_KEY` from the environment. Adapt the feed, recipe, or chart to what the developer asked for; building from this baseline is faster and more reliable than writing the plumbing from scratch.

## Ask for

- **The symbol.** Default to `BTC/USD` if they have none in mind.
- **The horizon.** Scalping → 1-minute bars; swing → 1-hour; position → daily. Daily bars do not visibly move in a demo, so show a shorter interval live and say why.

## Pick the feed by asset and clock

| Situation | Feed | Notes |
| --------- | ---- | ----- |
| Crypto, any time | Kraken public API, no key | What the reference app uses. Symbols look like `BTC/USD`, `ETH/USD`, `SOL/USD`. Available in the US. |
| US equity, market open | Alpaca market data, free plan (IEX feed) | Needs a free key pair in `ALPACA_API_KEY` and `ALPACA_SECRET_KEY`. Facts below. |
| US equity, market closed | Alpaca, same as above | Regular hours are 9:30–16:00 America/New_York, weekdays. Load the most recent real sessions, show **Market closed · opens {next open}**, and keep the feed subscribed so the first live bar lands at the open. To watch signals fire right now, offer a crypto pair, which trades around the clock. |

Facts about the Kraken feed you cannot infer from the code alone:

- The WebSocket `ohlc` snapshot carries only about ten candles, too few to warm up EMA55 or ADX. Seed from REST first: `https://api.kraken.com/0/public/OHLC?pair=BTC/USD&interval=1` returns about 720 one-minute rows of `[time, open, high, low, close, vwap, volume, count]`, prices as strings.
- WebSocket updates repeat the forming candle with the same `interval_begin`. Pass each one to `BarHub.Add`; a same-timestamp bar replaces the cached one and every chained hub recalculates.

Facts about the Alpaca stock feed:

- History: `GET https://data.alpaca.markets/v2/stocks/bars?symbols=MSFT&timeframe=1Min&feed=iex&limit=1000` with headers `APCA-API-KEY-ID` and `APCA-API-SECRET-KEY`. The response is `{"bars":{"MSFT":[{"t","o","h","l","c","v","n","vw"}]},"next_page_token"}`; follow `next_page_token` for more.
- Live: connect to `wss://stream.data.alpaca.markets/v2/iex`, send `{"action":"auth","key":"…","secret":"…"}`, then `{"action":"subscribe","bars":["MSFT"],"updatedBars":["MSFT"]}`. Messages arrive as JSON arrays.
- `bars` (`"T":"b"`) delivers each minute just after it closes, so there is no forming candle. `updatedBars` (`"T":"u"`) re-sends an earlier minute when a late trade changes it; pass it to `BarHub.Add` and the same-timestamp bar replaces the original. For movement within the minute, also subscribe to `trades` and build the forming candle with `TradeTickHub` and `ToTradeTickAggregatorHub(BarInterval.OneMinute)`.
- Market state: `GET https://paper-api.alpaca.markets/v2/clock` returns `is_open` and `next_open`.
- The free IEX feed carries few extended-hours trades; expect long gaps outside regular hours.
- An Alpaca MCP server, if your session has one, lets you inspect the same data while you work. The app still needs its own keys at runtime.

## Indicator recipes

Offer these three; let the developer pick or describe their own. Build each as hubs chained off one `BarHub`.

| Recipe | Signal | Filter and risk |
| ------ | ------ | --------------- |
| Trend (reference app) | EMA(21) crosses EMA(55) | ADX(14) ≥ 20 marks a strong signal; stop at 1.5×ATR(14) |
| Mean reversion | RSI(2) below 10 (buy) or above 90 (sell) | Trade only in the direction of SMA(200); exit at the SMA(5) |
| Breakout | Close outside the Keltner(20, 2, 10) channel | Volume above its 20-bar average; stop at the opposite band |

## Get these right

- **Evaluate signals on closed candles only.** The last bar is still forming, so a crossover on it can appear and vanish within the minute. Read the bar before it.
- **Feed the hub from one sequential receive loop.** Concurrent handlers can reorder bars.
- **The chart is replaceable.** The reference uses TradingView Lightweight Charts 4.2 from a CDN (its v5 API differs; keep the pinned version or port the calls). Any chart that renders candles, lines, and markers works, as does a terminal UI if they want no browser.
- **Do not claim profitability.** A signal on a live chart is not evidence that a recipe makes money; the backtest playbook is how to check.

## Offer next

- Alerts: post signals to Discord, Slack, or ntfy.
- A watchlist: one `BarHub` per symbol behind the same page.
- "Does this recipe work?" → [backtest](https://dotnet.stockindicators.dev/agents/backtest.md) the same rules on years of history.
