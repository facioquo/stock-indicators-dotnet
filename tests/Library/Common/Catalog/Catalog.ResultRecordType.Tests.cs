using System.Reflection;

namespace Catalogging;

/// <summary>
/// Catalog result record tests asserting that <c>ResultRecordType</c> is populated and honest:
/// - every listing carries a value
/// - the bound method name resolves to exactly one result record, so the derivation
///   cannot depend on unspecified reflection ordering
/// - the three styles of one indicator agree on it
/// - the named record really carries every result the listing advertises
/// </summary>
/// <remarks>
/// <c>ResultRecordType</c> is derived from <c>MethodName</c> at build time rather than
/// hand-assigned per indicator. Re-deriving the expected value here would compare a
/// value against a copy of the function that produced it, so these tests instead
/// constrain the derivation from the outside: it must be populated, unambiguous,
/// consistent across styles, and resolvable by name to a record carrying the
/// advertised results — the path an external consumer actually takes.
/// </remarks>
[TestClass]
public class CatalogResultRecordTypeTests : TestBase
{
    [TestMethod]
    public void EveryListingHasResultRecordType()
    {
        IReadOnlyList<IndicatorListing> unpopulated = Catalog.Get()
            .Where(static l => string.IsNullOrWhiteSpace(l.ResultRecordType))
            .ToList();

        string.Join(
            Environment.NewLine,
            unpopulated.Select(CatalogReflection.Describe))
            .Should().BeEmpty("every catalog listing must report the result record it returns");
    }

    [TestMethod]
    public void EveryMethodNameResolvesToOneResultRecord()
    {
        List<string> violations = [];

        foreach (IndicatorListing listing in Catalog.Get())
        {
            IReadOnlyList<MethodInfo> overloads
                = CatalogReflection.GetOverloads(listing.MethodName);

            List<string> resultTypeNames = overloads
                .Select(CatalogReflection.GetResultType)
                .Where(static t => t != null)
                .Select(static t => t.Name)
                .Distinct(StringComparer.Ordinal)
                .ToList();

            if (resultTypeNames.Count == 0)
            {
                violations.Add($"{CatalogReflection.Describe(listing)}: '{listing.MethodName}' has no resolvable result record");
            }
            else if (resultTypeNames.Count > 1)
            {
                violations.Add(
                    $"{CatalogReflection.Describe(listing)}: overloads of '{listing.MethodName}' return "
                  + $"{string.Join(" and ", resultTypeNames)}, so the derived ResultRecordType depends on reflection order");
            }
        }

        string.Join(Environment.NewLine, violations).Should().BeEmpty(
            "the derivation picks one overload, so a method name whose overloads disagree on the result record would make ResultRecordType depend on unspecified reflection ordering");
    }

    [TestMethod]
    public void ResultRecordTypeIsConsistentAcrossStyles()
    {
        List<string> violations = [];

        foreach (IGrouping<string, IndicatorListing> group in Catalog.Get()
            .GroupBy(static l => l.Uiid, StringComparer.Ordinal))
        {
            List<string> distinct = group
                .Select(static l => l.ResultRecordType)
                .Distinct(StringComparer.Ordinal)
                .ToList();

            if (distinct.Count > 1)
            {
                violations.Add($"{group.Key}: styles disagree — {string.Join(", ", distinct)}");
            }
        }

        string.Join(Environment.NewLine, violations).Should().BeEmpty(
            "Series, Buffer, and Stream styles of one indicator produce the same result record");
    }

    [TestMethod]
    public void ResultRecordTypePropertiesCoverCatalogResults()
    {
        List<string> violations = [];

        foreach (IndicatorListing listing in Catalog.Get())
        {
            // resolve by name, exactly as a catalog-driven consumer would
            Type resultType = CatalogReflection.FindPublicType(listing.ResultRecordType);

            if (resultType is null)
            {
                violations.Add(
                    $"{CatalogReflection.Describe(listing)}: ResultRecordType '{listing.ResultRecordType}' "
                  + "does not resolve to a public type in the library");
                continue;
            }

            ISet<string> properties = CatalogReflection.GetPropertyNames(resultType);

            IEnumerable<string> missing = listing.Results
                .Select(static r => r.DataName)
                .Where(name => !properties.Contains(name));

            foreach (string name in missing)
            {
                violations.Add(
                    $"{CatalogReflection.Describe(listing)}: result '{name}' is absent from {listing.ResultRecordType}");
            }
        }

        string.Join(Environment.NewLine, violations).Should().BeEmpty(
            "a consumer that reflects over ResultRecordType must find every result the listing advertises");
    }
}
