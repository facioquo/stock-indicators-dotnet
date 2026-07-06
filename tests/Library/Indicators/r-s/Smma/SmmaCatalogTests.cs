namespace Catalogging;

/// <summary>
/// Test class for Smma catalog functionality.
/// </summary>
[TestClass]
public class SmmaCatalogTests : TestBase
{
    [TestMethod]
    public void SmmaSeries_InCatalog_ReturnsAllVariants()
    {
        // Act
        IndicatorListing listing = Smma.SeriesListing;

        // Assert
        listing.Should().NotBeNull();
        listing.Name.Should().Be("Smoothed Moving Average");
        listing.Uiid.Should().Be("SMMA");
        listing.Style.Should().Be(Style.Series);
        listing.Category.Should().Be(Category.MovingAverage);
        listing.MethodName.Should().Be("ToSmma");

        listing.Parameters.Should().NotBeNull();
        listing.Parameters.Should().HaveCount(1);

        IndicatorParam lookbackPeriodsParam = listing.Parameters.SingleOrDefault(static p => p.ParameterName == "lookbackPeriods");
        lookbackPeriodsParam.Should().NotBeNull();

        listing.Results.Should().NotBeNull();
        listing.Results.Should().HaveCount(1);

        IndicatorResult smmaResult = listing.Results.SingleOrDefault(static r => r.DataName == "Smma");
        smmaResult.Should().NotBeNull();
        smmaResult?.DisplayName.Should().Be("SMMA");
        smmaResult.IsReusable.Should().Be(true);
    }

    [TestMethod]
    public void SmmaStream_InCatalog_ReturnsAllVariants()
    {
        // Act
        IndicatorListing listing = Smma.StreamListing;

        // Assert
        listing.Should().NotBeNull();
        listing.Name.Should().Be("Smoothed Moving Average");
        listing.Uiid.Should().Be("SMMA");
        listing.Style.Should().Be(Style.Stream);
        listing.Category.Should().Be(Category.MovingAverage);
        listing.MethodName.Should().Be("ToSmmaHub");

        listing.Parameters.Should().NotBeNull();
        listing.Parameters.Should().HaveCount(1);

        IndicatorParam lookbackPeriodsParam = listing.Parameters.SingleOrDefault(static p => p.ParameterName == "lookbackPeriods");
        lookbackPeriodsParam.Should().NotBeNull();

        listing.Results.Should().NotBeNull();
        listing.Results.Should().HaveCount(1);

        IndicatorResult smmaResult = listing.Results.SingleOrDefault(static r => r.DataName == "Smma");
        smmaResult.Should().NotBeNull();
        smmaResult?.DisplayName.Should().Be("SMMA");
        smmaResult.IsReusable.Should().Be(true);
    }

    [TestMethod]
    public void SmmaBuffer_InCatalog_ReturnsAllVariants()
    {
        // Act
        IndicatorListing listing = Smma.BufferListing;

        // Assert
        listing.Should().NotBeNull();
        listing.Name.Should().Be("Smoothed Moving Average");
        listing.Uiid.Should().Be("SMMA");
        listing.Style.Should().Be(Style.Buffer);
        listing.Category.Should().Be(Category.MovingAverage);
        listing.MethodName.Should().Be("ToSmmaList");

        listing.Parameters.Should().NotBeNull();
        listing.Parameters.Should().HaveCount(1);

        IndicatorParam lookbackPeriodsParam = listing.Parameters.SingleOrDefault(static p => p.ParameterName == "lookbackPeriods");
        lookbackPeriodsParam.Should().NotBeNull();

        listing.Results.Should().NotBeNull();
        listing.Results.Should().HaveCount(1);

        IndicatorResult smmaResult = listing.Results.SingleOrDefault(static r => r.DataName == "Smma");
        smmaResult.Should().NotBeNull();
        smmaResult?.DisplayName.Should().Be("SMMA");
        smmaResult.IsReusable.Should().Be(true);
    }
}
