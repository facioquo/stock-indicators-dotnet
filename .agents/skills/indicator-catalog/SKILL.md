---
name: indicator-catalog
description: Create and register indicator catalog entries for automation. Use for Catalog.cs files, CatalogListingBuilder patterns, parameter/result definitions, and PopulateCatalog registration.
---

# Indicator catalog development

## File

`src/Indicators/{category}/{Indicator}/{Indicator}.Catalog.cs`

## Builder pattern

```csharp
public static partial class Ema
{
    /// <summary>
    /// EMA Common Base Listing
    /// </summary>
    internal static readonly IndicatorListing CommonListing =
        new CatalogListingBuilder()
            .WithName("Exponential Moving Average")
            .WithId("EMA")
            .WithCategory(Category.MovingAverage)
            .AddParameter<int>("lookbackPeriods", "Lookback Period",
                description: "Number of periods for the EMA calculation",
                isRequired: true, defaultValue: 20, minimum: 2, maximum: 250)
            .AddResult("Ema", "EMA", ResultType.Default, isReusable: true)
            .Build();

    /// <summary>
    /// EMA Series Listing
    /// </summary>
    internal static readonly IndicatorListing SeriesListing =
        new CatalogListingBuilder(CommonListing)
            .WithStyle(Style.Series)
            .WithMethodName("ToEma")
            .Build();

    /// <summary>
    /// EMA Stream Listing
    /// </summary>
    internal static readonly IndicatorListing StreamListing =
        new CatalogListingBuilder(CommonListing)
            .WithStyle(Style.Stream)
            .WithMethodName("ToEmaHub")
            .Build();

    /// <summary>
    /// EMA Buffer Listing
    /// </summary>
    internal static readonly IndicatorListing BufferListing =
        new CatalogListingBuilder(CommonListing)
            .WithStyle(Style.Buffer)
            .WithMethodName("ToEmaList")
            .Build();
}
```

## Method naming

| Style | Pattern | Example |
| ----- | ------- | ------- |
| Series | `To{Name}` | `ToEma` |
| Stream | `To{Name}Hub` | `ToEmaHub` |
| Buffer | `To{Name}List` | `ToEmaList` |

`.WithMethodName()` must be in style-specific listings, NOT in `CommonListing`.

## Parameter patterns

- `AddParameter<T>()` — primitive value types (typically `int` or `double`)
- `AddEnumParameter<T>()` — enum types
- `AddDateParameter()` — DateTime
- `AddSeriesParameter()` — `IReadOnlyList<T> where T : IReusable`
- `minimum` and `maximum` required for all numeric parameters
- `parameterName` must match the bound method's parameter name exactly — a mismatch makes a catalog-bound caller silently receive the default instead of the value it supplied
- Declare parameters in the same order the method's signature declares them; `ListingExecutor` binds them positionally

## Result patterns

- `dataName` must match property name in Models file exactly
- `isReusable: true` only for the property mapping to `IReusable.Value`
- `ISeries` models: all results must have `isReusable: false`
- Exactly one `isReusable: true` per `IReusable` indicator

## Categories

| Category | Examples |
| -------- | -------- |
| `CandlestickPattern` | Doji, Marubozu |
| `MovingAverage` | EMA, SMA, HMA, TEMA, WMA, DEMA |
| `Oscillator` | RSI, Stochastic, MACD, CCI, BOP, CMO, Chop, DPO |
| `PriceChannel` | Bollinger Bands, Keltner, Donchian, VWAP |
| `PriceCharacteristic` | ATR, Beta, Standard deviation, True Range |
| `PricePattern` | Fractal, Pivot Points |
| `PriceTransform` | Bar Part, ZigZag |
| `PriceTrend` | ADX, Aroon, Alligator, AtrStop, SuperTrend, Vortex |
| `StopAndReverse` | Chandelier, Parabolic SAR, Volatility Stop |
| `VolumeBased` | OBV, Chaikin Money Flow, Chaikin Oscillator |

## Registration

Add entries inside the host repository's catalog populator method. The backing collection is a `private static readonly List<IndicatorListing>` declared at the top of the catalog file; the host repository determines its field name (shown below as `listings`).

Indicators are grouped alphabetically by indicator ID (abbreviation) and separated by a blank line. Each block is preceded by a short comment header — `// {Abbreviation} ({Full Name})` when an abbreviation is conventional, otherwise just `// {Full Name}`. Within a block, the preferred order for the style listings is **Buffer → Series → Stream**:

```csharp
// EMA (Exponential Moving Average)
listings.Add(Ema.BufferListing);
listings.Add(Ema.SeriesListing);
listings.Add(Ema.StreamListing);

// HMA (Hull Moving Average)
listings.Add(Hma.BufferListing);
listings.Add(Hma.SeriesListing);
listings.Add(Hma.StreamListing);
```

Series-only indicators (no streamable variant) register a single `SeriesListing` line in the same alphabetical position; e.g.:

```csharp
// Beta
listings.Add(Beta.SeriesListing);
```

## Prohibited

- `.WithMethodName()` in `CommonListing`
- Wrong indicator method name
- `isReusable: true` for `ISeries` models
- Multiple `isReusable: true` results per indicator
- A `dataName` or `parameterName` that names a member the library does not have

## Testing

`tests/Library/Indicators/{folder}/{Indicator}/{Indicator}CatalogTests.cs`:

```csharp
[TestClass]
public class EmaCatalogTests : TestBase
{
    [TestMethod]
    public void EmaSeriesListing()
    {
        var listing = Ema.SeriesListing;
        listing.Name.Should().Be("Exponential Moving Average");
        listing.Style.Should().Be(Style.Series);
        listing.MethodName.Should().Be("ToEma");
    }
}
```

`tests/Library/Common/Catalog/Catalog.Binding.Tests.cs` additionally enforces, for every
listing in the catalog, that `MethodName` resolves to a real method, that each
`dataName` resolves to a property on that method's result record, and that each
`parameterName` resolves to a method parameter in signature order. A new listing that
names a member the library does not have fails there — no per-indicator test needed.
