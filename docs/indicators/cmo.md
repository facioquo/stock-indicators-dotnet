---
title: Chande Momentum Oscillator (CMO)
description: The Chande Momentum Oscillator is a momentum indicator depicting the weighted percent of higher prices in financial markets.
---

# Chande Momentum Oscillator (CMO)

Created by Tushar Chande, the [Chande Momentum Oscillator](https://www.investopedia.com/terms/c/chandemomentumoscillator.asp) is a weighted percent of higher prices over a lookback window.
[[Discuss] &#128172;](https://github.com/facioquo/stock-indicators-dotnet/discussions/892 "Community discussion about this indicator")

<ClientOnly>
  <StockIndicatorChart indicator="Cmo" withOverlay />
</ClientOnly>

```csharp
// C# usage syntax
IReadOnlyList<CmoResult> results =
  bars.ToCmo(lookbackPeriods);
```

## Parameters

| param | type | description |
| ----- | ---- | ----------- |
| `lookbackPeriods` | _`int`_ | Number of periods (`N`) in the lookback window.  Must be greater than 0. |

### Historical price bars requirements

You must have at least `N+1` periods of `bars` to cover the warmup periods.

`bars` is a collection of generic `TBar` historical price bars.  It should have a consistent frequency (day, hour, minute, etc).  See [the Guide](/guide/getting-started#historical-bars) for more information.

## Response

```csharp
IReadOnlyList<CmoResult>
```

- This method returns a time series of all available indicator values for the `bars` provided.
- It always returns the same number of elements as there are in the historical price bars.
- It does not return a single incremental indicator value.
- The first `N` periods will have `null` values for CMO since there's not enough data to calculate.

### `CmoResult`

| property | type | description |
| -------- | ---- | ----------- |
| `Timestamp` | _`DateTime`_ | Date from evaluated `TBar` |
| `Cmo` | _`double`_ | Chande Momentum Oscillator |

### Utilities

- [.Condense()](/utilities/results#condense)
- [.Find(lookupDate)](/utilities/results#find-by-date)
- [.RemoveWarmupPeriods()](/utilities/results#remove-warmup-periods)
- [.RemoveWarmupPeriods(removePeriods)](/utilities/results#remove-warmup-periods)

See [Utilities and helpers](/utilities/) for more information.

## Chaining

This indicator may be generated from any chain-enabled indicator or method.

```csharp
// example
var results = bars
    .Use(CandlePart.HL2)
    .ToCmo(..);
```

Results can be further processed on `Cmo` with additional chain-enabled indicators.

```csharp
// example
var results = bars
    .ToCmo(..)
    .ToEma(..);
```

See [Chaining indicators](/guide/chaining) for more.

## Streaming

Use the buffer-style `List<T>` when you need incremental calculations without a hub:

```csharp
CmoList cmoList = new(lookbackPeriods);

// call from your WebSocket or SSE message handler
void OnBarReceived(IBar bar) => cmoList.Add(bar);

// based on `ICollection<CmoResult>`
IReadOnlyList<CmoResult> results = cmoList;
```

Subscribe to a `BarHub` for advanced streaming scenarios:

```csharp
BarHub barHub = new();
CmoHub observer = barHub.ToCmoHub(lookbackPeriods);

// call from your WebSocket or SSE message handler
void OnBarReceived(IBar bar) => barHub.Add(bar);

IReadOnlyList<CmoResult> results = observer.Results;
```

See the [guide](/guide/) for more information.

See [Buffer lists](/guide/styles/buffer) and [Stream hubs](/guide/styles/stream) for full usage guides.
