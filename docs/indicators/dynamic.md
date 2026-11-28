---
title: McGinley Dynamic
description: Created by John R. McGinley, the McGinley Dynamic is a more responsive variant of exponential moving average.
---

# McGinley Dynamic

Created by John R. McGinley, the [McGinley Dynamic](https://www.investopedia.com/terms/m/mcginley-dynamic.asp) is a more responsive variant of exponential moving average.
[[Discuss] &#128172;](https://github.com/facioquo/stock-indicators-dotnet/discussions/866 "Community discussion about this indicator")

<StyleSwitch />

<StockIndicatorChart indicator="Dynamic" />

<a id="streaming"></a>

::: series

```csharp
// C# usage syntax (with Close price)
IReadOnlyList<DynamicResult> results =
  bars.ToDynamic(lookbackPeriods, kFactor);
```

:::

::: buffer
Create a buffer list and add each bar as it arrives:

```csharp
DynamicList dynamicList = new(lookbackPeriods, kFactor);

// call from your WebSocket or SSE message handler
void OnBarReceived(IBar bar) => dynamicList.Add(bar);

// the list itself holds the results; fills as the handler runs
IReadOnlyList<DynamicResult> results = dynamicList;
```

See [Buffer lists](/guide/styles/buffer) for the full usage guide.
:::

::: stream
Subscribe to a `BarHub` and add each bar as it arrives:

```csharp
BarHub barHub = new();
DynamicHub observer = barHub.ToDynamicHub(lookbackPeriods, kFactor);

// call from your WebSocket or SSE message handler
void OnBarReceived(IBar bar) => barHub.Add(bar);

// results fill as the handler runs
IReadOnlyList<DynamicResult> results = observer.Results;
```

See [Stream hubs](/guide/styles/stream) for the full usage guide.
:::

## Parameters

| param | description |
| ----- | ----------- |
| `lookbackPeriods`<br>_`int`_ | Number of periods (`N`) in the moving average.  Must be greater than 0. |
| `kFactor`<br>_`double`_ | Optional.  Range adjustment factor (`K`).  Must be greater than 0.  Default is 0.6 |

### Historical price bars requirements

You must have at least `2` periods of `bars`, to cover the [warmup and convergence](https://github.com/facioquo/stock-indicators-dotnet/discussions/688) periods.  Since this uses a smoothing technique, we recommend you use at least `4×N` data points prior to the intended usage date for better precision.

`bars` is a collection of generic `TBar` historical price bars.  It should have a consistent frequency (day, hour, minute, etc).  See [the Guide](/guide/getting-started#historical-bars) for more information.

### Pro tips

> Use a `kFactor` value of `1` if you do not want to adjust the `N` value.
>
> McGinley suggests that using a `K` value of 60% (0.6) allows you to use a `N` equivalent to other moving averages.  For example, DYNAMIC(20,0.6) is comparable to EMA(20); conversely, DYNAMIC(20,1) uses the raw 1:1 `N` value and is not equivalent.

## Response

::: series

```csharp
IReadOnlyList<DynamicResult>
```

- This method returns a time series of all available indicator values for the `bars` provided.
- It always returns the same number of elements as there are in the historical price bars.
- It does not return a single incremental indicator value.
- The first period will have a `null` value since there's not enough data to calculate.
:::

::: buffer

```csharp
DynamicList : IReadOnlyList<DynamicResult>
```

- The list itself holds the results, one for each bar or value added, in timestamp order.
- It keeps up to 100,000 results by default and drops the oldest beyond that; set `MaxListSize` to keep fewer.
- The first period will have a `null` value since there's not enough data to calculate.
:::

::: stream

```csharp
IReadOnlyList<DynamicResult> results = observer.Results;
```

- `observer.Results` holds the results, one for each bar in the `BarHub` cache, in timestamp order.
- Results update as bars arrive, including bars that arrive late or are revised.
- The cache keeps up to 100,000 results by default; set `maxCacheSize` on the `BarHub` to keep fewer.
- The first period will have a `null` value since there's not enough data to calculate.
:::

::: warning 🚩 ⚞ Convergence warning
The first `4×N` periods will have decreasing magnitude, convergence-related precision errors that can be as high as ~5% deviation in indicator values for earlier periods.
:::

### `DynamicResult`

| property | description |
| ----- | ----------- |
| `Timestamp`<br>_`DateTime`_ | Date from evaluated `TBar` |
| `Dynamic`<br>_`double?`_ | McGinley Dynamic |

## Chaining

::: series
This indicator may be generated from any chain-enabled indicator or method.

```csharp
// example
var results = bars
    .Use(CandlePart.HL2)
    .ToDynamic(..);
```

Results can be further processed on `Dynamic` with additional chain-enabled indicators.

```csharp
// example
var results = bars
    .ToDynamic(..)
    .ToRsi(..);
```

See [Chaining indicators](/guide/chaining) for more.
:::

::: buffer
This indicator may be generated from results of another chain-enabled buffer list.  Add each new result to it yourself.

```csharp
// example
SmaList smaList = new(..);
DynamicList dynamicList = new(..);

void OnBarReceived(IBar bar)
{
    smaList.Add(bar);
    dynamicList.Add(smaList[^1]);
}
```

Add each new result to another chain-enabled buffer list to process `Dynamic` further.

```csharp
// example
DynamicList dynamicList = new(..);
RsiList rsiList = new(..);

void OnBarReceived(IBar bar)
{
    dynamicList.Add(bar);
    rsiList.Add(dynamicList[^1]);
}
```

See [Chaining buffer lists](/guide/styles/buffer#chaining-indicators) for more.
:::

::: stream
This indicator may be subscribed to any chain-enabled hub.

```csharp
// example
DynamicHub observer = barHub
    .ToBarPartHub(CandlePart.HL2)
    .ToDynamicHub(..);
```

Subscribe other chain-enabled hubs to it to process `Dynamic` further.

```csharp
// example
RsiHub observer = barHub
    .ToDynamicHub(..)
    .ToRsiHub(..);
```

See [Chaining stream hubs](/guide/styles/stream#chaining-indicators) for more.
:::

## Utilities

::: series

- [.Condense()](/utilities/results#condense)
- [.Find(lookupDate)](/utilities/results#find-by-date)
- [.RemoveWarmupPeriods(removePeriods)](/utilities/results#remove-warmup-periods)

:::

::: buffer

- [.Find(lookupDate)](/utilities/results#find-by-date)
- [.Condense()](/utilities/results#condense)

Call these on the list itself, as in `dynamicList.Find(lookupDate)`. `.Condense()` returns a new list and leaves `dynamicList` unchanged.

Warmup removal is left out. Once the list is full it drops its oldest results, so trimming by position would remove valid values.
:::

::: stream

- [.Find(lookupDate)](/utilities/results#find-by-date)
- [.Condense()](/utilities/results#condense)

Call these on `observer.Results`, as in `observer.Results.Find(lookupDate)`. `.Condense()` returns a new list and leaves the hub's results unchanged.

Warmup removal is left out. Once the cache is full the hub drops its oldest results, so trimming by position would remove valid values.
:::

See [Utilities and helpers](/utilities/) for more information.
