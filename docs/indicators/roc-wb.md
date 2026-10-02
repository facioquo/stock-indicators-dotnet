---
title: ROC with Bands
description: Rate of Change with Bands, created by Vitali Apirine, is a volatility banded variant of the basic Rate of Change (ROC) indicator.
---

# ROC with Bands

Rate of Change (ROC) with Bands, created by Vitali Apirine, is a volatility banded variant of [Rate of Change (ROC)](/indicators/roc).
[[Discuss] &#128172;](https://github.com/facioquo/stock-indicators-dotnet/discussions/242 "Community discussion about this indicator")

<ClientOnly>
  <StockIndicatorChart indicator="RocWb" withOverlay />
</ClientOnly>

```csharp
// C# usage syntax
IReadOnlyList<RocWbResult> results =
  bars.ToRocWb(lookbackPeriods, emaPeriods, stdDevPeriods);
```

## Parameters

| param | type | description |
| ----- | ---- | ----------- |
| `lookbackPeriods` | _`int`_ | Number of periods (`N`) to go back.  Must be greater than 0.  Typical values range from 10-20. |
| `emaPeriods` | _`int`_ | Number of periods for the ROC EMA line.  Must be greater than 0.  Standard is 3. |
| `stdDevPeriods` | _`int`_ | Number of periods the standard deviation for upper/lower band lines.  Must be greater than 0 and not more than `lookbackPeriods`.  Standard is to use same value as `lookbackPeriods`. |

### Historical price bars requirements

You must have at least `N+1` periods of `bars` to cover the warmup periods.

`bars` is a collection of generic `TBar` historical price bars.  It should have a consistent frequency (day, hour, minute, etc).  See [the Guide](/guide/getting-started#historical-bars) for more information.

## Response

```csharp
IReadOnlyList<RocWbResult>
```

### `RocWbResult`

| property | type | description |
| -------- | ---- | ----------- |
| `Timestamp` | _`DateTime`_ | Date from evaluated `TBar` |
| `Roc` | _`double`_ | Rate of Change over `N` lookback periods (%, not decimal) |
| `RocEma` | _`double`_ | Exponential moving average (EMA) of `Roc` |
| `UpperBand` | _`double`_ | Upper band of ROC (overbought indicator) |
| `LowerBand` | _`double`_ | Lower band of ROC (oversold indicator) |

### Utilities

See [Utilities and helpers](/utilities/) for more information.

## Streaming

Use the buffer-style `List<T>` when you need incremental calculations without a hub:

```csharp
RocWbList rocWbList = new(lookbackPeriods, emaPeriods, stdDevPeriods);

// call from your WebSocket or SSE message handler
void OnBarReceived(IBar bar) => rocWbList.Add(bar);

// based on `ICollection<RocWbResult>`; fills as the handler runs
IReadOnlyList<RocWbResult> results = rocWbList;
```

Subscribe to a `BarHub` for advanced streaming scenarios:

```csharp
BarHub barHub = new();
RocWbHub observer = barHub.ToRocWbHub(lookbackPeriods, emaPeriods, stdDevPeriods);

// call from your WebSocket or SSE message handler
void OnBarReceived(IBar bar) => barHub.Add(bar);

// results fill as the handler runs
IReadOnlyList<RocWbResult> results = observer.Results;
```

See [Buffer lists](/guide/styles/buffer) and [Stream hubs](/guide/styles/stream) for full usage guides.

## Chaining

This indicator may be generated from any chain-enabled indicator or method.

```csharp
// example
var results = bars
    .Use(CandlePart.HL2)
    .ToRocWb(..);
```

Results can be further processed on `Roc` with additional chain-enabled indicators.

```csharp
// example
var results = bars
    .ToRocWb(..)
    .ToEma(..);
```

See [Chaining indicators](/guide/chaining) for more.
