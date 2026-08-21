namespace Catalogging;

/// <summary>
/// Test class for Vwap catalog functionality.
/// </summary>
[TestClass]
public class VwapCatalogTests : TestBase
{
    [TestMethod]
    public void VwapSeries_InCatalog_ReturnsAllVariants()
    {
        // Act
        IndicatorListing listing = Vwap.SeriesListing;

        // Assert
        listing.Should().NotBeNull();
        listing.Name.Should().Be("Volume Weighted Average Price");
        listing.Uiid.Should().Be("VWAP");
        listing.Style.Should().Be(Style.Series);
        listing.Category.Should().Be(Category.PriceChannel);
        listing.MethodName.Should().Be("ToVwap");

        listing.Parameters?.Count.Should().Be(1);
        // No parameters for this indicator

        listing.Results.Should().NotBeNull();
        listing.Results.Should().HaveCount(1);

        IndicatorResult vwapResult = listing.Results.SingleOrDefault(static r => r.DataName == "Vwap");
        vwapResult.Should().NotBeNull();
        vwapResult?.DisplayName.Should().Be("VWAP");
        vwapResult.IsReusable.Should().Be(true);
    }

    [TestMethod]
    public void VwapBuffer_InCatalog_ReturnsAllVariants()
    {
        // Act
        IndicatorListing listing = Vwap.BufferListing;

        // Assert
        listing.Should().NotBeNull();
        listing.Name.Should().Be("Volume Weighted Average Price");
        listing.Uiid.Should().Be("VWAP");
        listing.Style.Should().Be(Style.Buffer);
        listing.Category.Should().Be(Category.PriceChannel);
        listing.MethodName.Should().Be("ToVwapList");

        listing.Parameters?.Count.Should().Be(1);

        listing.Results.Should().NotBeNull();
        listing.Results.Should().HaveCount(1);
    }

    [TestMethod]
    public void VwapStream_InCatalog_ReturnsAllVariants()
    {
        // Act
        IndicatorListing listing = Vwap.StreamListing;

        // Assert
        listing.Should().NotBeNull();
        listing.Name.Should().Be("Volume Weighted Average Price");
        listing.Uiid.Should().Be("VWAP");
        listing.Style.Should().Be(Style.Stream);
        listing.Category.Should().Be(Category.PriceChannel);
        listing.MethodName.Should().Be("ToVwapHub");

        listing.Parameters?.Count.Should().Be(1);

        listing.Results.Should().NotBeNull();
        listing.Results.Should().HaveCount(1);
    }
}
