---
title: Volume Weighted Average Price (VWAP)
description: The Volume Weighted Average Price is a volume weighted average of price, typically used on intraday data. Trading above or below the VWAP line can assist in finding favorable short-term trading windows.
---

# Volume Weighted Average Price (VWAP)

The [Volume Weighted Average Price](https://en.wikipedia.org/wiki/Volume-weighted_average_price) is a Volume weighted average of price, typically used on intraday data.
[[Discuss] &#128172;](https://github.com/facioquo/stock-indicators-dotnet/discussions/310 "Community discussion about this indicator")

<ClientOnly>
  <StockIndicatorChart indicator="Vwap" />
</ClientOnly>

```csharp
// C# usage syntax
IReadOnlyList<VwapResult> results =
  bars.ToVwap();

// usage with optional anchored start date
IReadOnlyList<VwapResult> results =
  bars.ToVwap(startDate);
```

## Parameters

| param | type | description |
| ----- | ---- | ----------- |
| `startDate` | _`DateTime`_ | Optional.  The anchor date used to start the VWAP accumulation.  The earliest date in `bars` is used when not provided. |

### Historical price bars requirements

You must have at least one historical bar to calculate; however, more is often needed to be useful.  Historical price bars are typically provided for a single day using minute-based intraday periods.  Since this is an accumulated weighted average price, different start dates will produce different results.  The accumulation starts at the first period in the provided `bars`, unless it is specified in the optional `startDate` parameter.

`bars` is a collection of generic `TBar` historical price bars.  It should have a consistent frequency (day, hour, minute, etc).  See [the Guide](/guide/getting-started#historical-bars) for more information.

## Response

```csharp
IReadOnlyList<VwapResult>
```

- This method returns a time series of all available indicator values for the `bars` provided.
- It always returns the same number of elements as there are in the historical price bars.
- It does not return a single incremental indicator value.
- The first period or the `startDate` will have a `Vwap = Close` value since it is the initial starting point.
- `Vwap` values before `startDate`, if specified, will be `null`.

### `VwapResult`

| property | type | description |
| -------- | ---- | ----------- |
| `Timestamp` | _`DateTime`_ | Date from evaluated `TBar` |
| `Vwap` | _`double`_ | Volume Weighted Average Price |

### Utilities

- [.Condense()](/utilities/results#condense)
- [.Find(lookupDate)](/utilities/results#find-by-date)
- [.RemoveWarmupPeriods()](/utilities/results#remove-warmup-periods)
- [.RemoveWarmupPeriods(removePeriods)](/utilities/results#remove-warmup-periods)

See [Utilities and helpers](/utilities/) for more information.

## Chaining

Results can be further processed on `Vwap` with additional chain-enabled indicators.

```csharp
// example
var results = bars
    .ToVwap(..)
    .ToRsi(..);
```

This indicator must be generated from `bars` and **cannot** be generated from results of another chain-enabled indicator or method.

See [Chaining indicators](/guide/chaining) for more.

## Streaming

Use the buffer-style `List<T>` when you need incremental calculations without a hub:

```csharp
VwapList vwapList = new(startDate);

// call from your WebSocket or SSE message handler
void OnBarReceived(IBar bar) => vwapList.Add(bar);

// based on `ICollection<VwapResult>`; fills as the handler runs
IReadOnlyList<VwapResult> results = vwapList;
```

Subscribe to a `BarHub` for advanced streaming scenarios:

```csharp
BarHub barHub = new();
VwapHub observer = barHub.ToVwapHub(startDate);

// call from your WebSocket or SSE message handler
void OnBarReceived(IBar bar) => barHub.Add(bar);

// results fill as the handler runs
IReadOnlyList<VwapResult> results = observer.Results;
```

See [Buffer lists](/guide/styles/buffer) and [Stream hubs](/guide/styles/stream) for full usage guides.
