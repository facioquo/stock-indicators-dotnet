namespace Catalogging;

/// <summary>
/// Test class for Vwma catalog functionality.
/// </summary>
[TestClass]
public class VwmaCatalogTests : TestBase
{
    [TestMethod]
    public void VwmaSeries_InCatalog_ReturnsAllVariants()
    {
        // Act
        IndicatorListing listing = Vwma.SeriesListing;

        // Assert
        listing.Should().NotBeNull();
        listing.Name.Should().Be("Volume Weighted Moving Average");
        listing.Uiid.Should().Be("VWMA");
        listing.Style.Should().Be(Style.Series);
        listing.Category.Should().Be(Category.MovingAverage);
        listing.MethodName.Should().Be("ToVwma");

        listing.Parameters.Should().NotBeNull();
        listing.Parameters.Should().HaveCount(1);

        IndicatorParam lookbackPeriodsParam = listing.Parameters.SingleOrDefault(static p => p.ParameterName == "lookbackPeriods");
        lookbackPeriodsParam.Should().NotBeNull();

        listing.Results.Should().NotBeNull();
        listing.Results.Should().HaveCount(1);

        IndicatorResult vwmaResult = listing.Results.SingleOrDefault(static r => r.DataName == "Vwma");
        vwmaResult.Should().NotBeNull();
        vwmaResult?.DisplayName.Should().Be("VWMA");
        vwmaResult.IsReusable.Should().Be(true);
    }
}
