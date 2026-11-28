---
title: Fractal Chaos Bands (FCB)
description: Created by Edward William Dreiss, Fractal Chaos Bands outline high and low price channels to depict broad less-chaotic price movements.  FCB is a channelized depiction of Williams Fractal.
---

# Fractal Chaos Bands (FCB)

Created by Edward William Dreiss, Fractal Chaos Bands outline high and low price channels to depict broad less-chaotic price movements.  FCB is a channelized depiction of [Williams Fractal](/indicators/fractal).
[[Discuss] &#128172;](https://github.com/facioquo/stock-indicators-dotnet/discussions/347 "Community discussion about this indicator")

<StyleSwitch />

<StockIndicatorChart indicator="Fcb" />

<ChartKey indicator="Fcb" />

<a id="streaming"></a>

::: series

```csharp
// C# usage syntax
IReadOnlyList<FcbResult> results =
  bars.ToFcb(windowSpan);
```

:::

::: buffer
Create a buffer list and add each bar as it arrives:

```csharp
FcbList fcbList = new(windowSpan);

// call from your WebSocket or SSE message handler
void OnBarReceived(IBar bar) => fcbList.Add(bar);

// the list itself holds the results; fills as the handler runs
IReadOnlyList<FcbResult> results = fcbList;
```

See [Buffer lists](/guide/styles/buffer) for the full usage guide.
:::

::: stream
Subscribe to a `BarHub` and add each bar as it arrives:

```csharp
BarHub barHub = new();
FcbHub observer = barHub.ToFcbHub(windowSpan);

// call from your WebSocket or SSE message handler
void OnBarReceived(IBar bar) => barHub.Add(bar);

// results fill as the handler runs
IReadOnlyList<FcbResult> results = observer.Results;
```

See [Stream hubs](/guide/styles/stream) for the full usage guide.
:::

## Parameters

| param | description |
| ----- | ----------- |
| `windowSpan`<br>_`int`_ | Fractal evaluation window span width (`S`).  Must be at least 2.  Default is 2. |

The total evaluation window size is `2×S+1`, representing `±S` from the evaluation date.  See [Williams Fractal](/indicators/fractal) for more information about Fractals and `windowSpan`.

### Historical price bars requirements

You must have at least `2×S+1` periods of `bars` to cover the warmup periods; however, more is typically provided since this is a chartable candlestick pattern.

`bars` is a collection of generic `TBar` historical price bars.  It should have a consistent frequency (day, hour, minute, etc).  See [the Guide](/guide/getting-started#historical-bars) for more information.

## Response

::: series

```csharp
IReadOnlyList<FcbResult>
```

- This method returns a time series of all available indicator values for the `bars` provided.
- It always returns the same number of elements as there are in the historical price bars.
- It does not return a single incremental indicator value.
- The periods before the first fractal are `null` since they cannot be calculated.
:::

::: buffer

```csharp
FcbList : IReadOnlyList<FcbResult>
```

- The list itself holds the results, one for each bar or value added, in timestamp order.
- It keeps up to 100,000 results by default and drops the oldest beyond that; set `MaxListSize` to keep fewer.
- The periods before the first fractal are `null` since they cannot be calculated.
:::

::: stream

```csharp
IReadOnlyList<FcbResult> results = observer.Results;
```

- `observer.Results` holds the results, one for each bar in the `BarHub` cache, in timestamp order.
- Results update as bars arrive, including bars that arrive late or are revised.
- The cache keeps up to 100,000 results by default; set `maxCacheSize` on the `BarHub` to keep fewer.
- The periods before the first fractal are `null` since they cannot be calculated.
:::

::: warning 🖌️ Repaint warning
Fractal Chaos Bands are based on [Williams Fractal](/indicators/fractal), which uses future bars.  This indicator will never identify bands in the last `S` periods of `bars` since fractals are retroactively identified.
:::

### `FcbResult`

| property | description |
| ----- | ----------- |
| `Timestamp`<br>_`DateTime`_ | Date from evaluated `TBar` |
| `UpperBand`<br>_`decimal?`_ | FCB upper band |
| `LowerBand`<br>_`decimal?`_ | FCB lower band |

## Chaining

This indicator is not chain-enabled and must be generated from `bars`.  It **cannot** be used for further processing by other chain-enabled indicators.

See [Chaining indicators](/guide/chaining) for more.

## Utilities

::: series

- [.Condense()](/utilities/results#condense)
- [.Find(lookupDate)](/utilities/results#find-by-date)
- [.RemoveWarmupPeriods()](/utilities/results#remove-warmup-periods)
- [.RemoveWarmupPeriods(removePeriods)](/utilities/results#remove-warmup-periods)

:::

::: buffer

- [.Find(lookupDate)](/utilities/results#find-by-date)
- [.Condense()](/utilities/results#condense)

Call these on the list itself, as in `fcbList.Find(lookupDate)`. `.Condense()` returns a new list and leaves `fcbList` unchanged.

Warmup removal is left out. Once the list is full it drops its oldest results, so trimming by position would remove valid values.
:::

::: stream

- [.Find(lookupDate)](/utilities/results#find-by-date)
- [.Condense()](/utilities/results#condense)

Call these on `observer.Results`, as in `observer.Results.Find(lookupDate)`. `.Condense()` returns a new list and leaves the hub's results unchanged.

Warmup removal is left out. Once the cache is full the hub drops its oldest results, so trimming by position would remove valid values.
:::

See [Utilities and helpers](/utilities/) for more information.
