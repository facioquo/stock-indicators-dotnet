namespace Catalogging;

/// <summary>
/// Test class for WilliamsR catalog functionality.
/// </summary>
[TestClass]
public class WilliamsRCatalogTests : TestBase
{
    [TestMethod]
    public void WilliamsRSeries_InCatalog_ReturnsAllVariants()
    {
        // Act
        IndicatorListing listing = WilliamsR.SeriesListing;

        // Assert
        listing.Should().NotBeNull();
        listing.Name.Should().Be("Williams %R");
        listing.Uiid.Should().Be("WILLR");
        listing.Style.Should().Be(Style.Series);
        listing.Category.Should().Be(Category.Oscillator);
        listing.MethodName.Should().Be("ToWilliamsR");

        listing.Parameters.Should().NotBeNull();
        listing.Parameters.Should().HaveCount(1);

        IndicatorParam lookbackPeriodsParam = listing.Parameters.SingleOrDefault(static p => p.ParameterName == "lookbackPeriods");
        lookbackPeriodsParam.Should().NotBeNull();

        listing.Results.Should().NotBeNull();
        listing.Results.Should().HaveCount(1);

        IndicatorResult williamsrResult = listing.Results.SingleOrDefault(static r => r.DataName == "WilliamsR");
        williamsrResult.Should().NotBeNull();
        williamsrResult?.DisplayName.Should().Be("Williams %R");
        williamsrResult.IsReusable.Should().Be(true);
    }

    [TestMethod]
    public void WilliamsRBuffer_InCatalog_ReturnsAllVariants()
    {
        // Act
        IndicatorListing listing = WilliamsR.BufferListing;

        // Assert
        listing.Should().NotBeNull();
        listing.Name.Should().Be("Williams %R");
        listing.Uiid.Should().Be("WILLR");
        listing.Style.Should().Be(Style.Buffer);
        listing.Category.Should().Be(Category.Oscillator);
        listing.MethodName.Should().Be("ToWilliamsRList");

        listing.Parameters.Should().NotBeNull();
        listing.Parameters.Should().HaveCount(1);

        IndicatorParam lookbackPeriodsParam = listing.Parameters.SingleOrDefault(static p => p.ParameterName == "lookbackPeriods");
        lookbackPeriodsParam.Should().NotBeNull();

        listing.Results.Should().NotBeNull();
        listing.Results.Should().HaveCount(1);

        IndicatorResult williamsrResult = listing.Results.SingleOrDefault(static r => r.DataName == "WilliamsR");
        williamsrResult.Should().NotBeNull();
        williamsrResult?.DisplayName.Should().Be("Williams %R");
        williamsrResult.IsReusable.Should().Be(true);
    }

    [TestMethod]
    public void WilliamsRStream_InCatalog_ReturnsAllVariants()
    {
        // Act
        IndicatorListing listing = WilliamsR.StreamListing;

        // Assert
        listing.Should().NotBeNull();
        listing.Name.Should().Be("Williams %R");
        listing.Uiid.Should().Be("WILLR");
        listing.Style.Should().Be(Style.Stream);
        listing.Category.Should().Be(Category.Oscillator);
        listing.MethodName.Should().Be("ToWilliamsRHub");

        listing.Parameters.Should().NotBeNull();
        listing.Parameters.Should().HaveCount(1);

        IndicatorParam lookbackPeriodsParam = listing.Parameters.SingleOrDefault(static p => p.ParameterName == "lookbackPeriods");
        lookbackPeriodsParam.Should().NotBeNull();

        listing.Results.Should().NotBeNull();
        listing.Results.Should().HaveCount(1);

        IndicatorResult williamsrResult = listing.Results.SingleOrDefault(static r => r.DataName == "WilliamsR");
        williamsrResult.Should().NotBeNull();
        williamsrResult?.DisplayName.Should().Be("Williams %R");
        williamsrResult.IsReusable.Should().Be(true);
    }
}
