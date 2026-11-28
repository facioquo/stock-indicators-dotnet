---
title: Ichimoku Cloud
description: Created by Goichi Hosoda (細田悟一, Hosoda Goichi), Ichimoku Cloud, also known as Ichimoku Kinkō Hyō, is a collection of indicators that depict support and resistance, momentum, and trend direction.
---

# Ichimoku Cloud

Created by Goichi Hosoda (細田悟一, Hosoda Goichi), [Ichimoku Cloud](https://en.wikipedia.org/wiki/Ichimoku_Kink%C5%8D_Hy%C5%8D), also known as Ichimoku Kinkō Hyō, is a collection of indicators that depict support and resistance, momentum, and trend direction.
[[Discuss] &#128172;](https://github.com/facioquo/stock-indicators-dotnet/discussions/251 "Community discussion about this indicator")

<StyleSwitch />

<StockIndicatorChart indicator="Ichimoku" />

<ChartKey indicator="Ichimoku" />

<a id="streaming"></a>

::: series

```csharp
// C# usage syntax
IReadOnlyList<IchimokuResult> results =
  bars.ToIchimoku(tenkanPeriods, kijunPeriods, senkouBPeriods);

// usage with custom offset
IReadOnlyList<IchimokuResult> results =
  bars.ToIchimoku(tenkanPeriods, kijunPeriods, senkouBPeriods, offsetPeriods);

// usage with different custom offsets
IReadOnlyList<IchimokuResult> results =
  bars.ToIchimoku(tenkanPeriods, kijunPeriods, senkouBPeriods, senkouOffset, chikouOffset);
```

:::

::: buffer
Create a buffer list and add each bar as it arrives:

```csharp
IchimokuList ichimokuList = new(tenkanPeriods, kijunPeriods, senkouBPeriods);
// the offset overloads of the Series method apply here too

// call from your WebSocket or SSE message handler
void OnBarReceived(IBar bar) => ichimokuList.Add(bar);

// the list itself holds the results; fills as the handler runs
IReadOnlyList<IchimokuResult> results = ichimokuList;
```

See [Buffer lists](/guide/styles/buffer) for the full usage guide.
:::

::: stream
Subscribe to a `BarHub` and add each bar as it arrives:

```csharp
BarHub barHub = new();
IchimokuHub observer = barHub.ToIchimokuHub(tenkanPeriods, kijunPeriods, senkouBPeriods);
// or with one offset: .ToIchimokuHub(tenkanPeriods, kijunPeriods, senkouBPeriods, offsetPeriods)

// call from your WebSocket or SSE message handler
void OnBarReceived(IBar bar) => barHub.Add(bar);

// results fill as the handler runs
IReadOnlyList<IchimokuResult> results = observer.Results;
```

See [Stream hubs](/guide/styles/stream) for the full usage guide.
:::

## Parameters

| param | description |
| ----- | ----------- |
| `tenkanPeriods`<br>_`int`_ | Number of periods (`T`) in the Tenkan-sen midpoint evaluation.  Must be greater than 0.  Default is 9. |
| `kijunPeriods`<br>_`int`_ | Number of periods (`K`) in the shorter Kijun-sen midpoint evaluation.  Must be greater than 0.  Default is 26. |
| `senkouBPeriods`<br>_`int`_ | Number of periods (`S`) in the longer Senkou leading span B midpoint evaluation.  Must be greater than `K`.  Default is 52. |
| `offsetPeriods`<br>_`int`_ | Optional.  Number of periods to offset both `Senkou` and `Chikou` spans.  Must be non-negative.  Default is `kijunPeriods`. |
| `senkouOffset`<br>_`int`_ | Optional.  Number of periods to offset the `Senkou` span.  Must be non-negative.  Default is `kijunPeriods`. |
| `chikouOffset`<br>_`int`_ | Optional.  Number of periods to offset the `Chikou` span.  Must be non-negative.  Default is `kijunPeriods`. |

See overloads usage above to determine which parameters are relevant for each.  If you are customizing offsets, all parameter arguments must be specified.

### Historical price bars requirements

You must have at least the greater of `T`,`K`, `S`, and offset periods for `bars` to cover the warmup periods; though, given the leading and lagging nature, we recommend notably more.

`bars` is a collection of generic `TBar` historical price bars.  It should have a consistent frequency (day, hour, minute, etc).  See [the Guide](/guide/getting-started#historical-bars) for more information.

## Response

::: series

```csharp
IReadOnlyList<IchimokuResult>
```

- This method returns a time series of all available indicator values for the `bars` provided.
- It always returns the same number of elements as there are in the historical price bars.
- It does not return a single incremental indicator value.
- The first `T-1`, `K-1`, and `S-1` periods will have various `null` values since there's not enough data to calculate.  Custom offset periods may also increase `null` results for warmup periods.
:::

::: buffer

```csharp
IchimokuList : IReadOnlyList<IchimokuResult>
```

- The list itself holds the results, one for each bar or value added, in timestamp order.
- It keeps up to 100,000 results by default and drops the oldest beyond that; set `MaxListSize` to keep fewer.
- The first `T-1`, `K-1`, and `S-1` periods will have various `null` values since there's not enough data to calculate.  Custom offset periods may also increase `null` results for warmup periods.
:::

::: stream

```csharp
IReadOnlyList<IchimokuResult> results = observer.Results;
```

- `observer.Results` holds the results, one for each bar in the `BarHub` cache, in timestamp order.
- Results update as bars arrive, including bars that arrive late or are revised.
- The cache keeps up to 100,000 results by default; set `maxCacheSize` on the `BarHub` to keep fewer.
- The first `T-1`, `K-1`, and `S-1` periods will have various `null` values since there's not enough data to calculate.  Custom offset periods may also increase `null` results for warmup periods.
:::

### `IchimokuResult`

| property | description |
| ----- | ----------- |
| `Timestamp`<br>_`DateTime`_ | Date from evaluated `TBar` |
| `TenkanSen`<br>_`double?`_ | Conversion / signal line |
| `KijunSen`<br>_`double?`_ | Base line |
| `SenkouSpanA`<br>_`double?`_ | Leading span A |
| `SenkouSpanB`<br>_`double?`_ | Leading span B |
| `ChikouSpan`<br>_`double?`_ | Lagging span |

### Utilities

- [.Condense()](/utilities/results#condense)
- [.Find(lookupDate)](/utilities/results#find-by-date)
- [.RemoveWarmupPeriods(removePeriods)](/utilities/results#remove-warmup-periods)

See [Utilities and helpers](/utilities/) for more information.

## Chaining

This indicator must be generated from `bars` and **cannot** be generated from results of another chain-enabled indicator or method.

Results **cannot** be further chained with additional transforms.

See [Chaining indicators](/guide/chaining) for more.
