namespace Catalogging;

/// <summary>
/// Catalog metrics tests:
/// - per-listing field presence (Uiid and Name)
/// - exact style counts and total count matching current static catalog
/// </summary>
[TestClass]
public class CatalogMetricsTests : TestBase
{
    [TestMethod]
    public void StaticCatalogListingsShouldHavePopulatedFields()
    {
        IReadOnlyList<IndicatorListing> allListings = Catalog.Get();
        allListings.Should().NotBeEmpty();
        allListings.Should().OnlyContain(static l => !string.IsNullOrWhiteSpace(l.Uiid));
        allListings.Should().OnlyContain(static l => !string.IsNullOrWhiteSpace(l.Name));
    }

    [TestMethod]
    public void CatalogShouldHaveExactStyleCounts()
    {
        IReadOnlyList<IndicatorListing> catalog = Catalog.Listings;
        int seriesCount = catalog.Count(static x => x.Style == Style.Series);
        int streamCount = catalog.Count(static x => x.Style == Style.Stream);
        int bufferCount = catalog.Count(static x => x.Style == Style.Buffer);

        // 6 indicators (Beta, Correlation, Prs, RenkoAtr, StdDevChannels, ZigZag) are
        // Series-only and account for the 86 vs 80 gap.
        seriesCount.Should().Be(86);
        bufferCount.Should().Be(80);
        streamCount.Should().Be(80);

        int totalCount = seriesCount + streamCount + bufferCount;
        totalCount.Should().Be(246);
    }
}
