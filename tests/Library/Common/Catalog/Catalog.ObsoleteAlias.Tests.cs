namespace Catalogging;

/// <summary>
/// Tests for the obsolete <c>ReturnType</c> alias kept for source compatibility
/// with 3.0.0, where it was public API.
/// </summary>
/// <remarks>
/// The property was renamed to <c>ResultRecordType</c>. The value it carried was
/// always <c>null</c> in every shipped version, so nothing can depend on the value —
/// but code that merely referenced the name compiled against 3.0.0 and would not
/// compile without this alias. It tracks the new property rather than storing a
/// second copy, so the two cannot disagree.
/// </remarks>
[TestClass]
public class CatalogObsoleteAliasTests : TestBase
{
    [TestMethod]
    public void ReturnTypeAliasTracksResultRecordType()
    {
        List<string> mismatches = [];

        foreach (IndicatorListing listing in Catalog.Get())
        {
#pragma warning disable CS0618 // asserting the obsolete alias is exactly the point
            string alias = listing.ReturnType;
#pragma warning restore CS0618

            if (!string.Equals(alias, listing.ResultRecordType, StringComparison.Ordinal))
            {
                mismatches.Add(
                    $"{CatalogReflection.Describe(listing)}: ReturnType '{alias}' "
                  + $"!= ResultRecordType '{listing.ResultRecordType}'");
            }
        }

        string.Join(Environment.NewLine, mismatches).Should().BeEmpty(
            "the alias must report the same value as the property it forwards to");
    }

    [TestMethod]
    public void ReturnTypeAliasIsNotSerialized()
    {
        string json = Catalog.Get().ToJson();

        json.Should().Contain("\"resultRecordType\":",
            "the value is carried under its current name");
        json.Should().NotContain("\"returnType\":",
            "the alias exists for source compatibility, not to duplicate the wire format");
    }
}
