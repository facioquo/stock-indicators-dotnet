---
name: indicator-buffer
description: Implement BufferList incremental indicators (`{Name}List`, a `BufferList<TResult>` subclass) — increment interface choice, constructor and `To{Name}List()` conventions, rolling-buffer helpers, pruning and `Clear()` state, and the BufferList test interfaces. Use when creating or editing a `src/Indicators/**/{Name}List.cs` file or its `{Name}BufferListTests.cs`, when a BufferList test fails Series parity or pruning, or when a Buffer benchmark shows a list is slow.
---

# BufferList indicator development

A BufferList must produce results identical to the Series output for the same input. The indicator-series skill owns the full per-indicator file set and completion checklist; this skill covers only the `{Name}List.cs` file and its tests.

## Increment interface

Pick the interface from the Series input type. Both are `internal` and live in `src/Common/BufferLists/`.

| Series input | Interface | `Add` overloads to implement |
| ------------ | --------- | ---------------------------- |
| `IReadOnlyList<IReusable>` | `IIncrementFromChain` | `Add(DateTime timestamp, double value)`, `Add(IReusable value)`, `Add(IReadOnlyList<IReusable> values)` |
| `IReadOnlyList<IBar>` | `IIncrementFromBar` | `Add(IBar bar)`, `Add(IReadOnlyList<IBar> bars)` |

- Route every chain overload through `Add(DateTime, double)` so the calculation exists once.
- Pass `value.Value` from the `IReusable` overloads, or `value.Hl2OrValue()` where the Series defaults bars to HL2 (Alligator and Awesome, for example).
- An `IIncrementFromBar` list has no `IReusable` overloads; it cannot chain from another indicator.

## Class shape

`src/Indicators/e-j/Ema/EmaList.cs` (chain) and `src/Indicators/a-b/Adx/AdxList.cs` (bar) are the reference implementations.

```csharp
public class EmaList : BufferList<EmaResult>, IIncrementFromChain, IEma
{
    private readonly Queue<double> _buffer;

    public EmaList(int lookbackPeriods)
    {
        Ema.Validate(lookbackPeriods);
        LookbackPeriods = lookbackPeriods;
        _buffer = new Queue<double>(lookbackPeriods);
        Name = $"EMA({lookbackPeriods})";
    }

    public EmaList(int lookbackPeriods, IReadOnlyList<IReusable> values)
        : this(lookbackPeriods) => Add(values);
}

public static partial class Ema
{
    public static EmaList ToEmaList(
        this IReadOnlyList<IReusable> source,
        int lookbackPeriods)
        => new(lookbackPeriods) { source };
}
```

- The primary constructor validates with the Series `Validate(...)`, stores parameters as `I{Name}` properties, allocates buffers, and sets `Name`.
- The chaining constructor takes `IReadOnlyList<IReusable>` or `IReadOnlyList<IBar>` to match the interface and delegates with `: this(...) => Add(...)`.
- The `To{Name}List()` extension sits in the same file and uses a collection initializer.
- Append results with `AddInternal(...)`, which also enforces `MaxListSize`; revise an earlier result with `UpdateInternal(index, ...)`.

## Buffers and state

- Roll a fixed window with `_buffer.Update(capacity, value)` from `BufferListUtilities`.
- Use `_buffer.UpdateWithDequeue(capacity, value)` to maintain a running sum. For a value-type `T` it returns `default(T)` (0), not `null`, when nothing was dequeued, so gate the subtraction on whether the buffer was full before the call.
- Override `Clear()` to call `base.Clear()` and reset every buffer, running sum, and cached prior value.
- When the list keeps history outside a capacity-bounded `Queue<T>` (a `List<T>` cache, for example), override `PruneList()` so that history shrinks with the list.

## Tests

`{Name}BufferListTests` inherits `BufferListTestBase` and implements `ITestChainBufferList` for `IIncrementFromChain` or `ITestBarBufferList` for `IIncrementFromBar`, plus `ITestCustomBufferListCache` for a custom cache. Every test that produces results asserts `IsExactly` against the Series output. The testing-standards skill lists the required methods.

## Do not do these

- Do not add `Add(IBar)` overloads to an `IIncrementFromChain` list or `Add(IReusable)` overloads to an `IIncrementFromBar` list.
- Do not reimplement the Series formula differently; reuse its `Increment(...)` kernel where one exists.
- Do not grow an internal collection without bound; every buffer is capacity-limited or pruned with the list.
