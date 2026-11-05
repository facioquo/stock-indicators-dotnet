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

| Property | Description |
| ----- | ----------- |
| `Uiid`<br>`string` | Unique indicator ID (e.g. `"EMA"`) |
| `Name`<br>`string` | Display name |
| `Style`<br>`Style` | `Series`, `Buffer`, or `Stream` |
| `Category`<br>`Category` | Indicator category (see below) |
| `Parameters`<br>`IReadOnlyList<IndicatorParam>?` | Input parameters (`null` when there are none) |
| `Results`<br>`IReadOnlyList<IndicatorResult>` | Output fields the indicator produces |
| `Panes`<br>`IReadOnlyList<IndicatorPane>?` | Chart panes with a fixed axis range or reference lines (`null` when none) |
| `ResultRecordType`<br>`string?` | Name of the result record the method returns (e.g. `"EmaResult"`), the same for all three styles — the record type, not the method's literal return type (`EmaList` for Buffer, `EmaHub` for Stream). `null` when `MethodName` is unset. Derived when the catalog builds the listing, so a `with` copy that changes `MethodName` keeps the original value |
| `MethodName`<br>`string?` | Method name, for automation use cases |
| `LegendTemplate`<br>`string` | Legend template for charting |

`GetMethodOverloads()` returns the public static overloads of `MethodName` that return the listing's `Style`, as `IReadOnlyList<MethodInfo>`, for a caller that reads their parameters or invokes them directly. One name can serve two styles, such as IndyMath's `Minus`, which names both a Series method and a stream hub method; a listing gets only its own style's overloads, or every overload when none returns its style. The list is empty when `MethodName` is unset or no catalog listing names the method. The methods survive trimming and NativeAOT publishing, so no assembly scan is needed.

`IndicatorParam` (each input parameter):

| Property | Description |
| ----- | ----------- |
| `DisplayName`<br>`string` | Human-friendly label |
| `ParameterName`<br>`string` | Method parameter name (use this with `WithParamValue`) |
| `DataType`<br>`string` | Parameter type name |
| `Description`<br>`string?` | Optional description |
| `IsRequired`<br>`bool` | Whether the caller must supply the parameter. `false` means omitting it yields `DefaultValue` |
| `DefaultValue`<br>`object?` | Default value, if any |
| `Minimum` / `Maximum`<br>`double?` | Recommended bounds, if any |
| `EnumOptions`<br>`Dictionary<int, string>?` | Allowed values for enum parameters (`null` otherwise) |

`IndicatorResult` (each output field):

| Property | Description |
| ----- | ----------- |
| `DisplayName`<br>`string` | Human-friendly label |
| `DataName`<br>`string` | Result property name (e.g. `"Ema"`) |
| `DataType`<br>`ResultType` | Charting/display hint |
| `IsReusable`<br>`bool` | `true` for the field exposed as the chainable `IReusable.Value` |
| `ChartPane`<br>`string?` | Chart pane the result draws in: `"Price"` (`IndicatorResult.PricePane`) for the price axis, another name for a separate pane, `null` when not charted |

Results with the same `ChartPane` share units, so group a listing's results by it and draw each group as one chart. Bollinger Bands, for example, puts its bands on `"Price"` and `PercentB`, `ZScore` and `Width` each in a pane of its own. A `null` pane marks a flag or pattern code, such as a PSAR reversal, which has no axis to draw on.

`IndicatorPane` (each pane with a fixed range or reference lines):

| Property | Description |
| ----- | ----------- |
| `Name`<br>`string` | Pane name, matching the `ChartPane` of the results it draws |
| `Minimum` / `Maximum`<br>`double?` | Fixed axis bounds; `null` means autoscale |
| `Thresholds`<br>`IReadOnlyList<double>?` | Reference line values, such as overbought and oversold levels, in drawing order |

Nine indicators list a pane today: RSI, Stochastic, Stochastic RSI, MFI, Williams %R, Ultimate Oscillator, ConnorsRSI, ADX and Aroon. RSI, for example, draws on 0 to 100 with lines at 70 and 30. A pane that is not listed has no data here, which is not the same as autoscaling with no reference lines. Colours and line styles are left to the chart.

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

A Stream listing runs from bars the same way: the bars feed a `BarHub`, and `Execute` returns the indicator hub's results.

The fluent `ListingExecutionBuilder` supports `WithParamValue(name, value)`, `WithParams(dictionary)`, `WithoutParam(name)`, `FromSource(bars)`, `FromSource(series, parameterName?)` for chaining off another indicator's results, and `Execute<TResult>()`. Parameter values are type-checked against the listing's `IndicatorParam` metadata. A series value must be assignable to `IReadOnlyList<IReusable>`, so its elements are reference types; a list of `IReusable` structs is rejected.

### Leave a parameter out

A few indicators have an overload that means "this argument was not given" rather than "this argument took its default". `ToPrs(sourceEval, sourceBase)` computes no `PrsPercent` at all — no value in the parameter's advertised range (`Minimum: 1`, `Maximum: 250`) produces that, and the only override that does is the `int.MinValue` sentinel the overload forwards internally, which the catalog gives you no way to discover. `WithoutParam` drops the argument instead of supplying one:

```csharp
IndicatorListing prs = Catalog
  .Get("PRS", Style.Series)
  ?? throw new InvalidOperationException("Indicator 'PRS' (Series) not found.");

// declared default: lookbackPeriods = 20, PrsPercent computed
IReadOnlyList<PrsResult> withPercent = prs
  .WithParamValue("sourceBase", benchmarkBars)
  .FromSource((IEnumerable<IBar>)bars)
  .Execute<PrsResult>();

// no lookback argument at all: PrsPercent is null throughout
IReadOnlyList<PrsResult> ratioOnly = prs
  .WithParamValue("sourceBase", benchmarkBars)
  .WithoutParam("lookbackPeriods")
  .FromSource((IEnumerable<IBar>)bars)
  .Execute<PrsResult>();
```

A shorter overload is selected where one exists; where none does, the method's own default for that parameter applies instead. Arguments bind positionally, so only a trailing run of parameters can be dropped — omitting one while a later one is still supplied is rejected rather than silently shifting arguments into the wrong slots. Series parameters supply the indicator's data rather than a setting, so they cannot be omitted.

Omissions are part of a configuration, not a transient builder detail: `IndicatorConfig` round-trips them through `OmittedParameters` alongside `Parameters`, so a saved configuration reloads as the same indicator.

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

## Trimming and NativeAOT

The package is marked trimmable, so a trimmed app keeps only the library members it uses. Catalog lookup and execution work in trimmed and NativeAOT apps; `ToJson` does not (see below). A listing's `MethodName` must name a catalog indicator method: a listing you build yourself, or a `with` copy, cannot name any other library method.

::: warning Reflection-based helpers
`ToJson` serializes with reflection, so it carries `[RequiresUnreferencedCode]` and `[RequiresDynamicCode]` and raises IL2026 and IL3050 in a trimmed or NativeAOT build; export the catalog from an untrimmed app instead. `ToStringOut` and `ToConsole` require the public properties of `T` to be preserved, which an unannotated generic caller sees as IL2091; annotate the caller's own type parameter with `[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)]`, or call them with a concrete result type. In a NativeAOT app they print no property descriptions: they read descriptions from the XML documentation file beside the assembly, and a native executable has no assembly file to sit beside.
:::

## See also

- [Bar utilities](/utilities/bars) — prepare and transform price bars
- [Result utilities](/utilities/results) — work with indicator results after calculation
- [Additional helper utilities](/utilities/helpers) — math and numerical methods for custom indicators
- [Indicators](/indicators) — the full indicator reference behind the catalog metadata
