namespace Catalogging;

/// <summary>
/// Test class for VolatilityStop catalog functionality.
/// </summary>
[TestClass]
public class VolatilityStopCatalogTests : TestBase
{
    [TestMethod]
    public void VolatilityStopSeries_InCatalog_ReturnsAllVariants()
    {
        // Act
        IndicatorListing listing = VolatilityStop.SeriesListing;

        // Assert
        listing.Should().NotBeNull();
        listing.Name.Should().Be("Volatility Stop");
        listing.Uiid.Should().Be("VOL-STOP");
        listing.Style.Should().Be(Style.Series);
        listing.Category.Should().Be(Category.StopAndReverse);
        listing.MethodName.Should().Be("ToVolatilityStop");

        listing.Parameters.Should().NotBeNull();
        listing.Parameters.Should().HaveCount(2);

        IndicatorParam lookbackPeriodsParam = listing.Parameters.SingleOrDefault(static p => p.ParameterName == "lookbackPeriods");
        lookbackPeriodsParam.Should().NotBeNull();
        IndicatorParam multiplierParam1 = listing.Parameters.SingleOrDefault(static p => p.ParameterName == "multiplier");
        multiplierParam1.Should().NotBeNull();

        listing.Results.Should().NotBeNull();
        listing.Results.Should().HaveCount(2);

        IndicatorResult sarResult = listing.Results.SingleOrDefault(static r => r.DataName == "Sar");
        sarResult.Should().NotBeNull();
        sarResult?.DisplayName.Should().Be("Stop and Reverse");
        sarResult.IsReusable.Should().Be(true);
        IndicatorResult isstopResult1 = listing.Results.SingleOrDefault(static r => r.DataName == "IsStop");
        isstopResult1.Should().NotBeNull();
        isstopResult1?.DisplayName.Should().Be("Is Stop");
        isstopResult1.IsReusable.Should().Be(false);
    }

    [TestMethod]
    public void VolatilityStopBuffer_InCatalog_ReturnsAllVariants()
    {
        // Act
        IndicatorListing listing = VolatilityStop.BufferListing;

        // Assert
        listing.Should().NotBeNull();
        listing.Name.Should().Be("Volatility Stop");
        listing.Uiid.Should().Be("VOL-STOP");
        listing.Style.Should().Be(Style.Buffer);
        listing.Category.Should().Be(Category.StopAndReverse);
        listing.MethodName.Should().Be("ToVolatilityStopList");

        listing.Parameters.Should().NotBeNull();
        listing.Parameters.Should().HaveCount(2);

        listing.Results.Should().NotBeNull();
        listing.Results.Should().HaveCount(2);
    }

    [TestMethod]
    public void VolatilityStopStream_InCatalog_ReturnsAllVariants()
    {
        // Act
        IndicatorListing listing = VolatilityStop.StreamListing;

        // Assert
        listing.Should().NotBeNull();
        listing.Name.Should().Be("Volatility Stop");
        listing.Uiid.Should().Be("VOL-STOP");
        listing.Style.Should().Be(Style.Stream);
        listing.Category.Should().Be(Category.StopAndReverse);
        listing.MethodName.Should().Be("ToVolatilityStopHub");

        listing.Parameters.Should().NotBeNull();
        listing.Parameters.Should().HaveCount(2);

        IndicatorParam lookbackPeriodsParam = listing.Parameters.SingleOrDefault(static p => p.ParameterName == "lookbackPeriods");
        lookbackPeriodsParam.Should().NotBeNull();
        IndicatorParam multiplierParam = listing.Parameters.SingleOrDefault(static p => p.ParameterName == "multiplier");
        multiplierParam.Should().NotBeNull();

        listing.Results.Should().NotBeNull();
        listing.Results.Should().HaveCount(2);

        IndicatorResult sarResult = listing.Results.SingleOrDefault(static r => r.DataName == "Sar");
        sarResult.Should().NotBeNull();
        sarResult?.DisplayName.Should().Be("Stop and Reverse");
        sarResult.IsReusable.Should().Be(true);
        IndicatorResult isStopResult = listing.Results.SingleOrDefault(static r => r.DataName == "IsStop");
        isStopResult.Should().NotBeNull();
        isStopResult?.DisplayName.Should().Be("Is Stop");
        isStopResult.IsReusable.Should().Be(false);
    }
}
