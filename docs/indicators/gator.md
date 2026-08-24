---
title: Gator Oscillator
description: Created by Bill Williams, the Gator Oscillator is an expanded oscillator view of Williams Alligator's three moving averages.
---

# Gator Oscillator

Created by Bill Williams, the Gator Oscillator is an expanded oscillator view of [Williams Alligator](/indicators/alligator)'s three moving averages. [[Discuss] &#128172;](https://github.com/facioquo/stock-indicators-dotnet/discussions/385 "Community discussion about this indicator")

<ClientOnly>
  <StockIndicatorChart indicator="Gator" withOverlay />
</ClientOnly>

```csharp
// C# usage syntax
IReadOnlyList<GatorResult> results =
  bars.ToGator();

// with custom Alligator configuration
IReadOnlyList<GatorResult> results = bars
  .ToAlligator([see Alligator docs])
  .ToGator();
```

## Historical price bars requirements

If using default settings, you must have at least 121 periods of `bars` to cover the [warmup and convergence](https://github.com/facioquo/stock-indicators-dotnet/discussions/688) periods. Since this uses a smoothing technique, we recommend you use at least 271 data points prior to the intended usage date for better precision.  If using a custom Alligator configuration, see [Alligator documentation](/indicators/alligator#historical-price-bars-requirements) for Historical price bars requirements.

`bars` is a collection of generic `TBar` historical price bars.  It should have a consistent frequency (day, hour, minute, etc).  See [the Guide](/guide/getting-started#historical-bars) for more information.

## Response

```csharp
IReadOnlyList<GatorResult>
```

- This method returns a time series of all available indicator values for the `bars` provided.
- It always returns the same number of elements as there are in the historical price bars.
- It does not return a single incremental indicator value.
- The first 10-20 periods will have `null` values since there's not enough data to calculate.

::: warning 🚩 ⚞ Convergence warning
The first 150 periods will have decreasing magnitude, convergence-related precision errors that can be as high as ~5% deviation in indicator values for earlier periods.
:::

### `GatorResult`

| property | type | description |
| -------- | ---- | ----------- |
| `Timestamp` | _`DateTime`_ | Date from evaluated `TBar` |
| `Upper` | _`double`_ | Absolute value of Alligator `Jaw-Teeth` |
| `Lower` | _`double`_ | Absolute value of Alligator `Lips-Teeth` |
| `UpperIsExpanding` | _`bool`_ | Upper value is growing |
| `LowerIsExpanding` | _`bool`_ | Lower value is growing |

### Utilities

- [.Condense()](/utilities/results#condense)
- [.Find(lookupDate)](/utilities/results#find-by-date)
- [.RemoveWarmupPeriods()](/utilities/results#remove-warmup-periods)
- [.RemoveWarmupPeriods(removePeriods)](/utilities/results#remove-warmup-periods)

See [Utilities and helpers](/utilities/) for more information.

## Chaining

This indicator may be generated from any chain-enabled indicator or method.

```csharp
// example
var results = bars
    .Use(CandlePart.HLC3)
    .ToGator();
```

Results **cannot** be further chained with additional transforms.

See [Chaining indicators](/guide/chaining) for more.

## Streaming

Use the buffer-style `List<T>` when you need incremental calculations without a hub:

```csharp
GatorList gatorList = new();

foreach (IBar bar in bars)  // simulating stream
{
  gatorList.Add(bar);
}

// based on `ICollection<GatorResult>`
IReadOnlyList<GatorResult> results = gatorList;
```

Subscribe to a `BarHub` for advanced streaming scenarios:

```csharp
BarHub barHub = new();
GatorHub observer = barHub.ToGatorHub();

foreach (IBar bar in bars)  // simulating stream
{
  barHub.Add(bar);
}

IReadOnlyList<GatorResult> results = observer.Results;
```

::: info Compound hub
The Gator hub is based on the Alligator indicator. When the Gator hub is chained from an existing `AlligatorHub` instance it will reuse the existing Alligator hub values rather than creating its own internal Alligator calculations. **This is not a normal chaining model.**

```csharp
// creates an internal Alligator hub
var gatorHub = bars
  .ToGatorHub();

// this is helpful in cases where you have an independent 
// Alligator hub and do not want to create duplicate copies

var alligatorHub = bars
  .ToAlligatorHub();

// does not create 2nd internal huba separate internal Alligator hub
var gatorHub = alligatorHub
  .ToGatorHub();  // does not create 2nd internal hub

// ❌ Alligator → [ Alligator ] → Gator
// ✅ Alligator → Gator
```

:::

See [Buffer lists](/guide/styles/buffer) and [Stream hubs](/guide/styles/stream) for full usage guides.
