# Playbook: live market desk

Read [start.md](https://dotnet.stockindicators.dev/agents/start.md) first; its ground rules and finish line apply here.

The question this answers: **what is the market doing right now, underneath the prices?** For a basket of markets, the desk shows each one's daily regime (trend, directional strength, volatility against its own past year, distance from its one-year high), how it is rotating against a benchmark, and live one-minute buy and sell signals judged against that daily context. It narrates what changes as it happens, and you narrate it to the developer.

## Finish line

Start's finish line, delivered as:

- **The visual:** the desk running in a browser (cards per symbol, a rotation chart, live candles with signal markers, and the insight feed), or, in a sandbox, `out/desk.html` and `out/desk.svg` from a headless run.
- **Your narration:** the opening briefing in your own words, then each live insight as it arrives, in order, with its time. Lead with what is surprising; skip repeats.
- **Findings:** two or three sentences drawn from the briefing and the live feed, each with its numbers.
- **Provenance and speed:** both printed by the app; repeat them.

## Run the reference app

[`docs/examples/LiveSignals`](https://github.com/facioquo/stock-indicators-dotnet/tree/main/docs/examples/LiveSignals) is a complete, CI-built ASP.NET Core app that meets the finish line. Defaults: eight crypto pairs from Kraken (no key) against `BTC/USD`. Get it with a sparse clone, or download every example in one [ZIP](https://dotnet.stockindicators.dev/FacioQuo.Stock.Indicators-Examples.zip):

```bash
git clone --depth 1 --filter=blob:none --sparse https://github.com/facioquo/stock-indicators-dotnet.git
cd stock-indicators-dotnet && git sparse-checkout set docs/examples/LiveSignals
cd docs/examples/LiveSignals
```

Then choose the run that fits where you are:

| Where you run | Command | What you get |
| ------------- | ------- | ------------ |
| Sandbox or cloud agent | `dotnet run -- --headless --minutes 3` | Insights printed as they happen; `out/desk.html`, `out/desk.svg`, and `out/desk.json` written once history loads and again at the end |
| The developer's machine | `dotnet run` | The live desk at the URL it prints, with insights also printed to the terminal. **Narrate** reads new insights aloud |

In a sandbox, show `desk.svg` (or `desk.html` as an artifact) as soon as the first snapshot is written, narrate the briefing, then keep reading the output and narrate live insights until the run ends. If your tools return output only when a command finishes, run it in the background and read the output as it grows. In `desk.json`, insights with `live: false` are the briefing; `live: true` came after it.

Options: `--symbols ETH/USD,SOL/USD` for other Kraken pairs; `--symbols XLK,XLE,XLF,XLV,XLY,XLP,XLI,XLB,XLU,XLRE,XLC --benchmark SPY` for US sector rotation from Alpaca (needs `ALPACA_KEY` and `ALPACA_SECRET`); `--minutes` and `--out` for headless runs.

## How it works

Fetch these from `https://raw.githubusercontent.com/facioquo/stock-indicators-dotnet/main/docs/examples/LiveSignals/` when you adapt the app or explain it:

- `Program.cs`: options, feed choice, and the web or headless host
- `Tape.cs`: one symbol's hubs: a one-minute `BarHub` with EMA(21), EMA(55), ADX(14), and ATR(14) for signals, and a daily `BarHub` with SMA(50), SMA(200), ADX(14), and ATR(14) whose newest bar is today's, updated by every live minute
- `Analysis.cs`: the daily regime, the rotation math (an EMA(5) of the price ratio chained into SMA(50), then ROC(10)), and the briefing rules
- `Desk.cs`: the lock that serializes feed writes, the live insight rules, and the JSON snapshots
- `Report.cs`: the rotation chart and snapshot page, shared by both modes
- `KrakenFeed.cs` and `AlpacaFeed.cs`: history seeding and the live WebSocket loop for each provider
- `wwwroot/index.html`: the live page

Its terms have exact meanings; use them when you narrate:

- **Regime:** uptrend when price and the 50-day SMA are above the 200-day SMA, downtrend when both are below, otherwise transition.
- **Strength:** ADX(14) of 25 or more is strong, 20 or more moderate; "buying" or "selling" is whichever of +DI and −DI is higher. An uptrend under strong selling is a long-term trend whose recent daily moves have run against it.
- **Volatility:** today's ATR as a percent of price, ranked against its own past year: the 80th percentile or higher is stressed, the 20th or lower is calm.
- **Rotation:** RS-ratio above 100 means outperforming the benchmark; RS-momentum above 100 means that outperformance is growing. Quadrants: leading, weakening, lagging, improving. This is a simplified relative-rotation view, not the trademarked RRG calculation.
- **Provisional:** daily states use today's forming bar, so they can change before the daily close. Say so.

Adapt the basket, rules, or chart to what the developer asks; building from this baseline is faster and more reliable than writing the plumbing from scratch.

## Defaults

State these and proceed; change them only if the developer asks.

- **Symbols:** the eight-pair crypto basket against `BTC/USD`. A symbol they name joins its benchmark in the run.
- **Signal recipe:** the trend recipe below, on one-minute bars, evaluated on closed candles.
- **Horizon:** daily context and one-minute signals. Daily bars alone do not visibly move in a demo.

## Feeds

| Situation | Feed | Notes |
| --------- | ---- | ----- |
| Crypto, any time | Kraken public API, no key | The default. Symbols look like `BTC/USD`, `ETH/USD`, `SOL/USD`. Available in the US. |
| US equity, market open | Alpaca market data, free plan (IEX feed) | Needs a free key pair in `ALPACA_KEY` and `ALPACA_SECRET`. Facts below. |
| US equity, market closed | Alpaca, same as above | Regular hours are 9:30–16:00 America/New_York, weekdays. Load the most recent real sessions, show **Market closed · opens {next open}**, and keep the feed subscribed so the first live bar lands at the open. To watch signals fire right now, offer a crypto pair, which trades around the clock. |

Facts about the Kraken feed you cannot infer from the code alone:

- The WebSocket `ohlc` snapshot carries only about ten candles, too few to warm up EMA55 or ADX. Seed from REST first: `https://api.kraken.com/0/public/OHLC?pair=BTC/USD&interval=1` returns about 720 one-minute rows of `[time, open, high, low, close, vwap, volume, count]`, prices as strings; `interval=1440` returns about two years of daily rows, the newest still forming.
- WebSocket updates repeat the forming candle with the same `interval_begin`. Pass each one to `BarHub.Add`; a same-timestamp bar replaces the cached one and every chained hub recalculates.

Facts about the Alpaca stock feed:

- History: `GET https://data.alpaca.markets/v2/stocks/bars?symbols=MSFT&timeframe=1Min&feed=iex&limit=1000` with headers `APCA-API-KEY-ID` and `APCA-API-SECRET-KEY`. The response is `{"bars":{"MSFT":[{"t","o","h","l","c","v","n","vw"}]},"next_page_token"}`; follow `next_page_token` for more.
- Live: connect to `wss://stream.data.alpaca.markets/v2/iex`, send `{"action":"auth","key":"…","secret":"…"}`, then `{"action":"subscribe","bars":["MSFT"],"updatedBars":["MSFT"]}`. Messages arrive as JSON arrays.
- `bars` (`"T":"b"`) delivers each minute just after it closes, so there is no forming candle. `updatedBars` (`"T":"u"`) re-sends an earlier minute when a late trade changes it; pass it to `BarHub.Add` and the same-timestamp bar replaces the original. For movement within the minute, also subscribe to `trades` and build the forming candle with `TradeTickHub` and `ToTradeTickAggregatorHub(BarInterval.OneMinute)`.
- Market state: `GET https://paper-api.alpaca.markets/v2/clock` returns `is_open` and `next_open`.
- The free IEX feed carries few extended-hours trades; expect long gaps outside regular hours. The SIP feed (paid plans) uses `/v2/sip` for the stream and `feed=sip` for history.
- An Alpaca MCP server, if your session has one, lets you inspect the same data while you work. The app still needs its own keys at runtime.

## Indicator recipes

The app runs the trend recipe. Offer the other two, or the developer's own; build each as hubs chained off the one-minute `BarHub`.

| Recipe | Signal | Filter and risk |
| ------ | ------ | --------------- |
| Trend (reference app) | EMA(21) crosses EMA(55) | ADX(14) ≥ 20 marks a strong signal; stop at 1.5×ATR(14) |
| Mean reversion | RSI(2) below 10 (buy) or above 90 (sell) | Trade only in the direction of SMA(200); exit at the SMA(5) |
| Breakout | Close outside the Keltner(20, 2, 10) channel | Volume above its 20-bar average; stop at the opposite band |

## Get these right

- **Evaluate signals on closed candles only.** The last bar is still forming, so a crossover on it can appear and vanish within the minute. Read the bar before it.
- **Feed the hubs from one sequential receive loop.** Concurrent handlers can reorder bars.
- **Narrate what the app said, not what you expect.** Quote its numbers. If nothing happened during the run, say the market was quiet; never invent events.
- **The chart is replaceable.** The reference uses TradingView Lightweight Charts 4.2 from a CDN (its v5 API differs; keep the pinned version or port the calls). Any chart that renders candles, lines, and markers works, as does a terminal UI if they want no browser.
- **Do not claim profitability.** A signal on a live chart is not evidence that a recipe makes money; the backtest playbook is how to check.

## Next prompts

Offer these, adjusted to what the run showed:

- "Backtest the trend recipe on the leading symbol's daily history." → [backtest](https://dotnet.stockindicators.dev/agents/backtest.md)
- "Run the same desk on the US sector ETFs against SPY."
- "Post each insight to Discord, Slack, or ntfy."
- "Feed this desk from my app's own price data." → [integrate](https://dotnet.stockindicators.dev/agents/integrate.md)
