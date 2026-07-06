namespace Catalogging;

/// <summary>
/// Test class for BollingerBands catalog functionality.
/// </summary>
[TestClass]
public class BollingerBandsCatalogTests : TestBase
{
    [TestMethod]
    public void BollingerBandsSeries_InCatalog_ReturnsAllVariants()
    {
        // Act
        IndicatorListing listing = BollingerBands.SeriesListing;

        // Assert
        listing.Should().NotBeNull();
        listing.Name.Should().Be("Bollinger Bands®");
        listing.Uiid.Should().Be("BB");
        listing.Style.Should().Be(Style.Series);
        listing.Category.Should().Be(Category.PriceChannel);
        listing.MethodName.Should().Be("ToBollingerBands");

        listing.Parameters.Should().NotBeNull();
        listing.Parameters.Should().HaveCount(2);

        IndicatorParam lookbackPeriodsParam = listing.Parameters.SingleOrDefault(static p => p.ParameterName == "lookbackPeriods");
        lookbackPeriodsParam.Should().NotBeNull();
        IndicatorParam standardDeviationsParam1 = listing.Parameters.SingleOrDefault(static p => p.ParameterName == "standardDeviations");
        standardDeviationsParam1.Should().NotBeNull();

        listing.Results.Should().NotBeNull();
        listing.Results.Should().HaveCount(6);

        IndicatorResult smaResult = listing.Results.SingleOrDefault(static r => r.DataName == "Sma");
        smaResult.Should().NotBeNull();
        smaResult?.DisplayName.Should().Be("Centerline (SMA)");
        smaResult.IsReusable.Should().Be(false);
        IndicatorResult upperbandResult1 = listing.Results.SingleOrDefault(static r => r.DataName == "UpperBand");
        upperbandResult1.Should().NotBeNull();
        upperbandResult1?.DisplayName.Should().Be("Upper Band");
        upperbandResult1.IsReusable.Should().Be(false);
        IndicatorResult lowerbandResult2 = listing.Results.SingleOrDefault(static r => r.DataName == "LowerBand");
        lowerbandResult2.Should().NotBeNull();
        lowerbandResult2?.DisplayName.Should().Be("Lower Band");
        lowerbandResult2.IsReusable.Should().Be(false);
        IndicatorResult percentbResult3 = listing.Results.SingleOrDefault(static r => r.DataName == "PercentB");
        percentbResult3.Should().NotBeNull();
        percentbResult3?.DisplayName.Should().Be("%B");
        percentbResult3.IsReusable.Should().Be(true);
        IndicatorResult zscoreResult4 = listing.Results.SingleOrDefault(static r => r.DataName == "ZScore");
        zscoreResult4.Should().NotBeNull();
        zscoreResult4?.DisplayName.Should().Be("Z-score");
        zscoreResult4.IsReusable.Should().Be(false);
        IndicatorResult widthResult5 = listing.Results.SingleOrDefault(static r => r.DataName == "Width");
        widthResult5.Should().NotBeNull();
        widthResult5?.DisplayName.Should().Be("Width");
        widthResult5.IsReusable.Should().Be(false);
    }

    [TestMethod]
    public void BollingerBandsStream_InCatalog_ReturnsAllVariants()
    {
        // Act
        IndicatorListing listing = BollingerBands.StreamListing;

        // Assert
        listing.Should().NotBeNull();
        listing.Name.Should().Be("Bollinger Bands®");
        listing.Uiid.Should().Be("BB");
        listing.Style.Should().Be(Style.Stream);
        listing.Category.Should().Be(Category.PriceChannel);
        listing.MethodName.Should().Be("ToBollingerBandsHub");

        listing.Parameters.Should().NotBeNull();
        listing.Parameters.Should().HaveCount(2);

        listing.Results.Should().NotBeNull();
        listing.Results.Should().HaveCount(6);

        IndicatorResult percentbResult = listing.Results.SingleOrDefault(static r => r.DataName == "PercentB");
        percentbResult.Should().NotBeNull();
        percentbResult?.IsReusable.Should().Be(true);
    }

    [TestMethod]
    public void BollingerBandsBuffer_InCatalog_ReturnsAllVariants()
    {
        // Act
        IndicatorListing listing = BollingerBands.BufferListing;

        // Assert
        listing.Should().NotBeNull();
        listing.Name.Should().Be("Bollinger Bands®");
        listing.Uiid.Should().Be("BB");
        listing.Style.Should().Be(Style.Buffer);
        listing.Category.Should().Be(Category.PriceChannel);
        listing.MethodName.Should().Be("ToBollingerBandsList");

        listing.Parameters.Should().NotBeNull();
        listing.Parameters.Should().HaveCount(2);

        listing.Results.Should().NotBeNull();
        listing.Results.Should().HaveCount(6);

        IndicatorResult percentbResult = listing.Results.SingleOrDefault(static r => r.DataName == "PercentB");
        percentbResult.Should().NotBeNull();
        percentbResult?.IsReusable.Should().Be(true);
    }
}
