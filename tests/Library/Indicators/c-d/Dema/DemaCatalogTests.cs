namespace Catalogging;

/// <summary>
/// Test class for Dema catalog functionality.
/// </summary>
[TestClass]
public class DemaCatalogTests : TestBase
{
    [TestMethod]
    public void DemaSeries_InCatalog_ReturnsAllVariants()
    {
        // Act
        IndicatorListing listing = Dema.SeriesListing;

        // Assert
        listing.Should().NotBeNull();
        listing.Name.Should().Be("Double Exponential Moving Average");
        listing.Uiid.Should().Be("DEMA");
        listing.Style.Should().Be(Style.Series);
        listing.Category.Should().Be(Category.MovingAverage);
        listing.MethodName.Should().Be("ToDema");

        listing.Parameters.Should().NotBeNull();
        listing.Parameters.Should().HaveCount(1);

        IndicatorParam lookbackPeriodsParam = listing.Parameters.SingleOrDefault(static p => p.ParameterName == "lookbackPeriods");
        lookbackPeriodsParam.Should().NotBeNull();

        listing.Results.Should().NotBeNull();
        listing.Results.Should().HaveCount(1);

        IndicatorResult demaResult = listing.Results.SingleOrDefault(static r => r.DataName == "Dema");
        demaResult.Should().NotBeNull();
        demaResult?.DisplayName.Should().Be("DEMA");
        demaResult.IsReusable.Should().Be(true);
    }

    [TestMethod]
    public void DemaBuffer_InCatalog_ReturnsAllVariants()
    {
        // Act
        IndicatorListing listing = Dema.BufferListing;

        // Assert
        listing.Should().NotBeNull();
        listing.Name.Should().Be("Double Exponential Moving Average");
        listing.Uiid.Should().Be("DEMA");
        listing.Style.Should().Be(Style.Buffer);
        listing.Category.Should().Be(Category.MovingAverage);
        listing.MethodName.Should().Be("ToDemaList");

        listing.Parameters.Should().NotBeNull();
        listing.Parameters.Should().HaveCount(1);

        IndicatorParam lookbackPeriodsParam = listing.Parameters.SingleOrDefault(static p => p.ParameterName == "lookbackPeriods");
        lookbackPeriodsParam.Should().NotBeNull();

        listing.Results.Should().NotBeNull();
        listing.Results.Should().HaveCount(1);

        IndicatorResult demaResult = listing.Results.SingleOrDefault(static r => r.DataName == "Dema");
        demaResult.Should().NotBeNull();
        demaResult?.DisplayName.Should().Be("DEMA");
        demaResult.IsReusable.Should().Be(true);
    }

    [TestMethod]
    public void DemaStream_InCatalog_ReturnsAllVariants()
    {
        // Act
        IndicatorListing listing = Dema.StreamListing;

        // Assert
        listing.Should().NotBeNull();
        listing.Name.Should().Be("Double Exponential Moving Average");
        listing.Uiid.Should().Be("DEMA");
        listing.Style.Should().Be(Style.Stream);
        listing.Category.Should().Be(Category.MovingAverage);
        listing.MethodName.Should().Be("ToDemaHub");

        listing.Parameters.Should().NotBeNull();
        listing.Parameters.Should().HaveCount(1);

        IndicatorParam lookbackPeriodsParam = listing.Parameters.SingleOrDefault(static p => p.ParameterName == "lookbackPeriods");
        lookbackPeriodsParam.Should().NotBeNull();

        listing.Results.Should().NotBeNull();
        listing.Results.Should().HaveCount(1);

        IndicatorResult demaResult = listing.Results.SingleOrDefault(static r => r.DataName == "Dema");
        demaResult.Should().NotBeNull();
        demaResult?.DisplayName.Should().Be("DEMA");
        demaResult.IsReusable.Should().Be(true);
    }
}
