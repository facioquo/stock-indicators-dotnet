#nullable enable
namespace Catalogging;

/// <summary>
/// Catalog metadata tests to ensure automation viability and completeness:
/// - method naming presence and convention (To*)
/// - parameter types, required/defaults rules, and supported types
/// - results structure and data types
/// - uniqueness and style differentiation
/// </summary>
[TestClass]
public class CatalogMetadataTests : TestBase
{
    [TestMethod]
    public void AllIndicatorsShouldHaveMethodNames()
    {
        IReadOnlyList<IndicatorListing> catalog = Catalog.Listings;
        catalog.Should().NotBeEmpty();
        foreach (IndicatorListing listing in catalog)
        {
            listing.MethodName.Should().NotBeNullOrWhiteSpace($"indicator {listing.Uiid} should have a method name for automation");
            listing.MethodName.Should().StartWith("To");
        }
    }

    [TestMethod]
    public void CatalogShouldSupportParameterTypeValidation()
    {
        IndicatorListing emaListing = Catalog.Listings.First(l => l.Uiid == "EMA" && l.Style == Style.Series);
        Action validAction = () => emaListing.WithParamValue("lookbackPeriods", 20);
        validAction.Should().NotThrow();
        Action invalidAction = () => emaListing.WithParamValue("lookbackPeriods", "invalid");
        invalidAction.Should().Throw<ArgumentException>();
    }

    [TestMethod]
    public void CatalogShouldHaveValidStructure()
    {
        IReadOnlyList<IndicatorListing> catalog = Catalog.Listings;
        catalog.Should().NotBeEmpty();
        foreach (IndicatorListing listing in catalog)
        {
            listing.Uiid.Should().NotBeNullOrWhiteSpace();
            listing.Name.Should().NotBeNullOrWhiteSpace();
            listing.Style.Should().BeDefined();
            listing.Category.Should().BeDefined();

            if (listing.Results?.Count > 0)
            {
                foreach (IndicatorResult result in listing.Results)
                {
                    result.DataName.Should().NotBeNullOrWhiteSpace();
                    result.DisplayName.Should().NotBeNullOrWhiteSpace();
                }
            }
        }
    }

    [TestMethod]
    public void EachCatalogEntryShouldHaveRequiredMetadata()
    {
        IReadOnlyList<IndicatorListing> catalog = Catalog.Listings;
        catalog.Should().NotBeEmpty();
        foreach (IndicatorListing listing in catalog)
        {
            listing.Uiid.Should().NotBeNullOrWhiteSpace();
            listing.Name.Should().NotBeNullOrWhiteSpace();
            listing.Style.Should().BeDefined();
            listing.Category.Should().BeDefined();
            listing.MethodName.Should().NotBeNullOrWhiteSpace();
            listing.MethodName.Should().StartWith("To");

            if (listing.Parameters != null)
            {
                foreach (IndicatorParam param in listing.Parameters)
                {
                    param.ParameterName.Should().NotBeNullOrWhiteSpace();
                    param.DisplayName.Should().NotBeNullOrWhiteSpace();
                    param.DataType.Should().NotBeNullOrWhiteSpace();

                    if (param.IsRequired && param.DefaultValue == null)
                    {
                        string[] allowedNullDefaults = ["IEnumerable", "IReadOnlyList", "DateTime"];
                        if (!allowedNullDefaults.Any(t => param.DataType.Contains(t, StringComparison.OrdinalIgnoreCase)))
                        {
                            Assert.Fail($"Required parameter {param.ParameterName} in {listing.Uiid} should have a default value or be of allowed type (IEnumerable, DateTime, etc.)");
                        }
                    }
                }
            }

            listing.Results.Should().NotBeNullOrEmpty();
            foreach (IndicatorResult result in listing.Results!)
            {
                result.DataName.Should().NotBeNullOrWhiteSpace();
                result.DisplayName.Should().NotBeNullOrWhiteSpace();
                result.DataType.Should().BeDefined();
            }
        }
    }

    [TestMethod]
    public void EachCatalogEntryWithMethodNameShouldBeCallable()
    {
        IReadOnlyList<IndicatorListing> catalog = Catalog.Listings;
        List<IndicatorListing> entriesWithMethods = catalog.Where(static l => !string.IsNullOrWhiteSpace(l.MethodName)).ToList();
        entriesWithMethods.Should().NotBeEmpty();
        foreach (IndicatorListing listing in entriesWithMethods)
        {
            listing.MethodName.Should().StartWith("To");
            string cleanUiid = listing.Uiid.Replace("-", "", StringComparison.OrdinalIgnoreCase).Replace("_", "", StringComparison.OrdinalIgnoreCase);

            bool isRelated = listing.MethodName!.Contains(cleanUiid, StringComparison.OrdinalIgnoreCase) ||
                             (cleanUiid.Equals("BB", StringComparison.OrdinalIgnoreCase) && listing.MethodName!.Contains("BOLLINGER", StringComparison.OrdinalIgnoreCase)) ||
                             (cleanUiid.Equals("CHEXIT", StringComparison.OrdinalIgnoreCase) && listing.MethodName!.Contains("CHANDELIER", StringComparison.OrdinalIgnoreCase)) ||
                             listing.MethodName!.EndsWith(cleanUiid, StringComparison.OrdinalIgnoreCase) ||
                             listing.MethodName!.Contains(cleanUiid[..Math.Min(3, cleanUiid.Length)], StringComparison.OrdinalIgnoreCase);

            if (!isRelated)
            {
                listing.MethodName.Should().MatchRegex("^To[A-Z][a-zA-Z]*$");
            }
        }
    }

    [TestMethod]
    public void EachIndicatorShouldHaveUniqueIdStyleCombination()
    {
        IReadOnlyList<IndicatorListing> catalog = Catalog.Listings;
        var duplicates = catalog.GroupBy(static l => new { l.Uiid, l.Style }).Where(static g => g.Count() > 1).ToList();
        duplicates.Should().BeEmpty();
    }

    [TestMethod]
    public void IndicatorsWithSameUiidShouldHaveDifferentStyles()
    {
        IReadOnlyList<IndicatorListing> catalog = Catalog.Listings;
        foreach (IGrouping<string, IndicatorListing> group in catalog.GroupBy(static l => l.Uiid).Where(static g => g.Count() > 1))
        {
            List<Style> styles = group.Select(static l => l.Style).Distinct().ToList();
            styles.Should().HaveCount(group.Count());
        }
    }

    [TestMethod]
    public void EachCatalogEntryParametersShouldBeValidForAutomation()
    {
        IReadOnlyList<IndicatorListing> catalog = Catalog.Listings;
        List<IndicatorListing> entriesWithParameters = catalog.Where(l => l.Parameters?.Any() == true).ToList();
        entriesWithParameters.Should().NotBeEmpty();
        foreach (IndicatorListing listing in entriesWithParameters)
        {
            foreach (IndicatorParam parameter in listing.Parameters!)
            {
                parameter.DataType.Should().NotBeNullOrWhiteSpace();
                string[] supportedTypes =
                [
                    "Int32", "Double", "Boolean", "DateTime", "String", "enum", "Nullable",
                    "IEnumerable", "IReadOnlyList<T> where T : IReusable",
                    "BetaType", "ChandExitType", "PivotPointType", "MaType", "CandlePartType", "EndType"
                ];
                supportedTypes.Should().Contain(t => parameter.DataType.Contains(t) || parameter.DataType.StartsWith(t));

                if (parameter.IsRequired && parameter.DefaultValue == null)
                {
                    string[] allowedNullDefaults = ["IEnumerable", "IReadOnlyList", "DateTime"];
                    allowedNullDefaults.Should().Contain(t => parameter.DataType.Contains(t, StringComparison.OrdinalIgnoreCase));
                }
            }
        }
    }
}
