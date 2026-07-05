# Indicator Catalog — Developer Guide

Concise reference for building, discovering, and executing indicator listings.

## Authoring listings (fluent builder)

Use `IndicatorDefinitionBuilder` inside each indicator’s `*.Catalog.cs` to define metadata once per style:

```csharp
internal static readonly IndicatorListing SeriesListing = new IndicatorDefinitionBuilder()
        .WithName("Exponential Moving Average")
        .WithId("EMA")
        .WithStyle(Style.Series)
        .WithCategory(Category.MovingAverage)
        .WithMethodName("ToEma")
        .AddParameter<int>("lookbackPeriods", "Lookback Period",
                description: "Number of periods for the EMA calculation",
                isRequired: true, defaultValue: 20, minimum: 2, maximum: 250)
        .AddResult("Ema", "EMA", ResultType.Default, isReusable: true)
        .Build();
```

Core builder methods: `.WithName`, `.WithId`, `.WithStyle`, `.WithCategory`, `.WithMethodName`, `.AddParameter<T>`, `.AddEnumParameter<TEnum>`, `.AddDateParameter`, `.AddSeriesParameter`, `.AddResult`, `.Build`.

### Multi-style rules

- Use the same ID across styles (e.g., EMA)
- Define separate listings: `SeriesListing`, `StreamListing`, `BufferListing`
- Parameter names must exactly match method signatures

## Catalog and registry access

`IndicatorCatalog.Catalog` contains all listings; `IndicatorRegistry` is the query façade.

```csharp
// all listings
IReadOnlyList<IndicatorListing> allListings = IndicatorRegistry.Get();

// lookups
IReadOnlyList<IndicatorListing> rsiListings = IndicatorRegistry.GetById("RSI"); // all styles
IndicatorListing? emaSeriesListing = IndicatorRegistry.GetByIdAndStyle("EMA", Style.Series); // single
IReadOnlyList<IndicatorListing> streamListings = IndicatorRegistry.GetByStyle(Style.Stream);
IReadOnlyList<IndicatorListing> momentumListings = IndicatorRegistry.GetByCategory(Category.Momentum);
IReadOnlyList<IndicatorListing> searchResults = IndicatorRegistry.Search("ema");
```

## Executing via the catalog

Typed, fluent execution via `CatalogExecutionBuilder`:

```csharp
IndicatorListing indicatorListing = IndicatorRegistry
        .GetByIdAndStyle("EMA", Style.Series)
        ?? throw new InvalidOperationException("Indicator 'EMA' (Series) not found.");

IReadOnlyList<EmaResult> emaResults = indicatorListing
                .From(bars)
                .WithParamValue("lookbackPeriods", 20)
                .Execute<EmaResult>();
```

Dynamic shortcuts via `CatalogUtility` (no generic type required):

```csharp
// by ID + Style (typed)
IReadOnlyList<EmaResult> emaResultsById = bars.ExecuteById<EmaResult>("EMA", Style.Series, new() { ["lookbackPeriods"] = 20 });

// from JSON config (typed)
string rsiConfigJson = "{\"id\":\"RSI\",\"style\":\"Series\",\"parameters\":{\"lookbackPeriods\":14}}";
IReadOnlyList<RsiResult> rsiResultsFromJson = bars.ExecuteFromJson<RsiResult>(rsiConfigJson);
```

Config round-trip via `IndicatorConfig`:

```csharp
IndicatorConfig emaConfig = new IndicatorConfig {
        Id = "EMA",
        Style = Style.Series,
        Parameters = new() { ["lookbackPeriods"] = 20 }
};
IReadOnlyList<EmaResult> emaResultsFromConfig = emaConfig.Execute<EmaResult>(bars);
```

## Best practices

1. Parameter names must match method signatures exactly
2. Include min/max constraints where sensible
3. Keep catalog listings in `*.Catalog.cs` files alongside the indicator
4. Provide clear parameter/result descriptions
5. Add unit tests for catalog integrity and execution
