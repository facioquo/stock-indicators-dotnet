# Required test methods and precision constants

Use this when writing a new test class, when a test class fails to compile for a missing member, or when choosing a `Money*` tolerance. Every method below carries `[TestMethod]`; abstract methods are declared `public override void`.

## Series: `StaticSeriesTestBase`

- `DefaultParameters_ReturnsExpectedResults()` — result count, non-null count, and `Money*` spot values from `{Name}.Calc.xlsx`
- `BadBars_DoesNotFail()` — runs on `BadBars` without throwing
- `NoBars_ReturnsEmpty()` — `Nobars` input yields an empty list

## BufferList: `BufferListTestBase`

Abstract on the base class:

- `PruneList_OverMaxListSize_AutoAdjustsListAndBuffers()`
- `Clear_WithState_ResetsState()`

`ITestBarBufferList` (for `IIncrementFromBar`):

- `AddBar_IncrementsResults()`
- `AddBarsBatch_IncrementsResults()`
- `BarsCtor_OnInstantiation_IncrementsResults()`

`ITestChainBufferList` (for `IIncrementFromChain`) inherits `ITestBarBufferList` and adds:

- `AddReusableItem_IncrementsResults()`
- `AddReusableItemBatch_IncrementsResults()`
- `AddDateAndValue_IncrementsResults()`

`ITestCustomBufferListCache` (the list keeps history outside a capacity-bounded `Queue<T>`):

- `CustomBuffer_OverMaxListSize_AutoAdjustsListAndBuffers()`

## StreamHub: `StreamHubTestBase`

Abstract on the base class:

- `ToStringOverride_ReturnsExpectedName()`

Implement exactly one observer interface:

- `ITestBarObserver` — `BarObserver_WithWarmupLateArrivalAndRemoval_MatchesSeriesExactly()` and `WithCachePruning_MatchesSeriesExactly()`
- `ITestChainObserver` (a hub that accepts a chain provider) — inherits both `ITestBarObserver` methods and adds `ChainObserver_ChainedProvider_MatchesSeriesExactly()`

Add `ITestChainProvider` when the hub's results feed other hubs:

- `ChainProvider_MatchesSeriesExactly()`

`WithCachePruning_MatchesSeriesExactly()` compares against the last `MaxCacheSize` results of the full Series run, not a Series run over only the cached bars, because warmup differs.

## Regression: `RegressionTestBase<TResult>`

Pass the baseline filename to the base constructor and call each style with the catalog default parameters:

```csharp
[TestClass, TestCategory("Regression")]
public class EmaRegressionTests : RegressionTestBase<EmaResult>
{
    public EmaRegressionTests() : base("ema.standard.json") { }

    [TestMethod]
    public override void Series_AgainstBaseline_MatchesExactly() => Bars.ToEma(20).IsExactly(Expected);

    [TestMethod]
    public override void Buffer_AgainstBaseline_MatchesExactly() => Bars.ToEmaList(20).IsExactly(Expected);

    [TestMethod]
    public override void Stream_AgainstBaseline_MatchesExactly() => BarHub.ToEmaHub(20).Results.IsExactly(Expected);
}
```

## Precision constants

Defined on `TestBaseWithPrecision` for `BeApproximately()` against manually calculated values only. Each matches a `.Round(n)` expectation:

| Constant | Tolerance | Rounding |
| -------- | --------- | -------- |
| `Money2` | 0.005 | 2 places |
| `Money3` | 0.0005 | 3 places |
| `Money4` | 0.00005 | 4 places; the usual spot-check tolerance |
| `Money5` | 0.000005 | 5 places |
| `Money6` | 0.0000005 | 6 places |
| `Money8` | 0.000000005 | 8 places |
| `Money10` | 0.00000000005 | 10 places |
| `Money12` | 0.0000000000005 | 12 places |

Use the tightest constant the reference spreadsheet's precision supports. Style parity and regression baselines use `IsExactly`, never a tolerance.
