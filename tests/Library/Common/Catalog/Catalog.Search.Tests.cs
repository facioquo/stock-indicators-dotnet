#nullable enable
namespace Catalogging;

/// <summary>
/// Catalog search and retrieval tests:
/// - Get by UIID, by Style, by Category
/// - Search by partial name and with style/category filters
/// - Empty search returns all, non-existent returns empty
/// - Static catalog consistency across calls
/// </summary>
[TestClass]
public class CatalogSearchTests : TestBase
{
    [TestMethod]
    public void GetByUiidShouldReturnCorrectIndicator()
    {
        IReadOnlyList<IndicatorListing> emaListings = Catalog.Get("EMA");
        emaListings.Should().NotBeEmpty("EMA should exist in catalog");
        emaListings.Should().OnlyContain(static l => l.Uiid == "EMA");
        emaListings.Should().OnlyContain(static l => l.Name == "Exponential Moving Average");
    }

    [TestMethod]
    public void GetByStyleShouldReturnCorrectIndicators()
    {
        IReadOnlyList<IndicatorListing> seriesListings = Catalog.Get(Style.Series);
        IReadOnlyList<IndicatorListing> streamListings = Catalog.Get(Style.Stream);
        IReadOnlyList<IndicatorListing> bufferListings = Catalog.Get(Style.Buffer);

        seriesListings.Should().NotBeEmpty();
        streamListings.Should().NotBeEmpty();
        bufferListings.Should().NotBeEmpty();

        seriesListings.Should().OnlyContain(static l => l.Style == Style.Series);
        streamListings.Should().OnlyContain(static l => l.Style == Style.Stream);
        bufferListings.Should().OnlyContain(static l => l.Style == Style.Buffer);
    }

    [TestMethod]
    public void GetByCategoryShouldReturnCorrectIndicators()
    {
        IReadOnlyList<IndicatorListing> maListings = Catalog.Get(Category.MovingAverage);
        maListings.Should().NotBeEmpty();
        maListings.Should().Contain(static l => l.Uiid == "EMA");
        maListings.Should().Contain(static l => l.Uiid == "SMA");
        maListings.Should().OnlyContain(static l => l.Category == Category.MovingAverage);
    }

    [TestMethod]
    public void SearchShouldReturnPartialMatches()
    {
        IReadOnlyList<IndicatorListing> rsiResults = Catalog.Search("RSI");
        rsiResults.Should().NotBeEmpty();
        rsiResults.Should().Contain(static l => l.Uiid == "RSI");
        rsiResults.Should().OnlyContain(static l => l.Name.Contains("RSI") || l.Uiid.Contains("RSI"));
    }

    [TestMethod]
    public void SearchByPartialNameShouldReturnMultipleMatches()
    {
        IReadOnlyList<IndicatorListing> movingResults = Catalog.Search("Moving");
        movingResults.Should().NotBeEmpty();
        movingResults.Should().Contain(static l => l.Uiid == "EMA");
        movingResults.Should().Contain(static l => l.Uiid == "SMA");
        movingResults.Count.Should().BeGreaterThan(2);
        movingResults.Should().OnlyContain(static l => l.Name.Contains("Moving") || l.Uiid.Contains("Moving"));
    }

    [TestMethod]
    public void SearchByNameWithStyleShouldReturnCorrectIndicators()
    {
        IReadOnlyList<IndicatorListing> sut = Catalog.Search("Moving", Style.Series);
        sut.Should().NotBeEmpty();
        sut.Should().Contain(static l => l.Uiid == "EMA" && l.Style == Style.Series);
        sut.Should().Contain(static l => l.Uiid == "SMA" && l.Style == Style.Series);
        sut.Should().NotContain(static l => l.Style != Style.Series);
        sut.Should().OnlyContain(static l => l.Name.Contains("Moving"));
    }

    [TestMethod]
    public void SearchByNameWithCategoryShouldReturnCorrectIndicators()
    {
        IReadOnlyList<IndicatorListing> sut = Catalog.Search("Moving", Category.MovingAverage);
        sut.Should().NotBeEmpty();
        sut.Should().Contain(static l => l.Uiid == "EMA");
        sut.Should().Contain(static l => l.Uiid == "SMA");
        sut.Should().OnlyContain(static l => l.Category == Category.MovingAverage);
        sut.Should().OnlyContain(static l => l.Name.Contains("Moving"));
    }

    [TestMethod]
    public void InvalidUiidShouldReturnEmptyCollection()
    {
        IReadOnlyList<IndicatorListing> sut = Catalog.Get("NONEXISTENT_INDICATOR");
        sut.Should().BeEmpty();
    }

    [TestMethod]
    public void EmptySearchShouldReturnAllIndicators()
    {
        IReadOnlyList<IndicatorListing> sut = Catalog.Search("");
        IReadOnlyList<IndicatorListing> allIndicators = Catalog.Get();
        sut.Count.Should().Be(allIndicators.Count);
    }

    [TestMethod]
    public void StaticCatalogShouldBeConsistent()
    {
        IReadOnlyList<IndicatorListing> firstCall = Catalog.Get();
        IReadOnlyList<IndicatorListing> secondCall = Catalog.Get();

        firstCall.Count.Should().Be(secondCall.Count);
        foreach (IndicatorListing listing in firstCall)
        {
            secondCall.Should().Contain(l => l.Uiid == listing.Uiid && l.Style == listing.Style);
        }
    }
}
