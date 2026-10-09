# Playbook: backtest an idea

Read [start.md](https://dotnet.stockindicators.dev/agents/start.md) first; its ground rules and finish line apply here.

The question this answers: **would these rules have helped, and what would they have cost?** Not "does the signal fire," but how the rules compare with simply holding.

## Finish line

Start's finish line, delivered as:

- **The visual:** an equity curve for the rules against buy-and-hold, with drawdown beneath it.
- **The numbers:** trade count, win rate, total return, maximum drawdown, and time in the market, for the rules and for buy-and-hold over the same span; and the trade list, each entry with its date, side, price, and the indicator values that triggered it.
- **Findings:** the trade-off in a sentence or two, for example "gave up 9 points of return to cut the worst drawdown from 77% to 41%," never "this works."

## Defaults

State these and proceed unless the developer has rules of their own:

- **Rules:** the trend recipe from [live-signals](https://dotnet.stockindicators.dev/agents/live-signals.md): long when EMA(21) is above EMA(55) on daily closes, flat otherwise, so the same rules can later run live on the desk.
- **Data:** `BTC/USD` daily bars from Kraken's public OHLC endpoint (`interval=1440`, about two years, no key), through the latest closed day. For US equities, use Alpaca daily bars (free key in `ALPACA_KEY` and `ALPACA_SECRET`, `adjustment=all` for splits and dividends).
- **Costs:** 0.1% per side, stated in the result.

## Get these right

- **No look-ahead.** Decide on bar *i*'s close; fill at bar *i + 1*'s open. Never use a value from a later bar.
- **Skip the warmup.** Do not trade until every indicator in the rule has a non-null value, and for smoothed indicators wait out their convergence period as stated on the indicator page.
- **Charge costs.** Apply a per-trade fee or slippage, even a small one, and state it.
- **Name the biases.** One surviving blue-chip over a famous bull market flatters most rules; a single-symbol backtest over two years proves little. Say so in the summary.
- **Exclude today's forming bar.** Kraken and Alpaca both return the current day while it trades.
- Use Series style (`bars.ToEma(21)`) for the whole history at once; it is the fastest style and gives the same values a live stream hub would. Report the calculation time.

[`docs/examples/Backtest`](https://github.com/facioquo/stock-indicators-dotnet/tree/main/docs/examples/Backtest) is a compact Stochastic RSI example to borrow structure from; `KrakenFeed.cs` in [`docs/examples/LiveSignals`](https://github.com/facioquo/stock-indicators-dotnet/tree/main/docs/examples/LiveSignals) shows how to parse the Kraken history.

## Next prompts

- "Run these rules live on the market desk."
- "Test the same rules across the whole crypto basket, not one symbol."
- "Add an ADX filter and compare the two."
