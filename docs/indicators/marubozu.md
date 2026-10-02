---
title: Marubozu
description: Marubozu is a single-bar candlestick pattern that has no wicks, representing consistent directional movement.
---

# Marubozu {#marubozu-indicator}

[Marubozu](https://en.wikipedia.org/wiki/Marubozu) is a single-bar candlestick pattern that has no wicks, representing consistent directional movement.
[[Discuss] &#128172;](https://github.com/facioquo/stock-indicators-dotnet/discussions/512 "Community discussion about this indicator")

<ClientOnly>
  <StockIndicatorChart indicator="Marubozu" />
</ClientOnly>

```csharp
// C# usage syntax
IReadOnlyList<CandleResult> results =
  bars.ToMarubozu(minBodyPercent);
```

## Parameters

| param | type | description |
| ----- | ---- | ----------- |
| `minBodyPercent` | _`double`_ | Optional.  Minimum body size as a percent of total candle size.  Example: 85% would be entered as 85 (not 0.85).  Must be between 80 and 100, if specified.  Default is 95 (95%). |

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
- The candlestick pattern is indicated on dates where `Match` is `Match.BullSignal` or `Match.BearSignal`.
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
MarubozuList marubozuList = new(minBodyPercent);

// call from your WebSocket or SSE message handler
void OnBarReceived(IBar bar) => marubozuList.Add(bar);

// based on `ICollection<CandleResult>`
IReadOnlyList<CandleResult> results = marubozuList;
```

Subscribe to a `BarHub` for advanced streaming scenarios:

```csharp
BarHub barHub = new();
MarubozuHub observer = barHub.ToMarubozuHub(minBodyPercent);

// call from your WebSocket or SSE message handler
void OnBarReceived(IBar bar) => barHub.Add(bar);

IReadOnlyList<CandleResult> results = observer.Results;
```

See [Buffer lists](/guide/styles/buffer) and [Stream hubs](/guide/styles/stream) for full usage guides.
