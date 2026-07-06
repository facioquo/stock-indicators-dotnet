---
name: indicator-buffer
description: Implement BufferList incremental indicators with efficient state management. Use for IIncrementFromChain or IIncrementFromBar implementations. Covers interface selection, constructor patterns, and BufferListTestBase testing requirements.
---

# BufferList indicator development

## Interface selection

| Interface | Additional Inputs | Use Case |
| --------- | ----------------- | -------- |
| `IIncrementFromChain` | `IReusable`, `(DateTime, double)` | Chainable single-value indicators |
| `IIncrementFromBar` | (none — only IBar from base) | Requires OHLCV properties |

See [references/interface-selection.md](references/interface-selection.md) for decision tree.

## Constructor pattern

The chaining constructor parameter type depends on the interface implemented:

```csharp
// IIncrementFromChain — chaining ctor takes IReadOnlyList<IReusable>
public class EmaList : BufferList<EmaResult>, IIncrementFromChain, IEma
{
    public EmaList(int lookbackPeriods)
    {
        Ema.Validate(lookbackPeriods);
        LookbackPeriods = lookbackPeriods;
        _buffer = new Queue<double>(lookbackPeriods);
    }

    public EmaList(int lookbackPeriods, IReadOnlyList<IReusable> values)
        : this(lookbackPeriods) => Add(values);
}
```

```csharp
// IIncrementFromBar — chaining ctor takes IReadOnlyList<IBar>
public class AdxList : BufferList<AdxResult>, IIncrementFromBar, IAdx
{
    public AdxList(int lookbackPeriods)
    {
        Adx.Validate(lookbackPeriods);
        LookbackPeriods = lookbackPeriods;
    }

    public AdxList(int lookbackPeriods, IReadOnlyList<IBar> bars)
        : this(lookbackPeriods) => Add(bars);
}
```

## Buffer management

- `_buffer.Update(capacity, value)` — standard rolling buffer
- `_buffer.UpdateWithDequeue(capacity, value)` — returns dequeued value for sum adjustment

## State management

Use `Clear()` to reset all internal state:

```csharp
public override void Clear()
{
    base.Clear();
    _buffer.Clear();
    _bufferSum = 0;
}
```

## Testing constraints

- Inherit `BufferListTestBase`
- `IIncrementFromChain` → implement `ITestChainBufferList`
- `IIncrementFromBar` → implement `ITestBarBufferList`
- Series parity: `bufferResults.IsExactly(seriesResults)`

## Required implementation

- [ ] Source code: `src/**/{IndicatorName}List.cs` file exists
  - [ ] Inherits `BufferList<TResult>` and implements correct increment interface
  - [ ] Two constructors: primary + chaining via `: this(...) => Add(...);`
  - [ ] Uses `BufferListUtilities.Update()` or `UpdateWithDequeue()`
  - [ ] `Clear()` resets results and all internal buffers
- [ ] Unit testing: `tests/Library/Indicators/**/{IndicatorName}BufferListTests.cs` exists
  - [ ] Inherits `BufferListTestBase` and implements correct test interface
  - [ ] All required abstract + interface methods implemented
  - [ ] Verifies equivalence with Series results
- [ ] **Catalog registration**: Registered in `Catalog.Listings.cs`
- [ ] **Performance benchmark**: Add to `tools/performance/Perf.Buffer.cs`
- [ ] **Public documentation**: Update `docs/indicators/{IndicatorName}.md`
- [ ] **Regression tests**: Add to `tests/Library/Indicators/**/{IndicatorName}RegressionTests.cs`
- [ ] **Migration guide**: Update `docs/migration/v3.md` for notable and breaking changes from v2
