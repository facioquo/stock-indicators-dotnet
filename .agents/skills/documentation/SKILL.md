---
name: documentation
description: Author indicator reference pages on the VitePress documentation site — page section order, parameter and result tables, warmup and convergence wording, per-style (Series, Buffer, Stream) containers and examples, the StockIndicatorChart block, and the sidebar, category, and index entries a new page needs. Use when creating or editing a docs/indicators/*.md page, when an indicator's parameters, results, warmup, chainability, or streaming support changes, or when adding an indicator chart to the site.
---

# Indicator documentation pages

Indicator pages live at `docs/indicators/{slug}.md`, where the slug is lowercase kebab-case (`sma.md`, `atr-stop.md`, `stoch-rsi.md`). Copy the structure of a close existing page — `ema.md` for a chainable indicator with parameters, `adl.md` for one without — and apply the rules below.

The markdown skill owns formatting and linting. The vitepress skill owns `docs/.vitepress/` configuration, theme, and components.

## Adding a new indicator page

- [ ] Create `docs/indicators/{slug}.md` with the [page structure](#page-structure).
- [ ] Add a sidebar entry under the indicator's category in `docs/.vitepress/config.mts`. `llms.txt` derives its table of contents from the sidebar.
- [ ] Add a `features` card to the category hub page (e.g., `docs/indicators/oscillators.md`) with `title`, `details`, `icon.src` of `/assets/thumbs/indicators/{slug}.png`, and `link`, and add that thumbnail to `docs/.vitepress/public/assets/thumbs/indicators/`.
- [ ] Add the page to its category list under "Complete list" in `docs/indicators.md`.
- [ ] Add the [chart block](#chart-block) when the chart API serves the indicator.
- [ ] Run the docs-site test gates in the code-completion skill; the parity suite checks the page against the catalog and the source.
- [ ] From `docs/`, run `pnpm run test:links`, which builds the site and checks every link (it needs `htmlproofer` or Docker). Without either, run `pnpm run docs:build` (VS Code task `Build: Website`).

When code changes an existing indicator, update every section it touches: usage syntax for each style, parameter table, bars requirement, null-period bullets, convergence warning, result table, and chaining.

## Page structure

Sections appear in this order. Prose separates sentences with two spaces, matching existing pages.

### Style containers

Each page serves all three indicator styles from one file and one URL. Wrap content that applies to one style in a `::: series`, `::: buffer`, or `::: stream` container; content outside them is shared. The reader's chosen style hides the other two (`docs/.vitepress/style-containers.ts`, `theme/indicator-style.ts`), and the agent-facing `.md` twin unwraps all three under a label.

- Never put a heading inside a style container, so the outline, anchors, and search sections are the same for every style.
- Put a blank line between a container marker and a code fence (markdownlint MD031). Nest a `::: warning` inside a style container when it applies to that style only.
- Usage, Response, and Chaining are per-style; everything else is shared.
- Put `<StyleSwitch />` on its own line after the intro paragraph, before the chart block.

### Frontmatter and heading

- `title`: full name with the abbreviation in parentheses when one exists, e.g. `Exponential Moving Average (EMA)`. The H1 repeats it.
- `description`: one or two plain-text sentences on what the indicator measures; it becomes the page's meta description.
- First paragraph: "Created by {author}," when the author is known, a link to Wikipedia or the most authoritative source, and one sentence on what it measures.
- The next line, with no blank line between: `[[Discuss] &#128172;](https://github.com/facioquo/stock-indicators-dotnet/discussions/{id} "Community discussion about this indicator")`.

### Chart block

```html
<StockIndicatorChart indicator="Ema" />
```

- `indicator` is a key of `CHART_INDICATORS` in `docs/.vitepress/theme/chart-indicators.ts` — PascalCase (`Ema`, `BollingerBands`, `AtrStop`), not the slug. Every key a page uses must exist there.
- A new key needs a `uiid` that the chart API at `charts-api.stockindicators.dev` serves, a `title`, and `chartType: 'oscillator'` when the API lists it as an oscillator (the hint sizes the reserved chart frame before data loads). When the API does not serve the indicator, omit the chart block; never add a placeholder.
- The site renders charts from a committed offline snapshot in `docs/.vitepress/public/data/chart-api/`. After adding a key, run `pnpm run snapshot:charts` from `docs/` and commit the new files, or the offline tests fail until the monthly refresh workflow runs.
- Add `withOverlay` when the indicator plots in its own pane (oscillators such as `Rsi`, `Adx`); omit it for price overlays (`Ema`, `BollingerBands`).
- Stack related charts on consecutive lines with `withOverlay` on the first only (`std-dev.md`, `sma-analysis.md`). `with="DcPeriods"` adds a companion series to one chart (`ht-trendline.md`).
- Don't wrap the chart in `<ClientOnly>`. The component renders its sized frames during the static build, so the page doesn't shift when the chart loads.

### Usage syntax

Right after the chart block, one container per style:

- `::: series` — a `csharp` block opening with `// C# usage syntax`, plus a qualifier when useful (`(with Close price)`), then `IReadOnlyList<{Name}Result> results =` with `bars.To{Name}(params);` on the next line. When an indicator has several meaningful overloads or variants, show each in the same block under its own comment (`ichimoku.md`, `renko.md`).
- `::: buffer` — "Create a buffer list and add each bar as it arrives:" then `{Name}List {name}List = new(params);`, a handler that adds each bar (`// call from your WebSocket or SSE message handler` above `void OnBarReceived(IBar bar) => {name}List.Add(bar);`), then `// the list itself holds the results; fills as the handler runs` above `IReadOnlyList<{Name}Result> results = {name}List;`. End with "See [Buffer lists](/guide/styles/buffer) for the full usage guide."
- `::: stream` — "Subscribe to a `BarHub` and add each bar as it arrives:" then `BarHub barHub = new();`, `{Name}Hub observer = barHub.To{Name}Hub(params);`, the same handler calling `barHub.Add(bar)`, and `// results fill as the handler runs` above `observer.Results`. End with "See [Stream hubs](/guide/styles/stream) for the full usage guide."

Two-series indicators (`correlation.md`, `beta.md`, `prs.md`) show `AddSource`/`AddOther` and one `BarHub` per series, each followed by its pairing caveats.

When a style is not implemented, its usage container holds one paragraph: "Buffer lists are not supported for this indicator." (or "Stream hubs …") plus one sentence of architectural reason, then "Use the Series (batch) implementation with periodic recalculation instead." Existing reasons:

- Lookahead and repaint (`zig-zag.md`): "This indicator requires lookahead to confirm reversal points; output repaints as new data arrives, making incremental results undefined."
- Quadratic recalculation (`std-dev-channels.md`).

When only one variant supports a style (`renko.md`), show its example and nest a `::: warning 🚩` stating which method lacks it and why. Omit that style's Response and Chaining containers when it is not implemented.

### Parameters

`## Parameters` with a `| param | description |` table. The first cell holds the name in code, a `<br>`, then the type as italic code (`` `lookbackPeriods`<br>_`int`_ ``), so the description keeps its width at phone size. Each description names the formula variable (`` `N` ``), the validation constraint ("Must be greater than 0."), and the default when one exists. Variants with distinct parameter sets get an H3 table each (`renko.md`).

Omit `## Parameters` when the method takes none.

### Historical price bars requirements

An H3 under Parameters, or an H2 when Parameters is omitted (`adl.md`, `obv.md`, `tr.md`).

- State the minimum bar count in formula variables (`` `2×N` or `N+100` ``, whichever is more).
- For smoothed or recursive algorithms, add a sentence recommending more data (e.g., `` `N+250` ``) for precision.
- Close with the standard paragraph: "`bars` is a collection of generic `TBar` historical price bars.  It should have a consistent frequency (day, hour, minute, etc).  See [the Guide](/guide/getting-started#historical-bars) for more information."

### Response

One container per style, each with a `csharp` block and bullets, ending with the same warmup bullet naming the nulls in formula variables ("The first `N-1` periods will have `null` values since there's not enough data to calculate."); omit that bullet when no warmup period returns null.

- `::: series` — `IReadOnlyList<{Name}Result>`, then the three standard bullets: returns a time series of all available values for the `bars` provided; always returns the same number of elements as the bars; does not return a single incremental value.
- `::: buffer` — `{Name}List : IReadOnlyList<{Name}Result>`, then "The list itself holds the results, one for each bar or value added, in timestamp order." and "It keeps up to 100,000 results by default and drops the oldest beyond that; set `MaxListSize` to keep fewer."
- `::: stream` — `IReadOnlyList<{Name}Result> results = observer.Results;`, then "`observer.Results` holds the results, one for each bar in the `BarHub` cache, in timestamp order.", "Results update as bars arrive, including bars that arrive late or are revised.", and "The cache keeps up to 100,000 results by default; set `maxCacheSize` on the `BarHub` to keep fewer."

Two-series pages say "one for each timestamp both series hold" (buffer) and "both providers hold" (stream). For smoothing or recursive algorithms, a shared convergence warning follows the containers, with the period count and deviation adjusted to the indicator:

```markdown
::: warning 🚩 ⚞ Convergence warning
The first `N+100` periods will have decreasing magnitude, convergence-related precision errors that can be as high as ~5% deviation in indicator values for earlier periods.
:::
```

### Result table

`` ### `{Name}Result` `` with a `| property | description |` table. The first cell holds the name, a `<br>`, then the property's declared C# type in italic code (`` `Adx`<br>_`double`_ ``). `Timestamp` is the first row, described as "Date from evaluated `TBar`". Place a `::: warning 🚩` caveat immediately after the table it qualifies.

An enum table in `docs/shared/` has an `| enum / value | description |` header; the first cell holds the member name in code, a `<br>`, then its numeric value.

### Utilities

`### Utilities` lists `[.Condense()](/utilities/results#condense)`, `[.Find(lookupDate)](/utilities/results#find-by-date)`, and `[.RemoveWarmupPeriods(removePeriods)](/utilities/results#remove-warmup-periods)`, then "See [Utilities and helpers](/utilities/) for more information."

Add `[.RemoveWarmupPeriods()](/utilities/results#remove-warmup-periods)` before the `removePeriods` form when the no-argument overload removes the warmup: the indicator has its own overload in `{Name}.Utilities.cs`, or its result is `IReusable` and its first non-null value ends the warmup.

### Chaining

`## Chaining`. Shared, before the containers: the bar-only sentence ("This indicator must be generated from `bars` and **cannot** be generated from results of another chain-enabled indicator or method.") and, when the result has several values, the reusable one ("Note: `TenkanSen` is the primary reusable value for chaining purposes."). Then one container per style, each describing the directions it supports with an example:

- `::: series` — **Input**: "This indicator may be generated from any chain-enabled indicator or method." with a `.Use(CandlePart.HL2).To{Name}(..)` example.  **Output**: "Results can be further processed on `{Property}` with additional chain-enabled indicators." with a `.To{Name}(..).ToRsi(..)` example.  End with "See [Chaining indicators](/guide/chaining) for more."
- `::: buffer` — the reader passes results along: an `OnBarReceived` handler that adds the bar to one list and `[^1]` of it to the next (`smaList` into `{name}List` for input; `{name}List` into `rsiList` for output). Input needs `{Name}List` to implement `IIncrementFromChain`. End with "See [Chaining buffer lists](/guide/styles/buffer#chaining-indicators) for more."
- `::: stream` — input: `barHub.ToBarPartHub(CandlePart.HL2).To{Name}Hub(..)` when `To{Name}Hub` extends `IChainProvider<IReusable>`; output: `barHub.To{Name}Hub(..).ToRsiHub(..)` when `{Name}Hub` derives from `ChainHub`. When the hub derives from `StreamHub` directly, write "Stream hub results **cannot** be further chained with additional hubs." End with "See [Chaining stream hubs](/guide/styles/stream#chaining-indicators) for more."

Shared text must hold for every style. When styles differ, move the sentence into each container that it is true for (`pvo.md`: Series and Buffer take bars only, while the hub accepts any chain provider). When no style chains in either direction, write the whole section as shared text with no containers: "This indicator is not chain-enabled and must be generated from `bars`.  It **cannot** be used for further processing by other chain-enabled indicators." (`donchian.md`), or the bar-only sentence plus "Results **cannot** be further chained with additional transforms." (`keltner.md`, whose result is `ISeries`). A result based on `IBar` chains in every style (`renko.md`, `heikin-ashi.md`).

Follow `ema.md` for the exact layout, and `adl.md` for a bar-only indicator. Keep `<a id="streaming"></a>` just before the usage containers so old `#streaming` links still land on the examples.

## Secondary analysis pages

A variant with its own result type (e.g., `sma-analysis.md`) gets its own page with the full section set. Its first paragraph ends with "See also [{primary name}](/indicators/{slug})." A chart block is allowed when the variant has its own chart keys (`SmaMad`, `SmaMape`, `SmaMse`).

## Images

Put static images in `docs/.vitepress/public/assets/` and reference them from the site root (`/assets/...`) with Markdown image syntax. Optimize to `webp` at 832px width, e.g. `cwebp -resize 832 0 -q 100 example.png -o example-832.webp`.

## Do not do these

- Do not link to `docs/AGENTS.md`, `docs/README.md`, or `docs/PRINCIPLES.md` from a page; `srcExclude` keeps them out of the build.
- Do not use a GitHub alert (`> [!WARNING]`) on a page; use a `:::` container.
- Do not add a second chart to show the same output at different parameter values.
- Do not add commentary or caveats beyond what the code's behavior requires.
