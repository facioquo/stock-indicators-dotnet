namespace Catalogging;

/// <summary>
/// Test class for BarPart catalog functionality.
/// </summary>
[TestClass]
public class BarPartCatalogTests : TestBase
{
    [TestMethod]
    public void BarPartSeriesListing()
    {
        // Arrange
        IReadOnlyList<Bar> bars = Bars;
        IndicatorListing listing = BarParts.SeriesListing;
        listing.Should().NotBeNull("BarPart Series listing should be found");

        // Get catalog default value for test use
        IndicatorParam candlePartParam = listing.Parameters.Single(static p => p.ParameterName == "candlePart");
        CandlePart candlePartValue = (CandlePart)candlePartParam.DefaultValue;

        // Act - Use catalog utility to dynamically execute the indicator
        IReadOnlyList<TimeValue> catalogResults = ListingExecutor.Execute<TimeValue>(bars, listing);

        // Act - Direct call for comparison using catalog's default parameter value
        IReadOnlyList<TimeValue> directResults = bars.ToBarPart(candlePartValue);

        // Assert - Results from catalog-driven execution should match direct call
        catalogResults.IsExactly(directResults);

        // Assert - Lookup gets correct values
        candlePartValue.Should().Be(CandlePart.Close);

        // Additional assertions on catalog metadata (basic validation)
        listing.Name.Should().Be("Bar Part");
        listing.Uiid.Should().Be("BARPART");
        listing.Style.Should().Be(Style.Series);
        listing.Category.Should().Be(Category.PriceTransform);
        listing.MethodName.Should().Be("ToBarPart");
        listing.Parameters.Should().HaveCount(1);
        listing.Results.Should().HaveCount(1);
    }

    [TestMethod]
    public void BarPartStreamListing()
    {
        // Act
        IndicatorListing listing = BarParts.StreamListing;

        // Assert
        listing.Should().NotBeNull();
        listing.Name.Should().Be("Bar Part");
        listing.Uiid.Should().Be("BARPART");
        listing.Style.Should().Be(Style.Stream);
        listing.Category.Should().Be(Category.PriceTransform);

        listing.Parameters.Should().NotBeNull();
        listing.Parameters.Should().HaveCount(1);

        IndicatorParam candlePart = listing.Parameters.FirstOrDefault(static p => p.ParameterName == "candlePart");
        candlePart.Should().NotBeNull();
        candlePart?.DisplayName.Should().Be("Candle Part");

        listing.Results.Should().NotBeNull();
        listing.Results.Should().HaveCount(1);

        IndicatorResult result = listing.Results[0];
        result.DataName.Should().Be("Value");
        result.DisplayName.Should().Be("Value");
        result.DataType.Should().Be(ResultType.Default);
        result.IsReusable.Should().BeTrue();
    }

    [TestMethod]
    public void BarPartBufferListing()
    {
        // Act
        IndicatorListing listing = BarParts.BufferListing;

        // Assert
        listing.Should().NotBeNull();
        listing.Name.Should().Be("Bar Part");
        listing.Uiid.Should().Be("BARPART");
        listing.Style.Should().Be(Style.Buffer);
        listing.Category.Should().Be(Category.PriceTransform);

        listing.Parameters.Should().NotBeNull();
        listing.Parameters.Should().HaveCount(1);

        IndicatorParam candlePart = listing.Parameters.FirstOrDefault(static p => p.ParameterName == "candlePart");
        candlePart.Should().NotBeNull();
        candlePart?.DisplayName.Should().Be("Candle Part");

        listing.Results.Should().NotBeNull();
        listing.Results.Should().HaveCount(1);

        IndicatorResult result = listing.Results[0];
        result.DataName.Should().Be("Value");
        result.DisplayName.Should().Be("Value");
        result.DataType.Should().Be(ResultType.Default);
        result.IsReusable.Should().BeTrue();
    }

    [TestMethod]
    public void CatalogBrowsingIncludesBarPartSeries()
    {
        // Act
        IReadOnlyList<IndicatorListing> catalog = Catalog.Get();
        IndicatorListing barPartSeries = Catalog.Get("BARPART", Style.Series);
        IReadOnlyList<IndicatorListing> seriesListings = Catalog.Get(Style.Series);

        // Assert
        catalog.Should().NotBeNull();
        catalog.Should().Contain(static l => l.Uiid == "BARPART" && l.Style == Style.Series);
        barPartSeries.Should().NotBeNull();
        seriesListings.Should().Contain(static l => l.Uiid == "BARPART" && l.Style == Style.Series);
    }
}
