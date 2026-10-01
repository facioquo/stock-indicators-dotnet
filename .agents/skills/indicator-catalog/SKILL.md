---
name: indicator-catalog
description: Write `{Name}.Catalog.cs` listings with `CatalogListingBuilder` — parameters, results, chart panes, `isRequired` and `isReusable` rules — register them in `Catalog.Listings.cs`, refresh the catalog shape snapshot, and write `{Name}CatalogTests`. Use when creating or editing a `src/Indicators/**/{Name}.Catalog.cs` file, adding or renaming an indicator parameter or result property, editing `src/Common/Catalog/Catalog.Listings.cs`, or when a test under `tests/Library/Common/Catalog/` fails.
---

# Indicator catalog development

The catalog drives automated execution, code generation, and charting, so every listing must bind to a real method, parameter, and result property. The tests in `tests/Library/Common/Catalog/` enforce most rules below across every listing.

## Listing file

`src/Indicators/{folder}/{Name}/{Name}.Catalog.cs` declares one `CommonListing` and one listing per supported style. `src/Indicators/e-j/Ema/Ema.Catalog.cs` is the reference:

```csharp
public static partial class Ema
{
    internal static readonly IndicatorListing CommonListing =
        new CatalogListingBuilder()
            .WithName("Exponential Moving Average")
            .WithId("EMA")
            .WithCategory(Category.MovingAverage)
            .AddParameter<int>("lookbackPeriods", "Lookback Period", description: "Number of periods for the EMA calculation", isRequired: true, defaultValue: 20, minimum: 2, maximum: 250)
            .AddResult(nameof(EmaResult.Ema), "EMA", IndicatorResult.PricePane, ResultType.Default, isReusable: true)
            .Build();

    internal static readonly IndicatorListing SeriesListing =
        new CatalogListingBuilder(CommonListing)
            .WithStyle(Style.Series)
            .WithMethodName("ToEma")
            .Build();

    // BufferListing: Style.Buffer + "ToEmaList"; StreamListing: Style.Stream + "ToEmaHub"
}
```

- Set `.WithMethodName()` only on style listings: `To{Name}` for Series, `To{Name}List` for Buffer, `To{Name}Hub` for Stream.
- `Build()` derives `ResultRecordType` from the method name, so a method of the wrong style reports the wrong result shape.
- `WithId` sets the UIID that names the regression baseline file (`{uiid-lowercase}.standard.json`) and the `Catalog.Get(id, style)` lookup.
- Pick the `Category` an existing peer uses; MACD, Ichimoku, and Pivot Points are `PriceTrend`, and Beta and ATR are `PriceCharacteristic`.

## Parameters

| Builder method | Parameter type |
| -------------- | -------------- |
| `AddParameter<T>()` | `int`, `double`, `decimal`, and other primitive values; always pass `minimum` and `maximum` |
| `AddEnumParameter<T>()` | Enums |
| `AddDateParameter()` | `DateTime` |
| `AddSeriesParameter()` | An `IReadOnlyList<IReusable>` input, such as the comparison series of Beta, Correlation, and PRS |

- `parameterName` matches the method's parameter name exactly; a mismatch makes a catalog-bound caller silently receive the default instead of the supplied value.
- Declare parameters as an unbroken run in signature order. `ListingExecutor` binds them positionally and picks an overload by argument count.
- Set `isRequired: false` exactly when the C# signature gives the parameter a default, and set `defaultValue` equal to that C# default.
- When omitting the argument instead selects a shorter overload, use `isRequired: false` only if the listing declares no `defaultValue` (VWAP `startDate`). If the shorter overload behaves differently from the advertised default (`ToPrs(sourceEval, sourceBase)` computes no `PrsPercent`), use `isRequired: true`; callers reach the shorter form with `WithoutParam(name)` at execution time.

## Results

`AddResult(dataName, displayName, chartPane, dataType, isReusable)`:

- `dataName` is `nameof(TResult.Property)`, never a string literal, so a renamed property fails the build. `displayName` is a literal human label.
- `chartPane` is `IndicatorResult.PricePane` for a value at price level, a shared pane name (conventionally the indicator name, such as `"Macd"`) for results with common non-price units, and `null` for a non-numeric result such as a pattern match.
- `isReusable: true` marks exactly one result on an `IReusable` model, the property that `Value` returns. An `ISeries` model marks none. `Build()` throws when more than one is marked.
- `ResultType` sets the chart form: `Default`, `Centerline`, `Channel`, `Bar`, `BarStacked`, or `Point`.

## Registration and shape snapshot

Register each listing inside `PopulateCatalog()` in `src/Common/Catalog/Catalog.Listings.cs`. Blocks sort alphabetically by indicator ID, are separated by a blank line, and open with a `// {ID} ({Full Name})` comment, or `// {Full Name}` when no abbreviation is conventional. Within a block the order is Buffer, Series, Stream:

```csharp
// EMA (Exponential Moving Average)
_listings.Add(Ema.BufferListing);
_listings.Add(Ema.SeriesListing);
_listings.Add(Ema.StreamListing);

// Beta
_listings.Add(Beta.SeriesListing);
```

Adding or changing a parameter or result changes `tests/Library/TestData/catalog/shape.snapshot.txt`. Regenerate it, review the diff, then rerun the test without the variable to confirm it passes; the regeneration run reports Inconclusive:

```bash
UPDATE_CATALOG_SHAPE=1 dotnet test tests/Library/Tests.Indicators.csproj --filter CatalogShapeMatchesSnapshot
```

## Tests

`tests/Library/Indicators/{folder}/{Name}/{Name}CatalogTests.cs` declares `public class {Name}CatalogTests : TestBase` in namespace `Catalogging`, with one `{Name}{Style}_InCatalog_ReturnsAllVariants()` method per style asserting `Name`, `Uiid`, `Style`, `Category`, `MethodName`, and the parameter and result counts. `tests/Library/Indicators/t-z/Wma/WmaCatalogTests.cs` is a minimal example.

The catalog-wide tests verify method binding per style, `dataName` and `parameterName` resolution, parameter order, `isRequired` and default agreement with the C# signature, chart panes, and the shape snapshot. They do not check names, categories, or value ranges, so keep the per-indicator test.

## Do not do these

- Do not call `.WithMethodName()` on `CommonListing`.
- Do not write a `dataName` as a string literal where `nameof` can reach the member.
- Do not mark `isReusable: true` on an `ISeries` model or on more than one result.
- Do not set `isRequired: false` with a `defaultValue` the C# signature does not apply when the argument is omitted.
- Do not hand-edit `shape.snapshot.txt`; regenerate it.
