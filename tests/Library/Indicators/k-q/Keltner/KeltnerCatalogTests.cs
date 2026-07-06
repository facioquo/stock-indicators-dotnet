namespace Catalogging;

/// <summary>
/// Test class for Keltner catalog functionality.
/// </summary>
[TestClass]
public class KeltnerCatalogTests : TestBase
{
    [TestMethod]
    public void KeltnerSeries_InCatalog_ReturnsAllVariants()
    {
        // Act
        IndicatorListing listing = Keltner.SeriesListing;

        // Assert
        listing.Should().NotBeNull();
        listing.Name.Should().Be("Keltner Channels");
        listing.Uiid.Should().Be("KELTNER");
        listing.Style.Should().Be(Style.Series);
        listing.Category.Should().Be(Category.PriceChannel);
        listing.MethodName.Should().Be("ToKeltner");

        listing.Parameters.Should().NotBeNull();
        listing.Parameters.Should().HaveCount(3);

        IndicatorParam emaPeriodsParam = listing.Parameters.SingleOrDefault(static p => p.ParameterName == "emaPeriods");
        emaPeriodsParam.Should().NotBeNull();
        IndicatorParam multiplierParam1 = listing.Parameters.SingleOrDefault(static p => p.ParameterName == "multiplier");
        multiplierParam1.Should().NotBeNull();
        IndicatorParam atrPeriodsParam2 = listing.Parameters.SingleOrDefault(static p => p.ParameterName == "atrPeriods");
        atrPeriodsParam2.Should().NotBeNull();

        listing.Results.Should().NotBeNull();
        listing.Results.Should().HaveCount(4);

        IndicatorResult upperbandResult = listing.Results.SingleOrDefault(static r => r.DataName == "UpperBand");
        upperbandResult.Should().NotBeNull();
        upperbandResult?.DisplayName.Should().Be("Upper Band");
        upperbandResult.IsReusable.Should().Be(false);
        IndicatorResult centerlineResult1 = listing.Results.SingleOrDefault(static r => r.DataName == "Centerline");
        centerlineResult1.Should().NotBeNull();
        centerlineResult1?.DisplayName.Should().Be("Centerline");
        centerlineResult1.IsReusable.Should().Be(true);
        IndicatorResult lowerbandResult2 = listing.Results.SingleOrDefault(static r => r.DataName == "LowerBand");
        lowerbandResult2.Should().NotBeNull();
        lowerbandResult2?.DisplayName.Should().Be("Lower Band");
        lowerbandResult2.IsReusable.Should().Be(false);
        IndicatorResult widthResult3 = listing.Results.SingleOrDefault(static r => r.DataName == "Width");
        widthResult3.Should().NotBeNull();
        widthResult3?.DisplayName.Should().Be("Width");
        widthResult3.IsReusable.Should().Be(false);
    }

    [TestMethod]
    public void KeltnerStream_InCatalog_ReturnsAllVariants()
    {
        // Act
        IndicatorListing listing = Keltner.StreamListing;

        // Assert
        listing.Should().NotBeNull();
        listing.Name.Should().Be("Keltner Channels");
        listing.Uiid.Should().Be("KELTNER");
        listing.Style.Should().Be(Style.Stream);
        listing.Category.Should().Be(Category.PriceChannel);
        listing.MethodName.Should().Be("ToKeltnerHub");

        listing.Parameters.Should().NotBeNull();
        listing.Parameters.Should().HaveCount(3);

        IndicatorParam emaPeriodsParam = listing.Parameters.SingleOrDefault(static p => p.ParameterName == "emaPeriods");
        emaPeriodsParam.Should().NotBeNull();
        IndicatorParam multiplierParam1 = listing.Parameters.SingleOrDefault(static p => p.ParameterName == "multiplier");
        multiplierParam1.Should().NotBeNull();
        IndicatorParam atrPeriodsParam2 = listing.Parameters.SingleOrDefault(static p => p.ParameterName == "atrPeriods");
        atrPeriodsParam2.Should().NotBeNull();

        listing.Results.Should().NotBeNull();
        listing.Results.Should().HaveCount(4);

        IndicatorResult upperbandResult = listing.Results.SingleOrDefault(static r => r.DataName == "UpperBand");
        upperbandResult.Should().NotBeNull();
        upperbandResult?.DisplayName.Should().Be("Upper Band");
        upperbandResult.IsReusable.Should().Be(false);
        IndicatorResult centerlineResult1 = listing.Results.SingleOrDefault(static r => r.DataName == "Centerline");
        centerlineResult1.Should().NotBeNull();
        centerlineResult1?.DisplayName.Should().Be("Centerline");
        centerlineResult1.IsReusable.Should().Be(true);
        IndicatorResult lowerbandResult2 = listing.Results.SingleOrDefault(static r => r.DataName == "LowerBand");
        lowerbandResult2.Should().NotBeNull();
        lowerbandResult2?.DisplayName.Should().Be("Lower Band");
        lowerbandResult2.IsReusable.Should().Be(false);
        IndicatorResult widthResult3 = listing.Results.SingleOrDefault(static r => r.DataName == "Width");
        widthResult3.Should().NotBeNull();
        widthResult3?.DisplayName.Should().Be("Width");
        widthResult3.IsReusable.Should().Be(false);
    }
}
