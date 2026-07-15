---
name: documentation
description: Write and update the VitePress documentation website for stock indicators. Use when adding a new indicator page, updating an existing indicator page, or making structural changes to the docs site.
---

# Documentation skill

This skill covers writing and updating the VitePress documentation site located in the `docs/` folder, with focus on indicator reference pages.

## When to use this skill

- Adding a documentation page for a new indicator
- Updating an existing indicator page after API or behavioral changes
- Auditing indicator pages for structural consistency
- Adding chart panels, warnings, or special sections

## Documentation site overview

The site is built with [VitePress](https://vitepress.dev) and Vue 3.

| Path | Purpose |
| ---- | ------- |
| `docs/indicators/` | One `.md` file per indicator (the primary audience) |
| `docs/.vitepress/config.mts` | Site config — nav, sidebar, metadata |
| `docs/.vitepress/components/` | Vue components used inside Markdown |
| `docs/.vitepress/public/assets/` | Static images (webp, optimized) |
| `docs/.vitepress/public/data/chart-api/` | Chart data API — one lowercase-keyed JSON file per indicator, powers `<StockIndicatorChart>` |
| `docs/guide/getting-started.md` | Get started: install + first call |
| `docs/guide/index.md` | Guide overview + style comparison |
| `docs/guide/styles/batch.md` | Batch (Series) style guide |
| `docs/guide/styles/buffer.md` | Buffer list style guide |
| `docs/guide/styles/stream.md` | Stream hub style guide |
| `docs/guide/chaining.md` | Indicator chaining guide |
| `docs/guide/customization.md` | Custom indicators guide |
| `docs/guide/custom-observers.md` | Custom observer integration guide |
| `docs/indicators.md` | Indicators landing/index page |
| `docs/utilities/` | Utility API reference pages |

**Excluded from build**: `AGENTS.md`, `PRINCIPLES.md`, and `README.md` are excluded via `srcExclude` in `config.mts` and must not be linked from indicator pages.

### Local development

```bash
# from /docs folder
pnpm install
pnpm run docs:dev
# site opens at http://localhost:5173/
```

## Indicator page structure

Every indicator page at `docs/indicators/{indicator-slug}.md` (lowercase kebab-case, e.g. `sma.md`, `atr-stop.md`, `stoch-rsi.md`) follows this section order exactly. Never omit sections; use judgment about which optional elements apply.

### 1. Frontmatter

```yaml
---
title: Full Indicator Name (ABBR)
description: One-sentence description of what the indicator measures.
---
```

- `title`: Human-readable name with abbreviation in parentheses when one exists
- `description`: Concise (≤160 characters), plain-text sentence — used for SEO and link previews

### 2. H1 heading block

```markdown
# Full Indicator Name (ABBR)

Created by Author Name, [Indicator Name](https://en.wikipedia.org/wiki/...) is a brief description of what it measures.
[[Discuss] &#128172;](https://github.com/facioquo/stock-indicators-dotnet/discussions/{id} "Community discussion about this indicator")

<ClientOnly>
  <StockIndicatorChart indicator="{Indicator}" />
</ClientOnly>
```

Rules:

- Attribution ("Created by...") is required when a known author exists
- Include a Wikipedia link when one exists; otherwise link the most authoritative reference
- Always include the `[[Discuss]]` link using the correct GitHub Discussions URL
- Include the chart block when chart data exists at `docs/.vitepress/public/data/chart-api/{indicator-slug}.json` (lowercase); omit when no chart data exists (e.g., simple primitives or secondary analysis pages)
- `indicator` prop is the indicator's PascalCase key (e.g., `Sma`, `Rsi`) — it does not have to match the lowercase JSON filename literally, but must resolve to it via the chart API
- Always wrap the component in `<ClientOnly>` — the chart renders client-side only

### 3. Usage syntax

````markdown
```csharp
// C# usage syntax
IReadOnlyList<{Indicator}Result> results =
  bars.To{Indicator}(param1, param2);
```
````

- Use `// C# usage syntax` as the comment; add `(with Close price)` or similar qualifier when relevant
- Show each meaningful overload on separate `bars.To{Indicator}(...)` lines when multiple overloads exist
- Wrap long signatures to the next line for readability

### 4. Parameters section

```markdown
## Parameters

| param | type | description |
| ----- | ---- | ----------- |
| `paramName` | int | Description with constraints. Default is N. |
```

Omit this section entirely when the indicator has no parameters (e.g., `ToAdl()`, `ToTr()`).

Parameter description rules:

- State the variable name used in formulas (e.g., "`N`" or "`S`")
- State validation constraints ("Must be greater than 0")
- State default value when one exists ("Default is 14")

#### Historical price bars requirements subsection

Always present, even for parameter-free indicators:

```markdown
### Historical price bars requirements

You must have at least `N` periods of `bars` to cover the warmup periods.

`bars` is a collection of generic `TBar` historical price bars.  It should have a consistent frequency (day, hour, minute, etc).  See [the Guide](/guide/getting-started#historical-bars) for more information.
```

- Express the minimum in terms of formula variables (e.g., `N`, `2×(S+P)`, `S+P+100`)
- When the indicator uses exponential smoothing or recursive formulas, include an additional sentence recommending more data (e.g., "we recommend you use at least `10×N` data points prior to the intended usage date for better precision")

### 5. Response section

````markdown
## Response

```csharp
IReadOnlyList<{Indicator}Result>
```

- This method returns a time series of all available indicator values for the `bars` provided.
- It always returns the same number of elements as there are in the historical price bars.
- It does not return a single incremental indicator value.
- The first `N-1` periods will have `null` values since there's not enough data to calculate.
````

Adjust the null-period bullet to match the actual warmup (e.g., "The first `S-1` slow periods...").

#### Convergence warning (conditional)

Add immediately after the bullet list when the indicator uses a smoothing/recursive algorithm that introduces convergence error:

```markdown
::: warning 🚩 ⚞ Convergence warning
The first `10×N` periods will have decreasing magnitude, convergence-related precision errors that can be as high as ~5% deviation in indicator values for earlier periods.
:::
```

Adjust the period count and percentage to match the indicator's actual convergence behavior.

#### Result type table

```markdown
### `{Indicator}Result`

| property | type | description |
| -------- | ---- | ----------- |
| `Timestamp` | DateTime | Date from evaluated `TBar` |
| `PropertyName` | double | What this value represents |
```

- `Timestamp` is always the first row
- Use `double` for computed values, `decimal` for price-derived values (e.g., Ichimoku), `int` for counts
- Describe each property in plain language, including the formula when it aids understanding

#### Utilities subsection

```markdown
### Utilities

- [.Condense()](/utilities/results#condense)
- [.Find(lookupDate)](/utilities/results#find-by-date)
- [.RemoveWarmupPeriods()](/utilities/results#remove-warmup-periods)
- [.RemoveWarmupPeriods(removePeriods)](/utilities/results#remove-warmup-periods)

See [Utilities and helpers](/utilities/) for more information.
```

Utility links are anchors (`#section`) into the single `docs/utilities/results.md` page, not separate subpages. Omit `.RemoveWarmupPeriods(removePeriods)` overload when the indicator does not support a custom count.

### 6. Chaining section

````markdown
## Chaining

This indicator may be generated from any chain-enabled indicator or method.

```csharp
// example
var results = bars
    .Use(CandlePart.HL2)
    .To{Indicator}(..);
```

Results can be further processed on `{PrimaryValue}` with additional chain-enabled indicators.

```csharp
// example
var results = bars
    .To{Indicator}(..)
    .ToRsi(..);
```
````

Variations:

- When the indicator can only read from `bars` (not chains), replace the first block with: `This indicator must be generated from \`bars\` and **cannot** be generated from results of another chain-enabled indicator or method.`
- When the indicator can only output to chains (not read from them), describe that instead
- When chaining outputs to a specific property, note which property is the chainable value (e.g., "Note: \`TenkanSen\` is the primary reusable value for chaining purposes.")

End the section with a cross-reference to the usage guide:

```markdown
See [Chaining indicators](/guide/chaining) for more.
```

### 7. Streaming section

````markdown
## Streaming

Use the buffer-style `List<T>` when you need incremental calculations without a hub:

```csharp
{Indicator}List {indicator}List = new(param1, param2);

foreach (IBar bar in bars)  // simulating stream
{
  {indicator}List.Add(bar);
}

// based on `ICollection<{Indicator}Result>`
IReadOnlyList<{Indicator}Result> results = {indicator}List;
```

Subscribe to a `BarHub` for advanced streaming scenarios:

```csharp
BarHub barHub = new();
{Indicator}Hub observer = barHub.To{Indicator}Hub(param1, param2);

foreach (IBar bar in bars)  // simulating stream
{
  barHub.Add(bar);
}

IReadOnlyList<{Indicator}Result> results = observer.Results;
```
````

When the hub subscribes to an upstream chain-enabled hub (not a `BarHub`), adjust the hub variable type and creation accordingly.

End the section with a cross-reference to the usage guides:

```markdown
See [Buffer lists](/guide/styles/buffer) and [Stream hubs](/guide/styles/stream) for full usage guides.
```

### 7a. Streaming not applicable

When an indicator cannot support streaming due to an architectural constraint, include `## Streaming` with this standard wording instead of examples:

```markdown
## Streaming

Streaming is not supported for this indicator.
{One sentence stating the architectural reason.}
Use the Series (batch) implementation with periodic recalculation instead.
```

Standard reasons by category:

- **Dual-series indicators** (Beta, Correlation, Prs): "This indicator requires a second synchronized bar series, which cannot be expressed in the single-series streaming model."
- **Lookahead/repaint indicators** (ZigZag): "This indicator requires lookahead to confirm reversal points; output repaints as new data arrives, making incremental results undefined."
- **Full-dataset algorithms** (StdDevChannels): "This indicator recalculates the entire dataset on each new data point, making incremental streaming impractical."

## Optional and conditional sections

### Inline warnings

Use VitePress admonition containers for notable caveats that don't warrant a full section:

```markdown
::: warning 🚩
Brief warning text here.
:::
```

Place these immediately after the content they qualify (e.g., after the result table).

### Extended or secondary indicator pages

When an indicator has a secondary analysis variant (e.g., `SmaAnalysis`), create a separate page. Secondary pages:

- Omit the `<StockIndicatorChart>` block (chart lives on the primary page)
- Begin the H1 body with a cross-link to the primary page: "See also [Simple Moving Average](/indicators/sma)."
- Include the full Parameters, Response, Chaining, and Streaming sections for the variant

### Multiple overloads shown together

When an indicator has multiple overloads worth showing upfront (e.g., Ichimoku), show all meaningful signatures in a single code block before the Parameters table, with brief comments distinguishing each:

```csharp
// standard usage
IReadOnlyList<...> results = bars.ToIndicator(a, b);

// usage with custom option
IReadOnlyList<...> results = bars.ToIndicator(a, b, c);
```

### Multi-variant indicators (substantially different parameter sets)

When two variants share the same result type but have distinct parameter sets and different streaming support (e.g., Renko fixed-brick vs. ATR-derived), merge them into one page with a shared structure:

1. Show both signatures in the usage syntax block with distinguishing comments
2. Use `## Parameters` with H3 subsections per variant (`### Fixed brick size`, `### ATR-derived brick size`)
3. Combine `## Response` into one section (shared result type)
4. Combine `## Chaining` — note any variant-specific restrictions
5. In `## Streaming`, show full examples for the supported variant, then add a `warning` admonition for the unsupported variant:

````markdown
## Streaming

[Full buffer/hub examples for the supported variant]

::: warning 🚩
`ToVariantAtr()` does not support streaming.  {One sentence reason.}
:::
````

Do not create a separate H2 section for the unsupported variant — keep all streaming content under one `## Streaming` heading.

## Image assets

Charts render client-side via the `<StockIndicatorChart>` component (from `@facioquo/indy-charts`, registered in `docs/.vitepress/theme/index.ts`), fed by JSON data files at `docs/.vitepress/public/data/chart-api/{indicator-slug}.json` (lowercase). When adding a new indicator:

1. Confirm whether a chart-api JSON data file exists for the indicator
2. If it exists, add `<ClientOnly><StockIndicatorChart indicator="{Indicator}" /></ClientOnly>` to the page
3. If it does not exist, omit the chart block — do not add a placeholder

A second chart block mid-page is valid when it illustrates a behaviorally distinct mode of the same indicator (e.g., StdDevChannels with `null` lookback renders differently than a fixed-period run; HtTrendline exposes both a trendline output and a dominant cycle period output). In that case, place the second block immediately after the prose that introduces the distinct behavior, under its own H2 or H3 heading. Do not add a second block merely to show the same output at different parameter values.

For static (non-chart) images referenced in prose:

- Store in `docs/.vitepress/public/assets/`
- Optimize to `webp` format at 832px width: `cwebp -resize 832 0 -q 100 example.png -o example-832.webp`
- Reference via absolute path from public root: `/assets/filename-832.webp`

## Checklist: adding a new indicator page

- [ ] Create `docs/indicators/{indicator-slug}.md` (lowercase kebab-case) following the page structure above
- [ ] Frontmatter: `title` and `description` set
- [ ] H1 block: attribution, reference link, Discuss link, optional chart block
- [ ] Usage syntax: all meaningful overloads shown
- [ ] Parameters table present (or section omitted if none)
- [ ] Historical price bars requirements stated
- [ ] Response section: return type, bullet list, result table, Utilities links
- [ ] Chaining section: correct chainability direction described
- [ ] Streaming section: full examples present, or "not applicable" note with architectural reason
- [ ] Site builds without errors: `pnpm run docs:dev` from `docs/`

## Checklist: updating an existing indicator page

- [ ] Identify sections affected by the code change
- [ ] Update parameter table if parameters added, removed, or renamed
- [ ] Update result table if result properties added, removed, or renamed
- [ ] Update usage syntax examples to match new signatures
- [ ] Update bars requirements if warmup formula changed
- [ ] Add or remove convergence warning as appropriate
- [ ] Site builds without errors

## Style conventions

- Use two spaces (not one) between sentences in prose — the existing pages do this consistently
- Backtick inline code for all: type names, property names, parameter names, method names, numeric expressions like `N-1`
- Use `N`, `S`, `F`, `P` as single-letter formula variable shorthand — define them on first use
- Tables use the three-column format with aligned pipes
- Do not add commentary, caveats, or editorializing beyond what the code behavior requires — keep descriptions factual and concise
