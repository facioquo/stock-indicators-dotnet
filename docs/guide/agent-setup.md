---
title: Agent setup
description: Get a coding agent to set you up with Stock Indicators for .NET on real market data, from live signals to backtests, with a copy-paste prompt.
---

# Agent setup

Paste this prompt into your coding agent (Claude, ChatGPT, Copilot, etc.):

<!--@include: ../shared/agent-prompt.md-->

## What your agent will do

The [agent setup guide](https://dotnet.stockindicators.dev/agents/start.md) tells your agent to work from real market data, never invented bars, and to ask before installing tools or using API keys. It checks your workspace, asks what you want to build, and follows the matching playbook:

| You choose | You end up with |
| ---------- | --------------- |
| [Live signals](https://dotnet.stockindicators.dev/agents/live-signals.md) | A local web app streaming real candles for your symbol, with indicator overlays and explained buy and sell signals |
| [Backtest](https://dotnet.stockindicators.dev/agents/backtest.md) | Your trading rules run over years of real daily history up to the latest close, with a trade list and results against buy-and-hold |
| [Integrate](https://dotnet.stockindicators.dev/agents/integrate.md) | Indicators calculated inside your own app, from your own bar type, with a test that pins the results |
| [Tour](https://dotnet.stockindicators.dev/agents/tour.md) | Short, runnable demos of what sets the library apart, and ideas for what you could build |
| [Migrate](https://dotnet.stockindicators.dev/agents/migrate.md) | A v2 codebase moved to v3, with output checked against the old results |

Live signals for crypto pairs need no account. For US stocks, have a free [Alpaca](https://alpaca.markets/) API key ready in `ALPACA_KEY` and `ALPACA_SECRET`.

## Work with a coding agent

For later tasks, ask for something specific. Agents get better results when you also paste the relevant page, using **Copy page** beside its title. For example:

```prompt
Add a stream hub to my price feed handler that tracks
MACD and ADX, and raises an event when MACD crosses
its signal line while ADX is above 25.
```

```prompt
Backtest an RSI(2) mean-reversion rule on SPX daily bars,
trading only above the 200-day SMA, and compare the
result with buy-and-hold.
```

```prompt
Find the indicators that measure trend strength, then
show each on the same real price history and explain
how their warmup periods differ.
```

Point agents at the [`/llms.txt`](/llms.txt) index so they fetch only the pages they need. Save [`/llms-full.txt`](/llms-full.txt), all the documentation in one large file, for broad analysis; for routine questions it crowds your own code out of the agent's context.

## Machine-readable documentation

- **Markdown pages**: add `.md` to any page URL, such as `/indicators/sma.md`, or request the page with an `Accept: text/markdown` header.
- **Agent skill**: agents that support [Agent Skills](https://agentskills.io) can install the skill listed at [`/.well-known/agent-skills/index.json`](/.well-known/agent-skills/index.json).
- **Browser tools**: in browsers that support [WebMCP](https://webmachinelearning.github.io/webmcp/), every page exposes read-only tools to search the documentation, get any indexed page as Markdown, and get the current page as Markdown.
