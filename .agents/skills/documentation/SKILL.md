---
name: documentation
description: Author indicator reference pages on the VitePress documentation site — page section order, parameter and result tables, warmup and convergence wording, chaining and streaming examples, the StockIndicatorChart block, and the sidebar, category, and index entries a new page needs. Use when creating or editing a docs/indicators/*.md page, when an indicator's parameters, results, warmup, chainability, or streaming support changes, or when adding an indicator chart to the site.
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
- [ ] From `docs/`, run `pnpm run test:links`, which builds the site and checks every link (it needs `htmlproofer` or Docker). Without either, run `pnpm run docs:build` (VS Code task `Build: Website`).

When code changes an existing indicator, update every section it touches: usage syntax, parameter table, bars requirement, null-period bullet, convergence warning, result table, chaining, and streaming.

## Page structure

Sections appear in this order. Prose separates sentences with two spaces, matching existing pages.

### Frontmatter and heading

- `title`: full name with the abbreviation in parentheses when one exists, e.g. `Exponential Moving Average (EMA)`. The H1 repeats it.
- `description`: one or two plain-text sentences on what the indicator measures; it becomes the page's meta description.
- First paragraph: "Created by {author}," when the author is known, a link to Wikipedia or the most authoritative source, and one sentence on what it measures.
- The next line, with no blank line between: `[[Discuss] &#128172;](https://github.com/facioquo/stock-indicators-dotnet/discussions/{id} "Community discussion about this indicator")`.

### Chart block

```html
<ClientOnly>
  <StockIndicatorChart indicator="Ema" />
</ClientOnly>
```

- `indicator` is a key of the `indicators` map in `docs/.vitepress/theme/index.ts` — PascalCase (`Ema`, `BollingerBands`, `AtrStop`), not the slug. Every key a page uses must exist there.
- A new key needs a `uiid` that the chart API at `charts-api.stockindicators.dev` serves, and a `title`. When the API does not serve the indicator, omit the chart block; never add a placeholder.
- Add `withOverlay` when the indicator plots in its own pane (oscillators such as `Rsi`, `Adx`); omit it for price overlays (`Ema`, `BollingerBands`).
- Stack related charts inside one `<ClientOnly>` with `withOverlay` on the first only (`std-dev.md`, `sma-analysis.md`). `with="DcPeriods"` adds a companion series to one chart (`ht-trendline.md`).
- `<ClientOnly>` is required; the component renders only in the browser.

### Usage syntax

A `csharp` block opening with `// C# usage syntax`, plus a qualifier when useful (`(with Close price)`), then `IReadOnlyList<{Name}Result> results =` with `bars.To{Name}(params);` on the next line. When an indicator has several meaningful overloads or variants, show each in the same block under its own comment (`ichimoku.md`, `renko.md`).

### Parameters

`## Parameters` with a `| param | type | description |` table. Write the type as italic code (`` _`int`_ ``). Each description names the formula variable (`` `N` ``), the validation constraint ("Must be greater than 0."), and the default when one exists. Variants with distinct parameter sets get an H3 table each (`renko.md`).

Omit `## Parameters` when the method takes none.

### Historical price bars requirements

An H3 under Parameters, or an H2 when Parameters is omitted (`adl.md`, `obv.md`, `tr.md`).

- State the minimum bar count in formula variables (`` `2×N` or `N+100` ``, whichever is more).
- For smoothed or recursive algorithms, add a sentence recommending more data (e.g., `` `N+250` ``) for precision.
- Close with the standard paragraph: "`bars` is a collection of generic `TBar` historical price bars.  It should have a consistent frequency (day, hour, minute, etc).  See [the Guide](/guide/getting-started#historical-bars) for more information."

### Response

- A `csharp` block with the return type.
- The three standard bullets: returns a time series of all available values for the `bars` provided; always returns the same number of elements as the bars; does not return a single incremental value.
- A fourth bullet naming the warmup nulls in formula variables ("The first `N-1` periods will have `null` values since there's not enough data to calculate."); omit it when no warmup period returns null.
- For smoothing or recursive algorithms, a convergence warning right after the bullets, with the period count and deviation adjusted to the indicator:

```markdown
::: warning 🚩 ⚞ Convergence warning
The first `N+100` periods will have decreasing magnitude, convergence-related precision errors that can be as high as ~5% deviation in indicator values for earlier periods.
:::
```

### Result table

`` ### `{Name}Result` `` with a `| property | type | description |` table. `Timestamp` is the first row, described as "Date from evaluated `TBar`". Each type is the property's declared C# type in italic code (`` _`double`_ ``, `` _`decimal`_ ``). Place a `::: warning 🚩` caveat immediately after the table it qualifies.

### Utilities

`### Utilities` lists `[.Condense()](/utilities/results#condense)`, `[.Find(lookupDate)](/utilities/results#find-by-date)`, and `[.RemoveWarmupPeriods(removePeriods)](/utilities/results#remove-warmup-periods)`, then "See [Utilities and helpers](/utilities/) for more information."

Add `[.RemoveWarmupPeriods()](/utilities/results#remove-warmup-periods)` before the `removePeriods` form when the no-argument overload removes the warmup: the indicator has its own overload in `{Name}.Utilities.cs`, or its result is `IReusable` and its first non-null value ends the warmup.

### Chaining

`## Chaining`, ending with "See [Chaining indicators](/guide/chaining) for more." Describe each direction the indicator supports:

- **Input** — "This indicator may be generated from any chain-enabled indicator or method." with a `.Use(CandlePart.HL2).To{Name}(..)` example; or, for bar-only indicators, "This indicator must be generated from `bars` and **cannot** be generated from results of another chain-enabled indicator or method."
- **Output** — "Results can be further processed on `{Property}` with additional chain-enabled indicators." with a `.To{Name}(..).ToRsi(..)` example. When the result has several values, name the reusable one ("Note: `TenkanSen` is the primary reusable value for chaining purposes.").

### Streaming

`## Streaming` with two examples, ending with "See [Buffer lists](/guide/styles/buffer) and [Stream hubs](/guide/styles/stream) for full usage guides."

- "Use the buffer-style `List<T>` when you need incremental calculations without a hub:" then `{Name}List {name}List = new(params);`, a `foreach` adding each bar, and `IReadOnlyList<{Name}Result> results = {name}List;`.
- "Subscribe to a `BarHub` for advanced streaming scenarios:" then `BarHub barHub = new();`, `{Name}Hub observer = barHub.To{Name}Hub(params);`, a `foreach` calling `barHub.Add(bar)`, and `observer.Results`.

Follow `ema.md` for the exact layout. Show only the styles the indicator implements.

When no streaming style exists, write one paragraph under `## Streaming`: "Streaming is not supported for this indicator." plus one sentence of architectural reason, then "Use the Series (batch) implementation with periodic recalculation instead." Existing reasons:

- Second synchronized series (`beta.md`, `correlation.md`, `prs.md`): "This indicator requires a second synchronized bar series, which cannot be expressed in the single-series streaming model."
- Lookahead and repaint (`zig-zag.md`): "This indicator requires lookahead to confirm reversal points; output repaints as new data arrives, making incremental results undefined."

When only one variant streams (`renko.md`), show its examples and add a `::: warning 🚩` stating which method does not stream and why. Keep all streaming content under the one `## Streaming` heading.

## Secondary analysis pages

A variant with its own result type (e.g., `sma-analysis.md`) gets its own page with the full section set. Its first paragraph ends with "See also [{primary name}](/indicators/{slug})." A chart block is allowed when the variant has its own chart keys (`SmaMad`, `SmaMape`, `SmaMse`).

## Images

Put static images in `docs/.vitepress/public/assets/` and reference them from the site root (`/assets/...`) with Markdown image syntax. Optimize to `webp` at 832px width, e.g. `cwebp -resize 832 0 -q 100 example.png -o example-832.webp`.

## Do not do these

- Do not link to `docs/AGENTS.md`, `docs/README.md`, or `docs/PRINCIPLES.md` from a page; `srcExclude` keeps them out of the build.
- Do not use a GitHub alert (`> [!WARNING]`) on a page; use a `:::` container.
- Do not add a second chart to show the same output at different parameter values.
- Do not add commentary or caveats beyond what the code's behavior requires.
