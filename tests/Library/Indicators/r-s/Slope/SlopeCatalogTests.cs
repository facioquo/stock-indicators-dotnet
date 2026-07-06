namespace Catalogging;

/// <summary>
/// Test class for Slope catalog functionality.
/// </summary>
[TestClass]
public class SlopeCatalogTests : TestBase
{
    [TestMethod]
    public void SlopeSeries_InCatalog_ReturnsAllVariants()
    {
        // Act
        IndicatorListing listing = Slope.SeriesListing;

        // Assert
        listing.Should().NotBeNull();
        listing.Name.Should().Be("Slope");
        listing.Uiid.Should().Be("SLOPE");
        listing.Style.Should().Be(Style.Series);
        listing.Category.Should().Be(Category.PriceCharacteristic);
        listing.MethodName.Should().Be("ToSlope");

        listing.Parameters.Should().NotBeNull();
        listing.Parameters.Should().HaveCount(1);

        IndicatorParam lookbackPeriodsParam = listing.Parameters.SingleOrDefault(static p => p.ParameterName == "lookbackPeriods");
        lookbackPeriodsParam.Should().NotBeNull();

        listing.Results.Should().NotBeNull();
        listing.Results.Should().HaveCount(5);

        IndicatorResult slopeResult = listing.Results.SingleOrDefault(static r => r.DataName == "Slope");
        slopeResult.Should().NotBeNull();
        slopeResult?.DisplayName.Should().Be("Slope");
        slopeResult.IsReusable.Should().Be(true);
        IndicatorResult interceptResult1 = listing.Results.SingleOrDefault(static r => r.DataName == "Intercept");
        interceptResult1.Should().NotBeNull();
        interceptResult1?.DisplayName.Should().Be("Intercept");
        interceptResult1.IsReusable.Should().Be(false);
        IndicatorResult stddevResult2 = listing.Results.SingleOrDefault(static r => r.DataName == "StdDev");
        stddevResult2.Should().NotBeNull();
        stddevResult2?.DisplayName.Should().Be("Standard deviation");
        stddevResult2.IsReusable.Should().Be(false);
        IndicatorResult rsquaredResult3 = listing.Results.SingleOrDefault(static r => r.DataName == "RSquared");
        rsquaredResult3.Should().NotBeNull();
        rsquaredResult3?.DisplayName.Should().Be("R-squared");
        rsquaredResult3.IsReusable.Should().Be(false);
        IndicatorResult lineResult4 = listing.Results.SingleOrDefault(static r => r.DataName == "Line");
        lineResult4.Should().NotBeNull();
        lineResult4?.DisplayName.Should().Be("Line");
        lineResult4.IsReusable.Should().Be(false);
    }

    [TestMethod]
    public void SlopeStream_InCatalog_ReturnsAllVariants()
    {
        // Act
        IndicatorListing listing = Slope.StreamListing;

        // Assert
        listing.Should().NotBeNull();
        listing.Name.Should().Be("Slope");
        listing.Uiid.Should().Be("SLOPE");
        listing.Style.Should().Be(Style.Stream);
        listing.Category.Should().Be(Category.PriceCharacteristic);
        listing.MethodName.Should().Be("ToSlopeHub");

        listing.Parameters.Should().NotBeNull();
        listing.Parameters.Should().HaveCount(1);

        IndicatorParam lookbackPeriodsParam = listing.Parameters.SingleOrDefault(static p => p.ParameterName == "lookbackPeriods");
        lookbackPeriodsParam.Should().NotBeNull();

        listing.Results.Should().NotBeNull();
        listing.Results.Should().HaveCount(5);

        IndicatorResult slopeResult = listing.Results.SingleOrDefault(static r => r.DataName == "Slope");
        slopeResult.Should().NotBeNull();
        slopeResult?.DisplayName.Should().Be("Slope");
        slopeResult?.IsReusable.Should().Be(true);
        IndicatorResult interceptResult1 = listing.Results.SingleOrDefault(static r => r.DataName == "Intercept");
        interceptResult1.Should().NotBeNull();
        interceptResult1?.DisplayName.Should().Be("Intercept");
        interceptResult1?.IsReusable.Should().Be(false);
        IndicatorResult stddevResult2 = listing.Results.SingleOrDefault(static r => r.DataName == "StdDev");
        stddevResult2.Should().NotBeNull();
        stddevResult2?.DisplayName.Should().Be("Standard deviation");
        stddevResult2?.IsReusable.Should().Be(false);
        IndicatorResult rsquaredResult3 = listing.Results.SingleOrDefault(static r => r.DataName == "RSquared");
        rsquaredResult3.Should().NotBeNull();
        rsquaredResult3?.DisplayName.Should().Be("R-squared");
        rsquaredResult3?.IsReusable.Should().Be(false);
        IndicatorResult lineResult4 = listing.Results.SingleOrDefault(static r => r.DataName == "Line");
        lineResult4.Should().NotBeNull();
        lineResult4?.DisplayName.Should().Be("Line");
        lineResult4?.IsReusable.Should().Be(false);
    }

    [TestMethod]
    public void SlopeBuffer_InCatalog_ReturnsAllVariants()
    {
        // Act
        IndicatorListing listing = Slope.BufferListing;

        // Assert
        listing.Should().NotBeNull();
        listing.Name.Should().Be("Slope");
        listing.Uiid.Should().Be("SLOPE");
        listing.Style.Should().Be(Style.Buffer);
        listing.Category.Should().Be(Category.PriceCharacteristic);
        listing.MethodName.Should().Be("ToSlopeList");

        listing.Parameters.Should().NotBeNull();
        listing.Parameters.Should().HaveCount(1);

        IndicatorParam lookbackPeriodsParam = listing.Parameters.SingleOrDefault(static p => p.ParameterName == "lookbackPeriods");
        lookbackPeriodsParam.Should().NotBeNull();

        listing.Results.Should().NotBeNull();
        listing.Results.Should().HaveCount(5);

        IndicatorResult slopeResult = listing.Results.SingleOrDefault(static r => r.DataName == "Slope");
        slopeResult.Should().NotBeNull();
        slopeResult?.DisplayName.Should().Be("Slope");
        slopeResult?.IsReusable.Should().Be(true);
        IndicatorResult interceptResult1 = listing.Results.SingleOrDefault(static r => r.DataName == "Intercept");
        interceptResult1.Should().NotBeNull();
        interceptResult1?.DisplayName.Should().Be("Intercept");
        interceptResult1?.IsReusable.Should().Be(false);
        IndicatorResult stddevResult2 = listing.Results.SingleOrDefault(static r => r.DataName == "StdDev");
        stddevResult2.Should().NotBeNull();
        stddevResult2?.DisplayName.Should().Be("Standard deviation");
        stddevResult2?.IsReusable.Should().Be(false);
        IndicatorResult rsquaredResult3 = listing.Results.SingleOrDefault(static r => r.DataName == "RSquared");
        rsquaredResult3.Should().NotBeNull();
        rsquaredResult3?.DisplayName.Should().Be("R-squared");
        rsquaredResult3?.IsReusable.Should().Be(false);
        IndicatorResult lineResult4 = listing.Results.SingleOrDefault(static r => r.DataName == "Line");
        lineResult4.Should().NotBeNull();
        lineResult4?.DisplayName.Should().Be("Line");
        lineResult4?.IsReusable.Should().Be(false);
    }
}
