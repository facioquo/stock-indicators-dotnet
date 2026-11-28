---
title: Doji
description: Doji is a single-bar candlestick pattern where open and close price are virtually identical, representing market indecision.
---

# Doji {#doji-indicator}

[Doji](https://en.wikipedia.org/wiki/Doji) is a single-bar candlestick pattern where open and close price are virtually identical, representing market indecision.
[[Discuss] &#128172;](https://github.com/facioquo/stock-indicators-dotnet/discussions/734 "Community discussion about this indicator")

<StyleSwitch />

<StockIndicatorChart indicator="Doji" />

<a id="streaming"></a>

::: series

```csharp
// C# usage syntax
IReadOnlyList<CandleResult> results =
  bars.ToDoji(maxPriceChangePercent);
```

:::

::: buffer
Create a buffer list and add each bar as it arrives:

```csharp
DojiList dojiList = new(maxPriceChangePercent);

// call from your WebSocket or SSE message handler
void OnBarReceived(IBar bar) => dojiList.Add(bar);

// the list itself holds the results; fills as the handler runs
IReadOnlyList<CandleResult> results = dojiList;
```

See [Buffer lists](/guide/styles/buffer) for the full usage guide.
:::

::: stream
Subscribe to a `BarHub` and add each bar as it arrives:

```csharp
BarHub barHub = new();
DojiHub observer = barHub.ToDojiHub(maxPriceChangePercent);

// call from your WebSocket or SSE message handler
void OnBarReceived(IBar bar) => barHub.Add(bar);

// results fill as the handler runs
IReadOnlyList<CandleResult> results = observer.Results;
```

See [Stream hubs](/guide/styles/stream) for the full usage guide.
:::

## Parameters

| param | description |
| ----- | ----------- |
| `maxPriceChangePercent`<br>_`double`_ | Optional.  Maximum absolute percent difference in open and close price.  Example: 0.3% would be entered as 0.3 (not 0.003).  Must be between 0 and 0.5 percent, if specified.  Default is 0.1. |

### Historical price bars requirements

You must have at least one historical bar; however, more is typically provided since this is a chartable candlestick pattern.

`bars` is a collection of generic `TBar` historical price bars.  It should have a consistent frequency (day, hour, minute, etc).  See [the Guide](/guide/getting-started#historical-bars) for more information.

## Response

::: series

```csharp
IReadOnlyList<CandleResult>
```

- This method returns a time series of all available indicator values for the `bars` provided.
- It always returns the same number of elements as there are in the historical price bars.
- It does not return a single incremental indicator value.
- The candlestick pattern is indicated on dates where `Match` is `Match.Neutral`.
- `Price` is `Close` price; however, all OHLCV elements are included in `CandleProperties`.
- There is no intrinsic basis or confirmation signal provided for this pattern.
:::

::: buffer

```csharp
DojiList : IReadOnlyList<CandleResult>
```

- The list itself holds the results, one for each bar or value added, in timestamp order.
- It keeps up to 100,000 results by default and drops the oldest beyond that; set `MaxListSize` to keep fewer.
- The candlestick pattern is indicated on dates where `Match` is `Match.Neutral`.
- `Price` is `Close` price; however, all OHLCV elements are included in `CandleProperties`.
- There is no intrinsic basis or confirmation signal provided for this pattern.
:::

::: stream

```csharp
IReadOnlyList<CandleResult> results = observer.Results;
```

- `observer.Results` holds the results, one for each bar in the `BarHub` cache, in timestamp order.
- Results update as bars arrive, including bars that arrive late or are revised.
- The cache keeps up to 100,000 results by default; set `maxCacheSize` on the `BarHub` to keep fewer.
- The candlestick pattern is indicated on dates where `Match` is `Match.Neutral`.
- `Price` is `Close` price; however, all OHLCV elements are included in `CandleProperties`.
- There is no intrinsic basis or confirmation signal provided for this pattern.
:::

<!--@include: ../shared/candle-result.md-->

## Utilities

::: series

- [.Condense()](/utilities/results#condense)
- [.Find(lookupDate)](/utilities/results#find-by-date)
- [.RemoveWarmupPeriods(removePeriods)](/utilities/results#remove-warmup-periods)

:::

::: buffer

- [.Find(lookupDate)](/utilities/results#find-by-date)
- [.Condense()](/utilities/results#condense)

Call these on the list itself, as in `dojiList.Find(lookupDate)`. `.Condense()` returns a new list and leaves `dojiList` unchanged.

Warmup removal is left out. Once the list is full it drops its oldest results, so trimming by position would remove valid values.
:::

::: stream

- [.Find(lookupDate)](/utilities/results#find-by-date)
- [.Condense()](/utilities/results#condense)

Call these on `observer.Results`, as in `observer.Results.Find(lookupDate)`. `.Condense()` returns a new list and leaves the hub's results unchanged.

Warmup removal is left out. Once the cache is full the hub drops its oldest results, so trimming by position would remove valid values.
:::

See [Utilities and helpers](/utilities/) for more information.
