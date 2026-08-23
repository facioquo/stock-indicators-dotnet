using System.Reflection;

namespace Catalogging;

/// <summary>
/// Catalog binding tests asserting that every listing names members that exist:
/// - <c>MethodName</c> resolves to a public static indicator method
/// - each <c>Results[].DataName</c> resolves to a property on the result record
/// - each <c>Parameters[].ParameterName</c> resolves to a method parameter, in order
/// - each <c>Parameters[].IsRequired</c> agrees with whether C# lets a caller omit it
///   and still get the behavior the listing describes
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
    public void EveryParameterIsRequiredMatchesCallability()
    {
        List<string> violations = [];

        foreach (IndicatorListing listing in Catalog.Get())
        {
            if (listing.Parameters is null or { Count: 0 })
            {
                continue;
            }

            IReadOnlyList<MethodInfo> overloads
                = CatalogReflection.GetOverloads(listing.MethodName);

            string[] catalogNames = listing.Parameters
                .Select(static p => p.ParameterName)
                .ToArray();

            bool aligned = overloads.Any(m => CatalogReflection.IsContiguousRun(
                catalogNames,
                CatalogReflection.GetParameterNames(m)));

            if (!aligned)
            {
                continue; // reported by EveryParameterNameMatchesMethodSignature
            }

            string identity = CatalogReflection.Describe(listing);

            for (int i = 0; i < listing.Parameters.Count; i++)
            {
                IndicatorParam param = listing.Parameters[i];

                bool defaulted = CatalogReflection.TryGetCSharpDefault(
                    overloads, catalogNames, i, out object csharpDefault);

                bool shorter = CatalogReflection.HasShorterOverload(overloads, catalogNames, i);

                // no form of the method leaves it out, so a caller must supply it
                if (!defaulted && !shorter && !param.IsRequired)
                {
                    violations.Add(
                        $"{identity}: '{param.ParameterName}' is marked optional, but every form of "
                      + $"'{listing.MethodName}' requires it");
                }

                // C# already supplies a value, so demanding one over-constrains callers
                if (defaulted && param.IsRequired)
                {
                    violations.Add(
                        $"{identity}: '{param.ParameterName}' is marked required, but "
                      + $"'{listing.MethodName}' gives it a default");
                }

                // optional plus a declared default promises that omitting yields that
                // default; a shorter overload delivers its own behavior instead. The
                // no-way-to-omit case is already reported above, so this speaks only
                // where a shorter overload genuinely exists.
                //
                // This assumes a shorter overload never merely re-applies the declared
                // default. That holds across the catalog today — ToPrs drops PrsPercent
                // and ToVwap derives a start from the data — and reflection cannot tell
                // the difference, so an overload that did forward the same constant
                // would have to be marked required or drop its declared default.
                if (!param.IsRequired && param.DefaultValue is not null && !defaulted && shorter)
                {
                    violations.Add(
                        $"{identity}: '{param.ParameterName}' is marked optional with default "
                      + $"'{param.DefaultValue}', but omitting it selects a shorter overload of "
                      + $"'{listing.MethodName}' that does not apply that value");
                }

                // an advertised default that differs from the one C# applies is the
                // silent-wrong-value case: the caller omits the argument expecting the
                // documented number and the method uses another
                if (defaulted
                 && param.DefaultValue is not null
                 && !CatalogReflection.DefaultsAgree(csharpDefault, param.DefaultValue))
                {
                    violations.Add(
                        $"{identity}: '{param.ParameterName}' advertises default "
                      + $"'{param.DefaultValue}', but '{listing.MethodName}' applies "
                      + $"'{csharpDefault}' when the argument is omitted");
                }
            }
        }

        string.Join(Environment.NewLine, violations).Should().BeEmpty(
            "IsRequired must track whether C# lets a caller omit the argument and still get what "
          + "the listing describes; catalog-driven code generation reads it to decide whether to "
          + "emit one, so an understated value produces source that does not compile and a default "
          + "the shorter overload ignores produces a silently different indicator");
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
