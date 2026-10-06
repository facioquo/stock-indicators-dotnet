---
title: Doji
description: Doji is a single-bar candlestick pattern where open and close price are virtually identical, representing market indecision.
---

# Doji {#doji-indicator}

[Doji](https://en.wikipedia.org/wiki/Doji) is a single-bar candlestick pattern where open and close price are virtually identical, representing market indecision.
[[Discuss] &#128172;](https://github.com/facioquo/stock-indicators-dotnet/discussions/734 "Community discussion about this indicator")

<StockIndicatorChart indicator="Doji" />

```csharp
// C# usage syntax
IReadOnlyList<CandleResult> results =
  bars.ToDoji(maxPriceChangePercent);
```

## Parameters

| param | type | description |
| ----- | ---- | ----------- |
| `maxPriceChangePercent` | _`double`_ | Optional.  Maximum absolute percent difference in open and close price.  Example: 0.3% would be entered as 0.3 (not 0.003).  Must be between 0 and 0.5 percent, if specified.  Default is 0.1 (0.1%). |

### Historical price bars requirements

You must have at least one historical bar; however, more is typically provided since this is a chartable candlestick pattern.

`bars` is a collection of generic `TBar` historical price bars.  It should have a consistent frequency (day, hour, minute, etc).  See [the Guide](/guide/getting-started#historical-bars) for more information.

## Response

```csharp
IReadOnlyList<CandleResult>
```

- This method returns a time series of all available indicator values for the `bars` provided.
- It always returns the same number of elements as there are in the historical price bars.
- It does not return a single incremental indicator value.
- The candlestick pattern is indicated on dates where `Match` is `Match.Neutral`.
- `Price` is `Close` price; however, all OHLCV elements are included in `CandleProperties`.
- There is no intrinsic basis or confirmation signal provided for this pattern.

<!--@include: ../shared/candle-result.md-->

### Utilities

- [.Condense()](/utilities/results#condense)
- [.Find(lookupDate)](/utilities/results#find-by-date)
- [.RemoveWarmupPeriods(removePeriods)](/utilities/results#remove-warmup-periods)

See [Utilities and helpers](/utilities/) for more information.

## Streaming

Use the buffer-style `List<T>` when you need incremental calculations without a hub:

```csharp
DojiList dojiList = new(maxPriceChangePercent);

// call from your WebSocket or SSE message handler
void OnBarReceived(IBar bar) => dojiList.Add(bar);

// based on `ICollection<CandleResult>`; fills as the handler runs
IReadOnlyList<CandleResult> results = dojiList;
```

Subscribe to a `BarHub` for advanced streaming scenarios:

```csharp
BarHub barHub = new();
DojiHub observer = barHub.ToDojiHub(maxPriceChangePercent);

// call from your WebSocket or SSE message handler
void OnBarReceived(IBar bar) => barHub.Add(bar);

// results fill as the handler runs
IReadOnlyList<CandleResult> results = observer.Results;
```

See [Buffer lists](/guide/styles/buffer) and [Stream hubs](/guide/styles/stream) for full usage guides.
