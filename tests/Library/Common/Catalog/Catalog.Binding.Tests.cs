using System.Reflection;

namespace Catalogging;

/// <summary>
/// Catalog binding tests asserting that every listing names members that exist:
/// - <c>MethodName</c> resolves to a public static indicator method
/// - each <c>Results[].DataName</c> resolves to a property on the result record
/// - each <c>Parameters[].ParameterName</c> resolves to a method parameter, in order
/// </summary>
/// <remarks>
/// These names are plain strings in the <c>*.Catalog.cs</c> definitions, so the
/// compiler cannot catch a rename or removal on the library side. Catalog-driven
/// consumers — codegen, chart binding, tool wrappers — bind by these names, and a
/// stale one either advertises a field that is never populated or, for a parameter,
/// silently supplies the default instead of the requested value. Every listing is
/// checked so drift fails here rather than downstream.
/// </remarks>
[TestClass]
public class CatalogBindingTests : TestBase
{
    [TestMethod]
    public void EveryListingBindsToAnIndicatorMethod()
    {
        List<string> violations = [];

        foreach (IndicatorListing listing in Catalog.Get())
        {
            string identity = CatalogReflection.Describe(listing);

            if (string.IsNullOrWhiteSpace(listing.MethodName))
            {
                violations.Add($"{identity}: MethodName is not set");
                continue;
            }

            IReadOnlyList<MethodInfo> overloads
                = CatalogReflection.GetOverloads(listing.MethodName);

            if (overloads.Count == 0)
            {
                violations.Add($"{identity}: method '{listing.MethodName}' does not exist");
                continue;
            }

            if (overloads.Any(static m => CatalogReflection.GetResultType(m) is null))
            {
                violations.Add(
                    $"{identity}: result record type of '{listing.MethodName}' cannot be resolved");
            }
        }

        string.Join(Environment.NewLine, violations).Should().BeEmpty(
            "every catalog listing must name an indicator method that exists and returns a resolvable result record");
    }

    [TestMethod]
    public void EveryResultDataNameExistsOnResultRecord()
    {
        List<string> violations = [];

        foreach (IndicatorListing listing in Catalog.Get())
        {
            string identity = CatalogReflection.Describe(listing);

            IReadOnlyList<MethodInfo> overloads
                = CatalogReflection.GetOverloads(listing.MethodName);

            List<Type> resultTypes = overloads
                .Select(CatalogReflection.GetResultType)
                .Where(static t => t != null)
                .Distinct()
                .ToList();

            if (resultTypes.Count == 0)
            {
                continue; // reported by EveryListingBindsToAnIndicatorMethod
            }

            foreach (Type resultType in resultTypes)
            {
                ISet<string> properties = CatalogReflection.GetPropertyNames(resultType);

                foreach (IndicatorResult result in listing.Results)
                {
                    if (!properties.Contains(result.DataName))
                    {
                        violations.Add(
                            $"{identity}: DataName '{result.DataName}' is not a property on {resultType.Name} "
                          + $"(has: {string.Join(", ", properties.Order(StringComparer.Ordinal))})");
                    }
                }
            }
        }

        string.Join(Environment.NewLine, violations).Should().BeEmpty(
            "every catalog result must name a property that exists on the indicator's result record");
    }

    [TestMethod]
    public void EveryParameterNameMatchesMethodSignature()
    {
        List<string> violations = [];

        foreach (IndicatorListing listing in Catalog.Get())
        {
            if (listing.Parameters is null or { Count: 0 })
            {
                continue;
            }

            string identity = CatalogReflection.Describe(listing);

            IReadOnlyList<MethodInfo> overloads
                = CatalogReflection.GetOverloads(listing.MethodName);

            if (overloads.Count == 0)
            {
                continue; // reported by EveryListingBindsToAnIndicatorMethod
            }

            string[] catalogNames = listing.Parameters
                .Select(static p => p.ParameterName)
                .ToArray();

            bool bound = overloads.Any(m => CatalogReflection.IsContiguousRun(
                catalogNames,
                m.GetParameters().Select(static p => p.Name).ToArray()));

            if (!bound)
            {
                IEnumerable<string> signatures = overloads.Select(static m
                    => $"({string.Join(", ", m.GetParameters().Select(static p => p.Name))})");

                violations.Add(
                    $"{identity}: parameters [{string.Join(", ", catalogNames)}] are not a contiguous, "
                  + $"in-order run in any overload of '{listing.MethodName}' {string.Join(" | ", signatures)}");
            }
        }

        string.Join(Environment.NewLine, violations).Should().BeEmpty(
            "every catalog parameter must name a method parameter, in the contiguous order the executor binds them positionally");
    }

    [TestMethod]
    public void EveryListingBindsToMethodOfItsOwnStyle()
    {
        List<string> violations = [];

        foreach (IndicatorListing listing in Catalog.Get())
        {
            IReadOnlyList<MethodInfo> overloads
                = CatalogReflection.GetOverloads(listing.MethodName);

            if (overloads.Count == 0)
            {
                continue; // reported by EveryListingBindsToAnIndicatorMethod
            }

            if (!overloads.Any(m => CatalogReflection.GetImpliedStyle(m) == listing.Style))
            {
                IEnumerable<string> shapes = overloads
                    .Select(static m => CatalogReflection.GetImpliedStyle(m)?.ToString() ?? "unrecognized")
                    .Distinct(StringComparer.Ordinal);

                violations.Add(
                    $"{CatalogReflection.Describe(listing)}: '{listing.MethodName}' returns "
                  + $"{string.Join(" or ", shapes)} shape, not {listing.Style}");
            }
        }

        string.Join(Environment.NewLine, violations).Should().BeEmpty(
            "a listing must bind to a method of its own style; all three styles share a result record, so the record alone cannot reveal a cross-style mistake");
    }
}
