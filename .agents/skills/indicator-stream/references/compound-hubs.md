# Compound hubs

Use this when a hub's input is another indicator hub's output: `StochRsiHub` over `RsiHub`, `StcHub` over `MacdHub`, `EpmaHub` over `SlopeHub`, `GatorHub` over `AlligatorHub`.

The inner hub becomes the compound hub's provider, so `ProviderCache` holds the inner hub's results. Derive from `ChainHub<TInner, TResult>` or `StreamHub<TInner, TResult>` by the same output rule as any hub. A `ChainHub<IReusable, …>` base also accepts an inner hub whose result implements `IReusable`, because the provider interfaces are covariant (`StochRsiHub`).

## Constructors

Provide two `internal` constructors. The provider constructor builds the inner hub and delegates; the hub constructor passes the inner hub to `base`, validates, and initializes (`StochRsiHub`):

```csharp
internal StochRsiHub(
    IChainProvider<IReusable> provider,
    int rsiPeriods = 14,
    int stochPeriods = 14,
    int signalPeriods = 3,
    int smoothPeriods = 1)
    : this(
        provider.ToRsiHub(rsiPeriods),
        stochPeriods,
        signalPeriods,
        smoothPeriods)
{ }

internal StochRsiHub(
    RsiHub rsiHub,
    int stochPeriods = 14,
    int signalPeriods = 3,
    int smoothPeriods = 1)
    : base(rsiHub)
{
    ArgumentNullException.ThrowIfNull(rsiHub);
    StochRsi.Validate(rsiHub.LookbackPeriods, stochPeriods, signalPeriods, smoothPeriods);

    RsiPeriods = rsiHub.LookbackPeriods;
    // ... remaining parameters, Name, buffers, ValidateCacheSize
    Reinitialize();
}
```

Read inner-hub parameters (`rsiHub.LookbackPeriods`, `macdHub.FastPeriods`) from the hub rather than taking them again.

## Extension methods

Provide the standard `To{Name}Hub(this IChainProvider<IReusable> …)` overload, plus a hub-accepting overload (`this RsiHub rsiHub`) that reuses an existing inner hub instead of building a duplicate. Document the hub-accepting overload in its `<remarks>` as non-standard chaining: it reuses the hub as the inner hub rather than treating it as an ordinary chained input. `StochRsi`, `Stc`, and `Gator` show the wording.

## RollbackState

The inner hub rolls back its own state. Override in the compound hub only for state it keeps itself; `GatorHub` keeps none and has no override. `StochRsiHub` clears its window and smoothing buffers, then replays its own processing over the inner hub's cached results:

```csharp
protected override void RollbackState(int restoreIndex)
{
    _rsiBuffer.Clear();
    kBuffer.Clear();
    signalBuffer.Clear();

    if (restoreIndex < 0)
    {
        return;
    }

    for (int i = 0; i <= restoreIndex; i++)
    {
        double rsiValue = ProviderCache[i].Value;  // inner RSI results
        if (!double.IsNaN(rsiValue))
        {
            _ = UpdateOscillatorState(rsiValue);
        }
    }
}
```

Route both `ToIndicator` and the replay through one shared update method (`UpdateOscillatorState`) so they cannot diverge.

## Do not do these

- Do not create the inner hub inside `ToIndicator`. Build it once, in the provider constructor.
- Do not pass the original provider to `base` in the hub constructor. The compound hub then never receives the inner hub's results.
- Do not keep state that duplicates the inner hub's calculation. Keep only what processing its results requires.
