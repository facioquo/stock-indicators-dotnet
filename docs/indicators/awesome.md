---
title: Awesome Oscillator (AO)
description: Created by Bill Williams, the Awesome Oscillator (AO), also known as Super AO, is a measure of the gap between a fast and slow period modified moving average.
---

# Awesome Oscillator (AO)

Created by Bill Williams, the Awesome Oscillator (aka Super AO) is a measure of the gap between a fast and slow period modified moving average.
[[Discuss] &#128172;](https://github.com/facioquo/stock-indicators-dotnet/discussions/282 "Community discussion about this indicator")

<StyleSwitch />

<StockIndicatorChart indicator="Awesome" withOverlay />

<a id="streaming"></a>

::: series

```csharp
// C# usage syntax
IReadOnlyList<AwesomeResult> results =
  bars.ToAwesome(fastPeriods, slowPeriods);
```

:::

::: buffer
Create a buffer list and add each bar as it arrives:

```csharp
AwesomeList awesomeList = new(fastPeriods, slowPeriods);

// call from your WebSocket or SSE message handler
void OnBarReceived(IBar bar) => awesomeList.Add(bar);

// the list itself holds the results; fills as the handler runs
IReadOnlyList<AwesomeResult> results = awesomeList;
```

See [Buffer lists](/guide/styles/buffer) for the full usage guide.
:::

::: stream
Subscribe to a `BarHub` and add each bar as it arrives:

```csharp
BarHub barHub = new();
AwesomeHub observer = barHub.ToAwesomeHub(fastPeriods, slowPeriods);

// call from your WebSocket or SSE message handler
void OnBarReceived(IBar bar) => barHub.Add(bar);

// results fill as the handler runs
IReadOnlyList<AwesomeResult> results = observer.Results;
```

See [Stream hubs](/guide/styles/stream) for the full usage guide.
:::

## Parameters

| param | description |
| ----- | ----------- |
| `fastPeriods`<br>_`int`_ | Number of periods (`F`) for the faster moving average.  Must be greater than 0.  Default is 5. |
| `slowPeriods`<br>_`int`_ | Number of periods (`S`) for the slower moving average.  Must be greater than `fastPeriods`.  Default is 34. |

### Historical price bars requirements

You must have at least `S` periods of `bars` to cover the warmup periods.

`bars` is a collection of generic `TBar` historical price bars.  It should have a consistent frequency (day, hour, minute, etc).  See [the Guide](/guide/getting-started#historical-bars) for more information.

## Response

::: series

```csharp
IReadOnlyList<AwesomeResult>
```

- This method returns a time series of all available indicator values for the `bars` provided.
- It always returns the same number of elements as there are in the historical price bars.
- It does not return a single incremental indicator value.
- The first period `S-1` periods will have `null` values since there's not enough data to calculate.
:::

::: buffer

```csharp
AwesomeList : IReadOnlyList<AwesomeResult>
```

- The list itself holds the results, one for each bar or value added, in timestamp order.
- It keeps up to 100,000 results by default and drops the oldest beyond that; set `MaxListSize` to keep fewer.
- The first period `S-1` periods will have `null` values since there's not enough data to calculate.
:::

::: stream

```csharp
IReadOnlyList<AwesomeResult> results = observer.Results;
```

- `observer.Results` holds the results, one for each bar in the `BarHub` cache, in timestamp order.
- Results update as bars arrive, including bars that arrive late or are revised.
- The cache keeps up to 100,000 results by default; set `maxCacheSize` on the `BarHub` to keep fewer.
- The first period `S-1` periods will have `null` values since there's not enough data to calculate.
:::

### `AwesomeResult`

| property | description |
| ----- | ----------- |
| `Timestamp`<br>_`DateTime`_ | Date from evaluated `TBar` |
| `Oscillator`<br>_`double?`_ | Awesome Oscillator |
| `Normalized`<br>_`double?`_ | `100 × Oscillator ÷ (median price)` |

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
    .ToAwesome(..);
```

Results can be further processed on `Oscillator` with additional chain-enabled indicators.

```csharp
// example
var results = bars
    .ToAwesome(..)
    .ToRsi(..);
```

See [Chaining indicators](/guide/chaining) for more.
:::

::: buffer
This indicator may be generated from results of another chain-enabled buffer list.  Add each new result to it yourself.

```csharp
// example
SmaList smaList = new(..);
AwesomeList awesomeList = new(..);

void OnBarReceived(IBar bar)
{
    smaList.Add(bar);
    awesomeList.Add(smaList[^1]);
}
```

Add each new result to another chain-enabled buffer list to process `Oscillator` further.

```csharp
// example
AwesomeList awesomeList = new(..);
RsiList rsiList = new(..);

void OnBarReceived(IBar bar)
{
    awesomeList.Add(bar);
    rsiList.Add(awesomeList[^1]);
}
```

See [Chaining buffer lists](/guide/styles/buffer#chaining-indicators) for more.
:::

::: stream
This indicator may be subscribed to any chain-enabled hub.

```csharp
// example
AwesomeHub observer = barHub
    .ToBarPartHub(CandlePart.HL2)
    .ToAwesomeHub(..);
```

Subscribe other chain-enabled hubs to it to process `Oscillator` further.

```csharp
// example
RsiHub observer = barHub
    .ToAwesomeHub(..)
    .ToRsiHub(..);
```

See [Chaining stream hubs](/guide/styles/stream#chaining-indicators) for more.
:::
