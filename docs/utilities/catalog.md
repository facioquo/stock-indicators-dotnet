---
title: Indicator catalog
description: Programmatic access to indicator metadata for building dynamic UIs or automation.
---

# Indicator catalog (metadata)

Use the indicator catalog to discover indicators, inspect their parameters and results, build pickers or configuration UIs, and execute a selected indicator without hard-coding its method call.

- Discover indicators, parameters, and results at runtime
- Build configuration UIs from indicator metadata, or export it to JSON or Markdown
- Execute a selected indicator from its listing, without hard-coding the method call

::: warning Non-idiomatic
_The Catalog_ provides a programmatic way to interact with indicators and options; however, it is not the idiomatic .NET way to use this library. See the examples in [the Guide](/guide/getting-started) for normal usage.
:::

## Browse and search the catalog

All query methods are on the static `Catalog` class and return `IndicatorListing` metadata (no calculations are run).

```csharp
using FacioQuo.Stock.Indicators;

// all listings
IReadOnlyList<IndicatorListing> all = Catalog.Get();

// a single listing by ID + style (null if not found)
IndicatorListing? emaSeries
  = Catalog.Get("EMA", Style.Series);

// all styles available for one ID
IReadOnlyList<IndicatorListing> allEma
  = Catalog.Get("EMA");

// filter by style
IReadOnlyList<IndicatorListing> seriesOnly
  = Catalog.Get(Style.Series);

// filter by category
IReadOnlyList<IndicatorListing> movingAverages
  = Catalog.Get(Category.MovingAverage);

// partial-match search on ID or name (empty query returns all)
IReadOnlyList<IndicatorListing> matches 
  = Catalog.Search("average");
```

| Method | Returns | Description |
| ------ | ------- | ----------- |
| `Get()` | `IReadOnlyList<IndicatorListing>` | All listings |
| `Get(string id, Style style)` | `IndicatorListing?` | One listing, or `null` if not found |
| `Get(string id)` | `IReadOnlyList<IndicatorListing>` | All styles for an ID |
| `Get(Style style)` | `IReadOnlyList<IndicatorListing>` | Listings of a given style |
| `Get(Category category)` | `IReadOnlyList<IndicatorListing>` | Listings in a category |
| `Search(string query)` | `IReadOnlyList<IndicatorListing>` | Partial (case-insensitive) match on ID or name |

## Listing metadata

Each `IndicatorListing` describes one indicator-and-style combination. This is the metadata you bind a UI to.

| Property | Type | Description |
| -------- | ---- | ----------- |
| `Uiid` | `string` | Unique indicator ID (e.g. `"EMA"`) |
| `Name` | `string` | Display name |
| `Style` | `Style` | `Series`, `Buffer`, or `Stream` |
| `Category` | `Category` | Indicator category (see below) |
| `Parameters` | `IReadOnlyList<IndicatorParam>?` | Input parameters (`null` when there are none) |
| `Results` | `IReadOnlyList<IndicatorResult>` | Output fields the indicator produces |
| `ResultRecordType` | `string?` | Name of the result record the method returns (e.g. `"EmaResult"`), the same for all three styles — the record type, not the method's literal return type (`EmaList` for Buffer, `EmaHub` for Stream). `null` when `MethodName` is unset, or when the method cannot be resolved (possible under trimming or NativeAOT publishing) — check before use |
| `MethodName` | `string?` | Method name, for automation use cases |
| ~~`ReturnType`~~ | `string?` | **Obsolete.** Renamed to `ResultRecordType`; forwards to it and is removed in the next major version. Not serialized. |
| `LegendTemplate` | `string` | Legend template for charting |

`IndicatorParam` (each input parameter):

| Property | Type | Description |
| -------- | ---- | ----------- |
| `DisplayName` | `string` | Human-friendly label |
| `ParameterName` | `string` | Method parameter name (use this with `WithParamValue`) |
| `DataType` | `string` | Parameter type name |
| `Description` | `string?` | Optional description |
| `IsRequired` | `bool` | Whether the parameter must be supplied |
| `DefaultValue` | `object?` | Default value, if any |
| `Minimum` / `Maximum` | `double?` | Recommended bounds, if any |
| `EnumOptions` | `Dictionary<int, string>?` | Allowed values for enum parameters (`null` otherwise) |

`IndicatorResult` (each output field):

| Property | Type | Description |
| -------- | ---- | ----------- |
| `DisplayName` | `string` | Human-friendly label |
| `DataName` | `string` | Result property name (e.g. `"Ema"`) |
| `DataType` | `ResultType` | Charting/display hint |
| `IsReusable` | `bool` | `true` for the field exposed as the chainable `IReusable.Value` |

Example — inspect an indicator's inputs:

```csharp
IndicatorListing? listing = Catalog.Get("EMA", Style.Series);

foreach (IndicatorParam p in listing?.Parameters ?? [])
{
    Console.WriteLine(
        $"{p.DisplayName} ({p.DataType}), required: {p.IsRequired}, default: {p.DefaultValue}");
}
```

### Enums

- **`Style`**: `Series`, `Buffer`, `Stream` — the three [indicator styles](/guide/styles/).
- **`Category`**: `Undefined`, `CandlestickPattern`, `MovingAverage`, `Oscillator`, `PriceChannel`, `PriceCharacteristic`, `PricePattern`, `PriceTransform`, `PriceTrend`, `StopAndReverse`, `VolumeBased`.
- **`ResultType`**: `Default`, `Centerline`, `Channel`, `Bar`, `BarStacked`, `Point` — display hints for charting libraries.

## Export the catalog

These extension methods serialize an `IReadOnlyList<IndicatorListing>` (typically `Catalog.Get()`). Each accepts an optional file path; when provided, the content is written to that file and also returned.

```csharp
IReadOnlyList<IndicatorListing> catalog = Catalog.Get();

// JSON (indented, camelCase, string enums)
string json = catalog.ToJson();
catalog.ToJson("catalog.json");                       // also writes to file

// Markdown checklist: "- [ ] {id}: {name} ({styles})"
string checklist = catalog.ToMarkdownChecklist();

// Markdown table with ID, Name, Series, Buffer, Stream columns
string table = catalog.ToMarkdownTable();
```

| Method | Output |
| ------ | ------ |
| `ToJson(filePath?)` | JSON array of listings |
| `ToMarkdownChecklist(filePath?)` | Markdown checklist, one row per indicator ID |
| `ToMarkdownTable(filePath?)` | Markdown table with a column per style |

## Execute from a listing

Look up a listing, then run it for strongly-typed results without calling the indicator's extension method directly:

```csharp
IndicatorListing indicatorListing = Catalog
  .Get("EMA", Style.Series)
  ?? throw new InvalidOperationException("Indicator 'EMA' (Series) not found.");

// run with the catalog's default parameters
IReadOnlyList<EmaResult> emaWithDefaults = indicatorListing
  .Execute<EmaResult>(bars);

// or override parameters with the fluent builder
IReadOnlyList<EmaResult> emaWithParams = indicatorListing
  .WithParamValue("lookbackPeriods", 10)
  .FromSource((IEnumerable<IBar>)bars)  // cast selects the bars overload
  .Execute<EmaResult>();
```

The fluent `ListingExecutionBuilder` supports `WithParamValue(name, value)`, `WithParams(dictionary)`, `FromSource(bars)`, `FromSource(series, parameterName?)` for chaining off another indicator's results, and `Execute<TResult>()`. Parameter values are type-checked against the listing's `IndicatorParam` metadata.

::: tip ✨ Tip: disambiguate the bars overload
Because `Bar` implements both `IBar` and `IReusable`, a `bars` collection matches both `FromSource(bars)` and the `FromSource(series, …)` chaining overload. Cast to `(IEnumerable<IBar>)` (as above) to select the bars overload. The simpler `listing.Execute<TResult>(bars)` form needs no cast.
:::

## Execute from a saved configuration

`IndicatorConfig` is a serializable description of an indicator selection and its parameter overrides — useful for persisting a user's choices (per strategy, dashboard, etc.) and replaying them later.

```csharp
// build (or deserialize) a configuration
IndicatorConfig config = new()
{
    Id = "EMA",
    Style = Style.Series,
    Parameters = new Dictionary<string, object> { ["lookbackPeriods"] = 20 }
};

// run it against bars
IReadOnlyList<EmaResult> results = config.Execute<EmaResult>(bars);
```

Use `config.ToBuilder()` to obtain a `ListingExecutionBuilder` for further fluent configuration, or `IndicatorConfig.FromBuilder(builder)` to capture an existing builder's settings back into a config for storage.

## See also

- [Bar utilities](/utilities/bars) — prepare and transform price bars
- [Result utilities](/utilities/results) — work with indicator results after calculation
- [Additional helper utilities](/utilities/helpers) — math and numerical methods for custom indicators
- [Indicators](/indicators) — the full indicator reference behind the catalog metadata
