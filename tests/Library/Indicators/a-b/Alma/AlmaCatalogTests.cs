namespace Catalogging;

/// <summary>
/// Test class for Alma catalog functionality.
/// </summary>
[TestClass]
public class AlmaCatalogTests : TestBase
{
    [TestMethod]
    public void AlmaSeries_InCatalog_ReturnsAllVariants()
    {
        // Act
        IndicatorListing listing = Alma.SeriesListing;

        // Assert
        listing.Should().NotBeNull();
        listing.Name.Should().Be("Arnaud Legoux Moving Average (ALMA)");
        listing.Uiid.Should().Be("ALMA");
        listing.Style.Should().Be(Style.Series);
        listing.Category.Should().Be(Category.MovingAverage);
        listing.MethodName.Should().Be("ToAlma");

        listing.Parameters.Should().NotBeNull();
        listing.Parameters.Should().HaveCount(3);

        IndicatorParam lookbackPeriodsParam = listing.Parameters.SingleOrDefault(static p => p.ParameterName == "lookbackPeriods");
        lookbackPeriodsParam.Should().NotBeNull();
        IndicatorParam offsetParam1 = listing.Parameters.SingleOrDefault(static p => p.ParameterName == "offset");
        offsetParam1.Should().NotBeNull();
        IndicatorParam sigmaParam2 = listing.Parameters.SingleOrDefault(static p => p.ParameterName == "sigma");
        sigmaParam2.Should().NotBeNull();

        listing.Results.Should().NotBeNull();
        listing.Results.Should().HaveCount(1);

        IndicatorResult almaResult = listing.Results.SingleOrDefault(static r => r.DataName == "Alma");
        almaResult.Should().NotBeNull();
        almaResult?.DisplayName.Should().Be("Arnaud Legoux Moving Average (ALMA)");
        almaResult.IsReusable.Should().Be(true);
    }

    [TestMethod]
    public void AlmaStream_InCatalog_ReturnsAllVariants()
    {
        // Act
        IndicatorListing listing = Alma.StreamListing;

        // Assert
        listing.Should().NotBeNull();
        listing.Name.Should().Be("Arnaud Legoux Moving Average (ALMA)");
        listing.Uiid.Should().Be("ALMA");
        listing.Style.Should().Be(Style.Stream);
        listing.Category.Should().Be(Category.MovingAverage);
        listing.MethodName.Should().Be("ToAlmaHub");

        listing.Parameters.Should().NotBeNull();
        listing.Parameters.Should().HaveCount(3);

        IndicatorParam lookbackPeriodsParam = listing.Parameters.SingleOrDefault(static p => p.ParameterName == "lookbackPeriods");
        lookbackPeriodsParam.Should().NotBeNull();
        IndicatorParam offsetParam1 = listing.Parameters.SingleOrDefault(static p => p.ParameterName == "offset");
        offsetParam1.Should().NotBeNull();
        IndicatorParam sigmaParam2 = listing.Parameters.SingleOrDefault(static p => p.ParameterName == "sigma");
        sigmaParam2.Should().NotBeNull();

        listing.Results.Should().NotBeNull();
        listing.Results.Should().HaveCount(1);

        IndicatorResult almaResult = listing.Results.SingleOrDefault(static r => r.DataName == "Alma");
        almaResult.Should().NotBeNull();
        almaResult?.DisplayName.Should().Be("Arnaud Legoux Moving Average (ALMA)");
        almaResult.IsReusable.Should().Be(true);
    }

    [TestMethod]
    public void AlmaBuffer_InCatalog_ReturnsAllVariants()
    {
        // Act
        IndicatorListing listing = Alma.BufferListing;

        // Assert
        listing.Should().NotBeNull();
        listing.Name.Should().Be("Arnaud Legoux Moving Average (ALMA)");
        listing.Uiid.Should().Be("ALMA");
        listing.Style.Should().Be(Style.Buffer);
        listing.Category.Should().Be(Category.MovingAverage);
        listing.MethodName.Should().Be("ToAlmaList");

        listing.Parameters.Should().NotBeNull();
        listing.Parameters.Should().HaveCount(3);

        IndicatorParam lookbackPeriodsParam = listing.Parameters.SingleOrDefault(static p => p.ParameterName == "lookbackPeriods");
        lookbackPeriodsParam.Should().NotBeNull();
        IndicatorParam offsetParam1 = listing.Parameters.SingleOrDefault(static p => p.ParameterName == "offset");
        offsetParam1.Should().NotBeNull();
        IndicatorParam sigmaParam2 = listing.Parameters.SingleOrDefault(static p => p.ParameterName == "sigma");
        sigmaParam2.Should().NotBeNull();

        listing.Results.Should().NotBeNull();
        listing.Results.Should().HaveCount(1);

        IndicatorResult almaResult = listing.Results.SingleOrDefault(static r => r.DataName == "Alma");
        almaResult.Should().NotBeNull();
        almaResult?.DisplayName.Should().Be("Arnaud Legoux Moving Average (ALMA)");
        almaResult.IsReusable.Should().Be(true);
    }
}
