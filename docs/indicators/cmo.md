---
title: Chande Momentum Oscillator (CMO)
description: The Chande Momentum Oscillator is a momentum indicator depicting the weighted percent of higher prices in financial markets.
---

# Chande Momentum Oscillator (CMO)

Created by Tushar Chande, the [Chande Momentum Oscillator](https://www.investopedia.com/terms/c/chandemomentumoscillator.asp) is a weighted percent of higher prices over a lookback window.
[[Discuss] &#128172;](https://github.com/facioquo/stock-indicators-dotnet/discussions/892 "Community discussion about this indicator")

<StyleSwitch />

<StockIndicatorChart indicator="Cmo" withOverlay />

<a id="streaming"></a>

::: series

```csharp
// C# usage syntax
IReadOnlyList<CmoResult> results =
  bars.ToCmo(lookbackPeriods);
```

:::

::: buffer
Create a buffer list and add each bar as it arrives:

```csharp
CmoList cmoList = new(lookbackPeriods);

// call from your WebSocket or SSE message handler
void OnBarReceived(IBar bar) => cmoList.Add(bar);

// the list itself holds the results; fills as the handler runs
IReadOnlyList<CmoResult> results = cmoList;
```

See [Buffer lists](/guide/styles/buffer) for the full usage guide.
:::

::: stream
Subscribe to a `BarHub` and add each bar as it arrives:

```csharp
BarHub barHub = new();
CmoHub observer = barHub.ToCmoHub(lookbackPeriods);

// call from your WebSocket or SSE message handler
void OnBarReceived(IBar bar) => barHub.Add(bar);

// results fill as the handler runs
IReadOnlyList<CmoResult> results = observer.Results;
```

See the [guide](/guide/) for more information.

See [Stream hubs](/guide/styles/stream) for the full usage guide.
:::

## Parameters

| param | description |
| ----- | ----------- |
| `lookbackPeriods`<br>_`int`_ | Number of periods (`N`) in the lookback window.  Must be greater than 0.  Default is 14. |

### Historical price bars requirements

You must have at least `N+1` periods of `bars` to cover the warmup periods.

`bars` is a collection of generic `TBar` historical price bars.  It should have a consistent frequency (day, hour, minute, etc).  See [the Guide](/guide/getting-started#historical-bars) for more information.

## Response

::: series

```csharp
IReadOnlyList<CmoResult>
```

- This method returns a time series of all available indicator values for the `bars` provided.
- It always returns the same number of elements as there are in the historical price bars.
- It does not return a single incremental indicator value.
- The first `N` periods will have `null` values for CMO since there's not enough data to calculate.
:::

::: buffer

```csharp
CmoList : IReadOnlyList<CmoResult>
```

- The list itself holds the results, one for each bar or value added, in timestamp order.
- It keeps up to 100,000 results by default and drops the oldest beyond that; set `MaxListSize` to keep fewer.
- The first `N` periods will have `null` values for CMO since there's not enough data to calculate.
:::

::: stream

```csharp
IReadOnlyList<CmoResult> results = observer.Results;
```

- `observer.Results` holds the results, one for each bar in the `BarHub` cache, in timestamp order.
- Results update as bars arrive, including bars that arrive late or are revised.
- The cache keeps up to 100,000 results by default; set `maxCacheSize` on the `BarHub` to keep fewer.
- The first `N` periods will have `null` values for CMO since there's not enough data to calculate.
:::

### `CmoResult`

| property | description |
| ----- | ----------- |
| `Timestamp`<br>_`DateTime`_ | Date from evaluated `TBar` |
| `Cmo`<br>_`double?`_ | Chande Momentum Oscillator |

### Utilities

- [.Condense()](/utilities/results#condense)
- [.Find(lookupDate)](/utilities/results#find-by-date)
- [.RemoveWarmupPeriods()](/utilities/results#remove-warmup-periods)
- [.RemoveWarmupPeriods(removePeriods)](/utilities/results#remove-warmup-periods)

See [Utilities and helpers](/utilities/) for more information.

## Chaining

::: series
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
:::

::: buffer
This indicator may be generated from results of another chain-enabled buffer list.  Add each new result to it yourself.

```csharp
// example
SmaList smaList = new(..);
CmoList cmoList = new(..);

void OnBarReceived(IBar bar)
{
    smaList.Add(bar);
    cmoList.Add(smaList[^1]);
}
```

Add each new result to another chain-enabled buffer list to process `Cmo` further.

```csharp
// example
CmoList cmoList = new(..);
EmaList emaList = new(..);

void OnBarReceived(IBar bar)
{
    cmoList.Add(bar);
    emaList.Add(cmoList[^1]);
}
```

See [Chaining buffer lists](/guide/styles/buffer#chaining-indicators) for more.
:::

::: stream
This indicator may be subscribed to any chain-enabled hub.

```csharp
// example
CmoHub observer = barHub
    .ToBarPartHub(CandlePart.HL2)
    .ToCmoHub(..);
```

Subscribe other chain-enabled hubs to it to process `Cmo` further.

```csharp
// example
EmaHub observer = barHub
    .ToCmoHub(..)
    .ToEmaHub(..);
```

See [Chaining stream hubs](/guide/styles/stream#chaining-indicators) for more.
:::
