namespace Catalogging;

/// <summary>
/// Test class for Adl catalog functionality.
/// </summary>
[TestClass]
public class AdlCatalogTests : TestBase
{
    [TestMethod]
    public void AdlSeries_InCatalog_ReturnsAllVariants()
    {
        // Act
        IndicatorListing listing = Adl.SeriesListing;

        // Assert
        listing.Should().NotBeNull();
        listing.Name.Should().Be("Accumulation Distribution Line (ADL)");
        listing.Uiid.Should().Be("ADL");
        listing.Style.Should().Be(Style.Series);
        listing.Category.Should().Be(Category.VolumeBased);
        listing.MethodName.Should().Be("ToAdl");

        listing.Parameters?.Count.Should().Be(0);
        // No parameters for this indicator

        listing.Results.Should().NotBeNull();
        listing.Results.Should().HaveCount(1);

        IndicatorResult adlResult = listing.Results.SingleOrDefault(static r => r.DataName == "Adl");
        adlResult.Should().NotBeNull();
        adlResult?.DisplayName.Should().Be("Accumulation Distribution Line (ADL)");
        adlResult.IsReusable.Should().Be(true);
    }
    [TestMethod]
    public void AdlStream_InCatalog_ReturnsAllVariants()
    {
        // Act
        IndicatorListing listing = Adl.StreamListing;

        // Assert
        listing.Should().NotBeNull();
        listing.Name.Should().Be("Accumulation Distribution Line (ADL)");
        listing.Uiid.Should().Be("ADL");
        listing.Style.Should().Be(Style.Stream);
        listing.Category.Should().Be(Category.VolumeBased);
        listing.MethodName.Should().Be("ToAdlHub");

        listing.Parameters?.Count.Should().Be(0);
        // No parameters for this indicator

        listing.Results.Should().NotBeNull();
        listing.Results.Should().HaveCount(1);

        IndicatorResult adlResult = listing.Results.SingleOrDefault(static r => r.DataName == "Adl");
        adlResult.Should().NotBeNull();
        adlResult?.DisplayName.Should().Be("Accumulation Distribution Line (ADL)");
        adlResult.IsReusable.Should().Be(true);
    }

    [TestMethod]
    public void AdlBuffer_InCatalog_ReturnsAllVariants()
    {
        // Act
        IndicatorListing listing = Adl.BufferListing;

        // Assert
        listing.Should().NotBeNull();
        listing.Name.Should().Be("Accumulation Distribution Line (ADL)");
        listing.Uiid.Should().Be("ADL");
        listing.Style.Should().Be(Style.Buffer);
        listing.Category.Should().Be(Category.VolumeBased);
        listing.MethodName.Should().Be("ToAdlList");

        listing.Parameters?.Count.Should().Be(0);
        // No parameters for this indicator

        listing.Results.Should().NotBeNull();
        listing.Results.Should().HaveCount(1);

        IndicatorResult adlResult = listing.Results.SingleOrDefault(static r => r.DataName == "Adl");
        adlResult.Should().NotBeNull();
        adlResult?.DisplayName.Should().Be("Accumulation Distribution Line (ADL)");
        adlResult.IsReusable.Should().Be(true);
    }
}
