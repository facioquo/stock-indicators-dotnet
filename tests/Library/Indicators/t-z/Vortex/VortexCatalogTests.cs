namespace Catalogging;

/// <summary>
/// Test class for Vortex catalog functionality.
/// </summary>
[TestClass]
public class VortexCatalogTests : TestBase
{
    [TestMethod]
    public void VortexSeries_InCatalog_ReturnsAllVariants()
    {
        // Act
        IndicatorListing listing = Vortex.SeriesListing;

        // Assert
        listing.Should().NotBeNull();
        listing.Name.Should().Be("Vortex Indicator");
        listing.Uiid.Should().Be("VORTEX");
        listing.Style.Should().Be(Style.Series);
        listing.Category.Should().Be(Category.PriceTrend);
        listing.MethodName.Should().Be("ToVortex");

        listing.Parameters.Should().NotBeNull();
        listing.Parameters.Should().HaveCount(1);

        IndicatorParam lookbackPeriodsParam = listing.Parameters.SingleOrDefault(static p => p.ParameterName == "lookbackPeriods");
        lookbackPeriodsParam.Should().NotBeNull();

        listing.Results.Should().NotBeNull();
        listing.Results.Should().HaveCount(2);

        IndicatorResult pviResult = listing.Results.SingleOrDefault(static r => r.DataName == "Pvi");
        pviResult.Should().NotBeNull();
        pviResult?.DisplayName.Should().Be("VI+");
        pviResult.IsReusable.Should().Be(true);
        IndicatorResult nviResult1 = listing.Results.SingleOrDefault(static r => r.DataName == "Nvi");
        nviResult1.Should().NotBeNull();
        nviResult1?.DisplayName.Should().Be("VI-");
        nviResult1.IsReusable.Should().Be(false);
    }

    [TestMethod]
    public void VortexBuffer_InCatalog_ReturnsAllVariants()
    {
        // Act
        IndicatorListing listing = Vortex.BufferListing;

        // Assert
        listing.Should().NotBeNull();
        listing.Name.Should().Be("Vortex Indicator");
        listing.Uiid.Should().Be("VORTEX");
        listing.Style.Should().Be(Style.Buffer);
        listing.Category.Should().Be(Category.PriceTrend);
        listing.MethodName.Should().Be("ToVortexList");

        listing.Parameters.Should().NotBeNull();
        listing.Parameters.Should().HaveCount(1);

        listing.Results.Should().NotBeNull();
        listing.Results.Should().HaveCount(2);
    }

    [TestMethod]
    public void VortexStream_InCatalog_ReturnsAllVariants()
    {
        // Act
        IndicatorListing listing = Vortex.StreamListing;

        // Assert
        listing.Should().NotBeNull();
        listing.Name.Should().Be("Vortex Indicator");
        listing.Uiid.Should().Be("VORTEX");
        listing.Style.Should().Be(Style.Stream);
        listing.Category.Should().Be(Category.PriceTrend);
        listing.MethodName.Should().Be("ToVortexHub");

        listing.Parameters.Should().NotBeNull();
        listing.Parameters.Should().HaveCount(1);

        listing.Results.Should().NotBeNull();
        listing.Results.Should().HaveCount(2);
    }
}
