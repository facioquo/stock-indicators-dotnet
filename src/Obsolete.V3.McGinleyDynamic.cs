using System.Diagnostics.CodeAnalysis;

namespace FacioQuo.Stock.Indicators;
#pragma warning disable CS1591, IND001, RCS1163

// OBSOLETE IN v3.1.0 (issue #2138, Option A)
//
// MgDynamic unified its Hub/List/Result/Interface family under the `Dynamic`
// name; the series entry-point class itself is renamed `MgDynamic` -> `Dynamic`.
// Per the deprecate-then-remove pattern (issue #2146), the extension-syntax
// members below drop their `this` modifier so `list.ToDynamic(..)` binds
// unambiguously to the new `Dynamic` class, while explicit static calls
// (`MgDynamic.ToDynamic(list, ..)`) keep compiling with a deprecation warning.
// Removal is tracked as the breaking remainder in a future major version.
public static class MgDynamic
{
    [ExcludeFromCodeCoverage]
    [Obsolete("Rename `MgDynamic` to `Dynamic`.", false)]
    public static IReadOnlyList<DynamicResult> ToDynamic(
        IReadOnlyList<IReusable> source,
        int lookbackPeriods,
        double kFactor = 0.6)
        => source.ToDynamic(lookbackPeriods, kFactor);

    [ExcludeFromCodeCoverage]
    [Obsolete("Rename `MgDynamic` to `Dynamic`.", false)]
    public static DynamicHub ToDynamicHub(
        IChainProvider<IReusable> chainProvider,
        int lookbackPeriods,
        double kFactor = 0.6)
        => chainProvider.ToDynamicHub(lookbackPeriods, kFactor);

    [ExcludeFromCodeCoverage]
    [Obsolete("Rename `MgDynamic` to `Dynamic`.", false)]
    public static DynamicList ToDynamicList(
        IReadOnlyList<IReusable> source,
        int lookbackPeriods,
        double kFactor = 0.6)
        => source.ToDynamicList(lookbackPeriods, kFactor);

    [ExcludeFromCodeCoverage]
    [Obsolete("Rename `MgDynamic` to `Dynamic`.", false)]
    public static double Increment(
        int lookbackPeriods,
        double kFactor,
        double newVal,
        double prevDyn)
        => Dynamic.Increment(lookbackPeriods, kFactor, newVal, prevDyn);
}
