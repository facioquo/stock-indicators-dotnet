---
title: Agent setup
description: Paste one prompt into a coding agent and watch it run Stock Indicators for .NET on live market data, then narrate what it finds.
---

# Agent setup

The first run needs:

- **An agent that can run code**, such as Claude Code, Codex, or GitHub Copilot's coding agent. A chat without code execution can describe the library but cannot run it, and will tell you so.
- **The .NET 10 SDK**, which the reference apps target. A cloud sandbox installs it on its own; on your machine, the agent asks before installing it. The library itself also runs on .NET 8 and 9.
- **Optional:** a free [Alpaca](https://alpaca.markets/) API key in `ALPACA_KEY` and `ALPACA_SECRET`, for US stocks. Crypto markets need no key.

Paste this prompt into that agent:

<!--@include: ../shared/agent-prompt.md-->

## What your agent will do

The [agent guide](https://dotnet.stockindicators.dev/agents/start.md) tells your agent to show you the library working before it asks you to install anything:

- **If it can run code**, it does the work itself: installs the .NET SDK in its sandbox if needed, runs the library on live market data, and hands back a chart, findings backed by numbers, and the code that produced them.
- **If it can't**, it says so in its first sentence, before writing any code, and hands you the setup prompt above to run in an agent that can.
- **It never fakes results.** No invented prices, no indicators recalculated in another language, and no old test data passed off as current.

With no other instructions, it starts the live market desk and lists the other playbooks so you can redirect it:

| Playbook | You end up with |
| -------- | --------------- |
| [Live market desk](https://dotnet.stockindicators.dev/agents/live-signals.md) (default) | Eight crypto markets streaming live with no API key: each one's daily regime, its rotation against Bitcoin, explained buy and sell signals, and insights your agent narrates as they happen |
| [Backtest](https://dotnet.stockindicators.dev/agents/backtest.md) | Your rules run over years of real daily history up to the latest close, with an equity curve and drawdown against buy-and-hold |
| [Integrate](https://dotnet.stockindicators.dev/agents/integrate.md) | Indicators calculated inside your own app, from your own bar type, with a test that pins the results |
| [Migrate](https://dotnet.stockindicators.dev/agents/migrate.md) | A v2 codebase moved to v3, with output checked against the old results |
| [Tour](https://dotnet.stockindicators.dev/agents/tour.md) | Short, runnable demos of what sets the library apart, and ideas for what you could build |

The desk also runs US stocks and sector ETFs; for those, have a free [Alpaca](https://alpaca.markets/) API key ready in `ALPACA_KEY` and `ALPACA_SECRET`.

## Work with a coding agent

For later tasks, ask for something specific. Agents get better results when you also paste the relevant page, using **Copy page** beside its title. For example:

```prompt
Add a stream hub to my price feed handler that tracks
MACD and ADX, and raises an event when MACD crosses
its signal line while ADX is above 25.
```

```prompt
Backtest an RSI(2) mean-reversion rule on SPY daily bars,
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
