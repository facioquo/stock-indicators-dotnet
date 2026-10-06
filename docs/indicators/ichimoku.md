---
title: Ichimoku Cloud
description: Created by Goichi Hosoda (細田悟一, Hosoda Goichi), Ichimoku Cloud, also known as Ichimoku Kinkō Hyō, is a collection of indicators that depict support and resistance, momentum, and trend direction.
---

# Ichimoku Cloud

Created by Goichi Hosoda (細田悟一, Hosoda Goichi), [Ichimoku Cloud](https://en.wikipedia.org/wiki/Ichimoku_Kink%C5%8D_Hy%C5%8D), also known as Ichimoku Kinkō Hyō, is a collection of indicators that depict support and resistance, momentum, and trend direction.
[[Discuss] &#128172;](https://github.com/facioquo/stock-indicators-dotnet/discussions/251 "Community discussion about this indicator")

<StockIndicatorChart indicator="Ichimoku" />

```csharp
// C# usage syntax (batch)
IReadOnlyList<IchimokuResult> results =
  bars.ToIchimoku(tenkanPeriods, kijunPeriods, senkouBPeriods);

// usage with custom offset
IReadOnlyList<IchimokuResult> results =
  bars.ToIchimoku(tenkanPeriods, kijunPeriods, senkouBPeriods, offsetPeriods);

// usage with different custom offsets
IReadOnlyList<IchimokuResult> results =
  bars.ToIchimoku(tenkanPeriods, kijunPeriods, senkouBPeriods, senkouOffset, chikouOffset);

// buffered usage (incremental)
IchimokuList buffer = bars.ToIchimokuList(tenkanPeriods, kijunPeriods, senkouBPeriods);
IReadOnlyList<IchimokuResult> results = buffer;

// streaming usage (real-time)
BarHub barHub = new();
IchimokuHub observer = barHub.ToIchimokuHub(tenkanPeriods, kijunPeriods, senkouBPeriods);
IReadOnlyList<IchimokuResult> results = observer.Results;
```

## Parameters

| param | type | description |
| ----- | ---- | ----------- |
| `tenkanPeriods` | _`int`_ | Number of periods (`T`) in the Tenkan-sen midpoint evaluation.  Must be greater than 0.  Default is 9. |
| `kijunPeriods` | _`int`_ | Number of periods (`K`) in the shorter Kijun-sen midpoint evaluation.  Must be greater than 0.  Default is 26. |
| `senkouBPeriods` | _`int`_ | Number of periods (`S`) in the longer Senkou leading span B midpoint evaluation.  Must be greater than `K`.  Default is 52. |
| `offsetPeriods` | _`int`_ | Optional.  Number of periods to offset both `Senkou` and `Chikou` spans.  Must be non-negative.  Default is `kijunPeriods`. |
| `senkouOffset` | _`int`_ | Optional.  Number of periods to offset the `Senkou` span.  Must be non-negative.  Default is `kijunPeriods`. |
| `chikouOffset` | _`int`_ | Optional.  Number of periods to offset the `Chikou` span.  Must be non-negative.  Default is `kijunPeriods`. |

See overloads usage above to determine which parameters are relevant for each.  If you are customizing offsets, all parameter arguments must be specified.

### Historical price bars requirements

You must have at least the greater of `T`,`K`, `S`, and offset periods for `bars` to cover the warmup periods; though, given the leading and lagging nature, we recommend notably more.

`bars` is a collection of generic `TBar` historical price bars.  It should have a consistent frequency (day, hour, minute, etc).  See [the Guide](/guide/getting-started#historical-bars) for more information.

## Response

```csharp
IReadOnlyList<IchimokuResult>
```

- This method returns a time series of all available indicator values for the `bars` provided.
- It always returns the same number of elements as there are in the historical price bars.
- It does not return a single incremental indicator value.
- The first `T-1`, `K-1`, and `S-1` periods will have various `null` values since there's not enough data to calculate.  Custom offset periods may also increase `null` results for warmup periods.

### `IchimokuResult`

| property | type | description |
| -------- | ---- | ----------- |
| `Timestamp` | _`DateTime`_ | Date from evaluated `TBar` |
| `TenkanSen` | _`double`_ | Conversion / signal line |
| `KijunSen` | _`double`_ | Base line |
| `SenkouSpanA` | _`double`_ | Leading span A |
| `SenkouSpanB` | _`double`_ | Leading span B |
| `ChikouSpan` | _`double`_ | Lagging span |

### Utilities

- [.Condense()](/utilities/results#condense)
- [.Find(lookupDate)](/utilities/results#find-by-date)
- [.RemoveWarmupPeriods(removePeriods)](/utilities/results#remove-warmup-periods)

See [Utilities and helpers](/utilities/) for more information.

## Chaining

Results can be used for chaining in subsequent indicators when streaming.

```csharp
// example: chain to another indicator (streaming)
var emaHub = bars
    .ToIchimokuHub()
    .ToEmaHub(14);
```

Note: `TenkanSen` is the primary reusable value for chaining purposes.

See [Chaining indicators](/guide/chaining) for more.

## Streaming

Use the buffer-style `List<T>` when you need incremental calculations without a hub:

```csharp
IchimokuList ichimokuList = new(tenkanPeriods, kijunPeriods, senkouBPeriods);

// call from your WebSocket or SSE message handler
void OnBarReceived(IBar bar) => ichimokuList.Add(bar);

// based on `ICollection<IchimokuResult>`; fills as the handler runs
IReadOnlyList<IchimokuResult> results = ichimokuList;
```

Subscribe to a `BarHub` for advanced streaming scenarios:

```csharp
BarHub barHub = new();
IchimokuHub observer = barHub.ToIchimokuHub(tenkanPeriods, kijunPeriods, senkouBPeriods);

// call from your WebSocket or SSE message handler
void OnBarReceived(IBar bar) => barHub.Add(bar);

// results fill as the handler runs
IReadOnlyList<IchimokuResult> results = observer.Results;
```

See [Buffer lists](/guide/styles/buffer) and [Stream hubs](/guide/styles/stream) for full usage guides.
