---
title: Agent setup
description: Get a coding agent up to speed on Stock Indicators for .NET, with a copy-paste setup prompt and tips for working with it.
---

# Agent setup

Paste this prompt into your coding agent (Claude, ChatGPT, Copilot, etc.). It reads the documentation, installs the library, and helps you calculate your first indicator:

```prompt
Read https://dotnet.stockindicators.dev/llms.txt and its getting started guide,
then help me install the FacioQuo.Stock.Indicators NuGet package
and calculate my first indicator from my own price data.
```

## Work with a coding agent

For later tasks, ask for something specific. Agents get better results when you also paste the relevant page, using **Copy page** beside its title. For example:

```prompt
Add IBar to my existing price-bar class so I can use it
with Stock Indicators for .NET, then calculate a 20-period SMA.
```

```prompt
Compare Batch, Buffer, and Stream indicator styles for an
application that processes live market data.
```

```prompt
Find the indicator that measures trend strength, then provide
a minimal C# example and explain its warmup requirements.
```

Point agents at the [`/llms.txt`](/llms.txt) index so they fetch only the pages they need. Save [`/llms-full.txt`](/llms-full.txt), all the documentation in one large file, for broad analysis; for routine questions it crowds your own code out of the agent's context.

## Machine-readable documentation

- **Markdown pages**: add `.md` to any page URL, such as `/indicators/sma.md`, or request the page with an `Accept: text/markdown` header.
- **Agent skill**: agents that support [Agent Skills](https://agentskills.io) can install the skill listed at [`/.well-known/agent-skills/index.json`](/.well-known/agent-skills/index.json).
- **Browser tools**: in browsers that support [WebMCP](https://webmachinelearning.github.io/webmcp/), every page exposes read-only tools to search the documentation, get any indexed page as Markdown, and get the current page as Markdown.
