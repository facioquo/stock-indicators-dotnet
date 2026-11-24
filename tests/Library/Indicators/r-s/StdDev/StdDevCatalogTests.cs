namespace Catalogging;

/// <summary>
/// Test class for StdDev catalog functionality.
/// </summary>
[TestClass]
public class StdDevCatalogTests : TestBase
{
    [TestMethod]
    public void StdDevSeries_InCatalog_ReturnsAllVariants()
    {
        // Act
        IndicatorListing listing = StdDev.SeriesListing;

        // Assert
        listing.Should().NotBeNull();
        listing.Name.Should().Be("Standard Deviation");
        listing.Uiid.Should().Be("STDEV");
        listing.Style.Should().Be(Style.Series);
        listing.Category.Should().Be(Category.PriceCharacteristic);
        listing.MethodName.Should().Be("ToStdDev");

        listing.Parameters.Should().NotBeNull();
        listing.Parameters.Should().HaveCount(1);

        IndicatorParam lookbackPeriodsParam = listing.Parameters.SingleOrDefault(static p => p.ParameterName == "lookbackPeriods");
        lookbackPeriodsParam.Should().NotBeNull();

        listing.Results.Should().NotBeNull();
        listing.Results.Should().HaveCount(3);

        IndicatorResult stddevResult = listing.Results.SingleOrDefault(static r => r.DataName == "StdDev");
        stddevResult.Should().NotBeNull();
        stddevResult?.DisplayName.Should().Be("Standard Deviation");
        stddevResult.IsReusable.Should().Be(true);
        IndicatorResult meanResult = listing.Results.SingleOrDefault(static r => r.DataName == "Mean");
        meanResult.Should().NotBeNull();
        meanResult?.DisplayName.Should().Be("Mean");
        meanResult.IsReusable.Should().Be(false);
        IndicatorResult zScoreResult = listing.Results.SingleOrDefault(static r => r.DataName == "ZScore");
        zScoreResult.Should().NotBeNull();
        zScoreResult?.DisplayName.Should().Be("Z-Score");
        zScoreResult.IsReusable.Should().Be(false);
    }

    [TestMethod]
    public void StdDevStream_InCatalog_ReturnsAllVariants()
    {
        // Act
        IndicatorListing listing = StdDev.StreamListing;

        // Assert
        listing.Should().NotBeNull();
        listing.Name.Should().Be("Standard Deviation");
        listing.Uiid.Should().Be("STDEV");
        listing.Style.Should().Be(Style.Stream);
        listing.Category.Should().Be(Category.PriceCharacteristic);
        listing.MethodName.Should().Be("ToStdDevHub");

        listing.Parameters.Should().NotBeNull();
        listing.Parameters.Should().HaveCount(1);

        IndicatorParam lookbackPeriodsParam = listing.Parameters.SingleOrDefault(static p => p.ParameterName == "lookbackPeriods");
        lookbackPeriodsParam.Should().NotBeNull();

        listing.Results.Should().NotBeNull();
        listing.Results.Should().HaveCount(3);

        IndicatorResult stddevResult = listing.Results.SingleOrDefault(static r => r.DataName == "StdDev");
        stddevResult.Should().NotBeNull();
        stddevResult!.DisplayName.Should().Be("Standard Deviation");
        stddevResult.IsReusable.Should().Be(true);
        IndicatorResult meanResult = listing.Results.SingleOrDefault(static r => r.DataName == "Mean");
        meanResult.Should().NotBeNull();
        meanResult?.DisplayName.Should().Be("Mean");
        meanResult.IsReusable.Should().Be(false);
        IndicatorResult zScoreResult = listing.Results.SingleOrDefault(static r => r.DataName == "ZScore");
        zScoreResult.Should().NotBeNull();
        zScoreResult?.DisplayName.Should().Be("Z-Score");
        zScoreResult.IsReusable.Should().Be(false);
    }

    [TestMethod]
    public void StdDevBuffer_InCatalog_ReturnsAllVariants()
    {
        // Act
        IndicatorListing listing = StdDev.BufferListing;

        // Assert
        listing.Should().NotBeNull();
        listing.Name.Should().Be("Standard Deviation");
        listing.Uiid.Should().Be("STDEV");
        listing.Style.Should().Be(Style.Buffer);
        listing.Category.Should().Be(Category.PriceCharacteristic);
        listing.MethodName.Should().Be("ToStdDevList");

        listing.Parameters.Should().NotBeNull();
        listing.Parameters.Should().HaveCount(1);

        IndicatorParam lookbackPeriodsParam = listing.Parameters.SingleOrDefault(static p => p.ParameterName == "lookbackPeriods");
        lookbackPeriodsParam.Should().NotBeNull();

        listing.Results.Should().NotBeNull();
        listing.Results.Should().HaveCount(3);

        IndicatorResult stddevResult = listing.Results.SingleOrDefault(static r => r.DataName == "StdDev");
        stddevResult.Should().NotBeNull();
        stddevResult!.DisplayName.Should().Be("Standard Deviation");
        stddevResult.IsReusable.Should().Be(true);
        IndicatorResult meanResult = listing.Results.SingleOrDefault(static r => r.DataName == "Mean");
        meanResult.Should().NotBeNull();
        meanResult?.DisplayName.Should().Be("Mean");
        meanResult.IsReusable.Should().Be(false);
        IndicatorResult zScoreResult = listing.Results.SingleOrDefault(static r => r.DataName == "ZScore");
        zScoreResult.Should().NotBeNull();
        zScoreResult?.DisplayName.Should().Be("Z-Score");
        zScoreResult.IsReusable.Should().Be(false);
    }
}
