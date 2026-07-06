namespace Catalogging;

/// <summary>
/// Test class for Atr catalog functionality.
/// </summary>
[TestClass]
public class AtrCatalogTests : TestBase
{
    [TestMethod]
    public void AtrSeries_InCatalog_ReturnsAllVariants()
    {
        // Act
        IndicatorListing listing = Atr.SeriesListing;

        // Assert
        listing.Should().NotBeNull();
        listing.Name.Should().Be("Average True Range (ATR)");
        listing.Uiid.Should().Be("ATR");
        listing.Style.Should().Be(Style.Series);
        listing.Category.Should().Be(Category.PriceCharacteristic);
        listing.MethodName.Should().Be("ToAtr");

        listing.Parameters.Should().NotBeNull();
        listing.Parameters.Should().HaveCount(1);

        IndicatorParam lookbackPeriodsParam = listing.Parameters.SingleOrDefault(static p => p.ParameterName == "lookbackPeriods");
        lookbackPeriodsParam.Should().NotBeNull();

        listing.Results.Should().NotBeNull();
        listing.Results.Should().HaveCount(3);

        IndicatorResult trResult = listing.Results.SingleOrDefault(static r => r.DataName == "Tr");
        trResult.Should().NotBeNull();
        trResult?.DisplayName.Should().Be("True Range");
        trResult.IsReusable.Should().Be(false);
        IndicatorResult atrResult1 = listing.Results.SingleOrDefault(static r => r.DataName == "Atr");
        atrResult1.Should().NotBeNull();
        atrResult1?.DisplayName.Should().Be("ATR");
        atrResult1.IsReusable.Should().Be(false);
        IndicatorResult atrpResult2 = listing.Results.SingleOrDefault(static r => r.DataName == "Atrp");
        atrpResult2.Should().NotBeNull();
        atrpResult2?.DisplayName.Should().Be("ATR %");
        atrpResult2.IsReusable.Should().Be(true);
    }
    [TestMethod]
    public void AtrStream_InCatalog_ReturnsAllVariants()
    {
        // Act
        IndicatorListing listing = Atr.StreamListing;

        // Assert
        listing.Should().NotBeNull();
        listing.Name.Should().Be("Average True Range (ATR)");
        listing.Uiid.Should().Be("ATR");
        listing.Style.Should().Be(Style.Stream);
        listing.Category.Should().Be(Category.PriceCharacteristic);
        listing.MethodName.Should().Be("ToAtrHub");

        listing.Parameters.Should().NotBeNull();
        listing.Parameters.Should().HaveCount(1);

        IndicatorParam lookbackPeriodsParam = listing.Parameters.SingleOrDefault(static p => p.ParameterName == "lookbackPeriods");
        lookbackPeriodsParam.Should().NotBeNull();

        listing.Results.Should().NotBeNull();
        listing.Results.Should().HaveCount(3);

        IndicatorResult trResult = listing.Results.SingleOrDefault(static r => r.DataName == "Tr");
        trResult.Should().NotBeNull();
        trResult?.DisplayName.Should().Be("True Range");
        trResult.IsReusable.Should().Be(false);
        IndicatorResult atrResult1 = listing.Results.SingleOrDefault(static r => r.DataName == "Atr");
        atrResult1.Should().NotBeNull();
        atrResult1?.DisplayName.Should().Be("ATR");
        atrResult1.IsReusable.Should().Be(false);
        IndicatorResult atrpResult2 = listing.Results.SingleOrDefault(static r => r.DataName == "Atrp");
        atrpResult2.Should().NotBeNull();
        atrpResult2?.DisplayName.Should().Be("ATR %");
        atrpResult2.IsReusable.Should().Be(true);
    }

    [TestMethod]
    public void AtrBuffer_InCatalog_ReturnsAllVariants()
    {
        // Act
        IndicatorListing listing = Atr.BufferListing;

        // Assert
        listing.Should().NotBeNull();
        listing.Name.Should().Be("Average True Range (ATR)");
        listing.Uiid.Should().Be("ATR");
        listing.Style.Should().Be(Style.Buffer);
        listing.Category.Should().Be(Category.PriceCharacteristic);
        listing.MethodName.Should().Be("ToAtrList");

        listing.Parameters.Should().NotBeNull();
        listing.Parameters.Should().HaveCount(1);

        IndicatorParam lookbackPeriodsParam = listing.Parameters.SingleOrDefault(static p => p.ParameterName == "lookbackPeriods");
        lookbackPeriodsParam.Should().NotBeNull();

        listing.Results.Should().NotBeNull();
        listing.Results.Should().HaveCount(3);

        IndicatorResult trResult = listing.Results.SingleOrDefault(static r => r.DataName == "Tr");
        trResult.Should().NotBeNull();
        trResult?.DisplayName.Should().Be("True Range");
        trResult.IsReusable.Should().Be(false);
        IndicatorResult atrResult1 = listing.Results.SingleOrDefault(static r => r.DataName == "Atr");
        atrResult1.Should().NotBeNull();
        atrResult1?.DisplayName.Should().Be("ATR");
        atrResult1.IsReusable.Should().Be(false);
        IndicatorResult atrpResult2 = listing.Results.SingleOrDefault(static r => r.DataName == "Atrp");
        atrpResult2.Should().NotBeNull();
        atrpResult2?.DisplayName.Should().Be("ATR %");
        atrpResult2.IsReusable.Should().Be(true);
    }
}
