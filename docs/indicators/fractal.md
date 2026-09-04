---
title: Williams Fractal
description: Created by Bill Williams, Fractal is a retrospective price pattern that identifies a central high or low point chevron.
---

# Williams Fractal

Created by Bill Williams in _Trading Chaos_ (1995), [Fractal](https://www.investopedia.com/terms/f/fractal.asp) is a retrospective price pattern that identifies a central high or low point chevron.
[[Discuss] &#128172;](https://github.com/facioquo/stock-indicators-dotnet/discussions/255 "Community discussion about this indicator")

<ClientOnly>
  <StockIndicatorChart indicator="Fractal" />
</ClientOnly>

```csharp
// C# usage syntax
IReadOnlyList<FractalResult> results =
  bars.ToFractal(windowSpan);
```

## Parameters

| param | type | description |
| ----- | ---- | ----------- |
| `windowSpan` | _`int`_ | Evaluation window span width (`S`).  Must be at least 2.  Default is 2. |
| `endType` | _`EndType`_ | Determines whether `Close` or `High/Low` are used to find end points.  Default is `EndType.HighLow`. |

The total evaluation window size is `2×S+1`, representing `±S` from the evaluation date.

### Historical price bars requirements

You must have at least `2×S+1` periods of `bars` to cover the warmup periods; however, more is typically provided since this is a chartable candlestick pattern.

`bars` is a collection of generic `TBar` historical price bars.  It should have a consistent frequency (day, hour, minute, etc).  See [the Guide](/guide/getting-started#historical-bars) for more information.

<!--@include: ../shared/enum-endtype.md-->

## Response

```csharp
IReadOnlyList<FractalResult>
```

- This method returns a time series of all available indicator values for the `bars` provided.
- It always returns the same number of elements as there are in the historical price bars.
- It does not return a single incremental indicator value.
- The first and last `S` periods in `bars` are unable to be calculated since there's not enough prior/following data.
- Per Williams' definition the middle bar must be strictly beyond both wings, so when two or more bars tie for the extreme value, no fractal is identified.

::: warning ️🖌️ Repaint warning
This price pattern uses future bars and will never identify a `fractal` in the last `S` periods of `bars`.  Fractals are retroactively identified.
:::

### `FractalResult`

| property | type | description |
| -------- | ---- | ----------- |
| `Timestamp` | _`DateTime`_ | Date from evaluated `TBar` |
| `FractalBear` | _`decimal`_ | Value indicates a **high** point; otherwise `null` is returned. |
| `FractalBull` | _`decimal`_ | Value indicates a **low** point; otherwise `null` is returned. |

### Utilities

- [.Condense()](/utilities/results#condense)
- [.Find(lookupDate)](/utilities/results#find-by-date)
- [.RemoveWarmupPeriods(removePeriods)](/utilities/results#remove-warmup-periods)

See [Utilities and helpers](/utilities/) for more information.

## Chaining

This indicator is not chain-enabled and must be generated from `bars`.  It **cannot** be used for further processing by other chain-enabled indicators.

See [Chaining indicators](/guide/chaining) for more.

## Streaming

Use the buffer-style `List<T>` when you need incremental calculations without a hub:

```csharp
FractalList fractalList = new(windowSpan);

foreach (IBar bar in bars)  // simulating stream
{
  fractalList.Add(bar);
}

// based on `ICollection<FractalResult>`
IReadOnlyList<FractalResult> results = fractalList;
```

Subscribe to a `BarHub` for advanced streaming scenarios:

```csharp
BarHub barHub = new();
FractalHub observer = barHub.ToFractalHub(windowSpan);

foreach (IBar bar in bars)  // simulating stream
{
  barHub.Add(bar);
}

IReadOnlyList<FractalResult> results = observer.Results;
```

See [Buffer lists](/guide/styles/buffer) and [Stream hubs](/guide/styles/stream) for full usage guides.
