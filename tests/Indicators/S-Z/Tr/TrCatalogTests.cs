namespace Catalogging;

/// <summary>
/// Test class for Tr catalog functionality.
/// </summary>
[TestClass]
public class TrCatalogTests : TestBase
{
    [TestMethod]
    public void TrSeries_InCatalog_ReturnsAllVariants()
    {
        // Act
        IndicatorListing listing = Tr.SeriesListing;

        // Assert
        listing.Should().NotBeNull();
        listing.Name.Should().Be("True Range");
        listing.Uiid.Should().Be("TR");
        listing.Style.Should().Be(Style.Series);
        listing.Category.Should().Be(Category.PriceCharacteristic);
        listing.MethodName.Should().Be("ToTr");

        listing.Parameters?.Count.Should().Be(0);
        // No parameters for this indicator

        listing.Results.Should().NotBeNull();
        listing.Results.Should().HaveCount(1);

        IndicatorResult trResult = listing.Results.SingleOrDefault(static r => r.DataName == "Tr");
        trResult.Should().NotBeNull();
        trResult?.DisplayName.Should().Be("True Range");
        trResult.IsReusable.Should().Be(true);
    }
    [TestMethod]
    public void TrStream_InCatalog_ReturnsAllVariants()
    {
        // Act
        IndicatorListing listing = Tr.StreamListing;

        // Assert
        listing.Should().NotBeNull();
        listing.Name.Should().Be("True Range");
        listing.Uiid.Should().Be("TR");
        listing.Style.Should().Be(Style.Stream);
        listing.Category.Should().Be(Category.PriceCharacteristic);
        listing.MethodName.Should().Be("ToTrHub");

        listing.Parameters?.Count.Should().Be(0);
        // No parameters for this indicator

        listing.Results.Should().NotBeNull();
        listing.Results.Should().HaveCount(1);

        IndicatorResult trResult = listing.Results.SingleOrDefault(static r => r.DataName == "Tr");
        trResult.Should().NotBeNull();
        trResult?.DisplayName.Should().Be("True Range");
        trResult.IsReusable.Should().Be(true);
    }

    [TestMethod]
    public void TrBuffer_InCatalog_ReturnsAllVariants()
    {
        // Act
        IndicatorListing listing = Tr.BufferListing;

        // Assert
        listing.Should().NotBeNull();
        listing.Name.Should().Be("True Range");
        listing.Uiid.Should().Be("TR");
        listing.Style.Should().Be(Style.Buffer);
        listing.Category.Should().Be(Category.PriceCharacteristic);
        listing.MethodName.Should().Be("ToTrList");

        listing.Parameters?.Count.Should().Be(0);
        // No parameters for this indicator

        listing.Results.Should().NotBeNull();
        listing.Results.Should().HaveCount(1);

        IndicatorResult trResult = listing.Results.SingleOrDefault(static r => r.DataName == "Tr");
        trResult.Should().NotBeNull();
        trResult?.DisplayName.Should().Be("True Range");
        trResult.IsReusable.Should().Be(true);
    }
}
