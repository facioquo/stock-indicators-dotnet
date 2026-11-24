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

        // 3 indicators (RenkoAtr, StdDevChannels, ZigZag) are
        // Series-only and account for the 99 vs 96 gap; the 11
        // fixed-amount and two-series IndyMath IDs have no buffer list.
        seriesCount.Should().Be(99);
        bufferCount.Should().Be(85);
        streamCount.Should().Be(96);

        int totalCount = seriesCount + streamCount + bufferCount;
        totalCount.Should().Be(280);
    }
}
