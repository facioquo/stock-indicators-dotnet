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
            .AddResult(nameof(EmaResult.Ema), "EMA", ResultType.Default, isReusable: true)
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

`IndicatorListing.ResultRecordType` is derived from the method name when the listing is built — do not set it by hand and do not add a builder method for it. Naming a method of the wrong style (e.g. `ToEma` on the Buffer listing) makes it report the wrong result shape.

## Parameter patterns

- `AddParameter<T>()` — primitive value types (typically `int` or `double`)
- `AddEnumParameter<T>()` — enum types
- `AddDateParameter()` — DateTime
- `AddSeriesParameter()` — `IReadOnlyList<T> where T : IReusable`
- `minimum` and `maximum` required for all numeric parameters
- `parameterName` must match the bound method's parameter name exactly — a mismatch makes a catalog-bound caller silently receive the default instead of the value it supplied
- Declare parameters as an unbroken run in signature order, skipping none in the middle; `ListingExecutor` binds them positionally and picks an overload by argument count

## Result patterns

- `dataName` uses `nameof(TResult.Property)`, not a string literal, so a renamed result property fails the build instead of a test — e.g. `.AddResult(nameof(EmaResult.Ema), "EMA", ...)`
- `displayName` stays a string literal; it is a human label, not a member name
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
- A `dataName` as a string literal where `nameof` can reach the member
- A `parameterName` that names a member the library does not have

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
listing in the catalog, that `MethodName` resolves to a real method of the listing's own
style, that each `dataName` resolves to a property on that method's result record, and
that each `parameterName` forms a contiguous, in-order run in the signature. A listing
naming a member the library does not have fails there. It does not check parameter
*types* or value semantics, so keep writing the per-indicator test.
