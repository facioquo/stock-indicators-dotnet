#nullable enable
using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Observables;

/// <summary>
/// Generic rollback-equivalence contract: for every Style.Stream listing in
/// the catalog, a hub that has been rolled back via Rebuild(timestamp) and
/// re-played from its provider must produce the same Results as a
/// freshly-instantiated hub fed the same bars once. This pins the
/// RollbackState(int) contract across all 50+ overrides.
/// </summary>
/// <remarks>
/// Scope: catalog-registered Style.Stream listings only. Hubs that override
/// RollbackState but are NOT in the catalog (e.g. BarAggregatorHub,
/// TradeTickAggregatorHub) are out of scope and covered separately.
/// This contract exercises Rebuild(timestamp) in isolation; subsequent
/// live-add behaviour after rebuild is covered by per-indicator
/// late-arrival tests inside each *.StreamHub.Tests.cs file.
/// </remarks>
[TestClass]
public class StreamHubRollbackContractTests : TestBase
{
    private const int PrefixLength = 300;
    private const int TotalLength = 500;

    [TestMethod]
    public void AllStreamHubs_AfterRebuild_MatchFreshStream()
    {
        IReadOnlyList<Bar> bars = Bars.Take(TotalLength).ToList();
        bars.Should().HaveCount(TotalLength,
            "the contract relies on a 500-bar fixture; default test data must supply at least that many");

        DateTime rollbackTimestamp = bars[PrefixLength].Timestamp;

        IReadOnlyList<IndicatorListing> streamListings = Catalog.Get(Style.Stream);
        streamListings.Should().NotBeEmpty("the catalog must register Stream-style listings");

        List<string> failures = [];
        List<string> skipped = [];

        foreach (IndicatorListing listing in streamListings)
        {
            ContractOutcome outcome = RunContract(listing, bars, rollbackTimestamp);
            string entry = $"{listing.Uiid} ({listing.MethodName}): {outcome.Detail}";

            switch (outcome.Verdict)
            {
                case Verdict.Pass:
                    break;
                case Verdict.Skip:
                    skipped.Add(entry);
                    break;
                default:
                    failures.Add(entry);
                    break;
            }
        }

        skipped.Should().BeEmpty(
            "every Style.Stream listing must be exercised by this contract; "
            + $"{skipped.Count} of {streamListings.Count} listing(s) were skipped:\n  - "
            + string.Join("\n  - ", skipped));

        failures.Should().BeEmpty(
            "every Style.Stream hub must restore equivalent state via Rebuild(timestamp); "
            + $"{failures.Count} of {streamListings.Count} listing(s) failed:\n  - "
            + string.Join("\n  - ", failures));
    }

    private enum Verdict { Pass, Skip, Fail }

    private readonly record struct ContractOutcome(Verdict Verdict, string Detail);

    private static ContractOutcome RunContract(
        IndicatorListing listing,
        IReadOnlyList<Bar> bars,
        DateTime rollbackTimestamp)
    {
        try
        {
            string? methodName = listing.MethodName;
            if (string.IsNullOrWhiteSpace(methodName))
            {
                return new ContractOutcome(Verdict.Skip, "no MethodName in catalog listing");
            }

            (MethodInfo? factory, string? skipReason) = FindFactory(methodName, listing);
            if (factory == null)
            {
                return new ContractOutcome(Verdict.Skip, skipReason ?? "no matching factory found");
            }

            (BarHub rebuildSource, object rebuildHub) = BuildHub(factory, listing);
            (BarHub freshSource, object freshHub) = BuildHub(factory, listing);

            rebuildSource.Add(bars);
            freshSource.Add(bars);

            InvokeRebuild(rebuildHub, rollbackTimestamp);

            IReadOnlyList<ISeries> rebuildResults = GetResults(rebuildHub);
            IReadOnlyList<ISeries> freshResults = GetResults(freshHub);

            if (rebuildResults.Count != freshResults.Count)
            {
                return new ContractOutcome(
                    Verdict.Fail,
                    $"result count diverged after Rebuild: rebuild={rebuildResults.Count}, fresh={freshResults.Count}");
            }

            for (int i = 0; i < rebuildResults.Count; i++)
            {
                if (!Equals(rebuildResults[i], freshResults[i]))
                {
                    return new ContractOutcome(
                        Verdict.Fail,
                        $"results differ at index {i} (timestamp {rebuildResults[i].Timestamp:O})\n"
                        + $"      rebuild = {rebuildResults[i]}\n"
                        + $"      fresh   = {freshResults[i]}");
                }
            }

            return new ContractOutcome(Verdict.Pass, "ok");
        }
        catch (TargetInvocationException tie)
        {
            Exception? inner = tie.InnerException;
            return new ContractOutcome(
                Verdict.Fail,
                $"invocation threw {inner?.GetType().Name}: {inner?.Message}");
        }
        catch (Exception ex) when (ex is not AssertFailedException)
        {
            return new ContractOutcome(Verdict.Fail, $"threw {ex.GetType().Name}: {ex.Message}");
        }
    }

    private static (MethodInfo?, string?) FindFactory(string methodName, IndicatorListing listing)
    {
        Assembly assembly = typeof(Ema).Assembly;
        MethodInfo[] candidates = assembly.GetTypes()
            .Where(static t => t.IsClass && t.IsAbstract && t.IsSealed)
            .SelectMany(static t => t.GetMethods(BindingFlags.Public | BindingFlags.Static))
            .Where(m => string.Equals(m.Name, methodName, StringComparison.Ordinal)
                     && m.IsDefined(typeof(ExtensionAttribute), inherit: false))
            .ToArray();

        if (candidates.Length == 0)
        {
            return (null, $"no public static extension method named {methodName}");
        }

        // Keep only overloads whose first parameter accepts a BarHub
        // (i.e. IChainProvider<IReusable>, IBarProvider<IBar>, or IStreamObservable<IBar>)
        MethodInfo[] viable = candidates
            .Where(static m => FirstParameterAccepts<BarHub>(m))
            .ToArray();

        if (viable.Length == 0)
        {
            return (null, "no overload accepts a BarHub as its provider source");
        }

        HashSet<string> listingParamNames = new(
            listing.Parameters?.Select(static p => p.ParameterName) ?? [],
            StringComparer.OrdinalIgnoreCase);

        // Pick the overload whose parameter names best align with the listing's
        // (highest match count; tiebreaker: fewer total parameters).
        MethodInfo best = viable
            .OrderByDescending(m => m.GetParameters()
                .Skip(1)
                .Count(p => p.Name != null && listingParamNames.Contains(p.Name)))
            .ThenBy(static m => m.GetParameters().Length)
            .First();

        return (best, null);
    }

    private static (BarHub source, object hub) BuildHub(MethodInfo factory, IndicatorListing listing)
    {
        BarHub source = new();
        ParameterInfo[] methodParams = factory.GetParameters();
        object?[] args = new object?[methodParams.Length];
        args[0] = source;

        IReadOnlyList<IndicatorParam>? listParams = listing.Parameters;

        for (int i = 1; i < methodParams.Length; i++)
        {
            ParameterInfo p = methodParams[i];
            IndicatorParam? listingParam = listParams?
                .FirstOrDefault(lp => string.Equals(lp.ParameterName, p.Name, StringComparison.OrdinalIgnoreCase));

            object? rawDefault = listingParam?.DefaultValue;
            if (rawDefault == null)
            {
                if (p.HasDefaultValue)
                {
                    args[i] = p.DefaultValue;
                    continue;
                }

                throw new InvalidOperationException(
                    $"Listing {listing.Uiid} parameter '{p.Name}' has no DefaultValue and method exposes no default");
            }

            Type targetType = Nullable.GetUnderlyingType(p.ParameterType) ?? p.ParameterType;
            args[i] = targetType.IsEnum
                ? Enum.ToObject(targetType, rawDefault)
                : Convert.ChangeType(rawDefault, targetType, CultureInfo.InvariantCulture);
        }

        object hub = factory.Invoke(null, args)
            ?? throw new InvalidOperationException($"Factory {factory.Name} returned null");
        return (source, hub);
    }

    private static void InvokeRebuild(object hub, DateTime fromTimestamp)
    {
        MethodInfo rebuild = hub.GetType().GetMethod(
            "Rebuild",
            BindingFlags.Public | BindingFlags.Instance,
            binder: null,
            types: [typeof(DateTime)],
            modifiers: null)
        ?? throw new InvalidOperationException($"{hub.GetType().Name} has no Rebuild(DateTime)");

        rebuild.Invoke(hub, [fromTimestamp]);
    }

    private static bool FirstParameterAccepts<T>(MethodInfo method)
    {
        ParameterInfo[] parameters = method.GetParameters();
        return parameters.Length > 0
            && parameters[0].ParameterType.IsAssignableFrom(typeof(T));
    }

    private static IReadOnlyList<ISeries> GetResults(object hub)
    {
        PropertyInfo prop = hub.GetType().GetProperty("Results")
            ?? throw new InvalidOperationException($"{hub.GetType().Name} has no Results property");

        object value = prop.GetValue(hub)
            ?? throw new InvalidOperationException($"{hub.GetType().Name}.Results returned null");

        return ((System.Collections.IEnumerable)value).Cast<ISeries>().ToList();
    }
}
