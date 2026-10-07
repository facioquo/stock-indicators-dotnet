# Stock Indicators for .NET: agent setup guide

You are helping a developer get started with the `FacioQuo.Stock.Indicators` NuGet package (v3). This guide routes you to one playbook. Each playbook defines a finish line; reach it and hand the result back running.

The finish line is never "printed some SMA values." This library calculates indicators fast in batch, incrementally, and live off a stream. Show the developer that on real market data.

## Ground rules

- **Never invent price data.** No random walks, no hand-typed bars, no generated "sample" quotes standing in for the market. If you have no reachable live source, use the real historical datasets listed below and say which one you used.
- **Use v3 only.** Install `FacioQuo.Stock.Indicators`. `Skender.Stock.Indicators` is the superseded v2 package; v2 names such as `GetSma`, `Quote`, and `Date` do not exist in v3.
- **Read before you write.** Fetch the page for every indicator you use (`https://dotnet.stockindicators.dev/indicators/{name}.md`), and follow its parameters, warmup, and result properties exactly. The index is at `https://dotnet.stockindicators.dev/llms.txt`.
- **Ask before** installing SDKs or global tools, writing outside the current workspace, or using an API key. Keep keys in `dotnet user-secrets` or environment variables, never in source.
- **Signals inform; they do not trade.** Do not place orders. If the developer asks for execution, use a broker's paper-trading account and say so.

## Choose a playbook

First look at the workspace without asking:

- A project that references `Skender.Stock.Indicators` → [migrate](https://dotnet.stockindicators.dev/agents/migrate.md).
- An existing app with its own price-bar type, and the developer asked for something inside it → [integrate](https://dotnet.stockindicators.dev/agents/integrate.md).

Otherwise, if the developer has not already said what they want, ask one question and recommend the first option:

1. **Live signals on a symbol (recommended).** A local web app that streams real candles, overlays indicators, and explains each buy or sell signal as it fires. → [live-signals](https://dotnet.stockindicators.dev/agents/live-signals.md)
2. **Backtest an idea** on decades of real daily history. → [backtest](https://dotnet.stockindicators.dev/agents/backtest.md)
3. **Add indicators to my app.** → [integrate](https://dotnet.stockindicators.dev/agents/integrate.md)
4. **Show me what it can do.** A short, tailored tour of what sets the library apart. → [tour](https://dotnet.stockindicators.dev/agents/tour.md)

If they answer with a symbol or a trading idea, that is option 1 or 2; do not ask again.

## Real data you can always reach

These are real market histories from the library's test suite. No key needed. Columns: `date,open,high,low,close,volume`, a header row, and some files start with a UTF-8 byte order mark. Parse dates with `CultureInfo.InvariantCulture`; the daily files use `M/d/yyyy` or `yyyy-MM-dd`.

Base URL: `https://raw.githubusercontent.com/facioquo/stock-indicators-dotnet/main/tests/Library/TestData/quotes/`

| File | Contents |
| ---- | -------- |
| `msft.csv` | Microsoft daily, 1990-01-02 to 2022-03-10 |
| `spx.csv` | S&P 500 index daily, 1990-01-02 to 2022-03-10 |
| `bitcoin.csv` | Bitcoin daily, 2017-08-16 to 2021-01-12 |
| `btcusd15x69k.csv` | Bitcoin 15-minute bars, 2020-07-07 to 2022-06-30 |
| `intraday.csv` | One US equity, 1-minute bars, 2020-12-15 to 2020-12-18 |

Say plainly that this data is historical and ends on the dates above.

## Library facts that shape every playbook

- Bars must be in chronological order with one consistent interval. Every indicator returns one result per input bar, with `null` values until its warmup completes.
- Load at least the warmup the indicator page asks for, and more for smoothed indicators (EMA, RSI, ADX, MACD) whose values converge slowly; a few hundred bars is a sound floor.
- Three styles, identical results: Series (`bars.ToRsi(14)`), Buffer lists (`new RsiList(14)` then `.Add(bar)`), and Stream hubs (`barHub.ToRsiHub(14)`). Stream hubs accept late, out-of-order, and same-timestamp-revised bars and recalculate what they affect.
- Indicators chain: `bars.ToObv().ToRsi(14)`, or `barHub.ToEmaHub(20).ToRsiHub(14)`.
