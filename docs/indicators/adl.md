---
title: Accumulation / Distribution Line (ADL)
description: Created by Marc Chaikin, the Accumulation / Distribution Line is a rolling accumulation of Chaikin Money Flow Volume.  It can be a leading momentum indicator for financial market price movements.
---

# Accumulation / Distribution Line (ADL)

Created by Marc Chaikin, the [Accumulation/Distribution Line/Index](https://en.wikipedia.org/wiki/Accumulation/distribution_index) is a rolling accumulation of Chaikin Money Flow Volume.
[[Discuss] &#128172;](https://github.com/facioquo/stock-indicators-dotnet/discussions/271 "Community discussion about this indicator")

<StyleSwitch />

<StockIndicatorChart indicator="Adl" withOverlay />

<a id="streaming"></a>

::: series

```csharp
// C# usage syntax
IReadOnlyList<AdlResult> results =
  bars.ToAdl();
```

:::

::: buffer
Create a buffer list and add each bar as it arrives:

```csharp
AdlList adlList = new();

// call from your WebSocket or SSE message handler
void OnBarReceived(IBar bar) => adlList.Add(bar);

// the list itself holds the results; fills as the handler runs
IReadOnlyList<AdlResult> results = adlList;
```

See [Buffer lists](/guide/styles/buffer) for the full usage guide.
:::

::: stream
Subscribe to a `BarHub` and add each bar as it arrives:

```csharp
BarHub barHub = new();
AdlHub observer = barHub.ToAdlHub();

// call from your WebSocket or SSE message handler
void OnBarReceived(IBar bar) => barHub.Add(bar);

// results fill as the handler runs
IReadOnlyList<AdlResult> results = observer.Results;
```

See [Stream hubs](/guide/styles/stream) for the full usage guide.
:::

## Historical price bars requirements

You must have at least two historical price bars to cover the warmup periods; however, since this is a trendline, more is recommended.

`bars` is a collection of generic `TBar` historical price bars.  It should have a consistent frequency (day, hour, minute, etc).  See [the Guide](/guide/getting-started#historical-bars) for more information.

## Response

::: series

```csharp
IReadOnlyList<AdlResult>
```

- This method returns a time series of all available indicator values for the `bars` provided.
- It always returns the same number of elements as there are in the historical price bars.
- It does not return a single incremental indicator value.
:::

::: buffer

```csharp
AdlList : IReadOnlyList<AdlResult>
```

- The list itself holds the results, one for each bar or value added, in timestamp order.
- It keeps up to 100,000 results by default and drops the oldest beyond that; set `MaxListSize` to keep fewer.
:::

::: stream

```csharp
IReadOnlyList<AdlResult> results = observer.Results;
```

- `observer.Results` holds the results, one for each bar in the `BarHub` cache, in timestamp order.
- Results update as bars arrive, including bars that arrive late or are revised.
- The cache keeps up to 100,000 results by default; set `maxCacheSize` on the `BarHub` to keep fewer.
:::

### `AdlResult`

| property | description |
| ----- | ----------- |
| `Timestamp`<br>_`DateTime`_ | Date from evaluated `TBar` |
| `Adl`<br>_`double`_ | Accumulation Distribution Line (ADL) |
| `MoneyFlowMultiplier`<br>_`double?`_ | Money Flow Multiplier |
| `MoneyFlowVolume`<br>_`double?`_ | Money Flow Volume |

::: warning 🚩 Relative values only
Absolute values in ADL and MFV are somewhat meaningless.  Use with caution.
:::

## Chaining

This indicator must be generated from `bars` and **cannot** be generated from results of another chain-enabled indicator or method.

::: series
Results can be further processed on `Adl` with additional chain-enabled indicators.

```csharp
// example
var results = bars
    .ToAdl()
    .ToRsi(..);
```

See [Chaining indicators](/guide/chaining) for more.
:::

::: buffer
Add each new result to another chain-enabled buffer list to process `Adl` further.

```csharp
// example
AdlList adlList = new();
RsiList rsiList = new(..);

void OnBarReceived(IBar bar)
{
    adlList.Add(bar);
    rsiList.Add(adlList[^1]);
}
```

See [Chaining buffer lists](/guide/styles/buffer#chaining-indicators) for more.
:::

::: stream
Subscribe other chain-enabled hubs to it to process `Adl` further.

```csharp
// example
RsiHub observer = barHub
    .ToAdlHub()
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

Call these on the list itself, as in `adlList.Find(lookupDate)`. `.Condense()` returns a new list and leaves `adlList` unchanged.

Warmup removal is left out. Once the list is full it drops its oldest results, so trimming by position would remove valid values.
:::

::: stream

- [.Find(lookupDate)](/utilities/results#find-by-date)
- [.Condense()](/utilities/results#condense)

Call these on `observer.Results`, as in `observer.Results.Find(lookupDate)`. `.Condense()` returns a new list and leaves the hub's results unchanged.

Warmup removal is left out. Once the cache is full the hub drops its oldest results, so trimming by position would remove valid values.
:::

See [Utilities and helpers](/utilities/) for more information.
