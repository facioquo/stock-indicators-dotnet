namespace Catalogging;

/// <summary>
/// Test class for Aroon catalog functionality.
/// </summary>
[TestClass]
public class AroonCatalogTests : TestBase
{
    [TestMethod]
    public void AroonSeries_InCatalog_ReturnsAllVariants()
    {
        // Act
        IndicatorListing listing = Aroon.SeriesListing;

        // Assert
        listing.Should().NotBeNull();
        listing.Name.Should().Be("Aroon Up/Down");
        listing.Uiid.Should().Be("AROON");
        listing.Style.Should().Be(Style.Series);
        listing.Category.Should().Be(Category.PriceTrend);
        listing.MethodName.Should().Be("ToAroon");

        listing.Parameters.Should().NotBeNull();
        listing.Parameters.Should().HaveCount(1);

        IndicatorParam lookbackPeriodsParam = listing.Parameters.SingleOrDefault(static p => p.ParameterName == "lookbackPeriods");
        lookbackPeriodsParam.Should().NotBeNull();

        listing.Results.Should().NotBeNull();
        listing.Results.Should().HaveCount(3);

        IndicatorResult aroonupResult = listing.Results.SingleOrDefault(static r => r.DataName == "AroonUp");
        aroonupResult.Should().NotBeNull();
        aroonupResult?.DisplayName.Should().Be("Aroon Up");
        aroonupResult.IsReusable.Should().Be(false);
        IndicatorResult aroondownResult1 = listing.Results.SingleOrDefault(static r => r.DataName == "AroonDown");
        aroondownResult1.Should().NotBeNull();
        aroondownResult1?.DisplayName.Should().Be("Aroon Down");
        aroondownResult1.IsReusable.Should().Be(false);
        IndicatorResult oscillatorResult2 = listing.Results.SingleOrDefault(static r => r.DataName == "Oscillator");
        oscillatorResult2.Should().NotBeNull();
        oscillatorResult2?.DisplayName.Should().Be("Oscillator");
        oscillatorResult2.IsReusable.Should().Be(true);
    }
    [TestMethod]
    public void AroonStream_InCatalog_ReturnsAllVariants()
    {
        // Act
        IndicatorListing listing = Aroon.StreamListing;

        // Assert
        listing.Should().NotBeNull();
        listing.Name.Should().Be("Aroon Up/Down");
        listing.Uiid.Should().Be("AROON");
        listing.Style.Should().Be(Style.Stream);
        listing.Category.Should().Be(Category.PriceTrend);
        listing.MethodName.Should().Be("ToAroonHub");

        listing.Parameters.Should().NotBeNull();
        listing.Parameters.Should().HaveCount(1);

        IndicatorParam lookbackPeriodsParam = listing.Parameters.SingleOrDefault(static p => p.ParameterName == "lookbackPeriods");
        lookbackPeriodsParam.Should().NotBeNull();

        listing.Results.Should().NotBeNull();
        listing.Results.Should().HaveCount(3);

        IndicatorResult aroonupResult = listing.Results.SingleOrDefault(static r => r.DataName == "AroonUp");
        aroonupResult.Should().NotBeNull();
        aroonupResult?.DisplayName.Should().Be("Aroon Up");
        aroonupResult.IsReusable.Should().Be(false);
        IndicatorResult aroondownResult1 = listing.Results.SingleOrDefault(static r => r.DataName == "AroonDown");
        aroondownResult1.Should().NotBeNull();
        aroondownResult1?.DisplayName.Should().Be("Aroon Down");
        aroondownResult1.IsReusable.Should().Be(false);
        IndicatorResult oscillatorResult2 = listing.Results.SingleOrDefault(static r => r.DataName == "Oscillator");
        oscillatorResult2.Should().NotBeNull();
        oscillatorResult2?.DisplayName.Should().Be("Oscillator");
        oscillatorResult2.IsReusable.Should().Be(true);
    }
    [TestMethod]
    public void AroonBuffer_InCatalog_ReturnsAllVariants()
    {
        // Act
        IndicatorListing listing = Aroon.BufferListing;

        // Assert
        listing.Should().NotBeNull();
        listing.Name.Should().Be("Aroon Up/Down");
        listing.Uiid.Should().Be("AROON");
        listing.Style.Should().Be(Style.Buffer);
        listing.Category.Should().Be(Category.PriceTrend);
        listing.MethodName.Should().Be("ToAroonList");

        listing.Parameters.Should().NotBeNull();
        listing.Parameters.Should().HaveCount(1);

        IndicatorParam lookbackPeriodsParam = listing.Parameters.SingleOrDefault(static p => p.ParameterName == "lookbackPeriods");
        lookbackPeriodsParam.Should().NotBeNull();

        listing.Results.Should().NotBeNull();
        listing.Results.Should().HaveCount(3);

        IndicatorResult aroonupResult = listing.Results.SingleOrDefault(static r => r.DataName == "AroonUp");
        aroonupResult.Should().NotBeNull();
        aroonupResult?.DisplayName.Should().Be("Aroon Up");
        aroonupResult.IsReusable.Should().Be(false);
        IndicatorResult aroondownResult1 = listing.Results.SingleOrDefault(static r => r.DataName == "AroonDown");
        aroondownResult1.Should().NotBeNull();
        aroondownResult1?.DisplayName.Should().Be("Aroon Down");
        aroondownResult1.IsReusable.Should().Be(false);
        IndicatorResult oscillatorResult2 = listing.Results.SingleOrDefault(static r => r.DataName == "Oscillator");
        oscillatorResult2.Should().NotBeNull();
        oscillatorResult2?.DisplayName.Should().Be("Oscillator");
        oscillatorResult2.IsReusable.Should().Be(true);
    }
}
