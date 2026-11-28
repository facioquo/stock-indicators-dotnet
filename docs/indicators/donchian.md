---
title: Donchian Channels
description: Created by Richard Donchian, Donchian Channels, also called Price Channels, are price ranges derived from highest High and lowest Low values.
---

# Donchian Channels

Created by Richard Donchian, [Donchian Channels](https://en.wikipedia.org/wiki/Donchian_channel), also called Price Channels, are price ranges derived from highest High and lowest Low values.
[[Discuss] &#128172;](https://github.com/facioquo/stock-indicators-dotnet/discussions/257 "Community discussion about this indicator")

<StyleSwitch />

<StockIndicatorChart indicator="Donchian" />

<ChartKey indicator="Donchian" />

<a id="streaming"></a>

::: series

```csharp
// C# usage syntax
IReadOnlyList<DonchianResult> results =
  bars.ToDonchian(lookbackPeriods);
```

:::

::: buffer
Create a buffer list and add each bar as it arrives:

```csharp
DonchianList donchianList = new(lookbackPeriods);

// call from your WebSocket or SSE message handler
void OnBarReceived(IBar bar) => donchianList.Add(bar);

// the list itself holds the results; fills as the handler runs
IReadOnlyList<DonchianResult> results = donchianList;
```

See [Buffer lists](/guide/styles/buffer) for the full usage guide.
:::

::: stream
Subscribe to a `BarHub` and add each bar as it arrives:

```csharp
BarHub barHub = new();
DonchianHub observer = barHub.ToDonchianHub(lookbackPeriods);

// call from your WebSocket or SSE message handler
void OnBarReceived(IBar bar) => barHub.Add(bar);

// results fill as the handler runs
IReadOnlyList<DonchianResult> results = observer.Results;
```

See [Stream hubs](/guide/styles/stream) for the full usage guide.
:::

## Parameters

| param | description |
| ----- | ----------- |
| `lookbackPeriods`<br>_`int`_ | Number of periods (`N`) for lookback period.  Must be greater than 0 to calculate; however we suggest a larger value for an appropriate sample size.  Default is 20. |

### Historical price bars requirements

You must have at least `N+1` periods of `bars` to cover the warmup periods.

`bars` is a collection of generic `TBar` historical price bars.  It should have a consistent frequency (day, hour, minute, etc).  See [the Guide](/guide/getting-started#historical-bars) for more information.

## Response

::: series

```csharp
IReadOnlyList<DonchianResult>
```

- This method returns a time series of all available indicator values for the `bars` provided.
- It always returns the same number of elements as there are in the historical price bars.
- It does not return a single incremental indicator value.
- The first `N` periods will have `null` values since there's not enough data to calculate.
:::

::: buffer

```csharp
DonchianList : IReadOnlyList<DonchianResult>
```

- The list itself holds the results, one for each bar or value added, in timestamp order.
- It keeps up to 100,000 results by default and drops the oldest beyond that; set `MaxListSize` to keep fewer.
- The first `N` periods will have `null` values since there's not enough data to calculate.
:::

::: stream

```csharp
IReadOnlyList<DonchianResult> results = observer.Results;
```

- `observer.Results` holds the results, one for each bar in the `BarHub` cache, in timestamp order.
- Results update as bars arrive, including bars that arrive late or are revised.
- The cache keeps up to 100,000 results by default; set `maxCacheSize` on the `BarHub` to keep fewer.
- The first `N` periods will have `null` values since there's not enough data to calculate.
:::

### `DonchianResult`

| property | description |
| ----- | ----------- |
| `Timestamp`<br>_`DateTime`_ | Date from evaluated `TBar` |
| `UpperBand`<br>_`double?`_ | Upper line is the highest High over `N` periods |
| `Centerline`<br>_`double?`_ | Simple average of Upper and Lower bands |
| `LowerBand`<br>_`double?`_ | Lower line is the lowest Low over `N` periods |
| `Width`<br>_`double?`_ | Width as percent of Centerline price.  `(UpperBand-LowerBand)/Centerline` |

### Utilities

- [.Condense()](/utilities/results#condense)
- [.Find(lookupDate)](/utilities/results#find-by-date)
- [.RemoveWarmupPeriods()](/utilities/results#remove-warmup-periods)
- [.RemoveWarmupPeriods(removePeriods)](/utilities/results#remove-warmup-periods)

See [Utilities and helpers](/utilities/) for more information.

## Chaining

This indicator is not chain-enabled and must be generated from `bars`.  It **cannot** be used for further processing by other chain-enabled indicators.

See [Chaining indicators](/guide/chaining) for more.
