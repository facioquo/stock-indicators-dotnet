namespace Catalogging;

/// <summary>
/// Test class for Obv catalog functionality.
/// </summary>
[TestClass]
public class ObvCatalogTests : TestBase
{
    [TestMethod]
    public void ObvSeries_InCatalog_ReturnsAllVariants()
    {
        // Act
        IndicatorListing listing = Obv.SeriesListing;

        // Assert
        listing.Should().NotBeNull();
        listing.Name.Should().Be("On-Balance Volume");
        listing.Uiid.Should().Be("OBV");
        listing.Style.Should().Be(Style.Series);
        listing.Category.Should().Be(Category.VolumeBased);
        listing.MethodName.Should().Be("ToObv");

        listing.Parameters?.Count.Should().Be(0);
        // No parameters for this indicator

        listing.Results.Should().NotBeNull();
        listing.Results.Should().HaveCount(1);

        IndicatorResult obvResult = listing.Results.SingleOrDefault(static r => r.DataName == "Obv");
        obvResult.Should().NotBeNull();
        obvResult?.DisplayName.Should().Be("OBV");
        obvResult.IsReusable.Should().Be(true);
    }

    [TestMethod]
    public void ObvStream_InCatalog_ReturnsAllVariants()
    {
        // Act
        IndicatorListing listing = Obv.StreamListing;

        // Assert
        listing.Should().NotBeNull();
        listing.Name.Should().Be("On-Balance Volume");
        listing.Uiid.Should().Be("OBV");
        listing.Style.Should().Be(Style.Stream);
        listing.Category.Should().Be(Category.VolumeBased);
        listing.MethodName.Should().Be("ToObvHub");

        listing.Parameters?.Count.Should().Be(0);

        listing.Results.Should().NotBeNull();
        listing.Results.Should().HaveCount(1);
    }

    [TestMethod]
    public void ObvBuffer_InCatalog_ReturnsAllVariants()
    {
        // Act
        IndicatorListing listing = Obv.BufferListing;

        // Assert
        listing.Should().NotBeNull();
        listing.Name.Should().Be("On-Balance Volume");
        listing.Uiid.Should().Be("OBV");
        listing.Style.Should().Be(Style.Buffer);
        listing.Category.Should().Be(Category.VolumeBased);
        listing.MethodName.Should().Be("ToObvList");

        listing.Parameters?.Count.Should().Be(0);

        listing.Results.Should().NotBeNull();
        listing.Results.Should().HaveCount(1);
    }
}
