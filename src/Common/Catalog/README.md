# Indicator catalog

Developer guide for authoring indicator catalog listings.

## Authoring listings

Each indicator defines its listings in `{Name}.Catalog.cs`: a `CommonListing` with the shared name, ID, category, parameters, and results, then one listing per style built from it.

```csharp
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
```

`StreamListing` and `BufferListing` follow the same shape with their own style and method name. Register every listing in `PopulateCatalog()` in `Catalog.Listings.cs`.

### Multi-style rules

- Every style shares the ID and metadata of the `CommonListing`.
- Parameter names exactly match the method signature.
- A result's `dataName` uses `nameof(TResult.Property)` so a renamed result property fails the build.
- `isRequired: false` promises a caller may omit the argument **and get `defaultValue`**. That holds in exactly two shapes:
  - the parameter carries a C# default equal to `defaultValue`, or
  - the listing declares no `defaultValue` and so promises nothing (VWAP's `startDate`, omittable via `ToVwap(bars)`).
- When the argument can only be dropped by choosing a shorter overload that behaves differently — `ToPrs(sourceEval, sourceBase)` computes no `PrsPercent` — mark it `isRequired: true` and reach that overload with `WithoutParam(name)`, so a catalog-driven caller never silently gets a different indicator. `EveryParameterIsRequiredMatchesCallability` enforces these rules.

## Querying and executing

The public API — `Catalog.Get(...)`, `Catalog.Search(...)`, the `ListingExecutionBuilder` fluent execution, `IndicatorConfig`, and the JSON and Markdown exports — is documented for library users at [docs/utilities/catalog.md](../../../docs/utilities/catalog.md).

## Tests

`tests/Library/Common/Catalog/` checks listing integrity, binding to real method signatures, executability, and per-style counts (`Catalog.Metrics.Tests.cs`). Every new listing must pass them.
