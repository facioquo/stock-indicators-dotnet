namespace Catalogging;

/// <summary>
/// Test class for Wma catalog functionality.
/// </summary>
[TestClass]
public class WmaCatalogTests : TestBase
{
    [TestMethod]
    public void WmaSeries_InCatalog_ReturnsAllVariants()
    {
        // Act
        IndicatorListing listing = Wma.SeriesListing;

        // Assert
        listing.Should().NotBeNull();
        listing.Name.Should().Be("Weighted Moving Average");
        listing.Uiid.Should().Be("WMA");
        listing.Style.Should().Be(Style.Series);
        listing.Category.Should().Be(Category.MovingAverage);
        listing.MethodName.Should().Be("ToWma");

        listing.Parameters.Should().NotBeNull();
        listing.Parameters.Should().HaveCount(1);

        IndicatorParam lookbackPeriodsParam = listing.Parameters.SingleOrDefault(static p => p.ParameterName == "lookbackPeriods");
        lookbackPeriodsParam.Should().NotBeNull();

        listing.Results.Should().NotBeNull();
        listing.Results.Should().HaveCount(1);

        IndicatorResult wmaResult = listing.Results.SingleOrDefault(static r => r.DataName == "Wma");
        wmaResult.Should().NotBeNull();
        wmaResult?.DisplayName.Should().Be("WMA");
        wmaResult.IsReusable.Should().Be(true);
    }

    [TestMethod]
    public void WmaStream_InCatalog_ReturnsAllVariants()
    {
        // Act
        IndicatorListing listing = Wma.StreamListing;

        // Assert
        listing.Should().NotBeNull();
        listing.Name.Should().Be("Weighted Moving Average");
        listing.Uiid.Should().Be("WMA");
        listing.Style.Should().Be(Style.Stream);
        listing.Category.Should().Be(Category.MovingAverage);
        listing.MethodName.Should().Be("ToWmaHub");

        listing.Parameters.Should().NotBeNull();
        listing.Parameters.Should().HaveCount(1);

        listing.Results.Should().NotBeNull();
        listing.Results.Should().HaveCount(1);
    }

    [TestMethod]
    public void WmaBuffer_InCatalog_ReturnsAllVariants()
    {
        // Act
        IndicatorListing listing = Wma.BufferListing;

        // Assert
        listing.Should().NotBeNull();
        listing.Name.Should().Be("Weighted Moving Average");
        listing.Uiid.Should().Be("WMA");
        listing.Style.Should().Be(Style.Buffer);
        listing.Category.Should().Be(Category.MovingAverage);
        listing.MethodName.Should().Be("ToWmaList");

        listing.Parameters.Should().NotBeNull();
        listing.Parameters.Should().HaveCount(1);

        listing.Results.Should().NotBeNull();
        listing.Results.Should().HaveCount(1);
    }
}
