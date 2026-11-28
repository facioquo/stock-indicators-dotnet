---
title: Williams Fractal
description: Created by Bill Williams, Fractal is a retrospective price pattern that identifies a central high or low point chevron.
---

# Williams Fractal

Created by Bill Williams in _Trading Chaos_ (1995), [Fractal](https://www.investopedia.com/terms/f/fractal.asp) is a retrospective price pattern that identifies a central high or low point chevron.
[[Discuss] &#128172;](https://github.com/facioquo/stock-indicators-dotnet/discussions/255 "Community discussion about this indicator")

<StyleSwitch />

<StockIndicatorChart indicator="Fractal" />

<ChartKey indicator="Fractal" />

<a id="streaming"></a>

::: series

```csharp
// C# usage syntax
IReadOnlyList<FractalResult> results =
  bars.ToFractal(windowSpan);
```

:::

::: buffer
Create a buffer list and add each bar as it arrives:

```csharp
FractalList fractalList = new(windowSpan);

// call from your WebSocket or SSE message handler
void OnBarReceived(IBar bar) => fractalList.Add(bar);

// the list itself holds the results; fills as the handler runs
IReadOnlyList<FractalResult> results = fractalList;
```

See [Buffer lists](/guide/styles/buffer) for the full usage guide.
:::

::: stream
Subscribe to a `BarHub` and add each bar as it arrives:

```csharp
BarHub barHub = new();
FractalHub observer = barHub.ToFractalHub(windowSpan);

// call from your WebSocket or SSE message handler
void OnBarReceived(IBar bar) => barHub.Add(bar);

// results fill as the handler runs
IReadOnlyList<FractalResult> results = observer.Results;
```

See [Stream hubs](/guide/styles/stream) for the full usage guide.
:::

## Parameters

| param | description |
| ----- | ----------- |
| `windowSpan`<br>_`int`_ | Evaluation window span width (`S`).  Must be at least 2.  Default is 2. |
| `endType`<br>_`EndType`_ | Determines whether `Close` or `High/Low` are used to find end points.  Default is `EndType.HighLow`. |

The total evaluation window size is `2×S+1`, representing `±S` from the evaluation date.

An overload takes `leftSpan` and `rightSpan` in place of `windowSpan` to evaluate an asymmetric window: `bars.ToFractal(leftSpan, rightSpan, endType)`.  Each span must be at least 2, and the total evaluation window size is `leftSpan+rightSpan+1`.

### Historical price bars requirements

You must have at least `2×S+1` periods of `bars` (`leftSpan+rightSpan+1` for the overload) to cover the warmup periods; however, more is typically provided since this is a chartable candlestick pattern.

`bars` is a collection of generic `TBar` historical price bars.  It should have a consistent frequency (day, hour, minute, etc).  See [the Guide](/guide/getting-started#historical-bars) for more information.

<!--@include: ../shared/enum-endtype.md-->

## Response

::: series

```csharp
IReadOnlyList<FractalResult>
```

- This method returns a time series of all available indicator values for the `bars` provided.
- It always returns the same number of elements as there are in the historical price bars.
- It does not return a single incremental indicator value.
- The first and last `S` periods in `bars` are unable to be calculated since there's not enough prior/following data.
- Per Williams' definition the middle bar must be strictly beyond both wings, so when two or more bars tie for the extreme value, no fractal is identified.
:::

::: buffer

```csharp
FractalList : IReadOnlyList<FractalResult>
```

- The list itself holds the results, one for each bar or value added, in timestamp order.
- It keeps up to 100,000 results by default and drops the oldest beyond that; set `MaxListSize` to keep fewer.
- The first and last `S` periods in `bars` are unable to be calculated since there's not enough prior/following data.
- Per Williams' definition the middle bar must be strictly beyond both wings, so when two or more bars tie for the extreme value, no fractal is identified.
:::

::: stream

```csharp
IReadOnlyList<FractalResult> results = observer.Results;
```

- `observer.Results` holds the results, one for each bar in the `BarHub` cache, in timestamp order.
- Results update as bars arrive, including bars that arrive late or are revised.
- The cache keeps up to 100,000 results by default; set `maxCacheSize` on the `BarHub` to keep fewer.
- The first and last `S` periods in `bars` are unable to be calculated since there's not enough prior/following data.
- Per Williams' definition the middle bar must be strictly beyond both wings, so when two or more bars tie for the extreme value, no fractal is identified.
:::

::: warning 🖌️ Repaint warning
This price pattern uses future bars and will never identify a `fractal` in the last `S` periods of `bars`.  Fractals are retroactively identified.
:::

### `FractalResult`

| property | description |
| ----- | ----------- |
| `Timestamp`<br>_`DateTime`_ | Date from evaluated `TBar` |
| `FractalBear`<br>_`decimal?`_ | Value indicates a **high** point; otherwise `null` is returned. |
| `FractalBull`<br>_`decimal?`_ | Value indicates a **low** point; otherwise `null` is returned. |

## Chaining

This indicator is not chain-enabled and must be generated from `bars`.  It **cannot** be used for further processing by other chain-enabled indicators.

See [Chaining indicators](/guide/chaining) for more.

## Utilities

::: series

- [.Condense()](/utilities/results#condense)
- [.Find(lookupDate)](/utilities/results#find-by-date)
- [.RemoveWarmupPeriods(removePeriods)](/utilities/results#remove-warmup-periods)

:::

::: buffer

- [.Find(lookupDate)](/utilities/results#find-by-date)
- [.Condense()](/utilities/results#condense)

Call these on the list itself, as in `fractalList.Find(lookupDate)`. `.Condense()` returns a new list and leaves `fractalList` unchanged.

Warmup removal is left out. Once the list is full it drops its oldest results, so trimming by position would remove valid values.
:::

::: stream

- [.Find(lookupDate)](/utilities/results#find-by-date)
- [.Condense()](/utilities/results#condense)

Call these on `observer.Results`, as in `observer.Results.Find(lookupDate)`. `.Condense()` returns a new list and leaves the hub's results unchanged.

Warmup removal is left out. Once the cache is full the hub drops its oldest results, so trimming by position would remove valid values.
:::

See [Utilities and helpers](/utilities/) for more information.
