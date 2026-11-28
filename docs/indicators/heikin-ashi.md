---
title: Heikin-Ashi
description: Created by Munehisa Homma, [Heikin-Ashi](https://en.wikipedia.org/wiki/Candlestick_chart#Heikin-Ashi_candlesticks) is a modified candlestick pattern that transforms prices based on prior period prices for smoothing.
---

# Heikin-Ashi

Created by Munehisa Homma, [Heikin-Ashi](https://en.wikipedia.org/wiki/Candlestick_chart#Heikin-Ashi_candlesticks) is a modified candlestick pattern that transforms prices based on prior period prices for smoothing.
[[Discuss] &#128172;](https://github.com/facioquo/stock-indicators-dotnet/discussions/254 "Community discussion about this indicator")

<StyleSwitch />

<StockIndicatorChart indicator="HeikinAshi" />

<a id="streaming"></a>

::: series

```csharp
// C# usage syntax
IReadOnlyList<HeikinAshiResult> results =
  bars.ToHeikinAshi();
```

:::

::: buffer
Create a buffer list and add each bar as it arrives:

```csharp
HeikinAshiList heikinAshiList = new();

// call from your WebSocket or SSE message handler
void OnBarReceived(IBar bar) => heikinAshiList.Add(bar);

// the list itself holds the results; fills as the handler runs
IReadOnlyList<HeikinAshiResult> results = heikinAshiList;
```

See [Buffer lists](/guide/styles/buffer) for the full usage guide.
:::

::: stream
Subscribe to a `BarHub` and add each bar as it arrives:

```csharp
BarHub barHub = new();
HeikinAshiHub observer = barHub.ToHeikinAshiHub();

// call from your WebSocket or SSE message handler
void OnBarReceived(IBar bar) => barHub.Add(bar);

// results fill as the handler runs
IReadOnlyList<HeikinAshiResult> results = observer.Results;
```

See [Stream hubs](/guide/styles/stream) for the full usage guide.
:::

## Historical price bars requirements

You must have at least two periods of `bars` to cover the warmup periods; however, more is typically provided since this is a chartable candlestick pattern.

`bars` is a collection of generic `TBar` historical price bars.  It should have a consistent frequency (day, hour, minute, etc).  See [the Guide](/guide/getting-started#historical-bars) for more information.

## Response

::: series

```csharp
IReadOnlyList<HeikinAshiResult>
```

- This method returns a time series of all available indicator values for the `bars` provided.
- It always returns the same number of elements as there are in the historical price bars.
- It does not return a single incremental indicator value.
- `HeikinAshiResult` is based on `IBar`, so it can be used as a direct replacement for `bars`.
:::

::: buffer

```csharp
HeikinAshiList : IReadOnlyList<HeikinAshiResult>
```

- The list itself holds the results, one for each bar or value added, in timestamp order.
- It keeps up to 100,000 results by default and drops the oldest beyond that; set `MaxListSize` to keep fewer.
- `HeikinAshiResult` is based on `IBar`, so it can be used as a direct replacement for `bars`.
:::

::: stream

```csharp
IReadOnlyList<HeikinAshiResult> results = observer.Results;
```

- `observer.Results` holds the results, one for each bar in the `BarHub` cache, in timestamp order.
- Results update as bars arrive, including bars that arrive late or are revised.
- The cache keeps up to 100,000 results by default; set `maxCacheSize` on the `BarHub` to keep fewer.
- `HeikinAshiResult` is based on `IBar`, so it can be used as a direct replacement for `bars`.
:::

### `HeikinAshiResult`

| property | description |
| ----- | ----------- |
| `Timestamp`<br>_`DateTime`_ | Date from evaluated `TBar` |
| `Open`<br>_`decimal`_ | Modified open price |
| `High`<br>_`decimal`_ | Modified high price |
| `Low`<br>_`decimal`_ | Modified low price |
| `Close`<br>_`decimal`_ | Modified close price |
| `Volume`<br>_`decimal`_ | Volume (same as `bars`) |

### Utilities

- [.Find(lookupDate)](/utilities/results#find-by-date)
- [.RemoveWarmupPeriods(removePeriods)](/utilities/results#remove-warmup-periods)
- .ToBars() to convert to a `Bar` collection.  Example:

  ```csharp
  IReadOnlyList<Bar> results = bars
    .ToHeikinAshi()
    .ToBars();
  ```

See [Utilities and helpers](/utilities/) for more information.

## Chaining

This indicator must be generated from `bars` and **cannot** be generated from results of another chain-enabled indicator or method.

::: series
Results are based in `IBar` and can be further used in any indicator.

```csharp
// example
var results = bars
    .ToHeikinAshi(..)
    .ToRsi(..);
```

See [Chaining indicators](/guide/chaining) for more.
:::

::: buffer
Results are based in `IBar`, so add each new result to another buffer list yourself.

```csharp
// example
HeikinAshiList heikinAshiList = new();
RsiList rsiList = new(..);

void OnBarReceived(IBar bar)
{
    heikinAshiList.Add(bar);
    rsiList.Add(heikinAshiList[^1]);
}
```

See [Chaining buffer lists](/guide/styles/buffer#chaining-indicators) for more.
:::

::: stream
Results are based in `IBar`, so any indicator hub can subscribe to the Heikin-Ashi hub.

```csharp
// example
RsiHub observer = barHub
    .ToHeikinAshiHub()
    .ToRsiHub(..);
```

See [Chaining stream hubs](/guide/styles/stream#chaining-indicators) for more.
:::
