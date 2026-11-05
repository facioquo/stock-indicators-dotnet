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

        // 20 listings are Series-only: Beta, Correlation, Prs, RenkoAtr, StdDevChannels,
        // ZigZag, and the 14 IndyMath operations.
        seriesCount.Should().Be(99);
        bufferCount.Should().Be(79);
        streamCount.Should().Be(79);

        int totalCount = seriesCount + streamCount + bufferCount;
        totalCount.Should().Be(257);
    }
}
