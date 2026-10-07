# Playbook: backtest an idea

Read [start.md](https://dotnet.stockindicators.dev/agents/start.md) first; its ground rules apply here.

## Finish line

A console project that runs the developer's rules over decades of real daily bars and prints:

- the trade list, each entry with its date, side, price, and the indicator values that triggered it
- trade count, win rate, total return, maximum drawdown, and buy-and-hold over the same span for comparison

Offer an equity-curve chart only if they want one.

## Data

Use `msft.csv` or `spx.csv` (daily, 1990–2022) from start.md, or the developer's own provider. Ask which rules to test; if they have none, offer the recipes from [live-signals](https://dotnet.stockindicators.dev/agents/live-signals.md) so the same rules can later run live.

## Get these right

- **No look-ahead.** Decide on bar *i*'s close; fill at bar *i + 1*'s open. Never use a value from a later bar.
- **Skip the warmup.** Do not trade until every indicator in the rule has a non-null value, and for smoothed indicators wait out their convergence period as stated on the indicator page.
- **Charge costs.** Apply a per-trade fee or slippage, even a small one, and state it.
- **Name the biases.** One surviving blue-chip over a famous bull market flatters most rules; a single-symbol backtest proves little. Say so in the summary.
- Use Series style (`bars.ToEma(21)`) for the whole history at once; it is the fastest style and gives the same values a live stream hub would.

[`docs/examples/Backtest`](https://github.com/facioquo/stock-indicators-dotnet/tree/main/docs/examples/Backtest) is a compact Stochastic RSI example to borrow structure from.
