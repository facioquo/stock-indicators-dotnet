# Compound hub pattern

## Overview

Compound hubs are StreamHub indicators that depend on another hub's output as their input. Examples: StochRSI (requires RSI), Gator (requires Alligator).

**Problem**: Without proper construction, users could inadvertently create duplicate internal hubs, wasting computation.

**Solution**: Two-constructor pattern that allows reusing an existing hub OR creating one internally.

## Constructor pattern

### Required constructors

```csharp
public class StochRsiHub : ChainHub<IReusable, StochRsiResult>
{
    // Constructor 1: Create internal hub from provider
    internal StochRsiHub(
        IChainProvider<IReusable> provider,
        int rsiPeriods = 14,
        int stochPeriods = 14,
        int signalPeriods = 3,
        int smoothPeriods = 1)
        : this(
            provider.ToRsiHub(rsiPeriods),  // Create internal hub
            stochPeriods,
            signalPeriods,
            smoothPeriods)  // Delegate to constructor 2
    { }

    // Constructor 2: Accept existing hub (avoids duplicate creation)
    internal StochRsiHub(
        RsiHub rsiHub,
        int stochPeriods = 14,
        int signalPeriods = 3,
        int smoothPeriods = 1)
        : base(rsiHub)  // Pass hub to base (NOT original provider!)
    {
        ArgumentNullException.ThrowIfNull(rsiHub);
        StochRsi.Validate(rsiHub.LookbackPeriods, stochPeriods, signalPeriods, smoothPeriods);

        RsiPeriods = rsiHub.LookbackPeriods;
        StochPeriods = stochPeriods;
        SignalPeriods = signalPeriods;
        SmoothPeriods = smoothPeriods;

        Name = $"STOCH-RSI({RsiPeriods},{stochPeriods},{signalPeriods},{smoothPeriods})";

        // Initialize local state for processing hub results
        _rsiMaxWindow = new RollingWindowMax<double>(stochPeriods);
        _rsiMinWindow = new RollingWindowMin<double>(stochPeriods);
        kBuffer = new Queue<double>(smoothPeriods);
        signalBuffer = new Queue<double>(signalPeriods);

        Reinitialize();
    }
}
```

### Key principles

1. **Delegate constructors** - Constructor 1 creates internal hub and delegates to constructor 2
2. **Pass hub to base** - Constructor 2 passes the hub (not original provider) to base class
3. **Validate in constructor 2** - Only the hub-accepting constructor does validation and initialization
4. **Internal state only** - Only maintain state for processing the hub's results, not for the hub's calculations

## Extension methods

Provide both overloads with clear documentation:

```csharp
public static partial class StochRsi
{
    /// <summary>
    /// Converts the chain provider to a Stochastic RSI hub.
    /// </summary>
    public static StochRsiHub ToStochRsiHub(
        this IChainProvider<IReusable> chainProvider,
        int rsiPeriods = 14,
        int stochPeriods = 14,
        int signalPeriods = 3,
        int smoothPeriods = 1)
        => new(chainProvider, rsiPeriods, stochPeriods, signalPeriods, smoothPeriods);

    /// <summary>
    /// Creates a new Stochastic RSI hub, using RSI values from an existing RSI hub.
    /// </summary>
    /// <remarks>
    /// This extension overrides and enables a chain that specifically
    /// reuses the existing <see cref="RsiHub"/> as its internal construction.
    /// <para>IMPORTANT: This is not a normal chaining approach.</para>
    /// Do not use this interface if you want to instead want a StochRSI of an RSI hub.</remarks>
    public static StochRsiHub ToStochRsiHub(
        this RsiHub rsiHub,
        int stochPeriods = 14,
        int signalPeriods = 3,
        int smoothPeriods = 1)
        => new(rsiHub, stochPeriods, signalPeriods, smoothPeriods);
}
```

**Critical documentation**: The hub-accepting extension MUST document that it's non-standard chaining to prevent confusion.

## RollbackState considerations

### When NOT to override

Most compound hubs **do not need RollbackState override** because:

1. The internal hub manages its own state and RollbackState
2. The compound hub only processes results from the internal hub
3. No additional stateful fields beyond result processing buffers

**Example**: Gator hub has no stateful fields (simple transformation of Alligator results):

```csharp
public class GatorHub : StreamHub<AlligatorResult, GatorResult>
{
    // No stateful fields beyond base class
    // No RollbackState override needed
}
```

### When to override

Override RollbackState only when maintaining **additional state** beyond the internal hub's results:

**Example**: StochRSI maintains rolling windows and buffers for processing RSI values:

```csharp
private readonly CircularDoubleBuffer _rsiBuffer;
private readonly Queue<double> kBuffer;
private readonly Queue<double> signalBuffer;

// The base class computes restoreIndex (last ProviderCache index to preserve,
// or -1 to reset all state) before calling this override.
protected override void RollbackState(int restoreIndex)
{
    // Reset compound state
    _rsiBuffer.Clear();
    kBuffer.Clear();
    signalBuffer.Clear();

    if (restoreIndex < 0)
    {
        return;
    }

    // Replay the internal hub's cached results up to and including restoreIndex
    for (int i = 0; i <= restoreIndex; i++)
    {
        double rsiValue = ProviderCache[i].Value;  // ProviderCache holds RSI results
        if (!double.IsNaN(rsiValue))
        {
            _ = UpdateOscillatorState(rsiValue);  // Rebuild internal state
        }
    }
}
```

**Key pattern**: Replay the compound hub's processing logic using cached results from the internal hub (`ProviderCache[i].Value`).

## Anti-patterns

### Creating internal hub in ToIndicator (FORBIDDEN)

```csharp
// WRONG - Creates new hub on every tick
protected override (GatorResult result, int index)
    ToIndicator(IBar item, int? indexHint)
{
    var alligator = item.ToAlligatorHub();  // FORBIDDEN - O(n²) disaster
    var alligatorResult = alligator.GetNext(item);
    // ... process result
}
```

**Correct**: Create internal hub once in constructor, reuse throughout lifetime.

### Passing original provider to base (FORBIDDEN)

```csharp
// WRONG - Base doesn't receive the internal hub's results
internal StochRsiHub(RsiHub rsiHub, int stochPeriods)
    : base(/* original provider */)  // WRONG - loses hub connection
{
    // This breaks the chain - ToIndicator won't receive RSI results
}
```

**Correct**: Pass the internal hub to base: `: base(rsiHub)`

### Missing hub-accepting extension

```csharp
// INCOMPLETE - Only provides provider-based extension
public static StochRsiHub ToStochRsiHub(
    this IChainProvider<IReusable> chainProvider,
    int rsiPeriods = 14,
    int stochPeriods = 14)
    => new(chainProvider, rsiPeriods, stochPeriods);

// Missing: Extension accepting RsiHub directly
```

**Correct**: Provide both extensions (provider and hub-accepting).

## Testing compound hubs

Follow standard StreamHub testing requirements:

1. Inherit from `StreamHubTestBase`
2. Implement `ITestChainObserver` (most common for compound hubs)
3. Test rollback validation if RollbackState is overridden
4. Verify Series parity with the compound indicator

**Example**: `tests/Library/Indicators/r-s/StochRsi/StochRsiHubTests.cs`
