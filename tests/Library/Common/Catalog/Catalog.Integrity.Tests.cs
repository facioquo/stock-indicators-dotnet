namespace Catalogging;

/// <summary>
/// Catalog integrity tests enforcing the one-listing-per-style approach and related constraints:
/// - unique UIID+Style across the catalog
/// - multi-style indicators have separate listings with consistent base metadata
/// - single-style indicators appear once
/// - style distribution totals reconcile
/// </summary>
[TestClass]
public class CatalogIntegrityTests : TestBase
{
    [TestMethod]
    public void StaticCatalogShouldFollowOneListingPerStylePattern()
    {
        IReadOnlyList<IndicatorListing> allListings = Catalog.Get();
        foreach (var group in allListings.GroupBy(static listing => new { listing.Uiid, listing.Style }))
        {
            group.Should().HaveCount(1, $"UIID '{group.Key.Uiid}' with style '{group.Key.Style}' should appear exactly once in the catalog");
        }
    }

    [TestMethod]
    public void MultiStyleIndicatorsShouldHaveSeparateListings()
    {
        IReadOnlyList<IndicatorListing> emaListings = Catalog.Get("EMA");
        emaListings.Should().NotBeEmpty("EMA should exist in the catalog");
        List<IGrouping<Style, IndicatorListing>> styleGroups = emaListings.GroupBy(static l => l.Style).ToList();
        styleGroups.Should().HaveCountGreaterThan(1, "EMA should have multiple styles");
        foreach (IGrouping<Style, IndicatorListing> styleGroup in styleGroups)
        {
            styleGroup.Should().HaveCount(1, $"EMA should have exactly one listing for style {styleGroup.Key}");
            styleGroup.All(static l => l.Uiid == "EMA").Should().BeTrue();
            styleGroup.All(static l => l.Name == "Exponential Moving Average").Should().BeTrue();
        }
    }

    [TestMethod]
    public void StyleSpecificListingsShouldHaveConsistentMetadata()
    {
        IReadOnlyList<IndicatorListing> smaListings = Catalog.Get("SMA");
        if (smaListings.Count > 1)
        {
            string baseName = smaListings[0].Name;
            Category baseCategory = smaListings[0].Category;
            foreach (IndicatorListing listing in smaListings)
            {
                listing.Name.Should().Be(baseName);
                listing.Category.Should().Be(baseCategory);
                listing.Uiid.Should().Be("SMA");
            }
        }
    }

    [TestMethod]
    public void ShouldHaveCorrectQuantities()
    {
        IReadOnlyList<IndicatorListing> rsiListings = Catalog.Get("RSI");
        rsiListings.Should().NotBeEmpty();
        rsiListings.Should().HaveCount(3); // Series, Stream, Buffer
        rsiListings.Should().Contain(static x => x.Style == Style.Series);
        rsiListings.Should().Contain(static x => x.Style == Style.Stream);
        rsiListings.Should().Contain(static x => x.Style == Style.Buffer);
    }

    [TestMethod]
    public void CatalogShouldHaveBalancedStyleDistribution()
    {
        IReadOnlyList<IndicatorListing> allListings = Catalog.Get();
        IReadOnlyList<IndicatorListing> seriesListings = Catalog.Get(Style.Series);
        IReadOnlyList<IndicatorListing> streamListings = Catalog.Get(Style.Stream);
        IReadOnlyList<IndicatorListing> bufferListings = Catalog.Get(Style.Buffer);

        seriesListings.Should().NotBeEmpty();
        streamListings.Should().NotBeEmpty();
        bufferListings.Should().NotBeEmpty();

        seriesListings.Count.Should().BeGreaterThan(streamListings.Count);
        seriesListings.Count.Should().BeGreaterThan(bufferListings.Count);
        (seriesListings.Count + streamListings.Count + bufferListings.Count).Should().Be(allListings.Count);
    }
}
